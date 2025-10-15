using Godot;
using System;
using System.Text;
using System.Timers;

public partial class Lobby : Control
{
    // 🧾 Referencias a los nodos de la interfaz (UI)
    private ItemList _listaJugadoresUI;       // Lista donde se mostrarán los nombres de los jugadores conectados
    private Label _hostInfoLabel;             // Muestra la información del servidor (IP, puerto, etc.)
    private Button _codigoConexionButton;      // Muestra el código de conexión (IP y puerto en Base64)
    private Button _empezarPartidaButton;     // Botón para iniciar la partida (solo visible para el servidor)
    private Button _salirLobbyButton;         // Botón para salir del lobby
    private Button _ListoButton;         // Botón para marcarse como listo/no listo
    private Button _SelecionarClaseButton;         // Botón para seleccionar clase
    // Variables para gestionar el estado "listo/no listo" de los jugadores
    private bool _estadoListo = false; // Estado local del jugador (listo o no listo)


    public override void _Ready()
    {
        // 📦 Obtenemos los nodos de la interfaz
        _listaJugadoresUI = GetNode<ItemList>("CenterContainer/Panel/VBoxContainer/PlayerList");
        _hostInfoLabel = GetNode<Label>("CenterContainer/Panel/VBoxContainer/HBoxContainer/textoInfo");
        _codigoConexionButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/HBoxContainer/codigoInfo");
        _empezarPartidaButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/IniciarButton");
        _salirLobbyButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/SalirButton");
        _ListoButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/ListoButton");
        _SelecionarClaseButton = GetNodeOrNull<Button>("CenterContainer/Panel/VBoxContainer/SeleccionButton");

        // 🔗 Conectamos la señal del NetworkManager (autoload)
        // Esta señal se emite cada vez que cambia la lista de jugadores
        // Cuando eso pase, se llama a la función UpdatePlayerList() para refrescar la UI
        // ✅ CAMBIO: se corrigió el condicional. Antes era "== null", lo que impedía conectar la señal.
        // Debe ser "!= null" para asegurarse de que el autoload exista antes de conectar.
        if (NetworkManager.Instance != null)
        {
            // estos ejecuta metodos cuando cada jugador se conecta o desconecta por medio del rpc
            // cuando un jugador se desconecta en su maquina ejecuta un metodo pero esto es rpc lo ejecuta en todas las maquinas
            // y asi se mantiene la lista de jugadores actualizada en todas las maquinas
            // Cuando cambie la lista de jugadores, actualizamos la UI
            // es decir, si se conecta o desconecta un jugador
            NetworkManager.Instance.Connect(
                NetworkManager.SignalName.ListaJugadoresActualizada,
                new Callable(this, nameof(UpdatePlayerList))
            );
            // Cuando cambie el estado de un jugador (listo/no listo), actualizamos la UI
            // es decir, si un jugador se marca como listo o no listo
            NetworkManager.Instance.Connect(
                NetworkManager.SignalName.EstadosJugadoresActualizados,
                new Callable(this, nameof(UpdatePlayerList))
            );
        }

        // ⚙️ Conectamos los botones a sus métodos
        _empezarPartidaButton.Pressed += OnComenzarPartidaButtonPressed;
        _salirLobbyButton.Pressed += OnSalirLobbyButtonPressed;
        _ListoButton.Pressed += OnListoButtonPressed;
        _SelecionarClaseButton.Pressed += OnSeleccionarClaseButtonPressed;
        // boton con evento para copiar el codigo de conexion
        _codigoConexionButton.GuiInput += OnCodigoLabelClicked;

        // Actualizamos la informacion de lobby. Lista y los datos del host
        UpdatePlayerList();
        ActualizarLobbyInfo();

    }

    // ===============================================================
    // 🧩 FUNCIÓN: ENCRIPTAR IP Y PUERTO
    // ===============================================================
    private string GenerarCodigoConexion(string ip, int puerto)
    {
        // Unidos los datos en una sola cadena
        string datos = $"{ip}:{puerto}";
        // Convertimos a bytes y luego a Base64
        byte[] bytes = Encoding.UTF8.GetBytes(datos);
        string codigo = Convert.ToBase64String(bytes);
        return codigo;
    }
    


    // ===============================================================
    // 🧩 ACTUALIZA INFORMACIÓN DEL LOBBY
    // ===============================================================
    public void ActualizarLobbyInfo()
    {
        if (Multiplayer.IsServer())
        {
            string ip = NetworkManager.Instance.obtenerDireccionIPLocal();
            int puerto = NetworkManager.Instance.obtenerPuertoLocal();
            // Mostramos la informacion del host
            _hostInfoLabel.Text = $"Host: {NetworkManager.JugadorNombre}\n IP: {ip}\n Puerto: {puerto}";
            _codigoConexionButton.Text = GenerarCodigoConexion(ip, puerto);
            _hostInfoLabel.Visible = true;
            _empezarPartidaButton.Visible = true; // Solo el host puede ver el boton de empezar partida
            _ListoButton.Visible = false; // El host no necesita marcarse como listo
        }
        else
        {
            _hostInfoLabel.Visible = false;
            _empezarPartidaButton.Visible = false;
            _ListoButton.Visible = true; // Los clientes pueden marcarse como listos
        }
    }




    // ===============================================================
    // 🔄 Actualiza la lista de jugadores (sin cambios)
    // ===============================================================
    // Refresca la lista de jugadores en el UI del lobby
    private void UpdatePlayerList()
    {
        _listaJugadoresUI.Clear();
        // Recorremos la lsita de jugadores guardada en el NetworkManager (autoload) llamda public static Dictionary<int, string> ListaJugadores
        // lo recorremos para obtener el id y el nombre de cada jugador
        foreach (var jugador in NetworkManager.ListaJugadores)
        {
            int id = jugador.Key;
            string nombre = jugador.Value;

            // Verificamos si el jugador está listo (true) o no (false)
            bool estaListo = NetworkManager.EstadosJugadores.ContainsKey(jugador.Key) && NetworkManager.EstadosJugadores[jugador.Key];
            // El host siempre esta listo
            bool esHost = 1 == id;

            int itemIndex = _listaJugadoresUI.AddItem(nombre);

            Color color;
            if (esHost)
            {
                color = Colors.Green;
            }
            else if (estaListo)
            {
                color = Colors.Red;
            }
            else
            {
                color = Colors.White;
            }
            _listaJugadoresUI.SetItemCustomFgColor(itemIndex, color);
        }
    }










    // ===============================================================
    // 🧩 BOTONES
    // ===============================================================
    // Cuando el botón de "comenzar partida" es presionado para empezar la partida
    private void OnComenzarPartidaButtonPressed()
    {
        if (!Multiplayer.IsServer())
        {
            GD.PrintErr("Solo el servidor puede iniciar la partida.");
            return;
        }
        if (!NetworkManager.Instance.TodosClientesListos())
        {
            GD.Print("No todos los jugadores están listos.");
            return;
        }
        GD.Print("🚀 Todos listos iniciamos la partida");
        // para buscar la ecena cuando se presione el boton inicar partida
        //GetTree().ChangeSceneToFile("res://Scenes/")
    }


    // cuando el botón de "salir del lobby" es presionado para volver al menú principal
    private void OnSalirLobbyButtonPressed()
    {
        GD.Print(" Saliendo del lobby...");
        // Desconectar del servidor (red)
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
        }
        //limpiar la lista  local
        NetworkManager.ListaJugadores.Clear();
        // Volver al menú de multiplayer
        GetTree().ChangeSceneToFile("res://Scenes/UI/MultiplayerMenu.tscn");
    }


    // cuando el botón de "listo/no listo" es presionado para marcarse como listo o no listo
    private void OnListoButtonPressed()
    {
        // es decir si _stadoListo es false al poner diferente(!) se vuelve true y viceversa
        _estadoListo = !_estadoListo; // Alterna entre listo y no listo (true/false)
        _ListoButton.Text = _estadoListo ? "No Listo" : "Listo"; // Cambia el texto del botón según el estado
        // el if es para evitar errores en caso de que no se haya inicializado el NetworkManager
        // en espanol seria "si el NetworkManager no está inicializado"
        if (NetworkManager.Instance != null)
        {
            // se llama al metodo CambiarEstadoJugador del singleton NetworkManager (o clase estatica que gestiona la red)
            NetworkManager.Instance.CambiarEstadoJugador(_estadoListo);
        }

    }

    private void OnSeleccionarClaseButtonPressed()
    {
        GD.Print("Entro al menu de selecion de clase");
        GetTree().ChangeSceneToFile("res://Scenes/UI/SeleccionClase.tscn");
    }

    private void OnCodigoLabelClicked(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            // copiar el texto al portapapeles
            DisplayServer.ClipboardSet(_codigoConexionButton.Text);

            GD.Print("Código de conexión copiado al portapapeles: " + _codigoConexionButton.Text);

            // opcional: mostrar mensaje visual en pantalla
            Label aviso = new Label();
            aviso.Text = "Código copiado al portapapeles";
            aviso.AddThemeColorOverride("font_color", Colors.Green);
            AddChild(aviso);
            aviso.GlobalPosition = _codigoConexionButton.GlobalPosition + new Vector2(0, -20);
            GetTree().CreateTimer(1.5f).Timeout += () => aviso.QueueFree();
        }
    }



}
