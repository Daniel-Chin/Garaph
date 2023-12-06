using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	private List<Naode> naodes = new();
	private Camera camera;
	private PanelContainer contextMenu;
	public override void _Ready()
	{
		camera = GetNode<Camera>("Camera");
		contextMenu = GetNode<PanelContainer>("ContextMenu");

		contextMenu.Visible = false;
	}

    public override void _Process(double delta)
	{
		foreach (Naode a in naodes)
		{
			Vector2 force = Vector2.Zero;
			int rand_i = Shared.Rand.Next(naodes.Count);
			foreach (Naode b in naodes)
			{
				if (a == b)
					continue;
				Vector2 displace = b.Position - a.Position;
				float mag = displace.Length();
				if (mag == 0.0f)
					continue;
				float inv_mag = 1.0f / mag;
				Vector2 normed = displace * inv_mag;
				// repell
				force -= 1.0f * normed * inv_mag * inv_mag;
				// attract
				if (rand_i == b.id)
				{
					force += 1.0f * normed;
				}
				// spring
				if (
					a.Chaildren.Contains(b) || 
					b.Chaildren.Contains(a)
				)
				{
					force += 1.0f * displace;
				}
			}
			a.Velocity += force * (float) delta;
			a.Position += a.Velocity * (float) delta;
		}
	}

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eMB)
		{
			switch (eMB.ButtonIndex)
			{
				case MouseButton.Left:
					contextMenu.Visible = false;
					break;
				case MouseButton.Right:
					contextMenu.Visible = true;
					contextMenu.Position = camera.ToWorld(eMB.Position);
					break;
			}
		}
		base._UnhandledInput(@event);
    }

	private Naode NewNoade(Naode.Type type)
	{
		Naode naode = new(
			GlobalStates.NextId, type
		);
		GlobalStates.NextId ++;
		AddChild(naode);
		naode.Select();
		return naode;
	}
	
	public void OnClickNewState()
	{
		Naode naode = NewNoade(Naode.Type.STATE);
		naode.Position = contextMenu.Position;
		contextMenu.Visible = false;
	}

	public void OnClickNewProp()
	{
		Naode naode = NewNoade(Naode.Type.PROP);
		naode.Position = contextMenu.Position;
		contextMenu.Visible = false;
	}

	public void OnClickNewTag()
	{
		Naode naode = NewNoade(Naode.Type.TAG);
		naode.Position = contextMenu.Position;
		contextMenu.Visible = false;
	}
}
