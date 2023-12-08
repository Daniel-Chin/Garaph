using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Main : Node2D
{
	public static Main Singleton;
	public readonly Dictionary<int, Naode> Naodes = new();
	public Camera camera;
	private Node2D ground;
	private Node2D groundContextMenu;
	public PanelContainer NaodeContextMenu;
	private FileDialog fileDialog;
	private FreeArrow arrowPreview;
	public Label ChangesUnsaved;
	private ConfirmationDialog DiscardUnsavedDialog;
	public Main()
	{
		Singleton = this;
	}
	public override void _Ready()
	{
		camera = GetNode<Camera>("SubViewportContainer/World/Camera");
		ground = GetNode<Node2D>("SubViewportContainer/World/Ground");
		groundContextMenu = GetNode<Node2D>("SubViewportContainer/World/GroundContextMenu");
		NaodeContextMenu = GetNode<PanelContainer>("SubViewportContainer/World/NaodeContextMenu");
		fileDialog = GetNode<FileDialog>("FileDialog");
		ChangesUnsaved = GetNode<Label>("ChangesUnsaved");
		DiscardUnsavedDialog = GetNode<ConfirmationDialog>("DiscardUnsavedDialog");

		groundContextMenu.Visible = false;
		NaodeContextMenu.Visible = false;
		ChangesUnsaved.Visible = false;
		fileDialog.CurrentDir = OS.GetUserDataDir();

		arrowPreview = new();
		ground.AddChild(arrowPreview);

		// test
		// Naode a = NewNoade(Naode.EnumType.STATE);
		// a.Text = "State A";
		// a.Position = new Vector2(500f, 500f);

		// Naode b = NewNoade(Naode.EnumType.PROP);
		// b.Text = "Prop B";
		// b.Position = new Vector2(100f, 0f) + a.Position;

		// Naode c = NewNoade(Naode.EnumType.TAG);
		// c.Text = "Tag C";
		// c.Position = new Vector2(0f, 100f) + a.Position;

		// GlobalStates.SelectedId = null;
	}

    private const int N_MINI_STEPS = 8;
	public override void _PhysicsProcess(double db_delta)
	{
		bool accelerate = Input.IsKeyPressed(Key.Shift);
		float delta = (float) db_delta / N_MINI_STEPS;
		for (int i = (accelerate ? 4 : 1) * N_MINI_STEPS; i >= 0; i--)
		{
			Vector2 center = Vector2.Zero;
			foreach (Naode a in Naodes.Values)
			{
				center += a.Position;
			}
			center /= Naodes.Count;
			Vector2 total_attract = Vector2.Zero;
			foreach (Naode a in Naodes.Values)
			{
				a.Force = Vector2.Zero;
				// attract
				Vector2 attract = 500.0f * (center - a.Position).Normalized();
				total_attract += a.Force;
				a.Force += attract;
				foreach (Naode b in Naodes.Values)
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
						a.Force -= 500000.0f * direction * inv_mag * inv_mag;
					}
					// spring
					if (
						a.Chaildren.Contains(b) || 
						b.Chaildren.Contains(a)
					)
					{
						a.Force += 1f * displace;
					}
				}
			}
			if (total_attract != Vector2.Zero)
			{	// make sure attraction forces are balanced
				Naodes.Values.First().Force -= total_attract;
			}
			foreach (Naode a in Naodes.Values)
			{
				a.Velocity += a.Force * (float) delta;
				// sliding friction
				{
					float mag = a.Velocity.Length();
					if (mag != 0.0f)
					{
						Vector2 friction = -(
							accelerate ? 80.0f : 80.0f
						) * a.Velocity / mag;
						a.Velocity += friction * (float) delta;
					}
				}
				// static friction
				if (a.Velocity.Length() / delta < 1.0f)
				{
					a.Velocity = Vector2.Zero;
				}

				// velocity cap
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
			foreach (Naode a in Naodes.Values)
			{
				if (
					a.Id == GlobalStates.ArrowChild ||
					a.Id == GlobalStates.DraggedId
				)
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
			Naode parent = Naodes[parent_id];
			if (GlobalStates.ArrowChild is int child_id)
			{
				Naode child = Naodes[child_id];
				if (
					parent.Chaildren.Contains(child) ||
					child.Chaildren.Contains(parent)
				)
				{
					// don't show, leaving the red visible
				}
				else
				{
					arrowPreview.AddPoint(parent.Center());
					arrowPreview.AddPoint(child .Center());
				}
			}
			else 
			{
				arrowPreview.AddPoint(parent.Center());
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
					if (eMB.Pressed)
					{
						groundContextMenu.Visible = false;
						NaodeContextMenu.Visible = false;
						GlobalStates.SelectedId = null;
					}
					break;
				case MouseButton.Right:
					if (! eMB.Pressed)
					{
						groundContextMenu.Visible = true;
						groundContextMenu.Position = camera.GetWorldCursor();
					}
					break;
			}
		}
		else if (@event is InputEventKey iEK)
		{
			if (iEK.Keycode == Key.Ctrl)
			{
				groundContextMenu.Visible = iEK.Pressed;
				groundContextMenu.Position = camera.GetWorldCursor();
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
		Naodes.Add(naode.Id, naode);
		ground.AddChild(naode);
		naode.Select();
		ChangesUnsaved.Visible = true;
		return naode;
	}

	private void RemoveNoade(Naode naode)
	{
		Shared.Assert(Naodes.Remove(naode.Id));
		naode.QFree();
	}
	
	public void OnClickNewState()
	{
		Naode naode = NewNoade(Naode.EnumType.STATE);
		naode.Position = groundContextMenu.Position;
		groundContextMenu.Visible = false;
	}

	public void OnClickNewProp()
	{
		Naode naode = NewNoade(Naode.EnumType.PROP);
		naode.Position = groundContextMenu.Position;
		groundContextMenu.Visible = false;
	}

	public void OnClickNewTag()
	{
		Naode naode = NewNoade(Naode.EnumType.TAG);
		naode.Position = groundContextMenu.Position;
		groundContextMenu.Visible = false;
	}

	public void OnClickDelete()
	{
		if (GlobalStates.SelectedId is int id)
		{
			Naode naode = Naodes[id];
			RemoveNoade(naode);
			ChangesUnsaved.Visible = true;
		}
		NaodeContextMenu.Visible = false;
	}

	public void NaodeReleaseRMB(int id, bool is_click)
	{
		if (GlobalStates.ArrowChild is int child_id)
		{
			int parent_id = GlobalStates.ArrowParent ?? throw new Shared.FatalError();
			Naode parent = Naodes[parent_id];
			Naode child = Naodes[child_id];
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
			ChangesUnsaved.Visible = true;
		}
		else if (is_click)
		{
			GlobalStates.SelectedId = id;
			NaodeContextMenu.Visible = true;
			NaodeContextMenu.Position = camera.GetWorldCursor();
		}
		GlobalStates.ArrowParent = null;
		GlobalStates.ArrowChild = null;
	}

	public void DragNaode(int id, Vector2 relative)
	{
		Naode naode = Naodes[id];
		naode.Position += relative;
	}

	public void OnClickSave()
	{
		groundContextMenu.Visible = false;
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
		groundContextMenu.Visible = false;
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
		foreach (var (id, naode) in Naodes)
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
		foreach (var (id, naode) in Naodes)
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
		ChangesUnsaved.Visible = false;
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
			Naodes.Add(naode.Id, naode);
			ground.AddChild(naode);
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
			Naode parent = Naodes[parent_id];
			SkipLine();
			while (true)
			{
				string line = streamReader.ReadLine();
				if (line == "}")
					break;
				int child_id = int.Parse(line);
				Naode child = Naodes[child_id];
				parent.AddChaild(child);
			}
		}
		ChangesUnsaved.Visible = false;
	}

	private void Reset()
	{
		foreach (Naode naode in Naodes.Values.ToArray())
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
		groundContextMenu.Visible = false;
		OS.ShellOpen(OS.GetUserDataDir());
	}

	public void OnClickNew()
	{
		groundContextMenu.Visible = false;
		if (ChangesUnsaved.Visible)
		{
			DiscardUnsavedDialog.PopupCentered();
			return;
		}
		New();
	}

	private void New()
	{
		Reset();
		GlobalStates.FileName = null;
		ChangesUnsaved.Visible = false;
	}

	public void OnDiscardUnsavedDialogConfirmed()
	{
		New();
	}
}
