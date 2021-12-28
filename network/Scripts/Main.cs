using Godot;
using System;

public class Main : Node2D
{
	LineEdit nameInput;
	public override void _Ready()
	{
		nameInput = GetNode<LineEdit>("UI/NameInput");
		nameInput.Text = SceneManager.username;
	}

	private void ButtonHostPressed()
	{
		SetUsername();
		GetNode<Node>("/root/MasterScene").Call("GoToGameCreator");
	}

	private void ButtonClientPressed()
	{
		SetUsername();
		GetNode<Node>("/root/MasterScene").Call("GoToGameSelector");
	}

	private void SetUsername()
	{
		if (nameInput.Text != "")
		{
			SceneManager.username = nameInput.Text;
		}
		else
		{
			SceneManager.username = "Default_Name";
		}
		
	}
}
