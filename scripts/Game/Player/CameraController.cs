using Godot;
using System;

public partial class CameraController : Camera3D
{

	// taken from kidscancode.org, translated into C# (modified)

	[Export] public Kart Target { get; set; }
	[Export] public Vector3 TranslationOffset { get; set; }
	[Export] public float LerpSpeed { get; set; } = 3.0f;

	public override void _PhysicsProcess(double delta)
	{
		if (Target == null) return;

		var targetTrans = Target.GetModelGlobalTransform().TranslatedLocal(TranslationOffset);
		GlobalTransform = GlobalTransform.InterpolateWith(targetTrans, LerpSpeed * (float)delta);


		LookAt(Target.GlobalPosition);
	}
}
