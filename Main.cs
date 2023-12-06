using Godot;
using System;

public partial class Main : Node2D
{
	private Camera camera;
	public override void _Ready()
	{
		camera = GetNode<Camera>("Camera");
	}

    public override void _Process(double delta)
	{
	}
}
