using Godot;
using System;

public class MyPlayer : Node2D
{
    Label username;
    Sprite sprite;
    public static Player kinBody;
    Camera2D camera;
    float speed;
    float speedMult;
    float timer;
    public static float healthPoints;
    public ProgressBar healthBar;
    public Label healthStatus;
    public bool isDead;
    //public static Vector2 position;
    public static bool movementInputs;
    public override void _Ready()
    {
        healthPoints = 200;

        username = GetChild(0).GetNode<Label>("username");

        sprite = GetChild(0).GetNode<Sprite>("sprite");

        kinBody = GetNode<Player>("Player");

        camera = GetChild(0).GetNode<Camera2D>("Camera");

        healthStatus = GetChild(0).GetNode<Label>("health");

        healthBar = GetParent<Node2D>().GetNode<CanvasLayer>("HUD").GetNode<Node>("HealthBar").GetNode<ProgressBar>("HealthBar");

        healthBar.Modulate = new Color(0, 1, 0);

        camera.Current = true;

        username.Text = SceneManager.username;

        healthStatus.Text = "Healthy";
        healthStatus.Modulate = new Color(0, 1, 0);

        isDead = false;

        speed = 150;

        timer = 0.05f;
    }

    public override void _PhysicsProcess(float dt)
    {
        movementInputs = !TextBox.focused;

        Vector2 move = new Vector2(0,0);

        if (movementInputs && !isDead)
        {
            if (Input.IsActionJustPressed("action1"))
            {
                Vector2 rotVec = GetGlobalMousePosition() - kinBody.Position;
                if(SceneManager.isServer == true)
                GameManager.NewBullet(kinBody.Position, rotVec.Normalized(), true, -1);
                else
                GameManager.NewBullet(kinBody.Position, rotVec.Normalized(), true, Client.id);
                

                if(SceneManager.isServer == true)
                DataManager.Send.ServerNewBullet(kinBody.Position, rotVec.Normalized());
                else
                DataManager.Send.ClientNewBullet(kinBody.Position ,rotVec.Normalized());
            }

            if (Input.IsActionPressed("ui_right"))
            {
                move.x += 1;
            }
            if (Input.IsActionPressed("ui_left"))
            {
                move.x -= 1;
            }
            if (Input.IsActionPressed("ui_up"))
            {
                move.y -= 1;
            }
            if (Input.IsActionPressed("ui_down"))
            {
                move.y += 1;
            }

            if (Input.IsActionPressed("sprint"))
            speedMult = 2;
            else
            speedMult = 1;
        }
        
        kinBody.MoveAndSlide(move.Normalized() * speed * speedMult);

        timer -= dt;

        if (timer <= 0)
        {
            if(SceneManager.isServer == true)
            DataManager.Send.ServerMovement(kinBody.Position);
            else
            DataManager.Send.ClientMovement(kinBody.Position);
            timer += 0.05f;
        }
    }

    public void Hurt(float _dmg)
    {
        healthPoints -= _dmg;

        healthBar.Value = healthPoints;

        HealthStatus();

        if (!isDead)
        GetNode<Player>("Player").Hurt();
    }

    public void Respawn(Vector2 _pos)
    {
        healthPoints = 200;

        kinBody.Position = _pos;

        HealthStatus();
    }

    private void HealthStatus()
    {
        if (healthPoints <= 0)
        {
            healthBar.Modulate = new Color(0.2f, 0.2f, 0.2f); // Deceased
            healthStatus.Text = "Deceased";
            healthStatus.Modulate = new Color(0.2f, 0.2f, 0.2f);
        }
        else if (healthPoints <= 40)
        {
            healthBar.Modulate = new Color(0.6f, 0, 0.2f); // Near Death
            healthStatus.Text = "Near Death";
            healthStatus.Modulate = new Color(0.6f, 0, 0.2f);
        }
        else if (healthPoints <= 90)
        {
            healthBar.Modulate = new Color(1, 0, 0); // Wounded
            healthStatus.Text = "Wounded";
            healthStatus.Modulate = new Color(1, 0, 0);
        }
        else if (healthPoints <= 150)
        {
            healthBar.Modulate = new Color(1, 0.5f, 0); // Damaged
            healthStatus.Text = "Damaged";
            healthStatus.Modulate = new Color(1, 0.5f, 0);
        }
        else if (healthPoints < 200)
        {
            healthBar.Modulate = new Color(1, 1, 0); // Injured
            healthStatus.Text = "Injured";
            healthStatus.Modulate = new Color(1, 1, 0);
        }
        else
        {
            healthBar.Modulate = new Color(0, 1, 0); // Healthy
            healthStatus.Text = "Healthy";
            healthStatus.Modulate = new Color(0, 1, 0);
        }

        if (healthPoints <= 0)
        {
            isDead = true;
            kinBody.GetNode<CollisionShape2D>("collisionShape").Disabled = true;
            GetNode<Player>("Player").StopTween();
            sprite.Modulate = new Color(0.2f, 0.2f, 0.2f);
        }
        else
        {
            isDead = false;
            kinBody.GetNode<CollisionShape2D>("collisionShape").Disabled = false;
            sprite.Modulate = new Color(1, 1, 1);
        }
    }
}
