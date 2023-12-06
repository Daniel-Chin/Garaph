using Godot;
using System;

public partial class Camera : Camera2D
{
	private const float MOVE_SPEED = 100f;
	private const float SCROLL_SPEED = 1.1f;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Vector2 velocity = Vector2.Zero;
		if (Input.IsKeyPressed(Key.A))
		{
			velocity += new Vector2(-MOVE_SPEED, 0f);
		}
		if (Input.IsKeyPressed(Key.D))
		{
			velocity += new Vector2(+MOVE_SPEED, 0f);
		}
		if (Input.IsKeyPressed(Key.W))
		{
			velocity += new Vector2(0f, -MOVE_SPEED);
		}
		if (Input.IsKeyPressed(Key.S))
		{
			velocity += new Vector2(0f, +MOVE_SPEED);
		}
		Position += velocity / Zoom * (float) delta;
	}

    public override void _UnhandledInput(InputEvent @event)
    {
		if (@event is InputEventMouseButton eMB)
		{
			switch (eMB.ButtonIndex)
			{
				case MouseButton.WheelUp:
					Zoom *= SCROLL_SPEED;
					GetViewport().SetInputAsHandled();
					break;
				case MouseButton.WheelDown:
					Zoom /= SCROLL_SPEED;
					GetViewport().SetInputAsHandled();
					break;
			}
		}
		base._UnhandledInput(@event);
    }
}
