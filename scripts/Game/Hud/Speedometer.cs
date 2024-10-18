using Godot;
using System;

public partial class Speedometer : Control
{
	public float Speed_MetersPerSecond;

	private Label _label;

	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
	}

	public override void _Process(double delta)
	{
        _label.Text = $"{Math.Round(Speed_MetersPerSecond, 0)} m/s";
    }
}
