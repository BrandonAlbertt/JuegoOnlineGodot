using Godot;
using System;
using System.Buffers;
using System.Text;

public partial class JoinGame : Control
{
    private Label _nombreJugadorLabel;
    private LineEdit _ipInput;
    private SpinBox _puertoInput;
    private Button _unirseButton;
    private Button _volverButton;

    // variables de codigo
    private LineEdit _codigoInput;


    public override void _Ready()
    {
        _nombreJugadorLabel = GetNode<Label>("CenterContainer/Panel/VBoxContainer/HBoxContainer/NameLabel");
        _ipInput = GetNode<LineEdit>("CenterContainer/Panel/VBoxContainer/HBoxContainer2/IpInput");
        _puertoInput = GetNode<SpinBox>("CenterContainer/Panel/VBoxContainer/HBoxContainer3/PuertoInput");
        _unirseButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/UnirseButton");
        _volverButton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/VolverButton");
        _codigoInput = GetNode<LineEdit>("CenterContainer/Panel/VBoxContainer/HBoxContainer4/codigoInput");

        _unirseButton.Pressed += OnUnirsePressed;
        _volverButton.Pressed += OnVolverPressed;

        _nombreJugadorLabel.Text = UserSession.Instance.NombreUsuario;
        
    }

    private void OnUnirsePressed()
    {
        var (ipC, puertoC) = DesencriptarCodigoConexion(_codigoInput.Text);
        string nombreJugador = _nombreJugadorLabel.Text;
        string ip = ipC;//_ipInput.Text;
        int puerto = puertoC;//(int)_puertoInput.Value;

        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(ip) || puerto < 1000)
        {
            GD.Print("El nombre del jugador y la IP no pueden estar vacios y el puerto debe ser mayor a 1000");
            return;
        }
        // llamar al metodo JoinServer del singleton NetworkManager ( o clase estatica que gestiona la red)
        GD.Print("Unirse al servidor con nombre: " + nombreJugador + "en la IP: " + ip + " en el puerto: " + puerto);
        NetworkManager.Instance.UnirseServidor(ip, puerto, nombreJugador);
    }

    // Funcion para desencriptar el código (la usaría el cliente)
    private static (string ip, int puerto) DesencriptarCodigoConexion(string codigo)
    {
        byte[] datos = Convert.FromBase64String(codigo);
        string texto = Encoding.UTF8.GetString(datos);
        string[] partes = texto.Split(':');
        return (partes[0], int.Parse(partes[1]));
    }


    private void OnVolverPressed()
    {
        GD.Print("Volver al menu multijugador");
        GetTree().ChangeSceneToFile("res://Scenes/UI/MultiplayerMenu.tscn");
    }
    


}
