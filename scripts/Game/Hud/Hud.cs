using Godot;
using System;

public partial class Hud : CanvasLayer
{
	private Speedometer _speedo;



	public override void _Ready()
	{
		_speedo = GetNode<Speedometer>("Speedometer");



	}

	public override void _Process(double delta)
	{

        _speedo.Speed_MetersPerSecond = GlobalManager.Instance.LocalPlayer_Speed_MetersPerSecond;
    }
}
