using Godot;
using System;

public partial class JoinMenu : MenuScreen
{
    [Export] public int MaxUsernameLength;
    [Export] public int DefaultPort = 8080;

    private LineEdit _username;
	private LineEdit _ip;
	private LineEdit _port;
	private Button _connect;
	private Button _back;

	public override void _Ready()
	{
        _username = GetNode<LineEdit>("CenterContainer/VBoxContainer/username");
        _ip = GetNode<LineEdit>("CenterContainer/VBoxContainer/ip");
        _port = GetNode<LineEdit>("CenterContainer/VBoxContainer/port");
        _connect = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/connect");
        _back = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/back");


        _connect.Pressed += _connect_Pressed;
        _back.Pressed += MenuManager.Instance.GoToMainMenu;


        _username.MaxLength = MaxUsernameLength;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _username.PlaceholderText = "username (max length: " + MaxUsernameLength + " chars)";
        _port.PlaceholderText = "port (default: " + DefaultPort + ")";
        _connect.Disabled = _username.Text.Length <= 0 || _ip.Text.Length <= 0;
    }

    private void _connect_Pressed()
    {
        string username = _username.Text;
        string ip = _ip.Text;
        int port = _port.Text.Length <= 0 ? DefaultPort : int.Parse(_port.Text);

        NetworkManager.Instance.Connect(ip, port);
    }

    
}
