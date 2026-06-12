using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class NetworkManager : Node
{
    // el Instance es para poder acceder a este script desde cualquier otro script
    public static NetworkManager Instance;

    // Este script controla toda la lógica de red del juego (crear servidor, unirse, manejar jugadores).
    //  Guardará la conexión de red actual (ya sea servidor o cliente).
    // "ENetMultiplayerPeer" es la clase que se encarga de enviar y recibir datos entre jugadores.
    public static ENetMultiplayerPeer Peer;
    //  Lista de jugadores conectados.
    // Se guarda el ID único de cada jugador (clave) y su nombre (valor).
    public static Dictionary<int, string> ListaJugadores = new();
    // Decionario para estados de los jugadores (listo/no listo);
    public static Dictionary<int, bool> EstadosJugadores = new();
    //  Nombre del jugador actual.
    public static string JugadorNombre = "";
    //  Puerto en el que se creó el servidor (lo guardamos manualmente por que Enet no lo prvee GetLocalPort)
    public static int PuertoServidor = 0;
    //  Señal que se emitirá cuando cambie la lista de jugadores (para actualizar la UI, por ejemplo).
    [Signal]
    public delegate void ListaJugadoresActualizadaEventHandler();
    [Signal]
    public delegate void EstadosJugadoresActualizadosEventHandler();




    public override void _Ready()
    {

        Instance = this;


        if (Multiplayer == null)
        {
            GD.PrintErr("Error: Multiplayer no esta inicializado");
            return;
        }
        


        // 🔌 Aquí se conectan los eventos de red de Godot a los métodos que los manejarán.
        // Cuando alguien se conecta o se desconecta, se llamarán estos métodos automáticamente.
        Multiplayer.PeerConnected += OnPeerConectado;          // Cuando un jugador se conecta
        Multiplayer.PeerDisconnected += OnPeerDesconectado;    // Cuando un jugador se desconecta
        Multiplayer.ConnectedToServer += OnConectarAlServidor; // Cuando este cliente logra conectarse al servidor
        Multiplayer.ConnectionFailed += OnConexionFallida;    // Si falla la conexión
        Multiplayer.ServerDisconnected += OnServidorDesconectado; // Si el servidor se apaga o se pierde


        CerrarConexion(); // asegurarse de que no haya conexiones previas abiertas
    }


    // ===============================================================
    //  CREAR SERVIDOR
    // ===============================================================
    /* ===============================================================
       PARTE 2: CREAR Y UNIRSE A SERVIDORES
      Aquí está la lógica principal para hospedar una partida o unirse a una existente.
      =============================================================== */
    public void CrearServidor(string nombreJugador, int puerto)
    {
        // Guardamos el nombre del jugador que crea la partida.
        JugadorNombre = nombreJugador;
        PuertoServidor = puerto;
        // Creamos la conexión de red (ENet es el sistema de red de bajo nivel que usa Godot).
        Peer = new ENetMultiplayerPeer();
        // Intentamos crear un servidor en el puerto indicado.
        // "4" significa que se permiten hasta 4 jugadores conectados.
        var resultado = Peer.CreateServer(puerto, 4);
        // Si algo sale mal, se muestra un error y se sale del método.
        if (resultado != Error.Ok)
        {
            GD.PrintErr("❌ Error al crear el servidor: " + resultado);
            return;
        }
        //  IMPORTANTE: se conecta el "Peer" al sistema de red de Godot.
        // Sin esta línea, la red NO funcionará.
        Multiplayer.MultiplayerPeer = Peer;
        // Registramos al jugador que creó la partida como el primer jugador
        int idServidor = (int)Multiplayer.GetUniqueId();
        ListaJugadores[idServidor] = nombreJugador;
        // Emitimos la señal para avisar a la interfaz que la lista de jugadores ha cambiado.
        EmitSignal(SignalName.ListaJugadoresActualizada);
        // Cambiamos a la escena del lobby (sala de espera).
        GetTree().ChangeSceneToFile("res://Scenes/Networking/Lobby.tscn");
        GD.Print(" Servidor creado en el puerto: " + puerto);

        Rpc(nameof(SincronizarListaJugadores), ListaJugadores.Keys.ToArray(), ListaJugadores.Values.ToArray());
    }





    // ===============================================================
    // UNIRSE A UN SERVIDOR EXISTENTE
    // ===============================================================
    //  Unirse a un servidor existente
    public void UnirseServidor(string ip, int puerto, string nombreJugador)
    {
        JugadorNombre = nombreJugador;
        // Creamos un nuevo Peer para conectarnos como cliente.
        Peer = new ENetMultiplayerPeer();
        // Intentamos conectarnos al servidor.
        var resultado = Peer.CreateClient(ip, puerto);
        // Si falla la conexión, mostramos el error.
        if (resultado != Error.Ok)
        {
            GD.PrintErr("❌ Error al unirse al servidor: " + resultado);
            return;
        }
        // ⚡ Conectamos este Peer al sistema de red global de Godot.
        Multiplayer.MultiplayerPeer = Peer;
        GD.Print("🔗 Conectado al servidor: " + ip + " en el puerto: " + puerto);
    }





    // ===============================================================
    //  EVENTOS DE CONEXIÓN / DESCONEXIÓN
    // ===============================================================
    /* ===============================================================
        PARTE 3: EVENTOS DE RED (CONEXIÓN Y DESCONEXIÓN)
       Estas funciones se activan automáticamente según el estado de red.
       =============================================================== */
    // Cuando un jugador se conecta (solo el servidor lo ve)
    private void OnPeerConectado(long id)
    {
        GD.Print("👤 Jugador conectado con ID: " + id);
        //GetTree().ChangeSceneToFile("res://Scenes/Networking/Lobby.tscn");
    }

    // Cuando un jugador se desconecta
    private void OnPeerDesconectado(long id)
    {
        GD.Print("🚪 Jugador desconectado con ID: " + id);
        // Eliminamos al jugador de la lista si estaba registrado.
        if (ListaJugadores.ContainsKey((int)id))
        {
            ListaJugadores.Remove((int)id);
        }
        // Notificamos a la interfaz que hubo un cambio.
        EmitSignal(SignalName.ListaJugadoresActualizada);
        Rpc(nameof(SincronizarListaJugadores), ListaJugadores.Keys.ToArray(), ListaJugadores.Values.ToArray());
    }



    // Cuando el cliente logra conectarse al servidor correctamente.
    private void OnConectarAlServidor()
    {
        GD.Print(" Conectado al servidor, registrando jugador...");
        // El cliente envía su nombre al servidor usando un RPC.
        // "RpcId(1, ...)" significa que este mensaje se manda al peer con ID 1 (el servidor).
        RpcId(1, nameof(RegistrarJugador), JugadorNombre);
        
        if(GetTree().CurrentScene.Name != "Lobby")
        {
            GetTree().ChangeSceneToFile("res://Scenes/Networking/Lobby.tscn");
        }
    }



    // Si falla la conexión al servidor (por IP o puerto incorrecto)
    private void OnConexionFallida()
    {
        GD.PrintErr("❌ Falló la conexión al servidor.");
        // Aquí podrías volver al menú o mostrar un mensaje de error.
    }



    // Si el servidor se apaga o se desconecta
    private void OnServidorDesconectado()
    {
        GD.PrintErr(" Desconectado del servidor.");
        // Se borra la conexión actual
        Multiplayer.MultiplayerPeer = null;
        // Se limpia la lista de jugadores
        ListaJugadores.Clear();
        // Se notifica a la interfaz
        EmitSignal(SignalName.ListaJugadoresActualizada);
        // Se vuelve al menú principal
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }






    // ===============================================================
    //  REGISTRO Y SINCRONIZACIÓN DE JUGADORES
    // ===============================================================
    /* ===============================================================
        PARTE 4: REGISTRO Y SINCRONIZACIÓN DE JUGADORES
       Aquí se registra cada jugador, se actualizan listas
       y se sincronizan con todos los peers conectados.
       =============================================================== */
    //  RPC: Método remoto que registra al jugador en el servidor.
    // Este se ejecuta SOLO en el servidor cuando un cliente se conecta.
    //[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    // la diferencia entre Authority y AnyPeer es que Authority 
    // solo lo puede llamar el servidor
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void RegistrarJugador(string nombre)
    {
        // Obtenemos el ID del jugador que envió el RPC.
        int idRemoto = (int)Multiplayer.GetRemoteSenderId();
        // si idRemoto es 0, significa que el servidor se está registrando a sí mismo
        int id = idRemoto == 0 ? (int)Multiplayer.GetUniqueId() : idRemoto;

        /* ------------sala llena ----------------- */
        // Si ya hay 4 jugadores, rechazamos la conexión.
        if (ListaJugadores.Count >= 4)
        {
            GD.PrintErr("El lobby está lleno. No se pueden registrar más jugadores.");
            RpcId(id, nameof(RechazarPorLaSalaLlena)); // 1) avisar al cliente que se rechaza su conexion
            Peer.DisconnectPeer(id); // 2) desconectarlo
            return;
        }
        /* --------------------------------------- */

        // Registro del jugador
        // el if es opcional, pero evita que un mismo jugador se registre varias veces
        if (!ListaJugadores.ContainsKey(id))
        {
            // Registramos al nuevo jugador en la lista.
            ListaJugadores[id] = nombre;
            GD.Print($" jugador registrado: {nombre} con ID: {id}");
        }
        // Lista actualizada para TODOS (host y clientes)
        Rpc(nameof(SincronizarListaJugadores), ListaJugadores.Keys.ToArray(), ListaJugadores.Values.ToArray());
        // Emitimos la señal para actualizar la UI del lobby.
        EmitSignal(SignalName.ListaJugadoresActualizada);

    }



    // RPC: para avisar al cliente que su conexión fue rechazada por sala llena
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void RechazarPorLaSalaLlena()
    {
        GD.PrintErr("❌ Conexión rechazada: la sala está llena.");
        // Limpiamos la conexión actual
        ListaJugadores.Clear();
        // Aquí podrías mostrar un mensaje en la UI o volver al menú principal.
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }





    // RPC: para sincronizar la lista de jugadores en un nuevo cliente
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SincronizarListaJugadores(int[] ids, string[] nombres)
    {
        ListaJugadores.Clear();
        for (int i = 0; i < ids.Length; i++)
        {
            ListaJugadores[ids[i]] = nombres[i];
        }
        EmitSignal(SignalName.ListaJugadoresActualizada);
    }








    // ===============================================================
    //  FUNCIONALIDAD DE "LISTO / NO LISTO"
    // ===============================================================
    /* ===============================================================
        PARTE 5: FUNCIONALIDAD DE "LISTO / NO LISTO"
       Esta parte controla los estados de cada jugador en el lobby,
       sincroniza visualmente quién está listo, y avisa al servidor.
       =============================================================== */
    /* ----------------------------------------------------------
    RPC : CambiarEstadoJugador
    Se ejecuta en la máquina que pulsó “Listo” y se replica
    automáticamente en TODOS los peers (host + clientes).
    ---------------------------------------------------------- */
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void CambiarEstadoJugador(bool estado)
    {
        // 1. Guardar el estado localmente (clave = ID único de este peer)
        int id = (int)Multiplayer.GetUniqueId();
        EstadosJugadores[id] = estado;
        // 2. Log para depuración
        GD.Print($"Jugador {ListaJugadores[id]} cambio su estado a {(estado ? "Listo" : "No Listo")}");

        /* ------------------------------------------------------
       3. Preparar datos para enviarlos por red.
          Godot no puede serializar bool[] / int[] directamente,
          así que convertimos a Godot.Collections.Array<T>
       ------------------------------------------------------ */
        Godot.Collections.Array<int> ids = new(EstadosJugadores.Keys); // IDs de jugadores
        Godot.Collections.Array<bool> estados = new(EstadosJugadores.Values); // sus estados (listo/no listo)
        // 4. Enviar los datos a TODOS los peers (incluido el servidor)
        Rpc(nameof(SincronizarEstadosJugadores), ids, estados);
        // 5. Avisar localmente a la UI (Lobby.cs) para que repinte la lista
        EmitSignal(SignalName.EstadosJugadoresActualizados);
    }





    /* ----------------------------------------------------------
    RPC : SincronizarEstadosJugadores
    Llega a TODOS los peers justo después de CambiarEstadoJugador.
    Reconstruye el diccionario global y avisa a la UI.
    ---------------------------------------------------------- */
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SincronizarEstadosJugadores(Godot.Collections.Array<int> ids, Godot.Collections.Array<bool> estados)
    {
        // 1. Limpiar diccionario viejo (evita duplicados o basura)
        EstadosJugadores.Clear();
        // 2. Reconstruir el diccionario con los arrays recibidos
        for (int i = 0; i < ids.Count; i++)
        {
            EstadosJugadores[ids[i]] = estados[i];
        }
        // 3. Avisar a la UI para que actualice colores de nombres
        EmitSignal(SignalName.EstadosJugadoresActualizados);
    }




    // Metodo para verificar si todos los clientes están listos
    // El servidor no cuenta en esta verificación
    // Si hay menos de 2 jugadores, no se considera que todos estén listos
    // Chequeo de "todos los clientes están listos" (servidor no cuenta))
    public bool TodosClientesListos()
    {
        // Requerimos al menos 2 jugadores (host + 1 cliente)
        if (ListaJugadores.Count < 2) return false; // No hay suficientes jugadores
        int servidorId = (int)Multiplayer.GetUniqueId(); // id del peer del servidor (si esto corre en server sera su propio id)
        // Recorremos el diccionario de estados que tiene de nombre EstadosJugadores y verificamos que todos los clientes (excepto el servidor) estén listos
        // Si encontramos algún cliente que no esté listo, retornamos false
        foreach (var kv in EstadosJugadores)
        {
            // kv es un KeyValuePair<int, bool> donde kv.Key es el ID del jugador y kv.Value es su ESTADO (listo/no listo)
            int idJugador = kv.Key;
            if (idJugador == servidorId) continue; // Saltamos al servidor
            if (!EstadosJugadores.ContainsKey(idJugador) || !EstadosJugadores[idJugador])
            {
                return false; // Al menos un cliente no está listo
            }
        }
        return true; // Todos los clientes están listos
    }


    /*  -------------------------------------------------------------------------
    ------------------------ fin funcionalidad de listo/no listo -----------------------
    -----------------------------------------------------------------------------
    */







    // ===============================================================
    //  UTILIDADES DE RED
    // ===============================================================
    /* ===============================================================
     PARTE 6: UTILIDADES DE RED Y SISTEMA
    Estas funciones no afectan la jugabilidad, pero ayudan a
    mostrar información útil como IP y puerto del servidor.
    =============================================================== */
    //  Obtiene la IP local de la computadora (para mostrarla al crear el servidor)
    public string obtenerDireccionIPLocal()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // Solo se toma una IP de tipo IPv4 (no IPv6)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("❌ Error al obtener la IP local: " + e.Message);
        }

        return "Desconocida";
    }

    // funcion que retorna si hay un peer activo conectado (ya sea cliente o servidor)
    public bool HayConexionActiva()
    {
        bool conectado = Multiplayer.MultiplayerPeer != null && Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;
        GD.Print($"🔍 Estado de conexión: {(conectado ? "Activa" : "Desconectada")}");
        return conectado;
    }

    // 🔧 CORREGIDO: ya no lanza NullReferenceException
    public void CerrarConexion()
    {
        GD.Print("🧹 Intentando cerrar la conexión actual...");

        if (Multiplayer == null)
        {
            GD.PrintErr("⚠️ Multiplayer no inicializado.");
            return;
        }

        if (Multiplayer.MultiplayerPeer == null)
        {
            GD.Print("ℹ No hay conexión de red activa que cerrar.");
            return;
        }

        try
        {
            GD.Print("Cerrando conexión ENet...");
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
            Peer = null;
            ListaJugadores.Clear();
            EstadosJugadores.Clear();

            EmitSignal(SignalName.ListaJugadoresActualizada);
            EmitSignal(SignalName.EstadosJugadoresActualizados);
            GD.Print(" Conexión cerrada correctamente.");
        }
        catch (Exception e)
        {
            GD.PrintErr("❌ Error al cerrar conexión: " + e.Message);
        }
    }




    //  Obtiene el puerto actual del servidor (útil para mostrarlo en pantalla)
    public int obtenerPuertoLocal() => PuertoServidor;


}
