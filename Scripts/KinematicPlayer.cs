using Godot;
using System;

public class KinematicPlayer : KinematicBody
{

	

	public Transform playerView;     
	public float playerViewYOffset = 0.6f; 
	public float xMouseSensitivity = 30.0f;
	public float yMouseSensitivity = 30.0f;

	//GODOT DECLARATIONS

	//MOVEMENT VARIABLES
	[Export]public float moveSpeed = 10.0f;
	public float runAcceleration = 14.0f;
	public float runDeacceleration = 10.0f;
	public float airAcceleration = 14.0f;
	public float airDecceleration = 10.0f;
	public float airControl = 0.1f;
	public float sideStrafeAcceleration = 50.0f;
	public float sideStrafeSpeed = 1.0f;
	public float jumpSpeed = 8.0f;

	//CONSTANTS
	[Export]public float friction = 5.25f;     //force
	[Export]public float accelFriction = 0.0f; //limit 1 to 0
	[Export]public float gravity = 9.8f;
	//MOUSE
	float mouseSensitivity = 0.1f;
	public float rotx = 0;
	public float roty = 0;
	//KEYBOARD
	public Vector2 moveDir = Vector2.Zero;
	//JUMP
	bool holdJumpToBhop = true;
	bool wishJump = false;
	//PLAYER MOVEMENT
	public Vector3 playerVelocity = Vector3.Zero;
	public Vector3 playerlastVelocity = Vector3.Zero;
	Vector3 moveDirectionNorm = Vector3.Zero;


	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
		
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
			GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
		else if (@event is InputEventMouseMotion eventMouseMotion)
		{
			rotx = -eventMouseMotion.Relative.x / 180 * 3.1415f * mouseSensitivity;
			roty= -eventMouseMotion.Relative.y / 180 * 3.1415f * mouseSensitivity;
		}
		if (@event is InputEventKey eventKey)
		{

			if (eventKey.Pressed)
			{
				if (eventKey.Scancode == (int)KeyList.Escape)
					GetTree().Quit();

				if (eventKey.Scancode == (int)KeyList.Space)
					QueueJump(true);

			}
			else
			{
				if (eventKey.Scancode == (int)KeyList.Space)
					QueueJump(false);
			}

		}
			
	}

	public override void _Process(float delta)
	{
		//Mouse Movement
		Transform t = Transform;
		Quat newrot = new Quat(new Vector3(0, rotx, 0)) * t.basis.Quat();
		t.basis = new Basis(newrot);
		Transform = t;
		rotx = 0;

		if (holdJumpToBhop)
		{
			if(Input.GetActionStrength("PlayerJump")>0)
				QueueJump(true);
		}

		//Player Movement
		if (IsOnFloor())
			GroundMove(delta);
		else 
			AirMove(delta);
		//GD.Print(playerVelocity.Length());
	}

	Vector3 upDirection = Vector3.Up;

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		playerlastVelocity = playerVelocity;

		playerVelocity = MoveAndSlide(playerVelocity, upDirection);


		if (IsOnFloor())
		{
			//upDirection = GetFloorNormal();
		}
		else
		{
			//upDirection = Vector3.Up;
		}

	}

	private void SetMovementDir()
	{
		moveDir.y = Input.GetActionStrength("PlayerDown") - Input.GetActionStrength("PlayerUp");
		moveDir.x = Input.GetActionStrength("PlayerRight") - Input.GetActionStrength("PlayerLeft");
		moveDir.x *= 0.5f;
	}

	private void QueueJump(bool pressed)
	{
		if (holdJumpToBhop)
		{
			wishJump = pressed;
			return;
		}

		if (pressed && !wishJump)
			wishJump = true;
		if (!pressed)
			wishJump = false;
	}

	private void AirMove(float delta)
	{
		Vector3 wishdir;
		float wishvel = airAcceleration;
		float accel;

		SetMovementDir();
		ApplyFriction(0.025f, delta);
		wishdir = GlobalTransform.basis.z* moveDir.y + GlobalTransform.basis.x * moveDir.x;
		//wishdir = transform.TransformDirection(wishdir);

		float wishspeed = wishdir.Length();
		wishspeed *= moveSpeed;

		wishdir=wishdir.Normalized();
		moveDirectionNorm = wishdir;

		// CPM: Aircontrol
		float wishspeed2 = wishspeed;
		if (playerVelocity.Dot(wishdir) < 0)
			accel = airDecceleration;
		else
			accel = airAcceleration;
		
		if (moveDir.x == 0 && moveDir.y != 0)
		{
			if (wishspeed > sideStrafeSpeed)
				wishspeed = sideStrafeSpeed;
			accel = sideStrafeAcceleration;
		}

		Accelerate(wishdir, wishspeed, accel,delta);
		if (airControl > 0)
			AirControl(wishdir, wishspeed2, delta);

		playerVelocity.y-= gravity * delta;
	}

	private void AirControl(Vector3 wishdir, float wishspeed,float delta)
	{
		float zspeed;
		float speed;
		float dot;
		float k;

		
		// Can't control movement if not moving forward or backward
		if (Mathf.Abs(moveDir.y) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
			return;
		zspeed = playerVelocity.y;
		playerVelocity.y = 0;
		/* Next two lines are equivalent to idTech's VectorNormalize() */
		speed = playerVelocity.Length();
		playerVelocity=playerVelocity.Normalized();

		dot = playerVelocity.Dot(wishdir);
		k = 32;
		k *= airControl * dot * dot * delta;
		if (dot > 0)
		{
			playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
			playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
			playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

			playerVelocity = playerVelocity.Normalized();
			moveDirectionNorm = playerVelocity;
		}

		playerVelocity.x *= speed;
		playerVelocity.y = zspeed; 
		playerVelocity.z *= speed;
	}

	private void GroundMove(float delta)
	{
		Vector3 wishdir;

		if (!wishJump && moveDir.Length()>0)
			ApplyFriction(1.0f, delta);
		else if(!wishJump)
			ApplyFriction(0.01f, delta);
		else
			ApplyFriction(0, delta);

		SetMovementDir();

		wishdir = GlobalTransform.basis.z * moveDir.y + GlobalTransform.basis.x * moveDir.x;
		wishdir =wishdir.Normalized();
		moveDirectionNorm = wishdir;

		var wishspeed = wishdir.Length();
		wishspeed *= moveSpeed;

		Accelerate(wishdir, wishspeed, runAcceleration,delta);

		playerVelocity.y = -gravity * delta;

		if (wishJump)
		{
			playerVelocity.y = jumpSpeed;
			wishJump = false;
		}
	}
	private void ApplyFriction(float t,float delta)
	{
		Vector3 vec = playerVelocity; 
		float speed;
		float newspeed;
		float control;
		float drop;

		vec.y = 0.0f;
		speed = vec.Length();
		drop = 0.0f;

		/* Only if the player is on the ground then apply friction */
		//if (IsOnFloor())
		{
			control = speed < runDeacceleration ? runDeacceleration : speed;
			drop = control * friction * delta * t;
		}

		newspeed = speed - drop;
		if (newspeed < 0)
			newspeed = 0;
		if (speed > 0)
			newspeed /= speed;

		playerVelocity.x *= newspeed;
		playerVelocity.z *= newspeed;
	}

	private void Accelerate(Vector3 wishdir, float wishspeed, float accel,float delta)
	{
		float addspeed;
		float accelspeed;
		float currentspeed;

		currentspeed = playerVelocity.Dot(wishdir);
		addspeed = wishspeed - currentspeed;
		if (addspeed <= 0)
			return;
		accelspeed = accel * delta * wishspeed;
		if (accelspeed > addspeed)
			accelspeed = addspeed;

		playerVelocity.x += accelspeed * wishdir.x;
		playerVelocity.z += accelspeed * wishdir.z;
	}

}
