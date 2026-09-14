using System.Collections.Generic;
using School.PositionSync;
using UnityEngine;

namespace TeamCat.Map
{
    /// <summary>
    /// School.PositionSync.dll의 World 1-1 데이터를 Unity 오브젝트로 배치합니다.
    /// 이 컴포넌트를 장면에 하나만 배치하세요.
    /// </summary>
    public sealed class MapBuilder : MonoBehaviour
    {
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool useUndergroundMap;
        [SerializeField] private bool showHiddenBlocks;
        [SerializeField] private Transform generatedMapRoot;

        private static Sprite fallbackSprite;

        private static readonly Dictionary<TileKind, Color> TileColors =
            new Dictionary<TileKind, Color>
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
            if (buildOnStart)
            {
                BuildMap();
            }
        }

        [ContextMenu("Build Map")]
        public void BuildMap()
        {
            ClearMap();

            GridMap map = useUndergroundMap
                ? MapCatalog.World11Underground
                : MapCatalog.Default;

            Transform root = GetOrCreateRoot();

            foreach (GridCell cell in map.Cells)
            {
                if (cell.Kind == TileKind.Empty ||
                    (cell.Kind == TileKind.HiddenOneUpBlock && !showHiddenBlocks))
                {
                    continue;
                }

                GameObject prefab = Resources.Load<GameObject>(
                    "MapPrefabs/" + cell.Kind);

                if (prefab == null)
                {
                    Debug.LogWarning($"{cell.Kind} 프리팹을 찾지 못했습니다.", this);
                    continue;
                }

                PlayerPosition center = map.GetCellCenter(cell.X, cell.Y);
                GameObject tile = Instantiate(
                    prefab,
                    new Vector3(center.X, center.Y, center.Z),
                    Quaternion.identity,
                    root);

                tile.name = $"{cell.Kind}_{cell.X}_{cell.Y}";
                tile.transform.localScale = Vector3.one * map.CellSize;
                ConfigureTile(tile, cell);
            }

            Debug.Log(
                $"맵 배치 완료: {map.Id} v{map.Version}, {map.Cells.Count}개 셀",
                this);
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (generatedMapRoot == null)
            {
                Transform existing = transform.Find("Generated Map");
                if (existing == null)
                {
                    return;
                }

                generatedMapRoot = existing;
            }

            for (int i = generatedMapRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = generatedMapRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        public Vector3 GetSpawnFeetPosition()
        {
            GridMap map = useUndergroundMap
                ? MapCatalog.World11Underground
                : MapCatalog.Default;
            PlayerPosition spawn = map.SpawnFeet;
            return new Vector3(spawn.X, spawn.Y, spawn.Z);
        }

        public float GetFallBoundaryY()
        {
            return (useUndergroundMap
                ? MapCatalog.World11Underground
                : MapCatalog.Default).FallBoundaryY;
        }

        private Transform GetOrCreateRoot()
        {
            if (generatedMapRoot != null)
            {
                return generatedMapRoot;
            }

            Transform existing = transform.Find("Generated Map");
            if (existing != null)
            {
                generatedMapRoot = existing;
                return generatedMapRoot;
            }

            GameObject rootObject = new GameObject("Generated Map");
            generatedMapRoot = rootObject.transform;
            generatedMapRoot.SetParent(transform, false);
            return generatedMapRoot;
        }

        private static void ConfigureTile(GameObject tile, GridCell cell)
        {
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = tile.AddComponent<SpriteRenderer>();
            }

            // 프리팹에 실제 스프라이트가 지정되면 그것을 유지합니다.
            if (renderer.sprite == null)
            {
                renderer.sprite = GetFallbackSprite();
            }

            Color color;
            renderer.color = TileColors.TryGetValue(cell.Kind, out color)
                ? color
                : Color.magenta;

            if (cell.Kind == TileKind.Coin)
            {
                tile.transform.localScale *= 0.45f;
                renderer.sortingOrder = 2;
            }
            else if (!cell.IsSolid)
            {
                renderer.sortingOrder = -1;
            }

            Collider2D collider = tile.GetComponent<Collider2D>();
            if (cell.IsSolid)
            {
                if (collider == null)
                {
                    collider = tile.AddComponent<BoxCollider2D>();
                }

                collider.enabled = true;
            }
            else if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "Map Tile Fallback Texture";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            fallbackSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            fallbackSprite.name = "Map Tile Fallback Sprite";
            return fallbackSprite;
        }
    }
}
