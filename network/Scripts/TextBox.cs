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
        if (Input.IsActionJustPressed("ui_accept") && focused == false)
        {
            focused = true;
            textInput.GrabFocus();
        }
        else if (Input.IsActionJustPressed("ui_accept") && focused == true && textInput.Text == "")
        {
            focused = false;
            textInput.ReleaseFocus();
        }
        else if (Input.IsActionJustPressed("ui_accept") && focused == true && textInput.Text != "")
        {
            focused = false;
            textInput.ReleaseFocus();

            textBox.PushColor(new Color(1, 1, 1));
            textBox.AddText(SceneManager.username + ": ");

            textBox.PushColor(new Color(0.8f, 0.8f, 0.8f));
            string msg = (textInput.Text);

            int msgType = 0;

            if (msg.StartsWith("/shout ") || msg.StartsWith("/s "))
            {
                msgType = 1;
                if (msg.StartsWith("/s "))
                msg = msg.Remove(0, 3);
                else
                msg = msg.Remove(0, 7);
                msg = msg.ToUpper();
                textBox.PushBold();
                textBox.PushColor(new Color(0.9f, 0.9f, 0.9f));
            }

            if (msg.StartsWith("/whisper ") || msg.StartsWith("/w "))
            {
                msgType = 2;
                if (msg.StartsWith("/w "))
                msg = msg.Remove(0, 3);
                else
                msg = msg.Remove(0, 9);
                msg = msg.ToLower();
                textBox.PushItalics();
                textBox.PushColor(new Color(0.7f, 0.7f, 0.7f));
            }

            textInput.Clear();
            textBox.AddText(msg + "\n");

            textBox.PushNormal();

            if(SceneManager.isServer == true)
            {
                DataManager.Send.ServerChatMsg(msg, msgType);
            }
            else
            {
                DataManager.Send.ClientChatMsg(msg, msgType);
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

    public static void AddMsg(int _from, string _msg, int _msgType)
    {
        string name = GameManager.GetPlayerIdInfo(_from).username;

        bool inRange = false;
        float distance = Mathf.Abs((MyPlayer.kinBody.Position - GameManager.GetPlayerIdInfo(_from).position).Length());
        Color pushColor = new Color(1,1,1);

        if (_msgType == 0)
        {
            if (distance < 800f)
            inRange = true;
            pushColor = (new Color(0.8f, 0.8f, 0.8f));
        }
        if (_msgType == 1)
        {
            if (distance < 2000f)
            inRange = true;
            textBox.PushBold();
            pushColor = (new Color(0.9f, 0.9f, 0.9f));
        }
        else if (_msgType == 2)
        {
            if (distance < 300f)
            inRange = true;
            textBox.PushItalics();
            pushColor = (new Color(0.7f, 0.7f, 0.7f));
        }

        if (inRange)
        {
            textBox.PushColor(new Color(1, 1, 1));
            textBox.AddText(name + ": "); 

            textBox.PushColor(pushColor);
            textBox.AddText(_msg + "\n");
        }

        textBox.PushNormal();
    }

    public static void PlayerConnected(string _name)
    {
        textBox.PushColor(new Color(1, 1, 0));
        textBox.AddText(_name + " has joined the server.\n");
    }

    public static void PlayerDisconnected(int _who)
    {
        string name = GameManager.GetPlayerIdInfo(_who).username;

        textBox.PushColor(new Color(1, 1, 0));
        textBox.AddText(name + " has left the server.\n");
    }
}
