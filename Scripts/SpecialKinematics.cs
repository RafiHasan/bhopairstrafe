using Godot;
using System;

public class SpecialKinematics : KinematicBody
{

	//MOUSE MOVEMENT
	Vector2 mouseSpeedAxis = Vector2.Zero;

	//KEYBOARDMOVEMENT

	//CONSTANTS
	[Export] float speed = 5.0f;
	[Export] float gravity = -9.8f;
	[Export] float drag = 1.0f;
	[Export] float friction = 0.1f;
	[Export] float maxAirVel = 10;
	[Export] float maxGroundVel = 10;
	[Export] float airAccell = 5.0f;
	[Export] float airDeccell = 5.0f;
	[Export] float groundAccell = 4.0f;
	[Export] float groundDeccell = 4.0f;
	//VECTORS
	Vector3 kinematicSpeed = Vector3.Zero;
	public Vector3 momentum = Vector3.Zero;

	

	


	bool firstGround = false;
	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
	}


	public override void _Process(float delta)
	{
		float vertical = Input.GetActionStrength("PlayerDown") - Input.GetActionStrength("PlayerUp");
		float horizontal = Input.GetActionStrength("PlayerRight") - Input.GetActionStrength("PlayerLeft");

		float megahorizontal = Input.GetActionStrength("PlayerRightArrow") - Input.GetActionStrength("PlayerLeftArrow");

		Vector3 direction = Vector3.Zero;

		direction = GlobalTransform.basis.z * vertical * 5+ GlobalTransform.basis.x*horizontal*5.0f;

		kinematicSpeed = direction;


		if (Input.GetActionStrength("PlayerJump") != 0)
		{
			Jump();
		}


		Transform t = Transform;
		Quat newrot = new Quat(new Vector3(0, 0.5f * -mouseSpeedAxis.x / 180 * 3.1415f * delta, 0)) * t.basis.Quat();
		t.basis = new Basis(newrot);
		Transform = t;
		mouseSpeedAxis.x = 0;


		
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
			GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
		else if (@event is InputEventMouseMotion eventMouseMotion)
		{
			mouseSpeedAxis = eventMouseMotion.Relative;
		}
		if (@event is InputEventKey eventKey)
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Escape)
				GetTree().Quit();
	}


	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

		Vector3 velocity;


		if (kinematicSpeed.Length() == 0)
			kinematicSpeed = momentum.Normalized() * -groundDeccell;



		if (IsOnFloor())
			velocity = MoveGround(kinematicSpeed, momentum, friction, maxGroundVel, delta);
		else
			velocity = MoveAir(kinematicSpeed, momentum, maxAirVel, delta);

		velocity.y += gravity * delta;

		momentum = MoveAndSlide(velocity, Vector3.Up);
	
	}

	public Vector3 CalculateDragVector(Vector3 velocity)
	{
		Vector3 dragVector = Vector3.Zero;

		dragVector.x = -drag * 0.001455f * velocity.x * Math.Abs(velocity.x);
		dragVector.y = -drag * 0.001455f * velocity.y * Math.Abs(velocity.y);
		dragVector.z = -drag * 0.001455f * velocity.z * Math.Abs(velocity.z);

		return dragVector;
	}

	public void Jump()
	{

		if(IsOnFloor())
		{
			momentum += new Vector3(0,10,0);
			firstGround = true;
		}
		
	}

	private Vector3 Accelerate(Vector3 accel, Vector3 prevVelocity, float max_velocity,float delta)
	{
		float projVel = prevVelocity.Dot(accel.Normalized());
		float accelVel = accel.Length();

		if (projVel + accelVel > max_velocity)
			accelVel = max_velocity - projVel;

		return prevVelocity + accel.Normalized() * accelVel* delta;
	}

	private Vector3 MoveGround(Vector3 accel, Vector3 prevVelocity, float friction, float maxGroundVelocity, float delta)
	{
		float speed = prevVelocity.Length();
		if (speed != 0 && !firstGround) // To avoid divide by zero errors
		{
			float drop = speed * friction * delta;

			if(speed<1)
				drop= speed  * delta;

			prevVelocity *= Mathf.Max(speed - drop, 0) / speed;
		}
		
		firstGround =false;
		return Accelerate(accel, prevVelocity, maxGroundVelocity, delta);
	}

	private Vector3 MoveAir(Vector3 accel, Vector3 prevVelocity,float maxAirVelocity,float delta)
	{
		return Accelerate(accel, prevVelocity, maxAirVelocity, delta);
	}

}
