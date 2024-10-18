using Godot;
using System;
using System.Collections.Generic;

public partial class Lobby : Node3D
{
	[Export] public PackedScene KartScene { get; set; }	

	public Kart LocalKart { get; private set; }
	public Dictionary<int, Kart> RemoteKarts { get; set; }

	public override void _Ready()
	{
		var track = GlobalManager.Instance.Tracks[GlobalManager.Instance.LoadTrack].Instantiate<Track>();
		AddChild(track);

        RemoteKarts = new Dictionary<int, Kart>();
		LocalKart = GetNode<Kart>("Kart");

		LocalKart.GlobalPosition = track.Spawnpoint.GlobalPosition;

		GlobalManager.Instance.Lobby = this;
	}

	public void CreateNewKartFromUid(int uid, Vector3 pos)
	{
		if (RemoteKarts.ContainsKey(uid)) return;

		var kart = KartScene.Instantiate<Kart>();
		AddChild(kart);

		kart.Local = false;
		kart.Position = pos;
		kart.NetworkData = NetworkManager.Instance.NetworkedPlayers[uid];

		RemoteKarts[uid] = kart;
	}

    public void RemoveKart(int uid)
    {
        RemoteKarts[uid].QueueFree();
		RemoteKarts.Remove(uid);
    }
}
