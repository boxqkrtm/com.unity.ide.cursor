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

## Project Rules

This package always launches Cursor with the **Unity project folder** (the directory that contains the `.sln`), so Cursor can load Project Rules from `.cursor/rules`. Opening only a script file without a folder context can show “No Project Rules Yet” in Cursor.

Optional: in **Edit → Preferences → External Tools**, use **Reuse existing Cursor window** to open files in an existing Cursor window (`--reuse-window`) instead of always opening a new one (`--new-window`). Both modes still pass the project folder.

## Refresh On Save

Cursor’s Unity extension (`visualstudiotoolsforunity.vstuc`) can refresh Unity’s Asset Database when you save a script. For that to work with this package:

1. Set **Edit → Preferences → External Tools → External Script Editor** to **Cursor**.
2. Install the **Unity** / `vstuc` extension in Cursor and enable **Refresh Unity’s Asset Database on save**.
3. In Unity, keep **Preferences → Asset Pipeline → Auto Refresh** set to **Enabled** or **Enabled Outside Play Mode** (not **Disabled**).
4. Remove the legacy `com.unity.ide.vscode` package if it is still installed (it can conflict with this package).
5. Allow Unity and Cursor to use local UDP/TCP (firewall); ensure the project allows the Editor to run in the background (`Application.runInBackground`).

After a successful refresh, Unity regenerates the relevant `.csproj` files so IntelliSense picks up new scripts.
