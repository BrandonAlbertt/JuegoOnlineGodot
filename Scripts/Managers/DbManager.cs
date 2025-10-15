using Godot;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

public partial class DbManager : Node
{
    // 🔷 Singleton (única instancia global)
    // Esta propiedad permite acceder al gestor de base de datos
    // desde cualquier parte del juego con: DbManager.Instance
    public static DbManager Instance { get; private set; }

    // 🔷 Ruta donde se guardará el archivo de la base de datos
    // ProjectSettings.GlobalizePath convierte la ruta interna de Godot
    // a una ruta absoluta en el sistema del usuario.
    private string _dbPath;

    // 🔷 Este método se ejecuta automáticamente cuando el nodo entra al árbol de escenas
    // Aquí se inicializa la base de datos y se crea la tabla de jugadores si no existe.
    public override void _Ready()
    {
        // Si ya hay una instancia, destruimos la nueva (patrón singleton)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
            return;
        }

        // Guardamos la base de datos dentro de la carpeta "user://"
        // (segura para lectura y escritura en tiempo de ejecución)
        _dbPath = ProjectSettings.GlobalizePath("user://jugadores.db");

        GD.Print("Ruta de la base de datos:", _dbPath);

        // Crear tabla de jugadores si no existe
        CrearTablaJugadores();
    }


    // 🔷 Este método crea la tabla "jugadores" si no existe.
    // La tabla tiene: id (clave), nombre (único) y puntos (entero).
    private void CrearTablaJugadores()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        string sql = @"
            CREATE TABLE IF NOT EXISTS jugadores (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nombre TEXT NOT NULL UNIQUE,
                puntos INTEGER DEFAULT 0
            );
        ";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.ExecuteNonQuery();

        connection.Close();

        GD.Print("Tabla 'jugadores' verificada o creada correctamente.");
    }


    //  🔷 Este método guarda un nuevo jugador en la base de datos
    // Si el jugador ya existe, no se guarda.
    public bool GuardarJugador(string nombreJugador)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        string sql = @"
            INSERT INTO jugadores (nombre, puntos)
            VALUES ($nombre, $puntos)
            ON CONFLICT(nombre) DO UPDATE SET puntos = $puntos;
        ";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("$nombre", nombreJugador);
        cmd.Parameters.AddWithValue("$puntos", 0);
        cmd.ExecuteNonQuery();

        connection.Close();

        GD.Print($"✅ Jugador '{nombreJugador}' guardado");
        return true;
    }



    // 🔷 
    public int CargarPuntos(string nombreJugador)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        string sql = @"SELECT puntos FROM jugadores WHERE nombre = $nombre;";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("$nombre", nombreJugador);

        var resultado = cmd.ExecuteScalar();

        connection.Close();

        // Si el jugador no existe, resultado será null.
        if (resultado == null || resultado == DBNull.Value)
        {
            GD.Print($"⚠ Jugador '{nombreJugador}' no encontrado. Se devuelven 0 puntos.");
            return 0;
        }

        int puntos = Convert.ToInt32(resultado);
        GD.Print($"🎯 Jugador '{nombreJugador}' tiene {puntos} puntos.");
        return puntos;
    }



    // 🔷 Este método obtiene una lista de todos los jugadores con sus puntos.
    // Devuelve una lista de cadenas con formato: "Nombre - Puntos".
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

    // esta funcion override _ExitTree se llama cuando el nodo se elimina del árbol de escenas
    // es decir cuando se cierra el juego o se cambia de escena
    // en este caso se usa para cerrar la conexion con la base de datos
    // es una funcion interna de godot que se llama automaticamente
    // como el _Ready que se llama cuando el nodo entra en el arbol de escenas
    // y se usa para inicializar el nodo
    // en este caso se usa para abrir la conexion con la base de datos
    // y crear las tablas si no existen
    // y el _ExitTree se usa para cerrar la conexion con la base de datos
    // y liberar los recursos
    // es una buena practica cerrar la conexion con la base de datos
    // para evitar problemas de concurrencia y bloqueos
    // y para liberar los recursos del sistema
    // pero esto es para cuando el juego se cierra o se cambia de escena
    public override void _ExitTree()
    {
        GD.Print("Base de datos cerrada.");
    }


}
