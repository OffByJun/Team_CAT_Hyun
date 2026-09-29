using System;
using System.Collections.Generic;
using _001_Scripts.Map.Tiles;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Map
{
    public sealed class MapManager : GameBehaviour
    {
        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool useUndergroundMap;
        [SerializeField] private bool showHiddenBlocks;
        [SerializeField] private Transform generatedMapRoot;

        [SerializeField, HideInInspector] private GameObject groundPrefab;
        [SerializeField, HideInInspector] private GameObject brickPrefab;
        [SerializeField, HideInInspector] private GameObject questionPrefab;
        [SerializeField, HideInInspector] private GameObject pipePrefab;
        [SerializeField, HideInInspector] private GameObject stairPrefab;
        [SerializeField, HideInInspector] private GameObject hiddenOneUpBlockPrefab;
        [SerializeField, HideInInspector] private GameObject flagPrefab;
        [SerializeField, HideInInspector] private GameObject castlePrefab;
        [SerializeField, HideInInspector] private GameObject coinPrefab;
        [SerializeField, HideInInspector] private GameObject usedBlockPrefab;

        [Header("Server Monster Views")]
        [SerializeField] private GameObject goombaPrefab;
        [SerializeField] private GameObject koopaPrefab;

        [SerializeField] private Vector2 goombaPivotOffset;
        [SerializeField] private Vector2 koopaPivotOffset;
        [SerializeField] private Transform generatedMonsterRoot;

        private readonly MapInteractionState interactions = new();
        private readonly Dictionary<int, GameObject> monstersById = new();
        private readonly Dictionary<int, MonsterKind> monsterKinds = new();
        private readonly HashSet<int> receivedMonsterIds = new();
        private readonly List<int> removedMonsterIds = new();

        private MapTileFactory tileFactory;
        private GridMap serverMap;
        private bool mapBuilt;

        private Func<int, int, bool> sendBlockHit;
        private Func<int, bool> sendMonsterKill;

        private GridMap CurrentMap => serverMap ?? (useUndergroundMap
            ? MapCatalog.World11Underground
            : MapCatalog.Default);

        private void Awake()
        {
            EnsureTileFactory();
        }

        private void Start()
        {
            if (buildOnStart && !mapBuilt)
            {
                BuildMap();
            }
        }

        public void SetInteractionTransport(
            Func<int, int, bool> hitBlock,
            Func<int, bool> killMonster)
        {
            sendBlockHit = hitBlock;
            sendMonsterKill = killMonster;
            interactions.ClearPendingRequests();
        }

        public void OnMapUpdated(GridMap map)
        {
            if (map == null)
            {
                return;
            }

            serverMap = map;
            BuildMap();
        }

        public void OnMonstersUpdated(Monster[] monsters)
        {
            if (monsters == null)
            {
                return;
            }

            receivedMonsterIds.Clear();

            foreach (Monster monster in monsters)
            {
                receivedMonsterIds.Add(monster.Id);
                monstersById.TryGetValue(monster.Id, out GameObject view);

                if (view == null || monsterKinds[monster.Id] != monster.Kind)
                {
                    if (view != null)
                    {
                        RemoveGeneratedObject(view);
                    }

                    view = CreateMonsterView(monster);
                    if (view == null)
                    {
                        continue;
                    }

                    monstersById[monster.Id] = view;
                    monsterKinds[monster.Id] = monster.Kind;
                }
                view.transform.position = new Vector3(monster.X, monster.Y, 0f);
            }

            removedMonsterIds.Clear();
            foreach (int id in monstersById.Keys)
            {
                if (!receivedMonsterIds.Contains(id))
                {
                    removedMonsterIds.Add(id);
                }
            }

            foreach (int id in removedMonsterIds)
            {
                interactions.RemoveMonster(id);
                RemoveGeneratedObject(monstersById[id]);
                monstersById.Remove(id);
                monsterKinds.Remove(id);
            }
        }

        public void ResetLocalInteractions()
        {
            interactions.ResetLocalProgress();
            BuildMap();
        }

        [ContextMenu("Build Map")]
        public void BuildMap()
        {
            EnsureTileFactory();
            ClearMap();

            GridMap map = CurrentMap;
            interactions.ApplyMap(map);

            Transform root = GetOrCreateMapRoot();
            foreach (GridCell cell in map.Cells)
            {
                if (ShouldSkipCell(cell))
                {
                    continue;
                }

                GameObject tile = tileFactory.CreateTile(map, cell, root, showHiddenBlocks);
                if (tile == null)
                {
                    continue;
                }

                AddInteractionIfNeeded(tile, cell);
            }

            mapBuilt = true;
            Debug.Log($"맵 배치 완료: {map.Id} v{map.Version}, {map.Cells.Count}개 셀", this);
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            mapBuilt = false;

            if (generatedMapRoot == null)
            {
                generatedMapRoot = transform.Find("Generated Map");
            }

            if (generatedMapRoot == null)
            {
                return;
            }

            for (int i = generatedMapRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = generatedMapRoot.GetChild(i).gameObject;
                RemoveGeneratedObject(child);
            }
        }

        [ContextMenu("Reset Map")]
        public void ResetMap()
        {
            BuildMap();
        }

        public Vector3 GetSpawnFeetPosition()
        {
            PlayerPosition spawn = CurrentMap.SpawnFeet;
            return new Vector3(spawn.X, spawn.Y, spawn.Z);
        }

        public float GetFallBoundaryY()
        {
            return CurrentMap.FallBoundaryY;
        }

        internal bool TryHitBlock(GridCell cell, out BlockContent content)
        {
            content = BlockContent.None;

            // 서버 명령에는 맵 ID가 없으므로 오프라인 또는 다른 맵의 타격은 보내지 않습니다.
            if (serverMap == null || !IsCurrentCell(cell))
            {
                return false;
            }

            return interactions.TryHit(cell, sendBlockHit, out content);
        }

        internal bool TryCollectCoin(GridCell cell, out BlockContent content)
        {
            content = BlockContent.None;
            return IsCurrentCell(cell) && interactions.TryCollect(cell, out content);
        }

        internal bool TryStompMonster(int id)
        {
            return monstersById.TryGetValue(id, out GameObject view)
                && view != null
                && interactions.TryStomp(id, sendMonsterKill);
        }

        private void EnsureTileFactory()
        {
            if (tileFactory == null)
            {
                tileFactory = GetComponent<MapTileFactory>();
            }

            if (tileFactory == null)
            {
                tileFactory = gameObject.AddComponent<MapTileFactory>();
            }

            tileFactory.SetLegacyPrefab(TileKind.Ground, groundPrefab);
            tileFactory.SetLegacyPrefab(TileKind.Brick, brickPrefab);
            tileFactory.SetLegacyPrefab(TileKind.Question, questionPrefab);
            tileFactory.SetLegacyPrefab(TileKind.Pipe, pipePrefab);
            tileFactory.SetLegacyPrefab(TileKind.Stair, stairPrefab);
            tileFactory.SetLegacyPrefab(TileKind.HiddenOneUpBlock, hiddenOneUpBlockPrefab);
            tileFactory.SetLegacyPrefab(TileKind.Flag, flagPrefab);
            tileFactory.SetLegacyPrefab(TileKind.Castle, castlePrefab);
            tileFactory.SetLegacyPrefab(TileKind.Coin, coinPrefab);
            tileFactory.SetLegacyPrefab(TileKind.UsedBlock, usedBlockPrefab);
        }

        private bool IsCurrentCell(GridCell cell)
        {
            GridMap map = CurrentMap;

            return cell.X >= 0
                && cell.Y >= 0
                && cell.X < map.Width
                && cell.Y < map.Height
                && map.GetTile(cell.X, cell.Y) == cell.Kind;
        }

        private bool ShouldSkipCell(GridCell cell)
        {
            return cell.Kind == TileKind.Empty
                || (cell.Kind == TileKind.Coin && interactions.IsCollected(cell.Id));
        }

        private void AddInteractionIfNeeded(GameObject tile, GridCell cell)
        {
            bool isInteractive = cell.Kind == TileKind.Brick
                || cell.Kind == TileKind.Question
                || cell.Kind == TileKind.HiddenOneUpBlock
                || cell.Kind == TileKind.Coin;

            if (!isInteractive)
            {
                return;
            }

            InteractiveTile interaction = tile.GetComponent<InteractiveTile>();
            if (interaction == null)
            {
                interaction = tile.AddComponent<InteractiveTile>();
            }

            interaction.Initialize(this, cell);
        }

        private Transform GetOrCreateMapRoot()
        {
            if (generatedMapRoot != null)
            {
                return generatedMapRoot;
            }

            generatedMapRoot = transform.Find("Generated Map");
            if (generatedMapRoot != null)
            {
                return generatedMapRoot;
            }

            generatedMapRoot = new GameObject("Generated Map").transform;
            generatedMapRoot.SetParent(transform, false);
            return generatedMapRoot;
        }

        private GameObject CreateMonsterView(Monster monster)
        {
            bool isKoopa = monster.Kind == MonsterKind.Koopa;

            if (goombaPrefab == null)
            {
                goombaPrefab = Resources.Load<GameObject>("MapMonsters/Goomba");
            }

            if (koopaPrefab == null)
            {
                koopaPrefab = Resources.Load<GameObject>("MapMonsters/Koopa");
            }

            GameObject prefab = isKoopa ? koopaPrefab : goombaPrefab;
            if (prefab == null)
            {
                Debug.LogError($"{monster.Kind} 표시 프리팹을 찾을 수 없습니다.", this);
                return null;
            }

            Transform monsterRoot = GetOrCreateMonsterRoot();
            GameObject root = new($"{monster.Kind}_{monster.Id}");
            root.transform.SetParent(monsterRoot, false);
            root.transform.position = new Vector3(monster.X, monster.Y, 0f);

            GameObject visual = Instantiate(prefab, root.transform);
            visual.transform.localPosition = isKoopa ? koopaPivotOffset : goombaPivotOffset;

            DisableLocalMonsterPhysics(visual);
            AddMonsterContact(root, isKoopa);
            root.AddComponent<MonsterInteraction>().Initialize(this, monster.Id);

            return root;
        }

        private Transform GetOrCreateMonsterRoot()
        {
            if (generatedMonsterRoot != null)
            {
                return generatedMonsterRoot;
            }

            generatedMonsterRoot = new GameObject("Generated Monsters").transform;
            generatedMonsterRoot.SetParent(transform, false);
            return generatedMonsterRoot;
        }

        private static void DisableLocalMonsterPhysics(GameObject visual)
        {
            foreach (Rigidbody2D body in visual.GetComponentsInChildren<Rigidbody2D>(true))
            {
                body.simulated = false;
            }

            foreach (Collider2D collider in visual.GetComponentsInChildren<Collider2D>(true))
            {
                collider.enabled = false;
            }
        }

        private static void AddMonsterContact(GameObject root, bool isKoopa)
        {
            float height = isKoopa ? 1.2f : 0.8f;

            BoxCollider2D contact = root.AddComponent<BoxCollider2D>();
            contact.size = new Vector2(0.8f, height);
            contact.offset = new Vector2(0f, height * 0.5f);
            contact.isTrigger = true;
        }

        private static void RemoveGeneratedObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            target.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
