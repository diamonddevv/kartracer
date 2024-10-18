using Godot;
using System;

public partial class KartSelector : Control
{
	[Export] public float ModelSpinSpeed = 20f;

	public int Index = 0;


	private Node3D _renderWorldRoot;
	private Node3D _model;

	private Label _label;
	private Button _left;
	private Button _right;

	public override void _Ready()
	{
		_renderWorldRoot = GetNode<Node3D>("VBoxContainer/CenterContainer/SubViewportContainer/Renderer/RenderWorld");
		_label = GetNode<Label>("VBoxContainer/Label");

		_left = GetNode<Button>("VBoxContainer/HBoxContainer/left");
		_right = GetNode<Button>("VBoxContainer/HBoxContainer/right");

		UpdateModel();

		_left.Pressed  += () => { Index = Mathf.Wrap(Index - 1, 0, GlobalManager.Instance.KartModelScenes.Length); UpdateModel(); };
		_right.Pressed += () => { Index = Mathf.Wrap(Index + 1, 0, GlobalManager.Instance.KartModelScenes.Length); UpdateModel(); };
	}

	public void UpdateModel()
	{
		var rotation = Vector3.Zero;
		if (_model != null)
		{
			rotation = _model.Rotation;
			_renderWorldRoot.RemoveChild(_model);
		}

		var scn = GlobalManager.Instance.KartModelScenes[Index];
		_model = scn.Instantiate<Node3D>();
		_model.Rotation = rotation;
		_renderWorldRoot.AddChild(_model);

		_label.Text = _model.Name.ToString();
		_label.Text = _label.Text.Replace('-', ' ').Capitalize();
	}

	public override void _Process(double delta)
	{
		if (_model != null) _model.Rotate(Vector3.Up, Mathf.DegToRad(ModelSpinSpeed) * (float) delta);
	}
}
