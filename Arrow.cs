using Godot;
using System;

public partial class Arrow : Line2D
{
    private static readonly Gradient GRADIENT;
    static Arrow()
    {
        GRADIENT = new Gradient();
        GRADIENT.AddPoint(0f, new Color(1f, 1f, 1f));
        GRADIENT.AddPoint(1f, new Color(0f, 1f, 0f));
    }
    
    private Naode parent;
    private Naode child;

    public Arrow(Naode parent, Naode child)
    {
        this.parent = parent;
        this.child = child;
    }

    public override void _Ready()
    {
        AddPoint(parent.Position);
        AddPoint(child.Position);
        Gradient = GRADIENT;
    }

    public override void _Process(double delta)
    {
        Points[0] = parent.Position;
        Points[1] = child.Position;
    }
}
