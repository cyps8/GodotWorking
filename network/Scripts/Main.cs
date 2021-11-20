using Godot;
using System;

public class Main : Node2D
{
	public override void _Ready()
	{
		
	}

	private void ButtonHostPressed()
	{
		GetNode<Node>("/root/MasterScene").Call("StartServer");
	}

	private void ButtonClientPressed()
	{
		GetNode<Node>("/root/MasterScene").Call("StartClient");
	}
}
