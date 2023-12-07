using Godot;
using System;

public partial class Bautton : Button
{
    private int id;
    private bool is_dragging = false;
    public Bautton(int id)
    {
        this.id = id;

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eMB)
        {
            if (eMB.ButtonIndex == MouseButton.Right)
            {
                if (eMB.Pressed)
                {
                    GlobalStates.ArrowParent = id;
                    GlobalStates.ArrowChild = null;
                }
                else 
                {
                    Main.Singleton.NaodeReleaseRMB(id);
                }
                GetViewport().SetInputAsHandled();
            }
            else if (
                eMB.ButtonIndex == MouseButton.Left
            )
            {
                is_dragging = eMB.Pressed;
            }
        }
        else if (@event is InputEventMouseMotion eMM)
        {
            if (is_dragging)
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
