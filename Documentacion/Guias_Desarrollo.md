# Guías de Desarrollo y Buenas Prácticas

Esta guía proporciona la información necesaria para configurar tu entorno de desarrollo, compilar y ejecutar el proyecto, y añadir nuevas funcionalidades siguiendo las directrices de diseño técnico establecidas.

---

## 💻 Requisitos y Entorno de Desarrollo

Para trabajar en este proyecto necesitarás:

1. **Godot Engine 4.5 (.NET Edition)**
   * Asegúrate de descargar la versión "Mono / .NET", no la versión estándar.
2. **SDK de .NET**
   * **.NET 8.0 SDK** (Obligatorio para la compilación de escritorio de Windows, Linux, macOS).
   * **.NET 9.0 SDK** (Requerido si vas a empaquetar o depurar en dispositivos Android).
3. **IDE / Editor**
   * Se recomienda **Visual Studio Code** con la extensión oficial de C# de Microsoft o **JetBrains Rider**.

---

## 🛠️ Comandos de Compilación útiles

Puedes compilar el proyecto fuera de Godot utilizando el SDK de dotnet a través del terminal desde la raíz del proyecto:

```powershell
# Compilar el proyecto en modo Debug (por defecto)
dotnet build

# Limpiar los artefactos de compilación previos
dotnet clean

# Restaurar dependencias NuGet explícitamente
dotnet restore
```

---

## 🎨 Estándar de Estilo de Código (C#)

Mantenemos un estilo consistente para facilitar la lectura del código entre todos los desarrolladores:

### 1. Nomenclatura
* **Clases y Métodos:** Usar `PascalCase` (ej. `NetworkManager`, `CrearServidor`).
* **Variables Locales y Parámetros:** Usar `camelCase` (ej. `nombreJugador`, `puerto`).
* **Campos Privados de Clase:** Usar guion bajo + camelCase (ej. `_dbPath`, `_estadoListo`).
* **Propiedades Públicas:** Usar `PascalCase` (ej. `Instance`, `NombreUsuario`).
* **Nombres de Métodos de Ciclo de Vida de Godot:** Respetar los prefijos de Godot (ej. `_Ready`, `_Process`, `_ExitTree`).

### 2. Formato
* Usar siempre llaves `{ }` incluso para sentencias de una sola línea en `if`, `for`, `foreach` y `while`.
* Declarar las dependencias de recursos externos de SQLite o entrada/salida mediante bloques `using` para asegurar la correcta liberación de la memoria no administrada.

---

## ➕ Pasos para Crear una Nueva Pantalla de UI

Si deseas añadir una nueva escena al juego (por ejemplo, una pantalla de tienda o de inventario), sigue estos pasos:

### Paso 1: Crear la escena en Godot
1. Crea una nueva escena heredando de un nodo de tipo **Control** (o sus derivados como `Panel` o `MarginContainer`).
2. Guarda el archivo `.tscn` dentro del directorio:
   `res://Scenes/UI/MiNuevaPantalla.tscn`
3. Diseña la interfaz utilizando contenedores (`VBoxContainer`, `HBoxContainer`, `CenterContainer`) para que sea responsive.

### Paso 2: Crear el script C#
1. Crea un archivo C# dentro de:
   `res://Scripts/UI/MiNuevaPantalla.cs`
2. Hereda el script de la misma clase del nodo raíz (usualmente `Control`) y usa `partial class` para permitir que Godot genere su pegamento de C#:
   ```csharp
   using Godot;
   using System;

   public partial class MiNuevaPantalla : Control
   {
       public override void _Ready()
       {
           // Lógica de inicialización
       }
   }
   ```
3. Asocia este script al nodo raíz de tu escena `.tscn` en el editor de Godot.

### Paso 3: Referenciar elementos en C#
* Cuando obtengas referencias a otros nodos en la escena usando `GetNode`, utiliza rutas relativas sólidas.
* Si un nodo puede no estar siempre presente, usa `GetNodeOrNull<T>("ruta")` para evitar excepciones en tiempo de ejecución.
* **Ejemplo:**
  ```csharp
  Button botonVolver = GetNode<Button>("CenterContainer/Panel/VBoxContainer/BtnVolver");
  botonVolver.Pressed += OnVolverPressed;
  ```

---

## 🧩 Modificar Autoloads existentes

Si añades nuevos métodos globales, ten en cuenta:
1. Si creas un nuevo Autoload, debes registrarlo en el archivo `project.godot` (en la pestaña de Configuración del Proyecto > Autoloads en el editor) para que se cargue automáticamente al inicio.
2. Cada singleton debe tener una referencia estática a sí mismo, la cual se asigna en el método `_Ready()`:
   ```csharp
   public static MiManager Instance { get; private set; }

   public override void _Ready()
   {
       if (Instance == null) Instance = this;
       else QueueFree(); // Evita instancias duplicadas en memoria
   }
   ```
