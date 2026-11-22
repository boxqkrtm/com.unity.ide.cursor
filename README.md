## How to install
- Unity -> Window -> Package Manager  
- Click "+" at the top left corner  
- Add package from git URL  
- Insert `https://github.com/boxqkrtm/com.unity.ide.cursor.git`  
- Add  
- Done

> **Important Notice for Users Updating from Older Versions**  
> Starting from version **v2.0.24**, the package name has been changed from  
> `com.unity.ide.cursor` to `com.boxqkrtm.ide.cursor` to prevent potential issues with Unity regarding attribution.  
> Violating these attribution rules may trigger warnings in Unity.  
> If you experience errors during the update, please remove the existing package before reinstalling the new one to avoid conflicts.

## Cursor-specific features
Compared to Unity's Visual Studio / VS Code integration, this package adds:
- **Cursor detection** - Cursor shows up in External Script Editor on Windows, macOS and Linux  
- **Project folder context** - Cursor is always opened with the Unity project folder by default, so workspace features such as Project Rules (`.cursor/rules`) work  
- **Reuse existing Cursor window** - option in External Tools to open files in a running Cursor window instead of a new one  
- **Open `.code-workspace`** - option in External Tools to open the project's single `.code-workspace` file instead of the folder  
- **Refresh on save follows Auto Refresh** - a refresh request from Cursor is ignored when Unity's Auto Refresh is Disabled

## Refresh on save
- Set External Script Editor to **Cursor** (Edit -> Preferences -> External Tools)  
- Install the **Unity** extension (`visualstudiotoolsforunity.vstuc`) in Cursor and enable `vstuc.refreshOnSave`  
  - It is not in Cursor's extension store: switch Cursor's marketplace to the VS Code Marketplace, or install the `.vsix`  
- Keep Auto Refresh set to **Enabled** or **Enabled Outside Playmode** (Preferences -> Asset Pipeline)  
