using Godot;
using System;

public class Camera : Spatial
{
	[Export] public NodePath path;
	KinematicPlayer player;

	float cameraRootH = 0;
	float cameraRootV = 0;
	float cameraMaxV = 30;
	float cameraMinV = -30;
	float playerRoot = 0;
	float senH = 0.2f;
	float senV = 0.2f;
	float accelH = 10;
	float accelV = 10;

	float cameraDistanceZ = 6;
	float cameraDistanceY = 2;
	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
		player = GetNode<KinematicPlayer>(path);
		
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			cameraRootH += -eventMouseMotion.Relative.x / 180 * Mathf.Pi * senH;
			cameraRootV += -eventMouseMotion.Relative.y / 180 * Mathf.Pi * senV;
		}
	}

	public override void _Process(float delta)
	{


		cameraRootV = Mathf.Clamp(cameraRootV, cameraMinV/180*Mathf.Pi, cameraMaxV / 180 * Mathf.Pi);
		playerRoot = Mathf.Lerp(playerRoot, cameraRootH, delta * accelH);
		player.playerRootH = playerRoot;
		Transform t = Transform;
		Quat a = t.basis.Quat();
		Quat b = new Quat(new Vector3(cameraRootV, cameraRootH, 0));

		Quat newrot = a.Slerp(b, delta * accelH);
		t.basis = new Basis(newrot);
		t.origin = player.GlobalTransform.origin + player.GlobalTransform.basis.z * cameraDistanceZ+ player.GlobalTransform.basis.y*cameraDistanceY;
		Transform = t;
		
	}
}
