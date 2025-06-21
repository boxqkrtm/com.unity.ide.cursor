# 🎯 Unity Cursor Editor Integration

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**Vollständige Cursor-Editor-Integration für Unity-Entwickler**

*Automatische Projektkonfiguration • IntelliSense-Unterstützung • Plattformübergreifende Kompatibilität • Debug-Integration*

🇺🇸 [English](../README.md) | 🇹🇼 [繁體中文](README.zh-TW.md) | 🇨🇳 [简体中文](README.zh-CN.md) | 🇯🇵 [日本語](README.ja.md) | 🇰🇷 [한국어](README.ko.md) | 🇩🇪 [Deutsch](README.de.md) | 🇫🇷 [Français](README.fr.md) | 🇪🇸 [Español](README.es.md) | 🇷🇺 [Русский](README.ru.md) | 🇵🇹 [Português](README.pt.md) | 🇮🇹 [Italiano](README.it.md)

</div>

---

## ✨ Funktionen

🚀 **Ein-Klick-Integration** - Automatische Erkennung und Integration des Cursor-Editors in Unity-Workflows  
📁 **Intelligente Projektgenerierung** - Automatische Generierung von `.csproj`-Dateien mit vollständiger IntelliSense-Unterstützung  
🔍 **Multi-Version-Unterstützung** - Unterstützung für sowohl Cursor-Stable- als auch Insider-Versionen  
🌐 **Plattformübergreifende Kompatibilität** - Vollständige Plattformunterstützung für Windows, macOS, Linux  
⚙️ **Automatische Konfiguration** - Automatische Erstellung von VSCode-Konfigurationsdateien und empfohlenen Erweiterungen  
🎯 **Flexible Kontrolle** - Feinabstimmung der Projektgenerierung für verschiedene Pakettypen  

## 📋 Systemanforderungen

- **Unity**: 2019.4.25f1 oder neuer
- **Cursor**: Beliebige Version (automatische Erkennung)
- **Plattform**: Windows, macOS, Linux

## 📦 Installation

### Methode 1: Über Unity Package Manager (Empfohlen)

1. Unity Editor öffnen
2. `Window` → `Package Manager` auswählen
3. **`+`**-Schaltfläche oben links klicken
4. `Add package from git URL...` auswählen
5. Folgende URL eingeben:
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. `Add`-Schaltfläche klicken
7. Warten bis Installation abgeschlossen ist ✅

### Methode 2: Manueller Download

1. Neueste [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases) herunterladen
2. In den `Packages`-Ordner Ihres Projekts extrahieren

## 🛠️ Verwendung

### Grundkonfiguration

1. **Cursor als Standard-Editor festlegen**:
   - `Edit` → `Preferences` → `External Tools`
   - Cursor in `External Script Editor` auswählen

2. **Projektgenerierungsoptionen konfigurieren**:
   - In den External Tools-Einstellungen können Sie die Projektdateigenerierung für folgende Elemente auswählen:
     - ✅ Eingebettete Pakete (Embedded packages)
     - ✅ Lokale Pakete (Local packages)  
     - ✅ Registry-Pakete (Registry packages)
     - ✅ Git-Pakete (Git packages)
     - ✅ Eingebaute Pakete (Built-in packages)
     - ✅ Lokale Archive (Local tarball)
     - ✅ Unbekannte Quellpakete (Unknown sources)
     - ✅ Player-Projekte (Player projects)

3. **Projektdateien neu generieren**:
   - `Regenerate project files`-Schaltfläche klicken für sofortige Aktualisierung

### Automatische Funktionen

Nach der Paketinstallation automatisch:
- 🔍 Cursor-Installationspfad erkennen
- 📝 `.csproj`-Dateien generieren
- ⚙️ `.vscode/launch.json`-Konfiguration erstellen
- 🎨 `.vscode/settings.json`-Einstellungen einrichten
- 📦 Relevante VSCode-Erweiterungen empfehlen

## 🎯 Unterstützte Pakettypen

| Pakettyp | Beschreibung | Standard aktiviert |
|-------------|-------------|----------------|
| **Embedded** | Projekt-eingebettete Pakete | ✅ |
| **Local** | Lokale Entwicklungspakete | ✅ |
| **Registry** | Unity Registry-Pakete | ✅ |
| **Git** | Git-Quellpakete | ✅ |
| **Built-in** | Unity eingebaute Pakete | ❌ |
| **Local Tarball** | Lokale Archivpakete | ✅ |
| **Unknown** | Unbekannte Quellpakete | ❌ |

## ⚠️ Wichtiger Update-Hinweis

> **Hinweis für Benutzer beim Update von älteren Versionen**  
> Ab **v2.0.24** wurde der Paketname von `com.unity.ide.cursor` zu `com.boxqkrtm.ide.cursor` geändert, um potenzielle Probleme mit Unity bezüglich der Zuordnung zu vermeiden.  
> 
> Falls Sie während des Updates Fehler auftreten, entfernen Sie bitte das bestehende Paket, bevor Sie die neue Version installieren, um Konflikte zu vermeiden.

## 🔧 Erweiterte Konfiguration

### Benutzerdefinierte VSCode-Einstellungen

Das Paket erstellt automatisch die folgenden Konfigurationsdateien, die Sie manuell anpassen können:

<details>
<summary>Klicken Sie, um das .vscode/settings.json-Beispiel anzuzeigen</summary>

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

## 🤝 Beitragen

Wir begrüßen Community-Beiträge! Bitte schauen Sie sich [CONTRIBUTING.md](../CONTRIBUTING.md) für detaillierte Informationen an.

### Probleme melden

Wenn Sie auf Probleme stoßen, bitte:
1. [Bestehende Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues) prüfen
2. Neues Issue mit detaillierten Informationen erstellen
3. Unity-Version, Betriebssystem und Cursor-Version angeben

### Feature-Anfragen

Haben Sie eine großartige Idee? Schlagen Sie gerne Features in [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues) vor!

## 📄 Lizenz

Dieses Projekt ist unter der [MIT License](../LICENSE.md) lizenziert.

## 📚 Zusätzliche Ressourcen

- 📖 [Detaillierte Dokumentation](../Documentation~/README.md)
- 🔄 [Changelog](../CHANGELOG.md)
- 🐛 [Bug-Reports](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [Diskussionen](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**Genießen Sie Unity-Entwicklung mit Cursor!** 🎮✨

Wenn Ihnen dieses Projekt hilft, geben Sie uns bitte einen ⭐️

</div> 