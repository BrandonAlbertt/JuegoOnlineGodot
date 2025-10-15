using Godot;
using System;
using System.Runtime.Serialization.Formatters;

public partial class UserSession : Node
{
    public static UserSession Instance { get; private set; }
    public string NombreUsuario { get; private set; } = "";

    public override void _Ready()
    {
        if (Instance != null)
        {
            // el queuefree es para que si ya existe una instancia, se destruya la nueva
            QueueFree();
            return;


        }
        // Si no existe una instancia, esta se convierte en la instancia global
        Instance = this;
        SetProcess(false);
        GD.Print("UserSession cargado y persistente");
    }

    public void IniciarSession(string nombre)
    {
        NombreUsuario = nombre;
        GD.Print("Sesion iniciada con el usuario: " + NombreUsuario);
    }

    public void CerrarSession()
    {
        GD.Print("Sesion cerrada del usuario: " + NombreUsuario);
        NombreUsuario = "";
    }

    public bool EstaLogueado()
    {
        return !string.IsNullOrEmpty(NombreUsuario);
    }

}
