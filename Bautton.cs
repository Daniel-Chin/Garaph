using Godot;
using System;

public partial class Bautton : Button
{
    private int id;
    private Vector2 drag_start = Vector2.Zero;
    public Bautton(int id)
    {
        this.id = id;

        // FocusMode = FocusModeEnum.None;

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        MouseFilter = MouseFilterEnum.Pass;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eMB)
        {
            Vector2 world_cursor = Main.Singleton.camera.GetWorldCursor();
            if (eMB.ButtonIndex == MouseButton.Right)
            {
                if (eMB.Pressed)
                {
                    GlobalStates.ArrowParent = id;
                    GlobalStates.ArrowChild = null;
                    drag_start = world_cursor;
                }
                else 
                {
                    Main.Singleton.NaodeReleaseRMB(
                        id, drag_start.DistanceTo(world_cursor) < 10
                    );
                }
                GetViewport().SetInputAsHandled();
            }
            else if (
                eMB.ButtonIndex == MouseButton.Left
            )
            {
                if (eMB.Pressed)
                {
                    GlobalStates.DraggedId = id;
                    drag_start = world_cursor;
                }
                else
                {
                    GlobalStates.DraggedId = null;
                    // release the focus. Somehow ReleaseFocus() would leave the button black??
                    Visible = false;
                    Visible = true;
                    if (drag_start.DistanceTo(world_cursor) < 10)
                    {
                        Main.Singleton.Naodes[id].Select();
                    }
                }
            }
        }
        else if (@event is InputEventMouseMotion eMM)
        {
            if (GlobalStates.DraggedId == id)
            {
                Main.Singleton.DragNaode(id, eMM.Relative);
                GetViewport().SetInputAsHandled();
            }
        }
        base._GuiInput(@event);
    }

    private void OnMouseEntered()
    {
        if (
            GlobalStates.ArrowParent is int parent_id
            && parent_id != id
        )
        {
            GlobalStates.ArrowChild = id;
        }
    }

    private void OnMouseExited()
    {
        if (
            GlobalStates.ArrowChild is int child_id
            && child_id == id
        )
        {
            GlobalStates.ArrowChild = null;
        }
    }
}
