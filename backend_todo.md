## 구글 로그인 관련 해야되는 것

Step 1: 유니티에서 정보 추출하기 (완료)
안드로이드 ID를 만들려면 두 가지 핵심 정보가 필요합니다.
패키지 이름 (Package Name): * Project Settings > Player > Android (탭) > Other Settings > Identification > Package Name에서 확인 가능합니다. (예: com.YourTeam.MyMiniHero)
SHA-1 인증서 지문 (SHA-1 Certificate Fingerprint):
방법: Project Settings > Player > Publishing Settings로 이동합니다.
Keystore Manager를 통해 키스토어를 생성하거나 선택한 상태여야 합니다.
에디터에서 바로 확인이 어렵다면, 안티그라비티에게 **"내 Keystore 파일의 SHA-1 지문을 추출하는 명령어를 알려줘"**라고 요청하거나, 유니티 에디터 하단의 Build 시 로그에 찍히는 지문을 복사해야 합니다.

🛠️ Step 2: 구글 클라우드 콘솔 설정 (완료)
이제 준비물을 가지고 구글 콘솔로 갑니다.
Google Cloud Console 접속 > API 및 서비스 > 사용자 인증 정보로 이동합니다.
**[사용자 인증 정보 만들기] > [OAuth 클라이언트 ID]**를 클릭합니다.
애플리케이션 유형: **"Android"**를 선택합니다.
세부 정보 입력:
이름: Android Client (Debug) 혹은 Android Client (Release) 등 구분하기 쉽게 짓습니다.
패키지 이름: 유니티에서 복사한 com.xxx.xxx를 입력합니다.
SHA-1 인증서 지문: 유니티에서 추출한 40자리 영문/숫자 조합을 입력합니다.
**[만들기]**를 누릅니다.
[!NOTE] 중요: 안드로이드용 ID는 웹 애플리케이션 ID와 달리 '클라이언트 보안 비밀번호(Secret)'가 발행되지 않습니다. 지문(SHA-1) 자체가 보안 역할을 하기 때문입니다. 정상인이니 당황하지 마세요!
 - **디버그 여러 명이서 할 경우 각각 모두 구글 클라우드에 등록해주기**


------------------ 중지 ---------------------

🛠️ Step 3: 구글 플레이 콘솔에 연결
만든 ID를 구글 플레이 게임즈 서비스와 연결해야 앱에서 인식을 합니다.
Google Play Console 접속 > 게임 선택.
설정 > 구글 플레이 게임즈 서비스 > 설정 및 관리 > 구성으로 이동합니다.
사용자 인증 정보 섹션에서 **[사용자 인증 정보 추가]**를 누릅니다.
유형을 '게임 앱'으로 선택하고, 방금 구글 클라우드 콘솔에서 만든 Android 클라이언트 ID를 연결합니다.

step 4 : 안티그라비티
Antigravity(Vibe Coding)를 활용한 코드 구현
이제 백엔드 담당자인 당신의 핵심 업무인 **'인증 로직'**을 짤 차례입니다. 안티그라비티에게 아래와 같이 요청하여 완성도 높은 코드를 뽑아내세요.

📝 Antigravity 요청 프롬프트
"구글 플레이 게임즈(GPG) 로그인을 수행하고, 성공 시 얻은 ServerAuthCode를 사용하여 UGS Authentication에 로그인하는 BackendManager 스크립트를 작성해줘.
UnityServices.InitializeAsync()를 포함할 것.
로그인 성공 시 AuthenticationService.Instance.PlayerId를 출력할 것.
싱글톤(Singleton) 패턴을 적용해서 인게임/아웃게임 담당자가 어디서든 접근할 수 있게 해줘."
📂 구현될 핵심 로직 구현
안티그라비티가 생성해 줄 코드의 핵심 흐름은 다음과 같습니다:
GPG 로그인: 유저의 구글 계정 인증
Token 획득: 구글로부터 일회용 인증 코드(AuthCode)를 받음.
UGS 연동: SignInWithGooglePlayGamesAsync(authCode) 함수를 호출하여 유니티 서버에 유저를 등록.

step 5 : UI 만들기
로그인 UI 만들기, 프로필 만들기, 계정 전환 만들기