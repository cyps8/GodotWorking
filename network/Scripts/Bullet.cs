using Godot;
using System;

public class Bullet : Area2D
{
    float timer;
    public bool toBeRemoved;
    private Vector2 direction;
    public void Init(Vector2 _position, Vector2 _direction)
    {
        Position = _position;
        direction = _direction;

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

    public void Delete()
    {
        QueueFree();
    }
}
