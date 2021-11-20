using Godot;
using System;

public class GameClient : Node2D
{
    public override void _Ready()
    {
        
    }

    private void ButtonLeave()
    {
        GetNode<Node>("/root/MasterScene").Call("GoToMenu");
    }
}
