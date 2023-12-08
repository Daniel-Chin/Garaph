using Godot;
using System;
using System.Collections.Generic;

public partial class Naode : Node2D
{
	public enum EnumType
	{
		STATE, PROP, TAG, 
	}
	public static EnumType Str2Type(string str) => str switch {
		"STATE" => EnumType.STATE,
		"PROP"  => EnumType.PROP,
		"TAG"   => EnumType.TAG,
		_ => throw new Shared.FatalError(),
	};
	public static string Type2Str(EnumType type) => type switch {
		EnumType.STATE => "STATE",
		EnumType.PROP  => "PROP",
		EnumType.TAG   => "TAG",
		_ => throw new Shared.FatalError(),
	};

	public Vector2 Velocity = Vector2.Zero;
	public Vector2 Force;
	public List<Naode> Paarents = new();
	public List<Naode> Chaildren = new();
	
	public readonly int Id;
	public readonly EnumType Type;
	public string Text
	{
		get => bautton.Text;
		set 
		{
			bautton.Text = value;
			lineEdit.Text = value;
		}
	}
	public Vector2 Size
	{
		get => bautton.Size;
	}

	private Bautton bautton;
	private LineEdit lineEdit;
	private readonly Dictionary<int, Arrow> arrows = new();

	public Naode(int id, EnumType type)
	{
		Id = id;
		Type = type;

		bautton = new(id);
		AddChild(bautton);
		lineEdit = new();
		AddChild(lineEdit);
	// }

	// public override void _Ready()
	// {
		lineEdit.TextSubmitted += LineEditSubmit;
		lineEdit.TextChanged += LineEditChange;

		lineEdit.Theme = Shared.Themes.MAIN;
		bautton.Theme = type switch {
			EnumType.STATE => Shared.Themes.STATE,
			EnumType.PROP  => Shared.Themes.PROP,
			EnumType.TAG   => Shared.Themes.TAG,
			_ => throw new Shared.FatalError(),
		};
		lineEdit.PlaceholderText = "Type here";
		lineEdit.ExpandToTextLength = true;

		LineEditChange(Text);
	}

	public static void LineEditSubmit(string text)
	{
		GlobalStates.SelectedId = null;
	}

	public void LineEditChange(string text)
	{
		if (text.Length == 0)
		{
			bautton.Text = "Error: cannot be empty";
			return;
		}
		bautton.Text = text;
	}

	public override void _Process(double delta)
	{
		bool is_selected = GlobalStates.SelectedId == Id;
		bautton.Visible = ! is_selected;
		lineEdit.Visible = is_selected;
	}

	public void AddChaild(Naode chaild)
	{
		Chaildren.Add(chaild);
		chaild.Paarents.Add(this);
		Arrow arrow = new(this, chaild);
		arrows.Add(chaild.Id, arrow); 
		GetParent().AddChild(arrow);
	}
	public void RemoveChaild(Naode chaild)
	{
		Chaildren.Remove(chaild);
		chaild.Paarents.Remove(this);
		arrows[chaild.Id].QueueFree();
		arrows.Remove(chaild.Id);
	}
	public void AddPaarent(Naode paarent)
	{
		paarent.AddChaild(this);
	}
	public void RemovePaarent(Naode paarent)
	{
		paarent.RemoveChaild(this);
	}

	public void QFree()
	{
		foreach (Naode paarent in Paarents.ToArray())
		{
			RemovePaarent(paarent);
		}
		foreach (Naode chaild in Chaildren.ToArray())
		{
			RemoveChaild(chaild);
		}
		QueueFree();
	}

	public void Select()
	{
		GlobalStates.SelectedId = Id;
		Main.Singleton.NaodeContextMenu.Visible = false;
		lineEdit.GrabFocus();
	}

	public Vector2 Center() => Position + 0.5f * Size;
}
