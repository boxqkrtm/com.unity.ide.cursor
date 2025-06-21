# 🎯 Unity Cursor 編輯器整合

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**為 Unity 開發者提供完整的 Cursor 編輯器整合體驗**

*自動專案配置 • IntelliSense 支援 • 跨平台相容 • 除錯整合*

[English](README.md) | [繁體中文](README.zh-TW.md) | [简体中文](README.zh-CN.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

---

## ✨ 功能特色

🚀 **一鍵整合** - 自動發現並整合 Cursor 編輯器到 Unity 工作流程  
📁 **智慧專案生成** - 自動生成 `.csproj` 文件，完整支援 IntelliSense  
🔍 **多版本支援** - 支援 Cursor 正式版和 Insider 版本  
🌐 **跨平台相容** - Windows、macOS、Linux 全平台支援  
⚙️ **自動配置** - 自動創建 VSCode 配置文件和推薦擴充套件  
🎯 **靈活控制** - 細緻控制不同類型套件的專案生成  

## 📋 系統需求

- **Unity**: 2019.4.25f1 或更新版本
- **Cursor**: 任何版本（自動檢測）
- **平台**: Windows、macOS、Linux

## 📦 安裝方式

### 方法一：透過 Unity Package Manager（推薦）

1. 開啟 Unity 編輯器
2. 選擇 `Window` → `Package Manager`
3. 點擊左上角的 **`+`** 按鈕
4. 選擇 `Add package from git URL...`
5. 輸入以下 URL：
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. 點擊 `Add` 按鈕
7. 等待安裝完成 ✅

### 方法二：手動下載

1. 下載最新版本的 [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases)
2. 解壓縮到您的專案 `Packages` 資料夾中

## 🛠️ 使用說明

### 基本設定

1. **設定 Cursor 為預設編輯器**：
   - `Edit` → `Preferences` → `External Tools`
   - 在 `External Script Editor` 中選擇 Cursor

2. **配置專案生成選項**：
   - 在 External Tools 設定中，您可以選擇為以下項目生成專案文件：
     - ✅ 內嵌套件 (Embedded packages)
     - ✅ 本地套件 (Local packages)  
     - ✅ 註冊表套件 (Registry packages)
     - ✅ Git 套件 (Git packages)
     - ✅ 內建套件 (Built-in packages)
     - ✅ 本地壓縮檔 (Local tarball)
     - ✅ 未知來源套件 (Unknown sources)
     - ✅ 播放器專案 (Player projects)

3. **重新生成專案文件**：
   - 點擊 `Regenerate project files` 按鈕立即更新

### 自動功能

套件安裝後會自動：
- 🔍 檢測 Cursor 安裝位置
- 📝 生成 `.csproj` 文件
- ⚙️ 創建 `.vscode/launch.json` 配置
- 🎨 設定 `.vscode/settings.json` 偏好設定
- 📦 推薦相關的 VSCode 擴充套件

## 🎯 支援的套件類型

| 套件類型 | 描述 | 預設啟用 |
|---------|------|---------|
| **Embedded** | 專案內嵌套件 | ✅ |
| **Local** | 本地開發套件 | ✅ |
| **Registry** | Unity Registry 套件 | ✅ |
| **Git** | Git 來源套件 | ✅ |
| **Built-in** | Unity 內建套件 | ❌ |
| **Local Tarball** | 本地壓縮檔套件 | ✅ |
| **Unknown** | 未知來源套件 | ❌ |

## ⚠️ 重要更新通知

> **從舊版本更新的使用者請注意**  
> 從 **v2.0.24** 版本開始，套件名稱已從 `com.unity.ide.cursor` 變更為 `com.boxqkrtm.ide.cursor`，以避免與 Unity 官方套件命名衝突。  
> 
> 如果在更新過程中遇到錯誤，請先移除舊套件再重新安裝新版本以避免衝突。

## 🔧 進階配置

### 自訂 VSCode 設定

套件會自動創建以下配置文件，您也可以手動調整：

<details>
<summary>點擊查看 .vscode/settings.json 範例</summary>

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

## 🤝 貢獻指南

我們歡迎社群貢獻！請查看 [CONTRIBUTING.md](CONTRIBUTING.md) 了解詳細資訊。

### 報告問題

如果您遇到任何問題，請：
1. 檢查 [現有 Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
2. 創建新的 Issue 並提供詳細資訊
3. 包含您的 Unity 版本、作業系統和 Cursor 版本

### 功能建議

有好的想法？歡迎在 [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues) 中提出功能建議！

## 📄 授權

本專案採用 [MIT License](LICENSE.md) 授權。

## 📚 更多資源

- 📖 [詳細文件](Documentation~/README.md)
- 🔄 [更新日誌](CHANGELOG.md)
- 🐛 [問題回報](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [討論區](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**享受使用 Cursor 進行 Unity 開發的樂趣！** 🎮✨

如果這個專案對您有幫助，請給我們一個 ⭐️

</div> 