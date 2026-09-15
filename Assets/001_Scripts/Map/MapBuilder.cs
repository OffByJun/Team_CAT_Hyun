using System.Collections.Generic;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Map
{
    public sealed class MapBuilder : MonoBehaviour
    {
        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool useUndergroundMap;
        [SerializeField] private bool showHiddenBlocks;
        [SerializeField] private Transform generatedMapRoot;

        [Header("Tile Prefabs")]
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private GameObject brickPrefab;
        [SerializeField] private GameObject questionPrefab;
        [SerializeField] private GameObject pipePrefab;
        [SerializeField] private GameObject stairPrefab;
        [SerializeField] private GameObject hiddenOneUpBlockPrefab;
        [SerializeField] private GameObject flagPrefab;
        [SerializeField] private GameObject castlePrefab;
        [SerializeField] private GameObject coinPrefab;

        private static Sprite fallbackSprite;

        private static readonly Dictionary<TileKind, Color> TileColors = new()
        {
            { TileKind.Ground, new Color32(181, 91, 42, 255) },
            { TileKind.Brick, new Color32(203, 92, 47, 255) },
            { TileKind.Question, new Color32(247, 184, 49, 255) },
            { TileKind.Pipe, new Color32(57, 181, 74, 255) },
            { TileKind.Stair, new Color32(210, 140, 76, 255) },
            { TileKind.HiddenOneUpBlock, new Color32(160, 160, 160, 100) },
            { TileKind.Flag, new Color32(245, 245, 245, 255) },
            { TileKind.Castle, new Color32(126, 82, 52, 255) },
            { TileKind.Coin, new Color32(255, 215, 35, 255) }
        };

        private void Start()
        {
            if (buildOnStart) BuildMap();
        }

        [ContextMenu("Build Map")]
        public void BuildMap()
        {
            ClearMap();
            GridMap map = CurrentMap;
            Transform root = GetOrCreateRoot();

            foreach (GridCell cell in map.Cells)
            {
                if (cell.Kind == TileKind.Empty ||
                    (cell.Kind == TileKind.HiddenOneUpBlock && !showHiddenBlocks))
                    continue;

                GameObject prefab = GetPrefab(cell.Kind);
                if (prefab == null)
                {
                    Debug.LogWarning($"{cell.Kind} 프리팹이 지정되지 않았습니다.", this);
                    continue;
                }

                PlayerPosition center = map.GetCellCenter(cell.X, cell.Y);
                GameObject tile = Instantiate(prefab,
                    new Vector3(center.X, center.Y, center.Z),
                    Quaternion.identity, root);

                tile.name = $"{cell.Kind}_{cell.X}_{cell.Y}";
                tile.transform.localScale = Vector3.one * map.CellSize;
                ConfigureTile(tile, cell);
            }

            Debug.Log($"맵 배치 완료: {map.Id} v{map.Version}, {map.Cells.Count}개 셀", this);
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (generatedMapRoot == null)
                generatedMapRoot = transform.Find("Generated Map");
            if (generatedMapRoot == null) return;

            for (int i = generatedMapRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = generatedMapRoot.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        public Vector3 GetSpawnFeetPosition()
        {
            PlayerPosition spawn = CurrentMap.SpawnFeet;
            return new Vector3(spawn.X, spawn.Y, spawn.Z);
        }

        public float GetFallBoundaryY() => CurrentMap.FallBoundaryY;

        private GridMap CurrentMap => useUndergroundMap
            ? MapCatalog.World11Underground
            : MapCatalog.Default;

        private Transform GetOrCreateRoot()
        {
            if (generatedMapRoot != null) return generatedMapRoot;
            generatedMapRoot = transform.Find("Generated Map");
            if (generatedMapRoot != null) return generatedMapRoot;

            generatedMapRoot = new GameObject("Generated Map").transform;
            generatedMapRoot.SetParent(transform, false);
            return generatedMapRoot;
        }

        private GameObject GetPrefab(TileKind kind) => kind switch
        {
            TileKind.Ground => groundPrefab,
            TileKind.Brick => brickPrefab,
            TileKind.Question => questionPrefab,
            TileKind.Pipe => pipePrefab,
            TileKind.Stair => stairPrefab,
            TileKind.HiddenOneUpBlock => hiddenOneUpBlockPrefab,
            TileKind.Flag => flagPrefab,
            TileKind.Castle => castlePrefab,
            TileKind.Coin => coinPrefab,
            _ => null
        };

        private static void ConfigureTile(GameObject tile, GridCell cell)
        {
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = tile.AddComponent<SpriteRenderer>();
            if (renderer.sprite == null) renderer.sprite = GetFallbackSprite();
            renderer.color = TileColors.TryGetValue(cell.Kind, out Color color)
                ? color : Color.magenta;

            if (cell.Kind == TileKind.Coin)
            {
                tile.transform.localScale *= 0.45f;
                renderer.sortingOrder = 2;
            }
            else if (!cell.IsSolid) renderer.sortingOrder = -1;

            Collider2D collider = tile.GetComponent<Collider2D>();
            if (cell.IsSolid)
            {
                if (collider == null) collider = tile.AddComponent<BoxCollider2D>();
                collider.enabled = true;
            }
            else if (collider != null) collider.enabled = false;
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null) return fallbackSprite;
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Map Tile Fallback Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f), 1f);
            fallbackSprite.name = "Map Tile Fallback Sprite";
            return fallbackSprite;
        }
    }
}
