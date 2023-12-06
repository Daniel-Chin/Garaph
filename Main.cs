using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	public static Main Singleton;
	private readonly Dictionary<int, Naode> naodes = new();
	private Camera camera;
	private Node2D world;
	private PanelContainer worldContextMenu;
	private PanelContainer naodeContextMenu;
	private FreeArrow arrowPreview;
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

		arrowPreview = new();
		AddChild(arrowPreview);

		// test
		Naode a = NewNoade(Naode.EnumType.STATE);
		a.Text = "State A";
		a.Position = new Vector2(500f, 500f);

		Naode b = NewNoade(Naode.EnumType.PROP);
		b.Text = "Prop B";
		b.Position = new Vector2(100f, 0f) + a.Position;

		Naode c = NewNoade(Naode.EnumType.TAG);
		c.Text = "Tag C";
		c.Position = new Vector2(0f, 100f) + a.Position;

		GlobalStates.SelectedId = null;
	}

    public override void _PhysicsProcess(double db_delta)
	{
		bool accelerate = Input.IsKeyPressed(Key.Shift);
		float delta = (float) db_delta;
		for (int i = accelerate ? 4 : 1; i >= 0; i--)
		{
			foreach (Naode a in naodes.Values)
			{
				Vector2 force = Vector2.Zero;
				int rand_i = Shared.Rand.Next(naodes.Count);
				foreach (Naode b in naodes.Values)
				{
					if (a == b)
						continue;
					Vector2 displace = b.Position - a.Position;
					float mag = displace.Length();
					if (mag == 0.0f)
						continue;
					Vector2 direction = displace.Normalized();
					// repell
					{
						float adj_mag = Math.Max(mag - 200.0f, 20.0f);
						float inv_mag = 1.0f / adj_mag;
						force -= 2000000.0f * direction * inv_mag * inv_mag;
					}
					// attract
					if (rand_i == b.Id)
					{
						force += 200.0f * direction;
					}
					// spring
					if (
						a.Chaildren.Contains(b) || 
						b.Chaildren.Contains(a)
					)
					{
						force += 0.3f * displace;
					}
				}
				a.Velocity += force * (float) delta;
				// sliding friction
				{
					float mag = a.Velocity.Length();
					if (mag != 0.0f)
					{
						Vector2 friction = -(
							accelerate ? 60.0f : 20.0f
						) * a.Velocity / mag;
						a.Velocity += friction * (float) delta;
					}
				}
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
					id == a.Id
				)
				{
					a.Velocity = Vector2.Zero;
				}
			}
			foreach (Naode a in naodes.Values)
			{	// a seperate loop, to make sure all velocities are updated
				a.Position += a.Velocity * (float) delta;
			}
		}
	}

    public override void _Process(double db_delta)
	{
		arrowPreview.ClearPoints();
		if (GlobalStates.ArrowParent is int parent_id)
		{
			Naode parent = naodes[parent_id];
			if (GlobalStates.ArrowChild is int child_id)
			{
				Naode child = naodes[child_id];
				if (
					parent.Chaildren.Contains(child) ||
					child.Chaildren.Contains(parent)
				)
				{
					// don't show, leaving the red visible
				}
				else
				{
					arrowPreview.AddPoint(parent.Position);
					arrowPreview.AddPoint(child .Position);
				}
			}
			else 
			{
				arrowPreview.AddPoint(parent.Position);
				arrowPreview.AddPoint(camera.GetWorldCursor());
			}
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

	private Naode NewNoade(Naode.EnumType type)
	{
		Naode naode = new(
			GlobalStates.NextId, type
		);
		GlobalStates.NextId ++;
		naodes.Add(naode.Id, naode);
		world.AddChild(naode);
		naode.Select();
		return naode;
	}

	private void RemoveNoade(Naode naode)
	{
		Shared.Assert(naodes.Remove(naode.Id));
		naode.QFree();
	}
	
	public void OnClickNewState()
	{
		Naode naode = NewNoade(Naode.EnumType.STATE);
		naode.Position = worldContextMenu.Position;
		worldContextMenu.Visible = false;
	}

	public void OnClickNewProp()
	{
		Naode naode = NewNoade(Naode.EnumType.PROP);
		naode.Position = worldContextMenu.Position;
		worldContextMenu.Visible = false;
	}

	public void OnClickNewTag()
	{
		Naode naode = NewNoade(Naode.EnumType.TAG);
		naode.Position = worldContextMenu.Position;
		worldContextMenu.Visible = false;
	}

	public void OnClickDelete()
	{
		if (GlobalStates.SelectedId is int id)
		{
			Naode naode = naodes[id];
			RemoveNoade(naode);
		}
		naodeContextMenu.Visible = false;
	}

	public void NaodeReleaseRMB()
	{
		if (GlobalStates.ArrowChild is int child_id)
		{
			int parent_id = GlobalStates.ArrowParent ?? throw new Shared.FatalError();
			Naode parent = naodes[parent_id];
			Naode child = naodes[child_id];
			if (parent.Chaildren.Contains(child))
			{
				parent.RemoveChaild(child);
			}
			else if (child.Chaildren.Contains(parent))
			{
				child.RemoveChaild(parent);
			}
			else
			{
				parent.AddChaild(child);
			}
		}
		else
		{
			naodeContextMenu.Visible = true;
			naodeContextMenu.Position = camera.GetWorldCursor();
		}
		GlobalStates.ArrowParent = null;
		GlobalStates.ArrowChild = null;
	}

	public void DragNaode(int id, Vector2 relative)
	{
		Naode naode = naodes[id];
		naode.Position += relative;
	}
}
