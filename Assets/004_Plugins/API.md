# 위치 동기화·맵 DLL v1.4.0

DLL과 XML을 Unity `Assets/Plugins`에 넣는다. 네임스페이스는 `School.PositionSync`다. 학생 기준 버전은 **Unity 6000.3.23f1**이다. DLL 대상은 .NET Standard 2.1 데스크톱용이며 WebGL은 지원하지 않는다. Player Settings의 API Compatibility Level은 .NET Standard 2.1을 사용한다.

## 기본 사용은 세 가지

```csharp
using School.PositionSync;

// 시작 시 한 번. 온라인 교사 서버에 자동 접속
Server server = new Server();
// 로컬 테스트만 new Server("127.0.0.1");

// 매 프레임 내 캐릭터 이동 후
server.SetPos(new Info(myPosition.x, myPosition.y, 0));

// 매 프레임 최신 상대 위치
Info[] players = server.GetPos();
```

`Server`는 학생이 서버와 소통하는 객체다. 이 객체를 만든다고 중계 서버 프로그램이 실행되는 것은 아니다. 교사가 온라인 서버를 실행한다. 기본 주소는 sdh.asuscomm.com, 포트는 7777이며 생성자에서 변경할 수 있다. 자동 서버 탐색은 없다.

| 호출 | 동작 |
|---|---|
| `new Server()` 또는 `new Server(host, port)` | 자동 비동기 접속 시작. await 불필요 |
| `server.SetPos(new Info(x, y, z))` | 내 최신 위치 저장. 50ms마다 자동 전송. z 생략 시 0 |
| `Info[] players = server.GetPos()` | 나를 제외한 상대 위치의 최신 배열 |

Info의 읽기 전용 속성은 `Id`, `X`, `Y`, `Z`다. 전송할 때 ID를 지정하지 않으며 서버가 연결별로 부여한다. 재접속 시 ID가 바뀐다. 배열의 순서 대신 ID로 상대 캐릭터를 구분한다. 생성 직후 접속 중이거나 상대가 없으면 빈 배열이다. 빈 배열만으로 접속 완료를 판단하지 않는다.

접속 실패·연결 단절은 이후 SetPos/GetPos에서 InvalidOperationException으로 알려준다. Unity 코드에서 try/catch로 오류를 표시하고, 해당 연결을 Dispose한 뒤 반복 호출을 중단한다. 재접속은 새 Server 객체를 만든다. 잘못된 주소 인수·포트·NaN 좌표도 예외다.

게임 종료·장면 전환 시 `OnDestroy`에서 `server?.Dispose()`를 한 번 호출한다. Dispose는 종료 정리용이며 여러 번 호출해도 안전하다. 서버 객체는 프레임마다 만들지 않는다.

## 학생이 작성할 부분

1. 자신의 이동·점프·충돌을 구현한 뒤 결과 좌표를 SetPos에 전달한다.
2. GetPos 결과에 처음 나타난 Id는 상대 프리팹을 생성한다.
3. 기존 Id는 해당 캐릭터 위치를 X/Y/Z로 바꾼다.
4. 이전 목록에는 있고 새 목록에는 없는 Id는 캐릭터를 제거한다.
5. 상대 프리팹에는 로컬 입력·이동·중력을 실행하지 않는다.

Unity Update/LateUpdate에서 호출하면 된다. 접속·전송 루프·스레드 관리는 DLL이 담당한다. 배열과 좌표는 데이터이므로 GameObject 생성·제거·위치 적용은 학생이 작성한다. 각 팀에서 연결 소유자는 한 명으로 정한다.

동기화는 ID와 위치뿐이며 회전·속도·점프 상태는 전송하지 않는다. 예측·보정·보간도 없다. X=오른쪽, Y=위, Z=0 기준으로 팀 간 좌표와 이동 규칙을 맞춘다.

기존 SyncClient API는 호환성을 위해 남겨 두지만 이번 학생 과제는 Server API로 진행한다.

## v1.3: 원작 World 1-1 맵 그리드

```csharp
GridMap map = MapCatalog.Default;             // 지상
GridMap bonus = MapCatalog.World11Underground; // 지하 보너스 방
foreach (GridCell cell in map.Cells)
{
    PlayerPosition p = map.GetCellCenter(cell.X, cell.Y);
    // Kind에 맞는 오브젝트 생성, p 위치에 배치, IsSolid에 따라 충돌 설정
}
```

원작 NES 맵 이미지의 16×16픽셀 타일을 1×1 월드 단위로 변환했다. 지상은 이미지 전체 폭을 포함한 **224×15**, 지하 방은 **17×15**다. 원점은 왼쪽 아래이며 Y가 위로 증가한다. 맵 ID는 smb-1-1-overworld / smb-1-1-underground, 배치 버전은 2다.

| 데이터 | 의미 |
|---|---|
| Width / Height / CellSize | 맵 크기, 한 칸 = 1 |
| Cells | 빈 칸을 제외한 읽기 전용 셀 목록. 지상 633개, 지하 118개 |
| GridCell.Id / X / Y / Kind | 고유 셀 ID, 열·행, 종류 |
| GridCell.IsSolid | 일반 충돌 영역 여부. 장식·코인·숨겨진 블록은 false |
| GridCell.Content | 코인·버섯/꽃·다중 코인·스타·1UP 내용물 데이터 |
| GetTile(x,y) | 셀 종류. 맵 밖은 예외 |
| GetCellCenter(x,y) | 셀 중앙의 월드 XYZ. Z=0 |
| Spawn / SpawnFeet | 시작 셀과 발 기준 월드 좌표. 지상 발 위치 (2.5,2,0) |
| Goal | 지상 깃대 셀 (198,3), 지하는 출구 파이프 셀 (13,2) |
| FallBoundaryY | 수업용 추락 판정 권장값 -4. 원작 물리 데이터가 아님 |

Kind는 Empty, Ground, Brick, Question, Pipe, Stair, HiddenOneUpBlock, Flag, Castle, Coin이다. 파이프·계단·성은 셀별로 제공한다. 숨겨진 1UP 블록은 처음에 그리지 않고, 아래에서 치는 판정·출현은 별도로 구현한다. 지하 방 코인은 19개다. 성과 깃발은 장식이며 벽처럼 충돌시키지 않는다.

지상 진입 파이프 상단 셀은 (57,5), 복귀 파이프 상단의 발 높이는 y=4, x=163~165 범위다. 방 전환 기능은 DLL이 수행하지 않는다. 위치 동기화에는 맵 ID가 없으므로 함께 방을 이동하는 게임은 방 구분 규격을 추가로 합의해야 한다.

원본의 **지형·블록·파이프·계단·깃대·성 및 지하 코인 배치 데이터**다. 구름·언덕 등 배경 그래픽, 적의 동작, 아이템 생성·수집, 원작 물리·체크포인트 동작은 DLL 기능이 아니다. 원본 이미지를 그대로 그리는 텍스처도 제공하지 않는다.

출처: [NESMaps World 1-1 원작 맵](https://nesmaps.com/maps/SuperMarioBrothers/SuperMarioBrosMap1-1.png), 대조: [MarioWiki 원작 맵 자료](https://www.mariowiki.com/File:SMB_NES_World_1-1_Map.png). 좌표 추출 절차와 이미지 해시는 교사용 map-source 폴더에 기록했다. 네 팀은 같은 맵 버전을 사용해야 하며 서버는 맵 버전을 검사하지 않는다.
