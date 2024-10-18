using Godot;
using System;

public partial class KartSelector : Control
{
	[Export] public float ModelSpinSpeed = 20f;

	private int _index = 0;
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

		_left.Pressed  += () => { _index = Mathf.Wrap(_index - 1, 0, GlobalManager.Instance.KartModelScenes.Length); UpdateModel(); };
		_right.Pressed += () => { _index = Mathf.Wrap(_index + 1, 0, GlobalManager.Instance.KartModelScenes.Length); UpdateModel(); };
	}

	public void UpdateModel()
	{
		if (_model != null)
		{
			_renderWorldRoot.RemoveChild(_model);
		}

		var scn = GlobalManager.Instance.KartModelScenes[_index];
		_model = scn.Instantiate<Node3D>();

		_renderWorldRoot.AddChild(_model);

		_label.Text = _model.Name.ToString().Capitalize();
	}

	public override void _Process(double delta)
	{
		if (_model != null) _model.Rotate(Vector3.Up, Mathf.DegToRad(ModelSpinSpeed) * (float) delta);
	}
}
