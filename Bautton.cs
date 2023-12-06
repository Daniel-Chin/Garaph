using Godot;
using System;

public partial class Bautton : Button
{
    private int id;
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
        }
        base._GuiInput(@event);
    }
}
