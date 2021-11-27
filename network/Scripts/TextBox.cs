using Godot;
using System;

public class TextBox : Node
{
    private LineEdit textInput;
    private static RichTextLabel textBox;
    public static bool focused;

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
            focused = false;
            textInput.ReleaseFocus();

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
        TBEnter();
        focused = true;
    }

    public void FocusExit()
    {
        TBExit();
        focused = false;
    }

    public void TBEnter()
    {
        textBox.MarginTop = -200;
        textBox.ScrollActive = true;
    }

    public void TBExit()
    {
        textBox.MarginTop = -100;
        textBox.ScrollToLine(textBox.GetLineCount() - 1);
        textBox.ScrollActive = false;
    }

    public static void AddMsg(string _msg)
    {
        textBox.AddText(_msg + "\n");
    }
}
