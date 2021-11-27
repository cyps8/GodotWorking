using Godot;
using System;

public class Bullet : Area2D
{
    float timer;
    public bool toBeRemoved;
    public void Init(Vector2 _position, float _rotation)
    {
        Position = _position;
        Rotation = _rotation;

        timer = 0;
        toBeRemoved = false;
    }
    public override void _PhysicsProcess(float dt)
    {
        Position += (new Vector2(15, 0));
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
