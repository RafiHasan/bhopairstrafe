using Godot;
using System;

public class Player : Node2D
{

	[Export] NodePath pnode;
	SpecialKinematics special;

	Vector2 forward;
	Vector2 speed;
	Vector2 side;

	
	public override void _Ready()
	{
		special = GetNode<SpecialKinematics>(pnode);
		
	}
	public override void _Process(float delta)
	{
		forward = new Vector2(0,-special.momentum.Length());
		Update();

	}

	public override void _Draw()
	{
		DrawLine(new Vector2(0, 0), forward*3, new Color(255, 0, 0), 25);
	}

}
