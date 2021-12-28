using Godot;
using System;

public class Bullet : Area2D
{
    float timer;
    public bool toBeRemoved;
    private Vector2 direction;
    public bool isMine;
    public int owner;
    public void Init(Vector2 _position, Vector2 _direction, bool _isMine, int _owner)
    {
        Position = _position + new Vector2(0, 18);
        direction = _direction;
        isMine = _isMine;
        owner = _owner;

        LookAt(Position + direction);
        RotationDegrees += 90;

        timer = 0;
        toBeRemoved = false;
    }
    public override void _PhysicsProcess(float dt)
    {
        Position += direction * dt * 700;
        timer += dt;

        if (timer > 1)
        {
            toBeRemoved = true;
        }
    }

    public void Collision(Node body)
    {
        if (body.IsInGroup("Environment"))
        {
            toBeRemoved = true;
        }
        else if (body.IsInGroup("OtherPlayers"))
        {
            OtherPlayer oP = body.GetParent<OtherPlayer>();
            if (owner != oP.id)
            {
                toBeRemoved = true;
                if (isMine)
                oP.Hurt(true, 20f);
            }
        }
        else if (!isMine)
        {
            toBeRemoved = true;
        }
    }

    public void Delete()
    {
        QueueFree();
    }
}