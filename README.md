# 🎮 Nuevo Proyecto de Juego — Godot 4.5 C#

![Godot Engine](https://img.shields.io/badge/Godot-4.5.x%20.NET-478CBF?style=for-the-badge&logo=godot-engine&logoColor=white)
![C# / .NET](https://img.shields.io/badge/C%23-.NET%208.0%20%2F%209.0-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-v9.0.9%20Persistencia-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Network](https://img.shields.io/badge/Network-ENet%20Multiplayer-orange?style=for-the-badge)

Un proyecto base de videojuego multijugador online desarrollado en **Godot 4.5 (.NET Edition)** con **C#**. Cuenta con autenticación y base de datos local SQLite, selección de personajes y sincronización de red local en tiempo real con un lobby interactivo.

---

## 🗺️ Mapa de Documentación Interactiva

Hemos preparado una suite de documentación técnica y visual detallada. Haz clic en las tarjetas a continuación para acceder a cada sección:

| 📑 Sección | 🎨 Descripción | 🔗 Enlace Directo |
| :--- | :--- | :--- |
| **🗺️ Mapa Visual de Flujo** | Diagramas de flujo y mapas interactivos a color sobre variables y red. | **[Ver Mapa Visual](file:///c:/Users/tecni/OneDrive/Documentos/nuevo-proyecto-de-juego/Documentacion/Mapa_Visual_Proyecto.md)** |
| **🏗️ Manual de Arquitectura** | Información sobre la estructura física y los scripts globales de control. | **[Ver Arquitectura](file:///c:/Users/tecni/OneDrive/Documentos/nuevo-proyecto-de-juego/Documentacion/Arquitectura.md)** |
| **🔌 Red y Multijugador** | Protocolo ENet, llamadas remotas (RPCs) y sincronización de listos. | **[Ver Guía Multijugador](file:///c:/Users/tecni/OneDrive/Documentos/nuevo-proyecto-de-juego/Documentacion/Multijugador_Lobby.md)** |
| **💾 Base de Datos SQLite** | Almacenamiento local, consultas parametrizadas y perfiles de usuario. | **[Ver Guía de BD](file:///c:/Users/tecni/OneDrive/Documentos/nuevo-proyecto-de-juego/Documentacion/Base_de_Datos.md)** |
| **💻 Guías de Desarrollo** | Buenas prácticas de C#, compilación, Git y guías de desarrollo de UI. | **[Ver Guía de Desarrollo](file:///c:/Users/tecni/OneDrive/Documentos/nuevo-proyecto-de-juego/Documentacion/Guias_Desarrollo.md)** |
| **📋 Resumen de Estructura** | Archivo de referencia rápida inicial del proyecto. | **[Ver Estructura](file:///c:/Users/tecni/OneDrive/Documentos/nuevo-proyecto-de-juego/ESTRUCTURA_PROYECTO.md)** |

---

## 🛠️ Requisitos del Sistema

Para poder compilar y ejecutar este proyecto correctamente, debes tener instalado:

* **Godot Engine 4.5.x (.NET Edition):** Versión obligatoria con soporte Mono/C#.
* **SDK de .NET 8.0:** Para compilaciones de escritorio (Windows, Mac, Linux).
* **SDK de .NET 9.0:** Requerido para builds de la plataforma Android.
* **SQLite Dependency:** El proyecto ya instala automáticamente `Microsoft.Data.Sqlite` versión `9.0.9`.

---

## 🚀 Guía de Inicio Rápido

> [!TIP]
> Si deseas clonar el repositorio rápidamente mediante tu terminal favorita, sigue los siguientes pasos:

### 1. Clonación
```bash
git clone URL_DEL_REPOSITORIO
cd nuevo-proyecto-de-juego
```

### 2. Importar en Godot
1. Abre **Godot Engine 4.5 .NET**.
2. Haz clic en **Import (Importar)** y selecciona el archivo `project.godot` de la raíz del proyecto.
3. Al abrir por primera vez, deja que el motor compile el ensamblado de C# y genere la caché en `.godot/`.

### 3. Compilación manual (Opcional)
Si usas un IDE externo (como VS Code, Visual Studio o Rider), puedes compilar el proyecto ejecutando:
```powershell
dotnet build
```

---

## 🧩 Sistemas Globales Integrados (Autoloads)

El juego utiliza 4 **Autoloads** registrados globalmente. Funcionan como Singletons que persisten al cambiar de escena:

* 🟩 **`DbManager`**: Creación de base de datos SQLite local y persistencia de cuentas y puntuaciones.
* 🟩 **`UserSession`**: Mantiene en memoria de ejecución el perfil del usuario autenticado.
* 🟩 **`ClaseManager`**: Administrador de estadísticas y habilidades para las clases Guerrero, Mago y Arquero.
* 🟩 **`NetworkManager`**: Coordina servidores, clientes, puertos y sincronización de red local.

---

## ⚠️ Notas Importantes para el Desarrollo

> [!IMPORTANT]
> * **No uses la versión estándar de Godot:** El proyecto fallará de inmediato porque contiene scripts en C#. Asegúrate de usar la edición **.NET**.
> * **Autoloads Críticos:** Nunca elimines la configuración de Autoloads en `project.godot`, de lo contrario los scripts globales no se instanciarán.
> * **Rutas Hardcodeadas:** Los controladores de UI usan rutas directas como `GetNode("Panel/PlayButton")`. Si modificas la estructura en el editor visual de Godot, asegúrate de actualizar las rutas en su script de C#.
