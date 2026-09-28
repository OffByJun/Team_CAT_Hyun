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

        private MapTileFactory _tileFactory;

        private void Awake()
            => _tileFactory = GetComponent<MapTileFactory>();

        private void Start()
        {
            if (buildOnStart) BuildMap();
        }

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

                GameObject prefab = _tileFactory.GetPrefab(cell.Kind);
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
                MapTileFactory.ConfigureTile(tile, cell);
            }

            Debug.Log($"맵 배치 완료: {map.Id} v{map.Version}, {map.Cells.Count}개 셀", this);
        }

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
    }
}
