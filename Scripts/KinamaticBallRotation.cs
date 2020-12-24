using Godot;
using System;

public class KinamaticBallRotation : Spatial
{
    [Export] NodePath player;
    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        BallRotation(delta);
    }

    void BallRotation(float delta)
    {
        KinematicPlayer ballspace = GetNode<KinematicPlayer>(player);


        Transform t = GlobalTransform;
        Quat newrot= t.basis.Quat();
        if (ballspace.playerVelocity.Cross(Vector3.Up).Length()>0)
            newrot = new Quat((ballspace.playerVelocity.Cross(Vector3.Up)).Normalized(), -ballspace.playerVelocity.Length() * delta) * t.basis.Quat();
        t.basis = new Basis(newrot);
        GlobalTransform = t;

    }
}
