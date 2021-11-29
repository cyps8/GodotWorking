using Godot;
using System;

public class Player : Node2D
{
    Label username;
    Sprite sprite;
    KinematicBody2D kinBody;
    Camera2D camera;
    float speed;
    float speedMult;
    float timer;
    public static Vector2 position;
    public static bool movementInputs;
    public override void _Ready()
    {
        username = GetChild(0).GetChild<Label>(0);

        sprite = GetChild(0).GetChild<Sprite>(1);

        kinBody = GetChild<KinematicBody2D>(0);

        camera = GetChild(0).GetNode<Camera2D>("Camera");

        camera.Current = true;

        username.Text = SceneManager.username;

        speed = 150;

        timer = 0.05f;
    }

    public override void _PhysicsProcess(float dt)
    {
        movementInputs = !TextBox.focused;

        Vector2 move = new Vector2(0,0);

        if (movementInputs)
        {
            if (Input.IsActionJustPressed("action1"))
            {
                Vector2 rotVec = GetGlobalMousePosition() - kinBody.Position;
                GameManager.NewBullet(kinBody.Position, rotVec.Normalized());

                if(SceneManager.isServer == true)
                {
                    DataManager.Send.ServerNewBullet(kinBody.Position, rotVec.Normalized());
                }
                else
                {
                    DataManager.Send.ClientNewBullet(kinBody.Position ,rotVec.Normalized());
                }
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
            {
                DataManager.Send.ServerMovement(kinBody.Position);
            }
            else
            {
                DataManager.Send.ClientMovement(kinBody.Position);
            }
            timer += 0.05f;
        }

        position = kinBody.Position;
    }
}
