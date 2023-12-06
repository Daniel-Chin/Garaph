using Godot;
using System;

public partial class Main : Node2D
{
	private Camera camera;
	private PanelContainer contextMenu;
	public override void _Ready()
	{
		camera = GetNode<Camera>("Camera");
		contextMenu = GetNode<PanelContainer>("ContextMenu");

		contextMenu.Visible = false;
	}

    // public override void _Process(double delta)
	// {
	// }

    public override void _Input(InputEvent @event)
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
		base._Input(@event);
    }

	private Naode NewNoade(Naode.Type type)
	{
		Naode naode = new Naode(
			GlobalStates.NextId, type
		);
		GlobalStates.NextId ++;
		AddChild(naode);
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
