using Godot;
using System;

public partial class HostMenu : MenuScreen
{
    [Export] public int MaxUsernameLength;
    [Export] public int DefaultPort = 8080;

    private LineEdit _username;
    private LineEdit _ip;
    private LineEdit _port;
    private OptionButton _mapselect;

    private Button _host;
    private Button _back;

    public override void _Ready()
	{
        _username = GetNode<LineEdit>("CenterContainer/VBoxContainer/username");
        _ip = GetNode<LineEdit>("CenterContainer/VBoxContainer/ip");
        _port = GetNode<LineEdit>("CenterContainer/VBoxContainer/port");
        _mapselect = GetNode<OptionButton>("CenterContainer/VBoxContainer/track");
        _host = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/host");
        _back = GetNode<Button>("CenterContainer/VBoxContainer/HBoxContainer/back");

        _host.Pressed += _host_Pressed;
        _back.Pressed += MenuManager.Instance.GoToMainMenu;
	}

    public override void _Process(double delta)
	{
		base._Process(delta);
        _username.PlaceholderText = "username (max length: " + MaxUsernameLength + " chars)";
        _port.PlaceholderText = "port (default: " + DefaultPort + ")";

        _host.Disabled = _username.Text.Length <= 0 || _ip.Text.Length <= 0 || _mapselect.Selected == -1;
    }



    private void _host_Pressed()
    {
        string username = _username.Text;
        string ip = _ip.Text;
        int port = _port.Text.Length <= 0 ? DefaultPort : int.Parse(_port.Text);
        GlobalManager.Instance.LoadTrack = _mapselect.Selected;

        GetTree().ChangeSceneToPacked(MenuManager.Instance.Main);
    }
}
