using Godot;
using System;

public partial class Track : Node3D
{
	[Export] public Area3D ResetArea { get; set; }
	[Export] public Marker3D Spawnpoint { get; set; }

	public override void _Ready()
	{
		ResetArea.BodyEntered += (body) =>
		{
			if (body is Kart)
			{
				body.GlobalPosition = Spawnpoint.GlobalPosition;
			}
		};
	}

	public override void _Process(double delta)
	{
	}
}
