using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class SeleccionClase : Control
{
    // Botones de control entre escenas
    private Button _botonSeleccionarClase;
    private Button _BotonRegresar;
    // Botones de clases
    private Button _BotonGuerrero;
    private Button _BotonMago;
    private Button _BotonArquero;
    // Labels 
    private Label _LabelNombreClase;
    private Label _LabelTituloInfoClase;
    private Label _LabelHabilidades;
    private Label _LabelVentajas;
    private RichTextLabel _LabelDescripcionClase;
    // Elemento que muestra el personaje 
    private TextureRect _PersonajeClase;

    // Variable que almacena el nombre de la clase seleccionada
    private string claseSeleccionada = "";

   

    public override void _Ready()
    {
        // Obtenemos los nodos de la interfaz
        _botonSeleccionarClase = GetNode<Button>("HBoxContainer2/BtnSeleccionar");
        _BotonRegresar = GetNode<Button>("HBoxContainer2/BtnVolver");
        _BotonGuerrero = GetNode<Button>("HBoxContainer/VBoxContainer/HBoxContainer/BtnGuerrero");
        _BotonMago = GetNode<Button>("HBoxContainer/VBoxContainer/HBoxContainer/BtnMago");
        _BotonArquero = GetNode<Button>("HBoxContainer/VBoxContainer/HBoxContainer/BtnArquero");

        _LabelNombreClase = GetNode<Label>("HBoxContainer/VBoxContainer/LabelNombreClase");
        _LabelTituloInfoClase = GetNode<Label>("HBoxContainer/VBoxContainer2/LabelTituloInfo");
        _LabelHabilidades = GetNode<Label>("HBoxContainer/VBoxContainer2/LabelHabilidades");
        _LabelVentajas = GetNode<Label>("HBoxContainer/VBoxContainer2/LabelVentajas");
        _LabelDescripcionClase = GetNode<RichTextLabel>("HBoxContainer/VBoxContainer2/RichTextDescipcion");
        _PersonajeClase = GetNode<TextureRect>("HBoxContainer/VBoxContainer/VistaPersonaje");

        // Conectamos botones de selección de clase
        _BotonGuerrero.Pressed += () => OnSeleccionPressed("Guerrero");
        _BotonMago.Pressed += () => OnSeleccionPressed("Mago");
        _BotonArquero.Pressed += () => OnSeleccionPressed("Arquero");

        // Botón de selección final
        _botonSeleccionarClase.Pressed += OnFinalizarSeleccionPressed;

        // Actualizamos la interfaz según el modo (solo o multijugador)
        ActualizarUI();

        // Selección por defecto
        OnSeleccionPressed("Guerrero");

        GD.Print($"NetworkManager.Instance es: {NetworkManager.Instance}");
    }

    private void OnSeleccionPressed(string nombre)
    {
        var info = ClaseManager.Instance.obtenerClase(nombre);

        if (info == null)
        {
            GD.PrintErr("Error al obtener la información de la clase");
            return;
        }

        claseSeleccionada = nombre;
        _LabelNombreClase.Text = info.Nombre;
        _LabelDescripcionClase.Text = info.Descripcion;
        _LabelHabilidades.Text = info.Habilidades;
        _LabelVentajas.Text = info.Ventajas;

        // Cambiar textura (opcional)
        // _PersonajeClase.Texture = GD.Load<Texture2D>($"res://Assets/Personajes/{nombre.ToLower()}.png");
    }

    private void ActualizarUI()
    {
        bool hayConexion = NetworkManager.Instance.HayConexionActiva();

        if (hayConexion)
        {
            // Multijugador
            _BotonRegresar.Visible = true;
            _BotonRegresar.Text = "Regresar al Lobby";
            _BotonRegresar.Pressed += OnRegresarAlLobbyPressed;
        }
        else
        {
            // Modo solo
            _BotonRegresar.Visible = true;
            _BotonRegresar.Text = "Regresar al Menú Principal";
            _BotonRegresar.Pressed += OnRegresarAlMenuPressed;
        }
    }

    private void OnFinalizarSeleccionPressed()
    {
        GD.Print($"Clase seleccionada: {claseSeleccionada}");

        bool hayConexion = NetworkManager.Instance.HayConexionActiva();

        if (hayConexion)
            GetTree().ChangeSceneToFile("res://Scenes/Networking/Lobby.tscn");
        else
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }

    private void OnRegresarAlLobbyPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Networking/Lobby.tscn");
    }

    private void OnRegresarAlMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
