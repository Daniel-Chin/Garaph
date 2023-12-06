using Godot;
using System;

public partial class Camera : Camera2D
{
	private const float MOVE_SPEED = 500f;
	private const float SCROLL_SPEED = 1.05f;

	private bool is_dragging = false;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Move(delta);
	}

	private void Move (double delta)
	{
		if (GlobalStates.SelectedId != null)
			return;
		
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
					ZoomBy(SCROLL_SPEED);
					GetViewport().SetInputAsHandled();
					break;
				case MouseButton.WheelDown:
					ZoomBy(1f / SCROLL_SPEED);
					GetViewport().SetInputAsHandled();
					break;
				case MouseButton.Middle:
					is_dragging = eMB.Pressed;
					break;
			}
		}
		else if (@event is InputEventMouseMotion eMM)
		{
			if (is_dragging)
			{
				Position -= eMM.Relative / Zoom;
				GetViewport().SetInputAsHandled();
			}
		}
		base._UnhandledInput(@event);
    }

	private void ZoomBy(float scale)
	{
		Vector2 screen_cursor = GetViewport().GetMousePosition();
		Vector2 cursor = ToWorld(screen_cursor);
		Zoom *= scale;
		Position = ToWorld(FromWorld(cursor) - screen_cursor);
	}

	public Vector2 ToWorld(Vector2 screen)
	{
		return screen / Zoom + Position;
	}

	public Vector2 FromWorld(Vector2 world)
	{
		return (world - Position) * Zoom;
	}

	public Vector2 GetWorldCursor() => ToWorld(GetViewport().GetMousePosition());
}
