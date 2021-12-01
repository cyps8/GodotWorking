using Godot;
using System;

public class Player : KinematicBody2D
{
    Tween colourtween;

    public override void _Ready()
    {
        colourtween = GetNode<Tween>("ColourTween");
    }

    public void Hurt()
    {
        colourtween.InterpolateProperty(GetNode<Sprite>("sprite"), "modulate", new Color(1, 0, 0), new Color(1, 1, 1), 0.2f, Tween.TransitionType.Cubic, Tween.EaseType.In);
        colourtween.Start();
    }

    public void StopTween()
    {
        colourtween.Stop(GetNode<Sprite>("sprite"), "modulate");
    }
}
