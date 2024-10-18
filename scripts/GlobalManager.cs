using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalManager : Node
{
    [Export] public PackedScene[] KartModelScenes = null;

    public Dictionary<int, PackedScene> Tracks { get; private set; }

    public static GlobalManager Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;

        Tracks = new()
        {
            { 0, ResourceLoader.Load<PackedScene>("res://prefabs/game/track/mk/mario_gc_ds.tscn") },
            { 1, ResourceLoader.Load<PackedScene>("res://prefabs/game/track/mk/sky_garden_ds.tscn") }
        };
    }
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.ConnectedToServer)
            {
                NetworkManager.Instance.Disconnect();
            }
        }
    }

    public int LoadTrack { get; set; }

    public Lobby Lobby { get; set; }
    public float LocalPlayer_Speed_MetersPerSecond { get; set; }
    public Chat Chat { get; set; }
}
