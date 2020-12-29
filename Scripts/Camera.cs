using Godot;
using System;

public class Camera : Spatial
{
	[Export] public NodePath path;
	KinematicPlayer player;
	float angle=0;
	public override void _Ready()
	{
		player = GetNode<KinematicPlayer>(path);
	}


	public override void _Process(float delta)
	{
		angle += player.roty;
		Transform t = Transform;
		player.roty = 0;
		if (angle > 3.1415f/8)
			angle = 3.1415f / 8;
		if (angle < -3.1415f / 4)
			angle = -3.1415f / 4;

		Quat newrot = new Quat(new Vector3(angle, 0, 0));
		t.basis = new Basis(newrot);
		Transform = t;
	}
}
