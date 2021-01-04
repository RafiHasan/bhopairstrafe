using Godot;
using System;

public class ForceSpeed : Area
{
	[Export]public bool useDirection=false;
	[Export] public bool useMultiplier = false;
	[Export] public float fixedofmultiplyvelocity = 0;
	[Export] public float duration = 0;

	public void OnEnter(Node another)
	{
		if (!another.HasMethod("ForceVelocity"))
			return;
		
		KinematicPlayer player = GetNode<KinematicPlayer>(another.GetPath());

		if (player == null)
			return;

		Vector3 velocity;
		float magnetude;

		if (useDirection)
		{
			velocity = Transform.basis.z; 
		}
		else
		{
			velocity = player.playerVelocity.Normalized();
		}


		if (useMultiplier)
		{
			magnetude = player.playerVelocity.Length() * fixedofmultiplyvelocity;
		}
		else
		{
			magnetude = fixedofmultiplyvelocity;

		}


		player.ForceVelocity(velocity*magnetude, magnetude, duration);
	}
	
}
