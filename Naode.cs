using Godot;
using System;
using System.Collections.Generic;

public partial class Naode : Node2D
{
	public enum EnumType
	{
		STATE, PROP, TAG, 
	}

	public Vector2 Velocity = Vector2.Zero;
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

	private Bautton bautton;
	private LineEdit lineEdit;
	private readonly Dictionary<int, Arrow> arrows = new();

	public Naode(int id, EnumType type)
	{
		this.Id = id;
		this.Type = type;

		bautton = new(id);
		AddChild(bautton);
		lineEdit = new();
		AddChild(lineEdit);
	// }

	// public override void _Ready()
	// {
		bautton.Pressed += BauttonOnClick;
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

	public void BauttonOnClick()
	{
		Select();
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
		lineEdit.GrabFocus();
	}
}
