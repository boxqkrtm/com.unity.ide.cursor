# 🎯 Unity Cursor Editor Integration

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**Intégration complète de l'éditeur Cursor pour les développeurs Unity**

*Configuration Automatique de Projet • Support IntelliSense • Compatible Multi-Plateforme • Intégration Debug*

🇺🇸 [English](../README.md) | 🇹🇼 [繁體中文](README.zh-TW.md) | 🇨🇳 [简体中文](README.zh-CN.md) | 🇯🇵 [日本語](README.ja.md) | 🇰🇷 [한국어](README.ko.md) | 🇩🇪 [Deutsch](README.de.md) | 🇫🇷 [Français](README.fr.md) | 🇪🇸 [Español](README.es.md) | 🇷🇺 [Русский](README.ru.md) | 🇵🇹 [Português](README.pt.md) | 🇮🇹 [Italiano](README.it.md)

</div>

---

## ✨ Fonctionnalités

🚀 **Intégration en Un Clic** - Découverte et intégration automatiques de l'éditeur Cursor dans les workflows Unity  
📁 **Génération de Projet Intelligente** - Génération automatique de fichiers `.csproj` avec support complet d'IntelliSense  
🔍 **Support Multi-Version** - Support des versions stables et Insider de Cursor  
🌐 **Compatible Multi-Plateforme** - Support complet pour Windows, macOS, Linux  
⚙️ **Configuration Automatique** - Création automatique des fichiers de configuration VSCode et extensions recommandées  
🎯 **Contrôle Flexible** - Contrôle précis de la génération de projet pour différents types de packages  

## 📋 Configuration Système Requise

- **Unity**: 2019.4.25f1 ou plus récent
- **Cursor**: N'importe quelle version (détection automatique)
- **Plateforme**: Windows, macOS, Linux

## 📦 Installation

### Méthode 1: Via Unity Package Manager (Recommandé)

1. Ouvrir l'éditeur Unity
2. Aller à `Window` → `Package Manager`
3. Cliquer sur le bouton **`+`** en haut à gauche
4. Sélectionner `Add package from git URL...`
5. Entrer l'URL suivante:
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. Cliquer sur le bouton `Add`
7. Attendre la fin de l'installation ✅

### Méthode 2: Téléchargement Manuel

1. Télécharger la dernière [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases)
2. Extraire dans le dossier `Packages` de votre projet

## 🛠️ Utilisation

### Configuration de Base

1. **Définir Cursor comme Éditeur par Défaut**:
   - `Edit` → `Preferences` → `External Tools`
   - Sélectionner Cursor dans `External Script Editor`

2. **Configurer les Options de Génération de Projet**:
   - Dans les paramètres External Tools, vous pouvez choisir de générer des fichiers de projet pour:
     - ✅ Packages intégrés (Embedded packages)
     - ✅ Packages locaux (Local packages)  
     - ✅ Packages du registre (Registry packages)
     - ✅ Packages Git (Git packages)
     - ✅ Packages intégrés (Built-in packages)
     - ✅ Archives locales (Local tarball)
     - ✅ Sources inconnues (Unknown sources)
     - ✅ Projets joueur (Player projects)

3. **Régénérer les Fichiers de Projet**:
   - Cliquer sur le bouton `Regenerate project files` pour une mise à jour immédiate

### Fonctionnalités Automatiques

Après l'installation du package, automatiquement:
- 🔍 Détecter le chemin d'installation de Cursor
- 📝 Générer les fichiers `.csproj`
- ⚙️ Créer la configuration `.vscode/launch.json`
- 🎨 Configurer les préférences `.vscode/settings.json`
- 📦 Recommander les extensions VSCode pertinentes

## 🎯 Types de Packages Supportés

| Type de Package | Description | Activé par Défaut |
|-------------|-------------|----------------|
| **Embedded** | Packages intégrés au projet | ✅ |
| **Local** | Packages de développement local | ✅ |
| **Registry** | Packages Unity Registry | ✅ |
| **Git** | Packages sources Git | ✅ |
| **Built-in** | Packages intégrés Unity | ❌ |
| **Local Tarball** | Packages archives locales | ✅ |
| **Unknown** | Packages sources inconnues | ❌ |

## ⚠️ Avis de Mise à Jour Importante

> **Avis pour les Utilisateurs Mettant à Jour depuis des Versions Antérieures**  
> À partir de **v2.0.24**, le nom du package a été changé de `com.unity.ide.cursor` à `com.boxqkrtm.ide.cursor` pour éviter des problèmes potentiels avec Unity concernant l'attribution.  
> 
> Si vous rencontrez des erreurs pendant la mise à jour, veuillez supprimer le package existant avant de réinstaller la nouvelle version pour éviter les conflits.

## 🔧 Configuration Avancée

### Paramètres VSCode Personnalisés

Le package crée automatiquement les fichiers de configuration suivants, que vous pouvez ajuster manuellement:

<details>
<summary>Cliquer pour voir l'exemple .vscode/settings.json</summary>

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

## 🤝 Contribuer

Nous accueillons les contributions de la communauté! Veuillez consulter [CONTRIBUTING.md](../CONTRIBUTING.md) pour des informations détaillées.

### Signaler des Problèmes

Si vous rencontrez des problèmes, veuillez:
1. Vérifier les [Issues existantes](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
2. Créer une nouvelle Issue avec des informations détaillées
3. Inclure votre version Unity, OS, et version Cursor

### Demandes de Fonctionnalités

Vous avez une excellente idée? N'hésitez pas à suggérer des fonctionnalités dans [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)!

## 📄 Licence

Ce projet est sous licence [MIT License](../LICENSE.md).

## 📚 Ressources Supplémentaires

- 📖 [Documentation Détaillée](../Documentation~/README.md)
- 🔄 [Journal des Changements](../CHANGELOG.md)
- 🐛 [Rapports de Bugs](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [Discussions](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**Profitez du développement Unity avec Cursor!** 🎮✨

Si ce projet vous aide, donnez-nous une ⭐️

</div> 