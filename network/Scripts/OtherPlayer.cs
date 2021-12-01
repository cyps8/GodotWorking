using Godot;
using System;

public class OtherPlayer : Node2D
{
    Label username;
    Sprite sprite;
    KinematicBody2D kinBody;
    public int id;
    public string myUsername;
    public float healthPoints;
    public Label healthStatus;
    bool isDead;
    Vector2 oldPos;
    Vector2 newPos;

    float lTimer;
    public void Init(int _id, string _username, Vector2 _position, float _hP)
    {
        id = _id;

        myUsername = _username;

        username = GetChild(0).GetNode<Label>("username");

        sprite = GetChild(0).GetNode<Sprite>("sprite");

        kinBody = GetChild<KinematicBody2D>(0);

        healthStatus = GetChild(0).GetNode<Label>("health");

        username.Text = myUsername;

        Position = _position;

        healthPoints = _hP;

        HealthStatus();

        oldPos = Position;
        newPos = Position;
    }

    public void UpdatePos(Vector2 _pos)
    {
        oldPos = Position;
        newPos = _pos;
        lTimer = 0f;
    }

    public void Hurt(bool _spread, float _dmg)
    {
        healthPoints -= _dmg;

        HealthStatus();

        if (healthPoints > 0)
        GetNode<Player>("Player").Hurt();

        if (_spread)
        {
            if(SceneManager.isServer == true)
            DataManager.Send.ServerHurt(_dmg, id);
            else
            DataManager.Send.ClientHurt(_dmg, id);
        }
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

    private void HealthStatus()
    {
        if (healthPoints <= 0)
        {
            healthStatus.Text = "Deceased";
            healthStatus.Modulate = new Color(0.2f, 0.2f, 0.2f);
        }
        else if (healthPoints <= 40)
        {
            healthStatus.Text = "Near Death";
            healthStatus.Modulate = new Color(0.6f, 0, 0.2f);
        }
        else if (healthPoints <= 90)
        {
            healthStatus.Text = "Wounded";
            healthStatus.Modulate = new Color(1, 0, 0);
        }
        else if (healthPoints <= 150)
        {
            healthStatus.Text = "Damaged";
            healthStatus.Modulate = new Color(1, 0.5f, 0);
        }
        else if (healthPoints < 200)
        {
            healthStatus.Text = "Injured";
            healthStatus.Modulate = new Color(1, 1, 0);
        }
        else
        {
            healthStatus.Text = "Healthy";
            healthStatus.Modulate = new Color(0, 1, 0);
        }

        if (healthPoints <= 0)
        {
            isDead = true;
            kinBody.GetNode<CollisionShape2D>("collisionShape").SetDeferred("disabled", true);
            sprite.Modulate = new Color(0.2f, 0.2f, 0.2f);
        }
        else
        {
            isDead = false;
            kinBody.GetNode<CollisionShape2D>("collisionShape").SetDeferred("disabled", false);
            sprite.Modulate = new Color(1, 1, 1);
        }
    }
}