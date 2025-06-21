# 🎯 Unity Cursor 编辑器集成

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**为 Unity 开发者提供完整的 Cursor 编辑器集成体验**

*自动项目配置 • IntelliSense 支持 • 跨平台兼容 • 调试集成*

🇺🇸 [English](../README.md) | 🇹🇼 [繁體中文](README.zh-TW.md) | 🇨🇳 [简体中文](README.zh-CN.md) | 🇯🇵 [日本語](README.ja.md) | 🇰🇷 [한국어](README.ko.md) | 🇩🇪 [Deutsch](README.de.md) | 🇫🇷 [Français](README.fr.md) | 🇪🇸 [Español](README.es.md) | 🇷🇺 [Русский](README.ru.md) | 🇵🇹 [Português](README.pt.md) | 🇮🇹 [Italiano](README.it.md)

</div>

---

## ✨ 功能特色

🚀 **一键集成** - 自动发现并集成 Cursor 编辑器到 Unity 工作流程  
📁 **智能项目生成** - 自动生成 `.csproj` 文件，完整支持 IntelliSense  
🔍 **多版本支持** - 支持 Cursor 正式版和 Insider 版本  
🌐 **跨平台兼容** - Windows、macOS、Linux 全平台支持  
⚙️ **自动配置** - 自动创建 VSCode 配置文件和推荐扩展  
🎯 **灵活控制** - 精细控制不同类型包的项目生成  

## 📋 系统需求

- **Unity**: 2019.4.25f1 或更新版本
- **Cursor**: 任何版本（自动检测）
- **平台**: Windows、macOS、Linux

## 📦 安装方式

### 方法一：通过 Unity Package Manager（推荐）

1. 打开 Unity 编辑器
2. 选择 `Window` → `Package Manager`
3. 点击左上角的 **`+`** 按钮
4. 选择 `Add package from git URL...`
5. 输入以下 URL：
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. 点击 `Add` 按钮
7. 等待安装完成 ✅

### 方法二：手动下载

1. 下载最新版本的 [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases)
2. 解压到您的项目 `Packages` 文件夹中

## 🛠️ 使用说明

### 基本设置

1. **设置 Cursor 为默认编辑器**：
   - `Edit` → `Preferences` → `External Tools`
   - 在 `External Script Editor` 中选择 Cursor

2. **配置项目生成选项**：
   - 在 External Tools 设置中，您可以选择为以下项目生成项目文件：
     - ✅ 嵌入包 (Embedded packages)
     - ✅ 本地包 (Local packages)  
     - ✅ 注册表包 (Registry packages)
     - ✅ Git 包 (Git packages)
     - ✅ 内置包 (Built-in packages)
     - ✅ 本地压缩包 (Local tarball)
     - ✅ 未知来源包 (Unknown sources)
     - ✅ 播放器项目 (Player projects)

3. **重新生成项目文件**：
   - 点击 `Regenerate project files` 按钮立即更新

### 自动功能

包安装后会自动：
- 🔍 检测 Cursor 安装位置
- 📝 生成 `.csproj` 文件
- ⚙️ 创建 `.vscode/launch.json` 配置
- 🎨 设置 `.vscode/settings.json` 偏好设置
- 📦 推荐相关的 VSCode 扩展

## 🎯 支持的包类型

| 包类型 | 描述 | 默认启用 |
|---------|------|---------|
| **Embedded** | 项目嵌入包 | ✅ |
| **Local** | 本地开发包 | ✅ |
| **Registry** | Unity Registry 包 | ✅ |
| **Git** | Git 来源包 | ✅ |
| **Built-in** | Unity 内置包 | ❌ |
| **Local Tarball** | 本地压缩包 | ✅ |
| **Unknown** | 未知来源包 | ❌ |

## ⚠️ 重要更新通知

> **从旧版本更新的用户请注意**  
> 从 **v2.0.24** 版本开始，包名称已从 `com.unity.ide.cursor` 更改为 `com.boxqkrtm.ide.cursor`，以避免与 Unity 官方包命名冲突。  
> 
> 如果在更新过程中遇到错误，请先移除旧包再重新安装新版本以避免冲突。

## 🔧 高级配置

### 自定义 VSCode 设置

包会自动创建以下配置文件，您也可以手动调整：

<details>
<summary>点击查看 .vscode/settings.json 示例</summary>

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

## 🤝 贡献指南

我们欢迎社区贡献！请查看 [CONTRIBUTING.md](CONTRIBUTING.md) 了解详细信息。

### 报告问题

如果您遇到任何问题，请：
1. 检查 [现有 Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
2. 创建新的 Issue 并提供详细信息
3. 包含您的 Unity 版本、操作系统和 Cursor 版本

### 功能建议

有好的想法？欢迎在 [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues) 中提出功能建议！

## 📄 许可证

本项目采用 [MIT License](LICENSE.md) 许可证。

## 📚 更多资源

- 📖 [详细文档](Documentation~/README.md)
- 🔄 [更新日志](CHANGELOG.md)
- 🐛 [问题报告](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [讨论区](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**享受使用 Cursor 进行 Unity 开发的乐趣！** 🎮✨

如果这个项目对您有帮助，请给我们一个 ⭐️

</div> 