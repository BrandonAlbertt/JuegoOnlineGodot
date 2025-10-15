using Godot;
using System;

public partial class MultiplayerMenu : Control
{
    public override void _Ready()
    {
        GetNode<Button>("CenterContainer/Panel/VBoxContainer/createButton").Pressed += OnCreatePressed;
        GetNode<Button>("CenterContainer/Panel/VBoxContainer/UnirseButton").Pressed += OnUnirsePressed;
        GetNode<Button>("CenterContainer/Panel/VBoxContainer/VolverButton").Pressed += OnVolverPressed;
    }

    private void OnCreatePressed()
    {
        GD.Print("Crear sala");
        GetTree().ChangeSceneToFile("res://Scenes/UI/CreateGame.tscn");
    }

    private void OnUnirsePressed()
    {
        GD.Print("Unirse a sala");
        GetTree().ChangeSceneToFile("res://Scenes/UI/JoinGame.tscn");
    }

    private void OnVolverPressed()
    {
        GD.Print("Volver al menú principal");
        GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
    }
}
