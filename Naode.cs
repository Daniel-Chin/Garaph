using Godot;
using System;

public partial class Naode : Node2D
{
	public enum Type
	{
		STATE, PROP, 
	}

	private int id;
	private Type type;

	private Button button;
	private LineEdit lineEdit;

	// public Naode()
	// {
	//  	Shared.Assert(Engine.IsEditorHint());
	// 	id = 0;
	// }
	public Naode(int id, Type type)
	{
		this.id = id;
		this.type = type;
	}

	public override void _Ready()
	{
		button = GetNode<Button>("Button");
		lineEdit = GetNode<LineEdit>("LineEdit");

		button.Pressed += ButtonOnClick;
		lineEdit.TextSubmitted += LineEditSubmit;
		lineEdit.TextChanged += LineEditChange;

		button.Theme = Shared.THEME;
		lineEdit.Theme = Shared.THEME;
	}

	public void ButtonOnClick()
	{
		GlobalStates.SelectedId = id;
	}

	public static void LineEditSubmit(string text)
	{
		GlobalStates.SelectedId = null;
	}

	public void LineEditChange(string text)
	{
		button.Text = text;
	}

	public override void _Process(double delta)
	{
		// lineEdit.Size = button.Size;
		bool is_selected = GlobalStates.SelectedId == id;
		button.Visible = ! is_selected;
		lineEdit.Visible = is_selected;
	}
}
