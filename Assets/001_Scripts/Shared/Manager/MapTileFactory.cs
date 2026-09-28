using System.Collections.Generic;
using _001_Scripts.Map.Tiles;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Map
{
    public sealed class MapTileFactory : MonoBehaviour
    {
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
        [SerializeField] private GameObject usedBlockPrefab;

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
            { TileKind.Coin, new Color32(255, 215, 35, 255) },
            { TileKind.UsedBlock, new Color32(145, 105, 65, 255) }
        };

        private readonly Dictionary<TileKind, GameObject> legacyPrefabs = new();

        // 기존 씬의 MapManager 필드에 저장된 참조를 런타임에 이전합니다.
        // Factory Inspector에서 직접 지정한 프리팹이 항상 우선합니다.
        public void SetLegacyPrefab(TileKind kind, GameObject prefab)
        {
            if (prefab != null) legacyPrefabs[kind] = prefab;
        }

        public GameObject GetPrefab(TileKind kind)
        {
            GameObject assigned = GetAssignedPrefab(kind);
            if (assigned != null) return assigned;
            return legacyPrefabs.TryGetValue(kind, out GameObject legacy) ? legacy : null;
        }

        public GameObject CreateTile(GridMap map, GridCell cell, Transform root, bool showHiddenBlocks)
        {
            GameObject prefab = GetPrefab(cell.Kind);
            if (prefab == null && cell.Kind != TileKind.UsedBlock &&
                cell.Kind != TileKind.HiddenOneUpBlock)
            {
                Debug.LogWarning($"{cell.Kind} 프리팹이 지정되지 않았습니다.", this);
                return null;
            }

            PlayerPosition center = map.GetCellCenter(cell.X, cell.Y);
            Vector3 position = new Vector3(center.X, center.Y, center.Z);
            GameObject tile;
            if (prefab != null)
                tile = Instantiate(prefab, position, Quaternion.identity, root);
            else
            {
                tile = new GameObject(cell.Kind.ToString());
                tile.transform.SetParent(root, false);
                tile.transform.position = position;
                GameObject ground = GetPrefab(TileKind.Ground);
                int groundLayer = LayerMask.NameToLayer("Ground");
                tile.layer = ground != null ? ground.layer : (groundLayer >= 0 ? groundLayer : 0);
            }

            tile.name = $"{cell.Kind}_{cell.X}_{cell.Y}";
            tile.transform.localScale = Vector3.one * map.CellSize;
            ConfigureTile(tile, cell);
            if (cell.Kind == TileKind.HiddenOneUpBlock)
                tile.GetComponent<SpriteRenderer>().enabled = showHiddenBlocks;
            return tile;
        }
        private GameObject GetAssignedPrefab(TileKind kind) => kind switch
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
            TileKind.UsedBlock => usedBlockPrefab,
            _ => null
        };

        public static void ConfigureTile(GameObject tile, GridCell cell)
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
                collider.isTrigger = false;
                collider.enabled = true;
            }
            else if (collider != null) collider.enabled = false;

            if (cell.Kind == TileKind.Coin || cell.Kind == TileKind.HiddenOneUpBlock)
            {
                // 수집/탐지 트리거가 Ground 레이어의 접지 검사에 잡히지 않게 합니다.
                tile.layer = 0;
                if (collider == null) collider = tile.AddComponent<BoxCollider2D>();
                collider.enabled = true;
                collider.isTrigger = true;
            }

            if (cell.Kind == TileKind.Flag)
            {
                BoxCollider2D goalTrigger = tile.GetComponent<BoxCollider2D>();
                if (goalTrigger == null) goalTrigger = tile.AddComponent<BoxCollider2D>();
                goalTrigger.enabled = true;
                goalTrigger.isTrigger = true;
                if (tile.GetComponent<GoalPoint>() == null) tile.AddComponent<GoalPoint>();
            }
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
