# SchoolSync DLL 맵 배치

`Assets/002_Resources/Prefabs/Map/MapBuilder.prefab`을 사용할 장면의 Hierarchy에 배치합니다. Play를 누르면 `MapCatalog.Default`의 지상 맵이 자동으로 생성됩니다.

실제 Sprite가 없는 기본 프리팹은 타일 종류별 색상 사각형으로 표시됩니다. 아트를 적용할 때는 같은 폴더의 타일 프리팹에 `SpriteRenderer`를 추가하고 Sprite를 지정합니다.

- **Build On Start**: 시작 시 자동 배치
- **Use Underground Map**: 지하 보너스 방 사용
- **Show Hidden Blocks**: 숨겨진 1UP 블록을 테스트용으로 표시

고체 셀에는 `BoxCollider2D`가 자동 추가됩니다. 깃발, 성, 코인은 충돌하지 않습니다.
