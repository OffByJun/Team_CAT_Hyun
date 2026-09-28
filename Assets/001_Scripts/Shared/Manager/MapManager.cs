using System;
using System.Collections.Generic;
using _001_Scripts.Map.Tiles;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Map
{
    public sealed class MapManager : MonoBehaviour
    {
        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool useUndergroundMap;
        [SerializeField] private bool showHiddenBlocks;
        [SerializeField] private Transform generatedMapRoot;

        // 기존 씬/프리팹의 직렬화 참조 보존용. 새 설정은 MapTileFactory에서 합니다.
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
        [Tooltip("비워 두면 Resources/MapMonsters의 사각형 프리팹을 불러옵니다.")]
        [SerializeField] private GameObject goombaPrefab;
        [SerializeField] private GameObject koopaPrefab;
        [Tooltip("서버 좌표는 발 중앙입니다. 프리팹 피벗이 중앙이면 반높이만큼 Y를 더합니다.")]
        [SerializeField] private Vector2 goombaPivotOffset;
        [SerializeField] private Vector2 koopaPivotOffset;
        [SerializeField] private Transform generatedMonsterRoot;

        private GridMap serverMap;
        private bool mapBuilt;
        private readonly MapInteractionState interactions = new();
        private Func<int, int, bool> sendBlockHit;
        private Func<int, bool> sendMonsterKill;

        /// <summary>
        /// 서버 담당이 연결 성공 후 등록하고 연결 종료 시 (null, null)로 해제합니다.
        /// 콜백은 연결된 서버에 전송했으면 true, 전송 불가하면 false를 반환합니다.
        /// </summary>
        public void SetInteractionTransport(Func<int, int, bool> hitBlock, Func<int, bool> killMonster)
        {
            sendBlockHit = hitBlock;
            sendMonsterKill = killMonster;
            interactions.ClearPendingRequests();
        }

        internal bool TryHitBlock(GridCell cell, out BlockContent content)
        {
            content = BlockContent.None;
            // 현 DLL의 서버 명령에는 맵 ID가 없으므로 오프라인/다른 방 타격은 보내지 않습니다.
            if (serverMap == null || !IsCurrentCell(cell)) return false;
            return interactions.TryHit(cell, sendBlockHit, out content);
        }

        internal bool TryCollectCoin(GridCell cell, out BlockContent content)
        {
            content = BlockContent.None;
            return IsCurrentCell(cell) && interactions.TryCollect(cell, out content);
        }

        private bool IsCurrentCell(GridCell cell)
        {
            GridMap map = CurrentMap;
            return cell.X >= 0 && cell.Y >= 0 && cell.X < map.Width && cell.Y < map.Height &&
                map.GetTile(cell.X, cell.Y) == cell.Kind;
        }

        internal bool TryStompMonster(int id)
        {
            return monstersById.TryGetValue(id, out GameObject view) && view != null &&
                interactions.TryStomp(id, sendMonsterKill);
        }

        /// <summary>
        /// 서버 OnWorldReset에서 호출합니다. 이미 받은 서버 맵/적은 보존하고
        /// 로컬 수집 기록과 중복 요청 잠금만 초기화합니다. 플레이어는 이동시키지 않습니다.
        /// </summary>
        public void ResetLocalInteractions()
        {
            interactions.ResetLocalProgress();
            BuildMap();
        }
        private readonly Dictionary<int, GameObject> monstersById = new();
        private readonly Dictionary<int, MonsterKind> monsterKinds = new();
        private readonly HashSet<int> receivedMonsterIds = new();
        private readonly List<int> removedMonsterIds = new();

        private MapTileFactory tileFactory;

        private void Awake() => EnsureTileFactory();

        private void EnsureTileFactory()
        {
            if (tileFactory == null) tileFactory = GetComponent<MapTileFactory>();
            if (tileFactory == null) tileFactory = gameObject.AddComponent<MapTileFactory>();
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
        private void Start()
        {
            // 콜백이 Start보다 먼저 도착해도 받은 맵을 다시 덮어쓰지 않습니다.
            if (buildOnStart && !mapBuilt) BuildMap();
        }

        /// <summary>
        /// NetworkManager.OnMapUpdated에서 Unity 메인 스레드로 전달합니다.
        /// 연결을 새로 만들지 않고 기존 연결이 받은 전체 스냅샷을 사용합니다.
        /// </summary>
        public void OnMapUpdated(GridMap map)
        {
            if (map == null) return;
            serverMap = map;
            BuildMap();
        }

        /// <summary>
        /// NetworkManager.OnMonstersUpdated에서 최신 전체 목록을 전달합니다.
        /// null은 무시하고, 빈 배열은 모든 적을 제거합니다. 이동 계산은 서버가 담당합니다.
        /// </summary>
        public void OnMonstersUpdated(Monster[] monsters)
        {
            if (monsters == null) return;
            receivedMonsterIds.Clear();

            foreach (Monster monster in monsters)
            {
                receivedMonsterIds.Add(monster.Id);
                monstersById.TryGetValue(monster.Id, out GameObject view);
                if (view == null || monsterKinds[monster.Id] != monster.Kind)
                {
                    if (view != null) RemoveGeneratedObject(view);
                    view = CreateMonsterView(monster);
                    if (view == null) continue;
                    monstersById[monster.Id] = view;
                    monsterKinds[monster.Id] = monster.Kind;
                }

                // 루트는 항상 발 좌표. 프리팹 피벗 보정은 자식에서만 수행합니다.
                view.transform.position = new Vector3(monster.X, monster.Y, 0f);
            }

            removedMonsterIds.Clear();
            foreach (int id in monstersById.Keys)
                if (!receivedMonsterIds.Contains(id)) removedMonsterIds.Add(id);

            foreach (int id in removedMonsterIds)
            {
                interactions.RemoveMonster(id);
                RemoveGeneratedObject(monstersById[id]);
                monstersById.Remove(id);
                monsterKinds.Remove(id);
            }
        }

        private GameObject CreateMonsterView(Monster monster)
        {
            bool isKoopa = monster.Kind == MonsterKind.Koopa;
            if (goombaPrefab == null)
                goombaPrefab = Resources.Load<GameObject>("MapMonsters/Goomba");
            if (koopaPrefab == null)
                koopaPrefab = Resources.Load<GameObject>("MapMonsters/Koopa");
            GameObject prefab = isKoopa ? koopaPrefab : goombaPrefab;
            if (prefab == null)
            {
                Debug.LogError($"{monster.Kind} 표시 프리팹을 찾을 수 없습니다.", this);
                return null;
            }

            if (generatedMonsterRoot == null)
            {
                generatedMonsterRoot = new GameObject("Generated Monsters").transform;
                generatedMonsterRoot.SetParent(transform, false);
            }

            GameObject root = new GameObject($"{monster.Kind}_{monster.Id}");
            root.transform.SetParent(generatedMonsterRoot, false);
            root.transform.position = new Vector3(monster.X, monster.Y, 0f);
            GameObject visual = Instantiate(prefab, root.transform);
            visual.transform.localPosition = isKoopa ? koopaPivotOffset : goombaPivotOffset;
            // 서버 위치에 로컬 물리를 중복 적용하지 않습니다. 전투 판정은 별도 구현합니다.
            foreach (Rigidbody2D body in visual.GetComponentsInChildren<Rigidbody2D>(true))
                body.simulated = false;
            // 표시용 자식 콜라이더 대신 발 기준 루트에 접촉 트리거 하나를 둡니다.
            foreach (Collider2D collider in visual.GetComponentsInChildren<Collider2D>(true))
                collider.enabled = false;
            float height = isKoopa ? 1.2f : 0.8f;
            BoxCollider2D contact = root.AddComponent<BoxCollider2D>();
            contact.size = new Vector2(0.8f, height);
            contact.offset = new Vector2(0f, height * 0.5f);
            contact.isTrigger = true;
            root.AddComponent<MonsterInteraction>().Initialize(this, monster.Id);
            return root;
        }

        private static void RemoveGeneratedObject(GameObject target)
        {
            if (target == null) return;
            // Destroy는 프레임 끝에 실행되므로 이전 충돌/표시는 즉시 끕니다.
            target.SetActive(false);
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

        [ContextMenu("Build Map")]
        public void BuildMap()
        {
            EnsureTileFactory();
            ClearMap();
            GridMap map = CurrentMap;
            interactions.ApplyMap(map);
            Transform root = GetOrCreateRoot();

            foreach (GridCell cell in map.Cells)
            {
                if (cell.Kind == TileKind.Empty ||
                    (cell.Kind == TileKind.Coin && interactions.IsCollected(cell.Id)))
                    continue;

                GameObject tile = tileFactory.CreateTile(map, cell, root, showHiddenBlocks);
                if (tile == null) continue;
                if (cell.Kind == TileKind.Brick || cell.Kind == TileKind.Question ||
                    cell.Kind == TileKind.HiddenOneUpBlock || cell.Kind == TileKind.Coin)
                {
                    InteractiveTile interaction = tile.GetComponent<InteractiveTile>();
                    if (interaction == null) interaction = tile.AddComponent<InteractiveTile>();
                    interaction.Initialize(this, cell);
                }
            }

            mapBuilt = true;
            Debug.Log($"맵 배치 완료: {map.Id} v{map.Version}, {map.Cells.Count}개 셀", this);
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            mapBuilt = false;
            if (generatedMapRoot == null)
                generatedMapRoot = transform.Find("Generated Map");
            if (generatedMapRoot == null) return;

            for (int i = generatedMapRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = generatedMapRoot.GetChild(i).gameObject;
                RemoveGeneratedObject(child);
            }
        }

        /// <summary>
        /// 최신 서버 맵을 다시 표시합니다. 서버 수신 전에는 원본 맵을 사용합니다.
        /// 공유 월드 초기화는 서버의 다음 OnMapUpdated/OnMonstersUpdated로 반영합니다.
        /// </summary>
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

        public float GetFallBoundaryY() => CurrentMap.FallBoundaryY;

        private GridMap CurrentMap => serverMap ?? (useUndergroundMap
            ? MapCatalog.World11Underground
            : MapCatalog.Default);

        private Transform GetOrCreateRoot()
        {
            if (generatedMapRoot != null) return generatedMapRoot;
            generatedMapRoot = transform.Find("Generated Map");
            if (generatedMapRoot != null) return generatedMapRoot;

            generatedMapRoot = new GameObject("Generated Map").transform;
            generatedMapRoot.SetParent(transform, false);
            return generatedMapRoot;
        }

    }
}
