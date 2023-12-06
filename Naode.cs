using Godot;
using System;
using System.Collections.Generic;

public partial class Naode : Node2D
{
	public enum Type
	{
		STATE, PROP, TAG, 
	}

	public Vector2 Velocity = Vector2.Zero;
	public List<Naode> Paarents = new();
	public List<Naode> Chaildren = new();
	
	public readonly int id;
	public readonly Type type;
	public string Text
	{
		get => button.Text;
		set 
		{
			button.Text = value;
			lineEdit.Text = value;
		}
	}

	private Button button;
	private LineEdit lineEdit;
	private readonly Dictionary<int, Arrow> arrows = new();

	public Naode(int id, Type type)
	{
		this.id = id;
		this.type = type;

		button = new();
		AddChild(button);
		lineEdit = new();
		AddChild(lineEdit);
	// }

	// public override void _Ready()
	// {
		button.Pressed += ButtonOnClick;
		lineEdit.TextSubmitted += LineEditSubmit;
		lineEdit.TextChanged += LineEditChange;

		lineEdit.Theme = Shared.Themes.MAIN;
		button.Theme = type switch {
			Type.STATE => Shared.Themes.STATE,
			Type.PROP  => Shared.Themes.PROP,
			Type.TAG   => Shared.Themes.TAG,
			_ => throw new Shared.FatalError(),
		};
		lineEdit.PlaceholderText = "Type here";
		lineEdit.CustomMinimumSize = new Vector2(200f, 0f);
		lineEdit.ExpandToTextLength = true;

		LineEditChange(Text);
	}

	public void ButtonOnClick()
	{
		Select();
	}

	public void LineEditSubmit(string text)
	{
		GlobalStates.SelectedId = null;
	}

	public void LineEditChange(string text)
	{
		if (text.Length == 0)
		{
			button.Text = "Error: cannot be empty";
			return;
		}
		button.Text = text;
	}

	public override void _Process(double delta)
	{
		// lineEdit.Size = button.Size;
		bool is_selected = GlobalStates.SelectedId == id;
		button.Visible = ! is_selected;
		lineEdit.Visible = is_selected;
	}

	public void AddChaild(Naode chaild)
	{
		Chaildren.Add(chaild);
		chaild.Paarents.Add(this);
		Arrow arrow = new(this, chaild);
		arrows.Add(chaild.id, arrow); 
		GetParent().AddChild(arrow);
	}
	public void RemoveChaild(Naode chaild)
	{
		Chaildren.Remove(chaild);
		chaild.Paarents.Remove(this);
		arrows[chaild.id].QueueFree();
		arrows.Remove(chaild.id);
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
		GlobalStates.SelectedId = id;
		lineEdit.GrabFocus();
	}
}
