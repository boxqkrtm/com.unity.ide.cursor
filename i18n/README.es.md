# 🎯 Unity Cursor Editor Integration

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-2019.4.25f1+-brightgreen?logo=unity)
![Version](https://img.shields.io/badge/version-2.0.25-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

**Integración completa del editor Cursor para desarrolladores Unity**

*Configuración Automática de Proyectos • Soporte IntelliSense • Compatible Multiplataforma • Integración de Depuración*

🇺🇸 [English](../README.md) | 🇹🇼 [繁體中文](README.zh-TW.md) | 🇨🇳 [简体中文](README.zh-CN.md) | 🇯🇵 [日本語](README.ja.md) | 🇰🇷 [한국어](README.ko.md) | 🇩🇪 [Deutsch](README.de.md) | 🇫🇷 [Français](README.fr.md) | 🇪🇸 [Español](README.es.md) | 🇷🇺 [Русский](README.ru.md) | 🇵🇹 [Português](README.pt.md) | 🇮🇹 [Italiano](README.it.md)

</div>

---

## ✨ Características

🚀 **Integración con Un Clic** - Descubrimiento e integración automática del editor Cursor en flujos de trabajo Unity  
📁 **Generación Inteligente de Proyectos** - Generación automática de archivos `.csproj` con soporte completo de IntelliSense  
🔍 **Soporte Multi-Versión** - Soporte para versiones estables e Insider de Cursor  
🌐 **Compatible Multiplataforma** - Soporte completo para Windows, macOS, Linux  
⚙️ **Configuración Automática** - Creación automática de archivos de configuración VSCode y extensiones recomendadas  
🎯 **Control Flexible** - Control fino de la generación de proyectos para diferentes tipos de paquetes  

## 📋 Requisitos del Sistema

- **Unity**: 2019.4.25f1 o más reciente
- **Cursor**: Cualquier versión (detección automática)
- **Plataforma**: Windows, macOS, Linux

## 📦 Instalación

### Método 1: Vía Unity Package Manager (Recomendado)

1. Abrir el editor Unity
2. Ir a `Window` → `Package Manager`
3. Hacer clic en el botón **`+`** en la esquina superior izquierda
4. Seleccionar `Add package from git URL...`
5. Introducir la siguiente URL:
   ```
   https://github.com/boxqkrtm/com.unity.ide.cursor.git
   ```
6. Hacer clic en el botón `Add`
7. Esperar a que la instalación se complete ✅

### Método 2: Descarga Manual

1. Descargar la última [Release](https://github.com/boxqkrtm/com.unity.ide.cursor/releases)
2. Extraer en la carpeta `Packages` de tu proyecto

## 🛠️ Uso

### Configuración Básica

1. **Establecer Cursor como Editor Predeterminado**:
   - `Edit` → `Preferences` → `External Tools`
   - Seleccionar Cursor en `External Script Editor`

2. **Configurar Opciones de Generación de Proyectos**:
   - En la configuración de External Tools, puedes elegir generar archivos de proyecto para:
     - ✅ Paquetes integrados (Embedded packages)
     - ✅ Paquetes locales (Local packages)  
     - ✅ Paquetes del registro (Registry packages)
     - ✅ Paquetes Git (Git packages)
     - ✅ Paquetes incorporados (Built-in packages)
     - ✅ Archivos locales (Local tarball)
     - ✅ Fuentes desconocidas (Unknown sources)
     - ✅ Proyectos de jugador (Player projects)

3. **Regenerar Archivos de Proyecto**:
   - Hacer clic en el botón `Regenerate project files` para actualización inmediata

### Características Automáticas

Después de la instalación del paquete, automáticamente:
- 🔍 Detectar la ruta de instalación de Cursor
- 📝 Generar archivos `.csproj`
- ⚙️ Crear configuración `.vscode/launch.json`
- 🎨 Configurar preferencias `.vscode/settings.json`
- 📦 Recomendar extensiones VSCode relevantes

## 🎯 Tipos de Paquetes Soportados

| Tipo de Paquete | Descripción | Habilitado por Defecto |
|-------------|-------------|----------------|
| **Embedded** | Paquetes integrados en el proyecto | ✅ |
| **Local** | Paquetes de desarrollo local | ✅ |
| **Registry** | Paquetes Unity Registry | ✅ |
| **Git** | Paquetes fuente Git | ✅ |
| **Built-in** | Paquetes incorporados Unity | ❌ |
| **Local Tarball** | Paquetes archivo local | ✅ |
| **Unknown** | Paquetes fuente desconocida | ❌ |

## ⚠️ Aviso Importante de Actualización

> **Aviso para Usuarios Actualizando desde Versiones Anteriores**  
> A partir de **v2.0.24**, el nombre del paquete se cambió de `com.unity.ide.cursor` a `com.boxqkrtm.ide.cursor` para evitar problemas potenciales con Unity relacionados con la atribución.  
> 
> Si encuentras errores durante la actualización, por favor remueve el paquete existente antes de reinstalar la nueva versión para evitar conflictos.

## 🔧 Configuración Avanzada

### Configuración VSCode Personalizada

El paquete crea automáticamente los siguientes archivos de configuración, que puedes ajustar manualmente:

<details>
<summary>Hacer clic para ver el ejemplo .vscode/settings.json</summary>

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

## 🤝 Contribuir

¡Damos la bienvenida a las contribuciones de la comunidad! Por favor consulta [CONTRIBUTING.md](../CONTRIBUTING.md) para información detallada.

### Reportar Problemas

Si encuentras problemas, por favor:
1. Verificar [Issues existentes](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
2. Crear un nuevo Issue con información detallada
3. Incluir tu versión de Unity, SO, y versión de Cursor

### Solicitudes de Características

¿Tienes una gran idea? ¡Siéntete libre de sugerir características en [Issues](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)!

## 📄 Licencia

Este proyecto está licenciado bajo la [MIT License](../LICENSE.md).

## 📚 Recursos Adicionales

- 📖 [Documentación Detallada](../Documentation~/README.md)
- 🔄 [Registro de Cambios](../CHANGELOG.md)
- 🐛 [Reportes de Errores](https://github.com/boxqkrtm/com.unity.ide.cursor/issues)
- 💬 [Discusiones](https://github.com/boxqkrtm/com.unity.ide.cursor/discussions)

---

<div align="center">

**¡Disfruta del desarrollo Unity con Cursor!** 🎮✨

Si este proyecto te ayuda, ¡danos una ⭐️!

</div> 