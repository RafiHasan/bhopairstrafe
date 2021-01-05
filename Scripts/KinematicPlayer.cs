using Godot;
using System;

public class KinematicPlayer : KinematicBody
{

	[Export] NodePath speedlable;
	[Export] NodePath framelable;

	Label slable=null;
	Label flable=null;

	//MOVEMENT VARIABLES
	public float moveSpeed = 10.0f;
	[Export] public float defaultmoveSpeed = 10.0f;
	public float forcedmoveSpeed = 10.0f;
	public float forcedTime = 0.0f;
	[Export]public float runAcceleration = 14.0f;
	[Export]public float runDeacceleration = 10.0f;
	[Export]public float airAcceleration = 14.0f;
	[Export]public float airDecceleration = 10.0f;
	public float airControl = 0.1f;
	[Export]public float sideStrafeAcceleration = 50.0f;
	[Export]public float sideStrafeSpeed = 1.0f;
	[Export]public float jumpSpeed = 8.0f;

	//CONSTANTS
	[Export]public float friction = 5.25f;
	[Export] public float airfriction = 5.25f;
	[Export]public float gravity = 9.8f;
	//MOUSE
	public float playerRootH = 0;
	//KEYBOARD
	public Vector2 moveDir = Vector2.Zero;
	//JUMP
	[Export]bool holdJumpToBhop = true;
	[Export] bool lockLikeCS = true;
	bool wishJump = false;
	//PLAYER MOVEMENT
	public Vector3 playerVelocity = Vector3.Zero;
	public Vector2 absPlayerVelocity = Vector2.Zero;
	Vector3 playerJumpVelocity = Vector3.Zero;
	Vector3 jumpForward = Vector3.Zero;
	Vector3 jumpRight = Vector3.Zero;




	public override void _Ready()
	{
		slable = GetNode<Label>(speedlable);
		flable = GetNode<Label>(framelable);
	}
	public override void _Input(InputEvent @event)
	{
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

		slable.Text="Speed : "+ ((int)absPlayerVelocity.Length()).ToString();
		flable.Text = "Frame : " + ((int)(1 / delta)).ToString();
		if (forcedTime > 0)
		{
			moveSpeed = forcedmoveSpeed;
			forcedTime -= delta;
		}
		else
			moveSpeed = defaultmoveSpeed;

		absPlayerVelocity.x = playerVelocity.x;
		absPlayerVelocity.y = playerVelocity.z;

		Transform t = Transform;
		Quat newrot = new Quat(new Vector3(0, playerRootH, 0));
		t.basis = new Basis(newrot);
		Transform = t;

		if (holdJumpToBhop)
		{
			if(Input.GetActionStrength("PlayerJump")>0)
				QueueJump(true);
		}
		if (IsOnFloor()||IsOnWall())
			GroundMove(delta);
		else 
			AirMove(delta);
		
	}

	Vector3 upDirection = Vector3.Up;

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		playerVelocity = MoveAndSlide(playerVelocity, upDirection);
	}

	private void SetMovementDir()
	{
		moveDir.y = Input.GetActionStrength("PlayerDown") - Input.GetActionStrength("PlayerUp");
		moveDir.x = Input.GetActionStrength("PlayerRight") - Input.GetActionStrength("PlayerLeft");
		
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
		float accel;

		ApplyFriction(airfriction, delta);
		
		SetMovementDir();

		wishdir = CalculateAirWishDirection(playerJumpVelocity,jumpForward,jumpRight,moveDir);

		float wishspeed = wishdir.Length();
		wishspeed *= moveSpeed;

		float wishspeed2 = wishspeed;
		if (playerVelocity.Dot(wishdir) < 0)
			accel = airDecceleration;
		else
			accel = airAcceleration;
		
		if (moveDir.y == 0 && moveDir.x != 0)
		{
			if (wishspeed > sideStrafeSpeed)
				wishspeed = sideStrafeSpeed;
			accel = sideStrafeAcceleration;
		}

		Accelerate(wishdir, wishspeed, accel,delta);
		//if (airControl > 0)
		//	AirControl(wishdir, wishspeed2, delta);

		playerVelocity.y += -gravity * delta;
	}

	private void AirControl(Vector3 wishdir, float wishspeed,float delta)
	{
		float zspeed;
		float speed;
		float dot;
		float k;
		if (Mathf.Abs(moveDir.y) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
			return;
		zspeed = playerVelocity.y;
		playerVelocity.y = 0;
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
		}

		playerVelocity.x *= speed;
		playerVelocity.y = zspeed; 
		playerVelocity.z *= speed;
	}

	private void GroundMove(float delta)
	{

		if (wishJump && IsOnFloor())
		{
			jumpForward = GlobalTransform.basis.z.Normalized();
			jumpRight = GlobalTransform.basis.x.Normalized();
			playerJumpVelocity = GLobalTOLocal(playerVelocity, jumpForward, jumpRight);

			playerVelocity.y = jumpSpeed;
			wishJump = false;
			return;
		}



		Vector3 wishdir;
		SetMovementDir();
		float fric;

		Vector3 a = GetFloorNormal();
		Vector3 b = Vector3.Up;
		if (Math.Acos(a.Dot(b)) * 180 / 3.1415f < 5 || Math.Acos(a.Dot(b)) * 180 / 3.1415f >= 90)
		{
			fric = 1.0f;
		}
		else
		{
			fric = 1.0f*(float)Math.Acos(a.Dot(b)) * 180 / 3.1415f/90;
		}

		if (moveDir.Length() == 0)
			fric /= 5;

		ApplyFriction(fric, delta);

		

		wishdir = GlobalTransform.basis.z * moveDir.y + GlobalTransform.basis.x * moveDir.x;
		wishdir =wishdir.Normalized();

		var wishspeed = wishdir.Length();
		wishspeed *= moveSpeed;

		Accelerate(wishdir, wishspeed, runAcceleration,delta);

		playerVelocity.y += -gravity * delta;

		

		
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
		accelspeed = accel * delta ;
		if (accelspeed > addspeed)
			accelspeed = addspeed;

		playerVelocity += accelspeed * wishdir;
	}

	Vector3 GLobalTOLocal(Vector3 vec,Vector3 forward,Vector3 right)
	{
		Vector3 newVector = Vector3.Zero;
		newVector.z = vec.Dot(forward);
		newVector.x = vec.Dot(right);
		newVector.y = 0;

		return newVector;
	}

	Vector3 LocalToGlobal(Vector3 vec, Vector3 forward, Vector3 right)
	{
		Vector3 newVector = Vector3.Zero;
		newVector = forward * vec.z + right * vec.x;
		return newVector;
	}


	Vector3 CalculateAirWishDirection(Vector3 vec, Vector3 forward, Vector3 right,Vector2 movement)
	{
		Vector3 wish;

		float forcoef = 0;
		float rightcoef = 0;
		Vector3 tempVect = LocalToGlobal(vec, jumpForward, jumpRight);
		tempVect = GLobalTOLocal(tempVect, GlobalTransform.basis.z, GlobalTransform.basis.x);

		if (tempVect.x < 0)
			rightcoef = -tempVect.x;
		else
			rightcoef = tempVect.x;

		if (tempVect.z < 0)
			forcoef = -tempVect.z;
		else
			forcoef = tempVect.z;

		wish = jumpForward * forcoef * movement.y + jumpRight * rightcoef * movement.x;
		wish = wish.Normalized();

		Vector3 b = GlobalTransform.basis.z;
		Vector3 a = jumpForward;

		float dot = a.x * -b.z + a.z * b.x;


		Vector3 lastforward = jumpForward;

		if (dot > 0 && moveDir.x != 0)
		{
			jumpForward = GlobalTransform.basis.z;
			jumpRight = GlobalTransform.basis.x;
		}
		else if (dot < 0 && moveDir.x != 0)
		{
			jumpForward = -GlobalTransform.basis.z;
			jumpRight = GlobalTransform.basis.x;
		}
		else if(lockLikeCS)
		{
			wish = Vector3.Zero;
		}

		if (lastforward == forward && absPlayerVelocity.Length()<2)
		{
			jumpForward = GlobalTransform.basis.z;
			jumpRight = GlobalTransform.basis.x;
			wish = GlobalTransform.basis.x*moveDir.x;
		}


		return wish;
	}


	public void ForceVelocity(Vector3 velocity,float movespeed,float duration)
	{
		playerVelocity.x = velocity.x;
		playerVelocity.z = velocity.z;
		forcedmoveSpeed = moveSpeed;
		forcedTime = duration;
	}

}
