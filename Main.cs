using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Main : Node2D
{
	public static Main Singleton;
	private readonly Dictionary<int, Naode> naodes = new();
	public Camera camera;
	private Node2D world;
	private Node2D worldContextMenu;
	private PanelContainer naodeContextMenu;
	private FileDialog fileDialog;
	private FreeArrow arrowPreview;
	public Main()
	{
		Singleton = this;
	}
	public override void _Ready()
	{
		camera = GetNode<Camera>("Camera");
		world = GetNode<Node2D>("World");
		worldContextMenu = GetNode<Node2D>("WorldContextMenu");
		naodeContextMenu = GetNode<PanelContainer>("NaodeContextMenu");
		fileDialog = GetNode<FileDialog>("FileDialog");

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

    private const int N_MINI_STEPS = 8;
	public override void _PhysicsProcess(double db_delta)
	{
		bool accelerate = Input.IsKeyPressed(Key.Shift);
		float delta = (float) db_delta / N_MINI_STEPS;
		for (int i = (accelerate ? 4 : 1) * N_MINI_STEPS; i >= 0; i--)
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
					float x = displace.X;
					float y = displace.Y;
					if (x < - b.Size.X)
					{
						x += b.Size.X;
					}
					else if (x > a.Size.X)
					{
						x -= a.Size.X;
					}
					else
					{
						x = Math.Sign(x);
					}
					if (y < - b.Size.Y)
					{
						y += b.Size.Y;
					}
					else if (y > a.Size.Y)
					{
						y -= a.Size.Y;
					}
					else
					{
						y = Math.Sign(y);
					}
					displace = new Vector2(x, y);
					float mag = displace.Length();
					if (mag == 0.0f)
						continue;
					Vector2 direction = displace.Normalized();
					// repell
					{
						float adj_mag = Math.Max(mag - 100.0f, 20.0f);
						float inv_mag = 1.0f / adj_mag;
						force -= 500000.0f * direction * inv_mag * inv_mag;
					}
					// attract
					if (rand_i == b.Id)
					{
						force += 500.0f * direction;
					}
					// spring
					if (
						a.Chaildren.Contains(b) || 
						b.Chaildren.Contains(a)
					)
					{
						force += 1f * displace;
					}
				}
				a.Velocity += force * (float) delta;
				// sliding friction
				{
					float mag = a.Velocity.Length();
					if (mag != 0.0f)
					{
						Vector2 friction = -(
							accelerate ? 80.0f : 40.0f
						) * a.Velocity / mag;
						a.Velocity += friction * (float) delta;
					}
				}
				// static friction
				if (a.Velocity.Length() / delta < 200.0f)
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
				if (GlobalStates.ArrowChild == a.Id)
					continue;
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
					worldContextMenu.Position = camera.GetWorldCursor();
					break;
			}
		}
		else if (@event is InputEventKey iEK)
		{
			if (iEK.Keycode == Key.Ctrl)
			{
				worldContextMenu.Visible = iEK.Pressed;
				worldContextMenu.Position = camera.GetWorldCursor();
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

	public void NaodeReleaseRMB(int id)
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
			GlobalStates.SelectedId = id;
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

	public void OnClickSave()
	{
		worldContextMenu.Visible = false;
		if (GlobalStates.FileName == null)
		{
			fileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
			fileDialog.PopupCentered();
			return;
		}
		Save();
	}

	public void OnClickOpen()
	{
		worldContextMenu.Visible = false;
		fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		fileDialog.PopupCentered();
	}

	public void OnFileSelected(string path)
	{
		GlobalStates.FileName = ProjectSettings.GlobalizePath(path);
		switch (fileDialog.FileMode)
		{
			case FileDialog.FileModeEnum.SaveFile:
				Save();
				break;
			case FileDialog.FileModeEnum.OpenFile:
				Open();
				break;
			default:
				throw new Shared.FatalError();
		}
	}

	private void Save()
	{
		string file_name = GlobalStates.FileName ?? throw new Shared.FatalError();
        using StreamWriter streamWriter = new(file_name);
		streamWriter.WriteLine("Garaph save file version = ");
		streamWriter.WriteLine(1);
		streamWriter.WriteLine("next id = ");
		streamWriter.WriteLine(GlobalStates.NextId);
		streamWriter.WriteLine("camera zoom = ");
		streamWriter.WriteLine(camera.Zoom.X);
		streamWriter.WriteLine("naodes {");
		foreach (var (id, naode) in naodes)
		{
			streamWriter.WriteLine("id = ");
			streamWriter.WriteLine(id);
			streamWriter.WriteLine("type = ");
			streamWriter.WriteLine(Naode.Type2Str(naode.Type));
			streamWriter.WriteLine("text = ");
			streamWriter.WriteLine(naode.Text);
			Vector2 position = naode.Position - camera.Position;
			streamWriter.WriteLine("position.x = ");
			streamWriter.WriteLine(position.X);
			streamWriter.WriteLine("position.y = ");
			streamWriter.WriteLine(position.Y);
			streamWriter.WriteLine();
		}
		streamWriter.WriteLine("}");
		streamWriter.WriteLine("arrows {");
		foreach (var (id, naode) in naodes)
		{
			streamWriter.WriteLine("paarent = ");
			streamWriter.WriteLine(id);
			streamWriter.WriteLine("chaildren {");
			foreach (Naode chaild in naode.Chaildren)
			{
				streamWriter.WriteLine(chaild.Id);
			}
			streamWriter.WriteLine("}");
		}
		streamWriter.WriteLine("}");
    }

	private void Open()
	{
		string file_name = GlobalStates.FileName ?? throw new Shared.FatalError();
		Reset();
        using StreamReader streamReader = new(file_name);
		void SkipLine()
		{
			streamReader.ReadLine();
		}
		SkipLine();
		Shared.Assert(int.Parse(streamReader.ReadLine()) == 1);
		SkipLine();
		GlobalStates.NextId = int.Parse(streamReader.ReadLine());
		SkipLine();
		camera.Zoom = float.Parse(streamReader.ReadLine()) * Vector2.One; 
		camera.Position = Vector2.Zero;
		SkipLine();
		while (streamReader.ReadLine() != "}")
		{
			int id = int.Parse(streamReader.ReadLine());
			SkipLine();
			Naode.EnumType type = Naode.Str2Type(streamReader.ReadLine());
			SkipLine();
			Naode naode = new(id, type);
			naodes.Add(naode.Id, naode);
			world.AddChild(naode);
			naode.Text = streamReader.ReadLine();
			SkipLine();
			float x = float.Parse(streamReader.ReadLine());
			SkipLine();
			float y = float.Parse(streamReader.ReadLine());
			naode.Position = new Vector2(x, y);
			SkipLine();
		}
		SkipLine();
		while (streamReader.ReadLine() != "}")
		{
			int parent_id = int.Parse(streamReader.ReadLine());
			Naode parent = naodes[parent_id];
			SkipLine();
			while (true)
			{
				string line = streamReader.ReadLine();
				if (line == "}")
					break;
				int child_id = int.Parse(line);
				Naode child = naodes[child_id];
				parent.AddChaild(child);
			}
		}
	}

	private void Reset()
	{
		foreach (Naode naode in naodes.Values.ToArray())
		{
			RemoveNoade(naode);
		}
		GlobalStates.NextId = 0;
		GlobalStates.SelectedId = null;
		GlobalStates.ArrowParent = null;
		GlobalStates.ArrowChild = null;
	}

	public bool IsDialogOpen() => fileDialog.Visible;

	public void OnClickViewUserDataDir()
	{
		worldContextMenu.Visible = false;
		OS.ShellOpen(OS.GetUserDataDir());
	}
}
