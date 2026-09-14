# DLL 맵 배치 사용법

1. `Assets/Prefabs/Map/MapBuilder.prefab`을 사용할 장면의 Hierarchy로 끌어놓습니다.
2. Play를 누르면 `MapCatalog.Default`의 지상 맵이 자동으로 생성됩니다.
3. 생성된 타일은 `MapBuilder/Generated Map` 아래에 정리됩니다.

타일 원본은 `Assets/Resources/MapPrefabs`에 있습니다. 실제 그림을 사용할 때는 각 프리팹에 `SpriteRenderer`를 추가하고 원하는 Sprite를 지정하면 됩니다. Sprite가 지정되지 않은 기본 상태에서는 코드가 종류별 색상 사각형을 표시합니다.

`MapBuilder` Inspector 설정:

- **Build On Start**: 게임 시작 시 자동 배치
- **Use Underground Map**: 지하 보너스 방 배치
- **Show Hidden Blocks**: 숨겨진 1UP 블록을 테스트용으로 표시

고체 셀에는 `BoxCollider2D`가 자동으로 추가되고, 깃발·성·코인 같은 비고체 셀에는 충돌체가 생기지 않습니다.
