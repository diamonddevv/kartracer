using Godot;
using System;

public partial class MenuManager : Node
{
    public static MenuManager Instance { get; private set; }

    [Export] public PackedScene MainMenu;
    [Export] public PackedScene JoinMenu;
    [Export] public PackedScene HostMenu;
    [Export] public PackedScene Main;

    public bool MenuShaderEnabled = true;

    public override void _Ready()
    {
        Instance = this;
    }

    public void GoToMainMenu()
    {
        GetTree().ChangeSceneToPacked(MainMenu);
    }
}
