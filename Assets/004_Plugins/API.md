# 위치 동기화·맵 DLL v1.7.0

DLL과 XML을 Unity `Assets/Plugins`에 넣는다. 네임스페이스는 `School.PositionSync`다. 학생 기준 버전은 **Unity 6000.3.23f1**이다. DLL 대상은 .NET Standard 2.1 데스크톱용이며 WebGL은 지원하지 않는다. Player Settings의 API Compatibility Level은 .NET Standard 2.1을 사용한다.

## 사용법: `new Server(this)`

서버 소식은 **`IServerHandler`의 함수로 받고**, 내 쪽에서는 **`SetPos`·`HitBlock`·`KillMonster` 세 가지만 보낸다.** 연결 담당 MonoBehaviour 하나가 인터페이스를 구현한다.

```csharp
using School.PositionSync;
using UnityEngine;

public class NetworkManager : MonoBehaviour, IServerHandler
{
    Server server;

    void Awake()
    {
        server = new Server(this);                         // 온라인 교사 서버에 자동 접속
        // server = new Server("127.0.0.1", 7777, this);   // 로컬 테스트
    }

    void Update()
    {
        // 내 이동 계산이 끝난 뒤 매 프레임
        server.SetPos(new Info(myPosition.x, myPosition.y));
    }

    void OnDestroy() => server?.Dispose();

    public void OnConnected(MoveRules rules) { /* 스피드·중력·점프력 저장, 이동 시작 */ }
    public void OnMapUpdated(GridMap map) { /* 현재 맵 전체로 블록 다시 배치 */ }
    public void OnPlayersUpdated(Info[] players) { /* 상대 캐릭터 생성·이동·제거 */ }
    public void OnMonstersUpdated(Monster[] monsters) { /* 몬스터 생성·이동·제거 */ }
    public void OnWorldReset() { /* 내 캐릭터를 시작 위치로 */ }
    public void OnDisconnected(string reason) { /* 오류 표시 */ }
}
```

`Server`는 학생 프로그램이 서버와 소통하는 객체다. 만든다고 서버 프로그램이 실행되지는 않는다. 기본 주소는 sdh.asuscomm.com, 포트는 7777이다. 서버 객체는 한 번만 만들고 프레임마다 만들지 않는다. 팀에서 연결 담당은 한 명이다.

### 받는 함수 (IServerHandler)

모두 `Server`를 만든 **Unity 메인 스레드**에서 호출되므로 GameObject를 바로 다뤄도 된다. 쓰지 않는 함수도 빈 몸통으로 구현한다.

| 함수 | 언제 | 할 일 |
|---|---|---|
| `OnConnected(MoveRules rules)` | 접속이 끝났을 때 한 번 | 스피드·중력·점프력 저장. 받기 전에는 움직이지 않는다 |
| `OnMapUpdated(GridMap map)` | 접속 직후 한 번 + 맵이 바뀔 때마다 | **현재 맵 전체**가 온다. 이 맵으로 블록을 다시 배치한다 |
| `OnPlayersUpdated(Info[] players)` | 최신 위치가 올 때마다(프레임당 최대 한 번) | 나를 제외한 상대. 처음 보는 Id 생성, 기존 Id 위치 갱신, 사라진 Id 제거 |
| `OnMonstersUpdated(Monster[] monsters)` | 위와 같음 | 살아 있는 몬스터. 처리 방법은 플레이어와 같다 |
| `OnWorldReset()` | 교사가 서버를 리셋했을 때 | 맵·몬스터는 이미 처음 상태로 왔다. 내 캐릭터만 시작 위치로 |
| `OnDisconnected(string reason)` | 접속 실패·연결 끊김 때 한 번 | 오류 표시. 다시 접속하려면 새 Server를 만든다 |

### 보내는 함수

| 호출 | 동작 |
|---|---|
| `server.SetPos(new Info(x, y, z))` | 내 최신 위치 저장. 약 16ms마다 자동 전송. z 생략 시 0 |
| `server.HitBlock(x, y)` | 내 캐릭터 머리가 (x, y) 칸을 아래에서 쳤다고 알림. 벽돌 → Empty, 물음표·숨은 블록 → UsedBlock. 그 위에 서 있던 몬스터도 처치. 결과는 모두의 `OnMapUpdated`로 온다 |
| `server.KillMonster(id)` | 내가 몬스터를 밟았다고 알림. 이미 없는 Id는 무시 |
| `server.MyId` | 내 플레이어 ID. 접속 전에는 0 |
| `server.Dispose()` | `OnDestroy`에서 한 번. 이후 핸들러는 호출되지 않는다 |

- 연결이 끊긴 뒤의 보내기는 예외 없이 무시된다. 끊긴 것은 `OnDisconnected`로 안다. 좌표에 NaN을 넣으면 예외다.
- `Info`의 읽기 전용 속성은 `Id`, `X`, `Y`, `Z`다. Id는 서버가 연결마다 부여하며 재접속하면 바뀐다. 배열 순서가 아니라 Id로 캐릭터를 구분한다.
- `Monster`는 `Id`, `Kind`(Goomba·Koopa), `X`, `Y`다. 위치는 **발 기준**(아래쪽 중앙)이다.

## 서버가 정하는 이동 규칙

`OnConnected`로 받은 값을 그대로 쓰고 코드·인스펙터에 숫자를 따로 적지 않는다. 이동 계산은 학생이 한다.

| 값 | 단위 | 쓰는 법 |
|---|---|---|
| `rules.Speed` | 칸/초 | 좌우 속도 = 입력(-1~1) × Speed |
| `rules.Gravity` | 칸/초², 양수 | 매 초 세로 속도에서 Gravity만큼 뺀다 |
| `rules.JumpPower` | 칸/초 | 점프하는 순간 세로 속도를 JumpPower로 만든다 |
| `rules.JumpHeight` | 칸 | 이론상 최고 점프 높이. 팀 간 비교용 |

```csharp
// 2D (Rigidbody2D)
rb.gravityScale = rules.Gravity / Mathf.Abs(Physics2D.gravity.y);
rb.linearVelocityX = input * rules.Speed;
if (jumpPressed && grounded) rb.linearVelocityY = rules.JumpPower;

// 3D (Rigidbody). useGravity를 끄고 FixedUpdate에서 직접 중력을 더한다.
rb.useGravity = false;
rb.linearVelocity += Vector3.down * rules.Gravity * Time.fixedDeltaTime;
```

- 점프는 `AddForce`가 아니라 세로 속도를 직접 넣는다. 힘으로 넣으면 질량·ForceMode에 따라 높이가 달라진다.
- Linear Damping(Drag)은 0. 달리기, 누르는 시간에 따라 달라지는 점프, 점프 중 중력 배율 변경은 전체 팀 합의 전에는 쓰지 않는다.
- 판정 크기(콜라이더)는 서버가 보내지 않는다. 팀 간 합의한 값을 쓴다.
- 서버 없이 로컬 테스트할 때만 `new MoveRules(speed, gravity, jumpPower)`로 임시 값을 만든다.

## 공유 맵·몬스터

- 맵은 매번 **전체**가 온다. 바뀐 칸을 따로 추적할 필요 없이 받은 맵대로 다시 배치하면 된다. 늦게 접속해도 같은 맵이다. `MapCatalog.Default`는 원래 맵 그대로 남는다.
- 몬스터는 서버가 항상 움직인다. 초당 2칸으로 걷다가 벽이나 발밑이 빈 칸을 만나면 돌아선다. 떨어지지 않는다. 받은 위치를 그대로 표시하고 몬스터에게 중력·이동을 다시 적용하지 않는다.
- 플레이어와 몬스터의 충돌(밟기, 피해)과 블록을 쳤는지는 각자 판정하고 결과만 보낸다. 동시에 밟으면 서버에 먼저 도착한 요청만 반영된다.
- 블록 내용물(코인·버섯)은 서버가 만들지 않는다. `HitBlock`을 보낸 쪽이 자기 화면에서 처리한다.
- 몬스터끼리의 충돌과 엉금엉금 등껍질 동작은 없다. 리셋되면 몬스터는 새 Id로 다시 생긴다.

동기화는 위치뿐이며 회전·속도·점프 상태는 보내지 않는다. 예측·보정·보간도 없다. X=오른쪽, Y=위, Z=0 기준으로 좌표를 맞춘다.

## 맵 데이터 (원작 World 1-1)

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

Kind는 Empty, Ground, Brick, Question, Pipe, Stair, HiddenOneUpBlock, Flag, Castle, Coin, UsedBlock이다. UsedBlock은 기본 맵에 없고 OnMapUpdated로 받은 맵에만 나타나며 충돌한다. 파이프·계단·성은 셀별로 제공한다. 숨겨진 1UP 블록은 처음에 그리지 않고, 아래에서 치는 판정·출현은 별도로 구현한다. 지하 방 코인은 19개다. 성과 깃발은 장식이며 벽처럼 충돌시키지 않는다.

지상 진입 파이프 상단 셀은 (57,5), 복귀 파이프 상단의 발 높이는 y=4, x=163~165 범위다. 방 전환 기능은 DLL이 수행하지 않는다. 위치 동기화에는 맵 ID가 없으므로 함께 방을 이동하는 게임은 방 구분 규격을 추가로 합의해야 한다.

원본의 **지형·블록·파이프·계단·깃대·성 및 지하 코인 배치 데이터**다. 구름·언덕 등 배경 그래픽, 적의 동작, 아이템 생성·수집, 원작 물리·체크포인트 동작은 DLL 기능이 아니다. 원본 이미지를 그대로 그리는 텍스처도 제공하지 않는다.

출처: [NESMaps World 1-1 원작 맵](https://nesmaps.com/maps/SuperMarioBrothers/SuperMarioBrosMap1-1.png), 대조: [MarioWiki 원작 맵 자료](https://www.mariowiki.com/File:SMB_NES_World_1-1_Map.png). 좌표 추출 절차와 이미지 해시는 교사용 map-source 폴더에 기록했다. 네 팀은 같은 맵 버전을 사용해야 하며 서버는 맵 버전을 검사하지 않는다.
