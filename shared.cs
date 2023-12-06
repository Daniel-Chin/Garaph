using System;
using Godot;

#pragma warning disable CA1050 // Declare types in namespaces

public static partial class Shared
{
    public class AssertionFailed : Exception
    {
        public AssertionFailed(string message) : base(message) { }
    }

    public static void Assert(bool x, string message = "")
    {
        if (!x)
            throw new AssertionFailed(message);
    }
    public static readonly Theme THEME = GD.Load<Theme>(
        "res://main_theme.tres"
    );
}
