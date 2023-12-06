using System;
using Godot;

#pragma warning disable CA1050 // Declare types in namespaces

public static partial class Shared
{
    public static readonly Random Rand = new();
    public class FatalError : Exception { }
    public class AssertionFailed : Exception
    {
        public AssertionFailed(string message) : base(message) { }
    }

    public static void Assert(bool x, string message = "")
    {
        if (!x)
            throw new AssertionFailed(message);
    }
    public static class Themes
    {
        public static readonly Theme MAIN = GD.Load<Theme>(
            "res://main_theme.tres"
        );
        public static readonly Theme STATE = GD.Load<Theme>(
            "res://button_state.tres"
        );
        public static readonly Theme PROP = GD.Load<Theme>(
            "res://button_prop.tres"
        );
        public static readonly Theme TAG = GD.Load<Theme>(
            "res://button_tag.tres"
        );
    }
}
