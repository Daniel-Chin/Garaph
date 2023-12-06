using Godot;
using System;

public partial class Bautton : Button
{
    private int id;
    private bool is_dragging = false;
    public Bautton(int id)
    {
        this.id = id;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eMB)
        {
            if (
                eMB.ButtonIndex == MouseButton.Right && 
                ! eMB.Pressed
            )
            {
                Main.Singleton.SpawnNoadeMenu(
                    GetViewport().GetMousePosition()
                );
                GlobalStates.SelectedId = id;
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
}
