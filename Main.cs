using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public static Main Singleton;
	private readonly List<Naode> naodes = new();
	private Camera camera;
	private Node2D world;
	private PanelContainer worldContextMenu;
	private PanelContainer naodeContextMenu;
	public Main()
	{
		Singleton = this;
	}
	public override void _Ready()
	{
		camera = GetNode<Camera>("Camera");
		world = GetNode<Node2D>("World");
		worldContextMenu = GetNode<PanelContainer>("WorldContextMenu");
		naodeContextMenu = GetNode<PanelContainer>("NaodeContextMenu");

		worldContextMenu.Visible = false;
		naodeContextMenu.Visible = false;

		// test
		Naode a = NewNoade(Naode.Type.STATE);
		a.Text = "State A";
		a.Position = new Vector2(0f, 0f);

		Naode b = NewNoade(Naode.Type.PROP);
		b.Text = "Prop B";
		b.Position = new Vector2(100f, 0f);

		GlobalStates.SelectedId = null;
	}

    public override void _Process(double db_delta)
	{
		bool accelerate = Input.IsKeyPressed(Key.Shift);
		float delta = (float) db_delta;
		if (accelerate)
		{
			delta *= 2.0f;
		}
		foreach (Naode a in naodes)
		{
			Vector2 force = Vector2.Zero;
			int rand_i = Shared.Rand.Next(naodes.Count);
			foreach (Naode b in naodes)
			{
				if (a == b)
					continue;
				Vector2 displace = b.Position - a.Position;
				Vector2 direction = displace.Normalized();
				// repell
				{
					float mag = displace.Length() - 200.0f;
					mag = Math.Max(mag, 2.0f);
					float inv_mag = 1.0f / mag;
					force -= 1000000.0f * direction * inv_mag * inv_mag;
				}
				// attract
				if (rand_i == b.id)
				{
					force += 200.0f * direction;
				}
				// spring
				if (
					a.Chaildren.Contains(b) || 
					b.Chaildren.Contains(a)
				)
				{
					force += 100.0f * displace;
				}
			}
			a.Velocity += force * (float) delta;
			// sliding friction
			Vector2 friction = -(
				accelerate ? 40.0f : 20.0f
			) * a.Velocity.Normalized();
			a.Velocity += friction * (float) delta;
			// static friction
			if (a.Velocity.Length() / delta < 100.0f)
			{
				a.Velocity = Vector2.Zero;
			}
			if (! accelerate)
			{
				float v_mag = a.Velocity.Length();
				if (v_mag != 0.0f)
				{
					Vector2 direction = a.Velocity / v_mag;
					v_mag = Math.Min(v_mag, 100.0f);
					a.Velocity = direction * v_mag;
				}
			}
			if (
				GlobalStates.SelectedId is int id &&
				id == a.id
			)
			{
				a.Velocity = Vector2.Zero;
			}
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
					worldContextMenu.Visible = false;
					naodeContextMenu.Visible = false;
					GlobalStates.SelectedId = null;
					break;
				case MouseButton.Right:
					worldContextMenu.Visible = true;
					worldContextMenu.Position = camera.ToWorld(eMB.Position);
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
		naodes.Add(naode);
		world.AddChild(naode);
		naode.Select();
		return naode;
	}

	private void RemoveNoade(Naode naode)
	{
		naodes.Remove(naode);
		naode.QFree();
	}
	
	public void OnClickNewState()
	{
		Naode naode = NewNoade(Naode.Type.STATE);
		naode.Position = worldContextMenu.Position;
		worldContextMenu.Visible = false;
	}

	public void OnClickNewProp()
	{
		Naode naode = NewNoade(Naode.Type.PROP);
		naode.Position = worldContextMenu.Position;
		worldContextMenu.Visible = false;
	}

	public void OnClickNewTag()
	{
		Naode naode = NewNoade(Naode.Type.TAG);
		naode.Position = worldContextMenu.Position;
		worldContextMenu.Visible = false;
	}

	public void OnClickDelete()
	{
		if (GlobalStates.SelectedId is int id)
		{
			Naode naode = naodes.Find(n => n.id == id);
			RemoveNoade(naode);
		}
		naodeContextMenu.Visible = false;
	}

	public void SpawnNoadeMenu(Vector2 screen_position)
	{
		naodeContextMenu.Visible = true;
		naodeContextMenu.Position = camera.ToWorld(screen_position);
	}

	public void DragNaode(int id, Vector2 relative)
	{
		Naode naode = naodes.Find(n => n.id == id);
		naode.Position += relative;
	}
}
