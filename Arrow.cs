using Godot;
using System;

public partial class FreeArrow : Line2D
{
    protected static readonly Gradient GRADIENT;
    static FreeArrow()
    {
        GRADIENT = new Gradient();
        GRADIENT.AddPoint(0f, new Color(1f, 1f, 1f));
        GRADIENT.AddPoint(1f, new Color(0f, 1f, 0f));
    }

    public FreeArrow()
    {
        ZIndex = -1;
        Gradient = GRADIENT;
    }
}
public partial class Arrow : FreeArrow
{
    private Naode parent;
    private Naode child;

    public Arrow(Naode parent, Naode child) : base()
    {
        this.parent = parent;
        this.child = child;

        DefaultColor = Colors.Red;
    }

    public override void _Process(double delta)
    {
        ClearPoints();
        AddPoint(parent.Position);
        AddPoint(child.Position);
        if (
            GlobalStates.ArrowParent == parent.Id &&
            GlobalStates.ArrowChild == child.Id
        )
        {
            Gradient = null;
        }
        else
        {
            Gradient = GRADIENT;
        }
    }
}
