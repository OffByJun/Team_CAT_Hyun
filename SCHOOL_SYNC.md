# 공통 위치 동기화 DLL

Unity 프로젝트: `Cat`. 공통 DLL 위치: `Cat/Assets/Plugins/SchoolSync`.

## 처음 한 번

Unity를 닫고 Git이 설치된 PC에서 `SetupSchoolSync.cmd`를 실행한다. macOS는 `sh SetupSchoolSync.sh`를 실행한다. 이 설정은 각 PC·clone마다 한 번 필요하다. Git이 PATH에 없으면 설치하거나 Git 터미널에서 명령을 실행한다.

설정 후 일반 `git pull`은 팀 저장소가 지정한 DLL 버전을 함께 받는다. Fork에서 업데이트되지 않으면 `UpdateSchoolSync.cmd`를 실행한다. 이 파일은 프로젝트 pull과 서브모듈 초기화·업데이트를 함께 수행하며, 충돌·분기 시 덮어쓰지 않고 중단한다.

## 학생 사용

```csharp
using School.PositionSync;
Server server = new Server("교사 PC IP");
server.SetPos(new Info(x, y, 0));
Info[] others = server.GetPos();
GridMap map = MapCatalog.Default;
```

위치는 직접 계산한다. 상대 ID별 생성·갱신·제거와 맵 생성·충돌은 학생이 구현한다. OnDestroy에서 `server?.Dispose()`를 호출한다. API 상세는 `Cat/Assets/Plugins/SchoolSync/API.md`에 있다. 내장 맵은 원작 World 1-1 이미지에서 추출한 지상·지하 그리드다.

## 버전 약속

서브모듈은 지정된 커밋을 사용한다. DLL 배포 때 교사가 이 저장소의 참조 커밋도 올리면 학생은 pull로 받는다. DLL 저장소의 최신 커밋을 임의로 받는 방식이 아니다. SchoolSync 내부 파일을 직접 수정하거나 다른 DLL 사본을 추가하지 않는다. 같은 서버·공통 이동 규칙·맵 버전을 사용한다.
