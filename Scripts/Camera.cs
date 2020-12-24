using Godot;
using System;

public class Camera : Spatial
{
	[Export] public NodePath path;
	KinematicPlayer player;
	float aaa=0;
	public override void _Ready()
	{
		player = GetNode<KinematicPlayer>(path);
	}


	public override void _Process(float delta)
	{
		aaa += player.roty;
		Transform t = Transform;
		player.roty = 0;
		if (aaa > 3.1415f/2)
			aaa = 3.1415f / 2;
		if (aaa < -3.1415f / 2)
			aaa = -3.1415f / 2;

		Quat newrot = new Quat(new Vector3(aaa, 0, 0));
		t.basis = new Basis(newrot);
		Transform = t;
	}
}
