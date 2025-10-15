using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{

	// del main menu
	private Button _PlayBoton;
	private Button _MultiplayerBoton;
	

	// Declarar variables de los nodos de funcionalidad login
    private Button _Login;
    private Label _nombreUsuarioLabel;
    private Window _popupLogin;

    private Button _iniciarSesion;
    private Button _AgregarCuenta;
    private Button _cancelar;
    private OptionButton _opcionesUser;
    private LineEdit _inputUser;
    private Label _inputTitulo;

	private string VnombreUsuario = "";
	
	public override void _Ready()
	{
		// Vincular botones principales
		_PlayBoton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/PlayButton");
        _MultiplayerBoton = GetNode<Button>("CenterContainer/Panel/VBoxContainer/MultiplayerButton");
        GetNode<Button>("CenterContainer/Panel/VBoxContainer/OptionButton").Pressed += OnOptionPressed;
        GetNode<Button>("CenterContainer/Panel/VBoxContainer/ExitButton").Pressed += OnExitPressed;

        _Login = GetNode<Button>("CenterContainer/Panel/HBoxContainer/BtnLogin");
        _nombreUsuarioLabel = GetNode<Label>("CenterContainer/Panel/HBoxContainer/LabelNombre");

        // Popup
        _popupLogin = GetNode<Window>("PopupLogin");
        _iniciarSesion = GetNode<Button>("PopupLogin/CenterContainer/VBoxContainer/btnIniciarSesion");
        _AgregarCuenta = GetNode<Button>("PopupLogin/CenterContainer/VBoxContainer/btnAgregarCuenta");
        _cancelar = GetNode<Button>("PopupLogin/CenterContainer/VBoxContainer/btnCancelar");
        _opcionesUser = GetNode<OptionButton>("PopupLogin/CenterContainer/VBoxContainer/OptionUsuarioButton");
        _inputUser = GetNode<LineEdit>("PopupLogin/CenterContainer/VBoxContainer/LineEditNombre");
        _inputTitulo = GetNode<Label>("PopupLogin/CenterContainer/VBoxContainer/LabelTitulo");



		_PlayBoton.Pressed += OnPlayPressed;
		_MultiplayerBoton.Pressed += OnMultiplayerPressed;

		// Conectar señales
		_Login.Pressed += OnLoginPressed;
        _cancelar.Pressed += OnCancelarPressed;
        _iniciarSesion.Pressed += OnIniciarSesionPressed;
		_AgregarCuenta.Pressed += OnAgregarCuentaPressed;

		_popupLogin.CloseRequested += () => _popupLogin.Hide();

		ActualizarInterfaz();
		_popupLogin.Hide();


	}

	// Funcion para abrir el popup de login
	private void OnLoginPressed()
	{
		if (UserSession.Instance.EstaLogueado())
		{
			// usanos el sigleton UserSession y para cerrar la session llamamos al metodo CerrarSession
			UserSession.Instance.CerrarSession();
			VnombreUsuario = "";

			GD.Print(" Sesion cerrada");
			ActualizarInterfaz();
			return;
		}

		_popupLogin.PopupCentered();
		CargarUsuarios();
	}

	private void CargarUsuarios()
    {
		_opcionesUser.Clear();

		List<string> usuarios = DbManager.Instance.ObtenerJugadores();

		if (usuarios.Count == 0)
		{
			_opcionesUser.AddItem("No hay usuarios");
			_opcionesUser.Disabled = true;
		}
		else
		{
			_opcionesUser.Disabled = false;
			foreach (var user in usuarios)
            {
				_opcionesUser.AddItem(user);
            }
        }

    }


	// Funcion para agregar una nueva cuenta
	private void OnAgregarCuentaPressed()
	{
		string nombre = _inputUser.Text.Trim();
		if (string.IsNullOrEmpty(nombre))
		{
			GD.Print("El nombre de usuario no puede estar vacio");
			return;
		}

		bool agregado = DbManager.Instance.GuardarJugador(nombre);
		if (agregado)
		{
			GD.Print("Usuario agregado: " + nombre);
			_inputUser.Text = "";
			CargarUsuarios();
		}
		else
        {
			GD.Print("El usuario ya existe: " + nombre);
        }
        

	}

	// Funcion para iniciar sesion con el usuario seleccionado
	private void OnIniciarSesionPressed()
	{
		if (_opcionesUser.Selected == -1 || _opcionesUser.Disabled)
		{
			GD.Print("No hay usuarios disponibles");
			return;
		}
		string nombreJugador = _opcionesUser.GetItemText(_opcionesUser.Selected);
		VnombreUsuario = nombreJugador;
		// guardar el nombre en el singleton UserSession
		UserSession.Instance.IniciarSession(nombreJugador);
		GD.Print("Sesion iniciada con el usuario: " + VnombreUsuario);
		_popupLogin.Hide();
		ActualizarInterfaz();
	} 



	// Funcion para cancelar el login
	private void OnCancelarPressed()
	{
		_popupLogin.Hide();
	}

	
	// Funcion para actualizar la interfaz segun el estado de la sesion
	private void ActualizarInterfaz()
	{
		// EXTRAEMOS EL NOMBRE GUARDADO EN EL SINGLETON UserSession Y ACTUALIZAMOS LA VARIABLE LOCAL VnombreUsuario
		VnombreUsuario = UserSession.Instance.NombreUsuario;
		_nombreUsuarioLabel.Text = VnombreUsuario == "" ? "Sesion no inciada" : VnombreUsuario;
		_Login.Text = VnombreUsuario == "" ? "Inciar Sesion" : "Cerrar Session";

		// Si no hay usuarios, deshabilitamos el boton de login
		bool hayUsuarios = UserSession.Instance.EstaLogueado();
		_PlayBoton.Visible = hayUsuarios;
		_MultiplayerBoton.Visible = hayUsuarios;
	}










    // ==== Botones principales del menú ====
	private void OnPlayPressed()
	{
		GD.Print("Play iniciado");
		//cargar la escena principal (Main.tscn)
		GetTree().ChangeSceneToFile("res://Scenes/UI/SeleccionClase.tscn");

	}

	private void OnMultiplayerPressed()
	{
		GD.Print("Multijugador iniciado");
		GetTree().ChangeSceneToFile("res://Scenes/UI/MultiplayerMenu.tscn");
	}
	
	private void OnOptionPressed()
	{
		GD.Print("Opciones iniciadas");
	}
	
	private void OnExitPressed()
    {
		GD.Print("Salir del juego");
		GetTree().Quit();
    }

	
}
