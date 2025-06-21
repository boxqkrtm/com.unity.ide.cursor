# 🎯 Unity Cursor 에디터 통합

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**Unity 개발자를 위한 완전한 Cursor 에디터 통합 경험**

*자동 프로젝트 구성 • IntelliSense 지원 • 크로스 플랫폼 호환 • 디버그 통합*

[English](README.md) | [繁體中文](README.zh-TW.md) | [简体中文](README.zh-CN.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

---

## ✨ 기능

🚀 **원클릭 통합** - Cursor 에디터를 Unity 워크플로우에 자동으로 발견하고 통합  
📁 **스마트 프로젝트 생성** - `.csproj` 파일을 자동 생성하여 IntelliSense를 완전 지원  
🔍 **멀티 버전 지원** - Cursor 정식 버전과 Insider 버전 모두 지원  
🌐 **크로스 플랫폼 호환** - Windows, macOS, Linux 전체 플랫폼 지원  
⚙️ **자동 구성** - VSCode 구성 파일과 권장 확장 기능을 자동 생성  
🎯 **유연한 제어** - 다양한 패키지 유형의 프로젝트 생성을 세밀하게 제어  

## 📋 시스템 요구사항

- **Unity**: 2019.4.25f1 이상
- **Cursor**: 임의 버전 (자동 감지)
- **플랫폼**: Windows, macOS, Linux

## 📦 설치

### 방법 1: Unity Package Manager를 통해 (권장)

1. Unity 에디터 열기
2. `Window` → `Package Manager` 선택
3. 왼쪽 상단의 **`+`** 버튼 클릭
4. `Add package from git URL...` 선택
5. 다음 URL 입력:
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. `Add` 버튼 클릭
7. 설치 완료까지 대기 ✅

### 방법 2: 수동 다운로드

1. 최신 [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases) 다운로드
2. 프로젝트의 `Packages` 폴더에 압축 해제

## 🛠️ 사용법

### 기본 설정

1. **Cursor를 기본 에디터로 설정**:
   - `Edit` → `Preferences` → `External Tools`
   - `External Script Editor`에서 Cursor 선택

2. **프로젝트 생성 옵션 구성**:
   - External Tools 설정에서 다음 항목에 대한 프로젝트 파일 생성을 선택할 수 있습니다:
     - ✅ 임베디드 패키지 (Embedded packages)
     - ✅ 로컬 패키지 (Local packages)  
     - ✅ 레지스트리 패키지 (Registry packages)
     - ✅ Git 패키지 (Git packages)
     - ✅ 내장 패키지 (Built-in packages)
     - ✅ 로컬 아카이브 (Local tarball)
     - ✅ 알 수 없는 소스 패키지 (Unknown sources)
     - ✅ 플레이어 프로젝트 (Player projects)

3. **프로젝트 파일 재생성**:
   - `Regenerate project files` 버튼을 클릭하여 즉시 업데이트

### 자동 기능

패키지 설치 후 자동으로:
- 🔍 Cursor 설치 경로 감지
- 📝 `.csproj` 파일 생성
- ⚙️ `.vscode/launch.json` 구성 생성
- 🎨 `.vscode/settings.json` 환경 설정
- 📦 관련 VSCode 확장 기능 권장

## 🎯 지원되는 패키지 유형

| 패키지 유형 | 설명 | 기본 활성화 |
|-------------|-------------|----------------|
| **Embedded** | 프로젝트 임베디드 패키지 | ✅ |
| **Local** | 로컬 개발 패키지 | ✅ |
| **Registry** | Unity Registry 패키지 | ✅ |
| **Git** | Git 소스 패키지 | ✅ |
| **Built-in** | Unity 내장 패키지 | ❌ |
| **Local Tarball** | 로컬 아카이브 패키지 | ✅ |
| **Unknown** | 알 수 없는 소스 패키지 | ❌ |

## ⚠️ 중요한 업데이트 공지

> **이전 버전에서 업데이트하는 사용자를 위한 공지**  
> **v2.0.24** 버전부터 패키지 이름이 `com.unity.ide.cursor`에서 `com.boxqkrtm.ide.cursor`로 변경되었습니다. 이는 Unity 관련 잠재적 문제를 방지하기 위함입니다.  
> 
> 업데이트 중 오류가 발생하면 충돌을 피하기 위해 기존 패키지를 제거한 후 새 버전을 재설치하십시오.

## 🔧 고급 구성

### 사용자 정의 VSCode 설정

패키지는 다음 구성 파일을 자동으로 생성하며, 수동으로 조정할 수도 있습니다:

<details>
<summary>.vscode/settings.json 예제 보기</summary>

```json
{
    "files.exclude": {
        "**/.DS_Store": true,
        "**/.git": true,
        "**/.gitignore": true,
        "**/.gitmodules": true,
        "**/*.booproj": true,
        "**/*.pidb": true,
        "**/*.suo": true,
        "**/*.user": true,
        "**/*.userprefs": true,
        "**/*.unityproj": true,
        "**/*.dll": true,
        "**/*.exe": true,
        "**/*.pdf": true,
        "**/*.mid": true,
        "**/*.midi": true,
        "**/*.wav": true,
        "**/*.gif": true,
        "**/*.ico": true,
        "**/*.jpg": true,
        "**/*.jpeg": true,
        "**/*.png": true,
        "**/*.psd": true,
        "**/*.tga": true,
        "**/*.tif": true,
        "**/*.tiff": true,
        "**/*.3ds": true,
        "**/*.3DS": true,
        "**/*.fbx": true,
        "**/*.FBX": true,
        "**/*.lxo": true,
        "**/*.LXO": true,
        "**/*.ma": true,
        "**/*.MA": true,
        "**/*.obj": true,
        "**/*.OBJ": true,
        "**/*.asset": true,
        "**/*.cubemap": true,
        "**/*.flare": true,
        "**/*.mat": true,
        "**/*.meta": true,
        "**/*.prefab": true,
        "**/*.unity": true,
        "build/": true,
        "Build/": true,
        "Library/": true,
        "library/": true,
        "obj/": true,
        "Obj/": true,
        "ProjectSettings/": true,
        "temp/": true,
        "Temp/": true
    }
}
```

</details>

## 🤝 기여하기

커뮤니티 기여를 환영합니다! 자세한 내용은 [CONTRIBUTING.md](CONTRIBUTING.md)를 확인하세요.

### 문제 보고

문제가 발생하면:
1. [기존 Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues) 확인
2. 자세한 정보를 포함한 새 Issue 생성
3. Unity 버전, OS, Cursor 버전 포함

### 기능 요청

좋은 아이디어가 있으신가요? [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)에서 기능을 제안해 주세요!

## 📄 라이센스

이 프로젝트는 [MIT License](LICENSE.md) 하에 라이센스됩니다.

## 📚 추가 리소스

- 📖 [자세한 문서](Documentation~/README.md)
- 🔄 [변경 로그](CHANGELOG.md)
- 🐛 [버그 리포트](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [토론](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**Cursor로 Unity 개발을 즐기세요!** 🎮✨

이 프로젝트가 도움이 되었다면 ⭐️를 눌러주세요

</div> 