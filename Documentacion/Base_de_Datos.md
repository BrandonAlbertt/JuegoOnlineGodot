# Manual de Base de Datos y Persistencia

Este documento describe la base de datos local SQLite utilizada por el juego para registrar cuentas, persistir la sesión y almacenar las puntuaciones de los jugadores.

---

## 📦 Motor e Integración

El proyecto utiliza **SQLite** como motor de persistencia de datos local. Esto permite almacenar información de forma estructurada en un solo archivo físico en el disco del usuario, sin necesidad de un servidor de base de datos externo.

* **Biblioteca C#:** `Microsoft.Data.Sqlite` (Versión `9.0.9`).
* **Namespace:** `using Microsoft.Data.Sqlite;`
* **Administrador global:** `DbManager.cs` (Autoload / Singleton).

---

## 💾 Ubicación física del Archivo de Base de Datos

En Godot, las rutas como `res://` (recursos del proyecto) son de **solo lectura** una vez exportado el juego. Para escribir datos de forma persistente, se utiliza el directorio virtual **`user://`**.

* **Ruta de desarrollo/juego:** `user://jugadores.db`
* **Conversión a absoluta:** Para que el controlador de SQLite del sistema operativo pueda encontrar el archivo, se utiliza:
  ```csharp
  _dbPath = ProjectSettings.GlobalizePath("user://jugadores.db");
  ```
  Esto resolverá la ruta a una carpeta del sistema de archivos del usuario específica para la aplicación (por ejemplo, en Windows `C:\Users\<usuario>\AppData\Roaming\Godot\app_userdata\<nombre-proyecto>\jugadores.db`).

---

## 📊 Esquema de la Base de Datos

El juego cuenta con una única tabla llamada `jugadores`. Su estructura es la siguiente:

```sql
CREATE TABLE IF NOT EXISTS jugadores (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL UNIQUE,
    puntos INTEGER DEFAULT 0
);
```

### Detalle de Campos:
* **`id`**: Clave primaria autoincremental de tipo entero.
* **`nombre`**: Texto que identifica de forma única a cada jugador. No se permiten nombres duplicados (`UNIQUE`).
* **`puntos`**: Entero que almacena el score actual del usuario (por defecto empieza en `0`).

---

## 🛠️ Operaciones y Consultas SQL

A continuación se detallan los métodos expuestos por el Singleton `DbManager.Instance`:

### 1. Inicialización y Validación de la Tabla
Ejecutado automáticamente dentro de `_Ready()`. Abre la conexión SQLite y crea la tabla si esta no existiera previamente.
```csharp
private void CrearTablaJugadores()
{
    using var connection = new SqliteConnection($"Data Source={_dbPath}");
    connection.Open();
    string sql = @"
        CREATE TABLE IF NOT EXISTS jugadores (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL UNIQUE,
            puntos INTEGER DEFAULT 0
        );";
    using var cmd = new SqliteCommand(sql, connection);
    cmd.ExecuteNonQuery();
    connection.Close();
}
```

### 2. Guardar o Actualizar un Jugador
Se invoca cuando un usuario añade una cuenta nueva o actualiza datos. Si el nombre ya existe, sobrescribe sus puntos mediante la cláusula `ON CONFLICT`.
```csharp
public bool GuardarJugador(string nombreJugador)
{
    using var connection = new SqliteConnection($"Data Source={_dbPath}");
    connection.Open();
    string sql = @"
        INSERT INTO jugadores (nombre, puntos)
        VALUES ($nombre, $puntos)
        ON CONFLICT(nombre) DO UPDATE SET puntos = $puntos;";
    using var cmd = new SqliteCommand(sql, connection);
    cmd.Parameters.AddWithValue("$nombre", nombreJugador);
    cmd.Parameters.AddWithValue("$puntos", 0);
    cmd.ExecuteNonQuery();
    connection.Close();
    return true;
}
```

### 3. Cargar Puntuación de un Jugador
Obtiene los puntos acumulados por un usuario a partir de su nombre. Si el usuario no existe, devuelve `0` de forma segura.
```csharp
public int CargarPuntos(string nombreJugador)
{
    using var connection = new SqliteConnection($"Data Source={_dbPath}");
    connection.Open();
    string sql = "SELECT puntos FROM jugadores WHERE nombre = $nombre;";
    using var cmd = new SqliteCommand(sql, connection);
    cmd.Parameters.AddWithValue("$nombre", nombreJugador);
    var resultado = cmd.ExecuteScalar();
    connection.Close();
    if (resultado == null || resultado == DBNull.Value) return 0;
    return Convert.ToInt32(resultado);
}
```

### 4. Listar Todos los Jugadores
Utilizado para llenar los dropdowns y listas de perfiles en los menús de inicio de sesión.
```csharp
public List<string> ObtenerJugadores()
{
    var jugadores = new List<string>();
    using var connection = new SqliteConnection($"Data Source={_dbPath}");
    connection.Open();
    string sql = "SELECT nombre FROM jugadores;";
    using var cmd = new SqliteCommand(sql, connection);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        jugadores.Add(reader.GetString(0));
    }
    connection.Close();
    return jugadores;
}
```

---

## 🔒 Buenas Prácticas de Persistencia en el Proyecto

* **Manejo de Conexiones:** Todas las conexiones SQLite y comandos SQL se declaran usando la palabra clave `using` de C# (`using var connection = ...`). Esto asegura que los recursos se liberen inmediatamente (haciendo un Dispose implícito del socket y manejador de archivo) apenas termine de ejecutarse el método, evitando que el archivo `.db` quede bloqueado.
* **Prevención de Inyección SQL:** Los parámetros dinámicos (como el nombre del jugador) nunca se concatenan directamente en el string SQL. En su lugar, se utilizan marcadores de parámetros parametrizados (`$nombre`) y `Parameters.AddWithValue` para mitigar cualquier vulnerabilidad de inyección o fallos sintácticos por caracteres especiales.
