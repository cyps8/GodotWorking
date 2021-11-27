using Godot;
using System;

public class OtherPlayer : Node2D
{
    Label username;
    Sprite sprite;
    KinematicBody2D kinBody;

    public int id;
    public string myUsername;

    Vector2 oldPos;
    Vector2 newPos;

    float lTimer;
    public void Init(int _id, string _username, Vector2 _position)
    {
        id = _id;

        myUsername = _username;

        username = GetChild(0).GetChild<Label>(0);

        sprite = GetChild(0).GetChild<Sprite>(1);

        kinBody = GetChild<KinematicBody2D>(0);

        username.Text = myUsername;

        Position = _position;

        oldPos = Position;
        newPos = Position;
    }

    public void UpdatePos(Vector2 _pos)
    {
        oldPos = Position;
        newPos = _pos;
        lTimer = 0f;
    }

    public override void _Process(float dt)
    {
        lTimer += dt;
        Position = new Vector2(Mathf.Lerp(oldPos.x, newPos.x, lTimer/0.05f), Mathf.Lerp(oldPos.y, newPos.y, lTimer/0.05f));
    }

    public void Delete()
    {
        QueueFree();
    }
}