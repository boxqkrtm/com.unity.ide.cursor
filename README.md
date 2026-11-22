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
Compared to Unity's Visual Studio package (`com.unity.ide.visualstudio`), this package adds:
- **Cursor detection** - Cursor shows up in External Script Editor on Windows, macOS and Linux  
- **Reuse existing Cursor window** - option in External Tools to open files in a running Cursor window instead of a new one  
- **Open `.code-workspace`** - option in External Tools (off by default) to open the project's single `.code-workspace` file instead of the project folder
