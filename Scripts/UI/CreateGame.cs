using Godot;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

public partial class CreateGame : Control
{
    // declarar variables de los nodos
    private SpinBox _puertoInput;
    private Button _crearButton;
    private Button _volverButton;
    private Label _nombreUsuarioLabel;
    private string VnombreUsuario = "";

    public override void _Ready()
    {

        _puertoInput = GetNode<SpinBox>("CenterContainer/Panel/VBoxContainer/HBoxContainer2/PuertoInput");
        _crearButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/CrearButton");
        _volverButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/VolverButton");
        _nombreUsuarioLabel = GetNode<Label>("CenterContainer/Panel/VBoxContainer/HBoxContainer/LabelNombre");


        _crearButton.Pressed += OnCrearPressed;
        _volverButton.Pressed += OnVolverPressed;

        VnombreUsuario = UserSession.Instance.NombreUsuario;
        _nombreUsuarioLabel.Text = VnombreUsuario;

        //ActualizarInfoUser();
    }

    private void OnCrearPressed()
    {
        string nombreJugador = VnombreUsuario;
        int puerto = (int)_puertoInput.Value;

        // Cambio de && (que sirve para comprobar que ambas condiciones son verdaderas)
        // a || (que sirve para comprobar que al menos una de las condiciones es verdadera)
        if (string.IsNullOrEmpty(nombreJugador) || puerto < 1000)
        {
            GD.Print("El nombre del jugador no puede estar vacio y el puerto debe ser mayor a 1000");
            return;
        }

        // llamar al metodo CreateServer del singleton NetworkManager (o clase estatica que gestiona la red)
        GD.Print("Crear servidor con nombre: " + nombreJugador + " en el puerto: " + puerto);
        //pasamos el nombre del jugador y el puerto al metodo CrearServidor
        NetworkManager.Instance.CrearServidor(nombreJugador, puerto);
    }
    /*
    private void ActualizarInfoUser()
    {
        string nombreUsuario = DbManager.Instance.
        
    }
*/

    private void OnVolverPressed()
    {
        GD.Print("Volver al menu multijugador");
        GetTree().ChangeSceneToFile("res://Scenes/UI/MultiplayerMenu.tscn");
    }

}

 