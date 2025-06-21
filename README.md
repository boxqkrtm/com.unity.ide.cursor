# 🎯 Unity Cursor Editor Integration

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**Complete Cursor editor integration for Unity developers**

*Auto Project Configuration • IntelliSense Support • Cross-Platform Compatible • Debug Integration*

🇺🇸 [English](README.md) | 🇹🇼 [繁體中文](README.zh-TW.md) | 🇨🇳 [简体中文](README.zh-CN.md) | 🇯🇵 [日本語](README.ja.md) | 🇰🇷 [한국어](README.ko.md)

</div>

---

## ✨ Features

🚀 **One-Click Integration** - Automatically discover and integrate Cursor editor into Unity workflow  
📁 **Smart Project Generation** - Auto-generate `.csproj` files with full IntelliSense support  
🔍 **Multi-Version Support** - Support both Cursor stable and Insider versions  
🌐 **Cross-Platform Compatible** - Windows, macOS, Linux full platform support  
⚙️ **Auto Configuration** - Automatically create VSCode config files and recommended extensions  
🎯 **Flexible Control** - Fine-grained control over project generation for different package types  

## 📋 System Requirements

- **Unity**: 2019.4.25f1 or newer
- **Cursor**: Any version (auto-detected)
- **Platform**: Windows, macOS, Linux

## 📦 Installation

### Method 1: Via Unity Package Manager (Recommended)

1. Open Unity Editor
2. Go to `Window` → `Package Manager`
3. Click the **`+`** button at the top left
4. Select `Add package from git URL...`
5. Enter the following URL:
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. Click `Add` button
7. Wait for installation to complete ✅

### Method 2: Manual Download

1. Download the latest [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases)
2. Extract to your project's `Packages` folder

## 🛠️ Usage

### Basic Setup

1. **Set Cursor as Default Editor**:
   - `Edit` → `Preferences` → `External Tools`
   - Select Cursor in `External Script Editor`

2. **Configure Project Generation Options**:
   - In External Tools settings, you can choose to generate project files for:
     - ✅ Embedded packages
     - ✅ Local packages  
     - ✅ Registry packages
     - ✅ Git packages
     - ✅ Built-in packages
     - ✅ Local tarball
     - ✅ Unknown sources
     - ✅ Player projects

3. **Regenerate Project Files**:
   - Click `Regenerate project files` button to update immediately

### Automatic Features

After installation, the package will automatically:
- 🔍 Detect Cursor installation path
- 📝 Generate `.csproj` files
- ⚙️ Create `.vscode/launch.json` configuration
- 🎨 Setup `.vscode/settings.json` preferences
- 📦 Recommend relevant VSCode extensions

## 🎯 Supported Package Types

| Package Type | Description | Default Enabled |
|-------------|-------------|----------------|
| **Embedded** | Project embedded packages | ✅ |
| **Local** | Local development packages | ✅ |
| **Registry** | Unity Registry packages | ✅ |
| **Git** | Git source packages | ✅ |
| **Built-in** | Unity built-in packages | ❌ |
| **Local Tarball** | Local tarball packages | ✅ |
| **Unknown** | Unknown source packages | ❌ |

## ⚠️ Important Update Notice

> **Notice for Users Updating from Older Versions**  
> Starting from **v2.0.24**, the package name has been changed from `com.unity.ide.cursor` to `com.boxqkrtm.ide.cursor` to prevent potential issues with Unity regarding attribution.  
> 
> If you experience errors during the update, please remove the existing package before reinstalling the new one to avoid conflicts.

## 🔧 Advanced Configuration

### Custom VSCode Settings

The package automatically creates the following configuration files, which you can manually adjust:

<details>
<summary>Click to view .vscode/settings.json example</summary>

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

## 🤝 Contributing

We welcome community contributions! Please check [CONTRIBUTING.md](CONTRIBUTING.md) for detailed information.

### Reporting Issues

If you encounter any problems, please:
1. Check [existing Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
2. Create a new Issue with detailed information
3. Include your Unity version, OS, and Cursor version

### Feature Requests

Have a great idea? Feel free to suggest features in [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)!

## 📄 License

This project is licensed under the [MIT License](LICENSE.md).

## 📚 Additional Resources

- 📖 [Detailed Documentation](Documentation~/README.md)
- 🔄 [Changelog](CHANGELOG.md)
- 🐛 [Bug Reports](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [Discussions](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**Enjoy Unity development with Cursor!** 🎮✨

If this project helps you, please give us a ⭐️

</div>
