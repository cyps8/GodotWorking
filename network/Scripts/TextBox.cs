using Godot;
using System;

public class TextBox : Node
{
    private LineEdit textInput;

    private static RichTextLabel textBox;

    private bool focused;

    public override void _Ready()
    {
        textInput = GetChild<LineEdit>(1);
        textBox = GetChild<RichTextLabel>(2);
        focused = false;
    }
    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_accept") && focused == true && textInput.Text != "")
        {
            string msg = (SceneManager.username + ": " + textInput.Text);
            textInput.Clear();
            textBox.AddText(msg + "\n");

            if(SceneManager.isServer == true)
            {
                DataManager.Send.ServerChatMsg(msg);
            }
            else
            {
                DataManager.Send.ClientChatMsg(msg);
            }
        }
    }

    public void FocusEnter()
    {
        focused = true;
    }

    public void FocusExit()
    {
        focused = false;
    }

    public static void AddMsg(string _msg)
    {
        textBox.AddText(_msg + "\n");
    }
}
