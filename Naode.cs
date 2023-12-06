using Godot;
using System;

public partial class Naode : Node2D
{
	// props
	private int id;
	private bool is_selected;
	private Action<int> on_click;

	private Button button;
	private LineEdit lineEdit;
	public override void _Ready()
	{
		button = GetNode<Button>("Button");
		lineEdit = GetNode<LineEdit>("LineEdit");

		button.Pressed += ButtonOnClick;
		lineEdit.TextSubmitted += LineEditSubmit;
	}

	public void ButtonOnClick()
	{
		on_click(id);
	}

	public void LineEditSubmit(string text)
	{
		
	}

	public void SetProps(int id_, bool is_selected_, Action<int> on_click_)
	{
		id = id_;
		is_selected = is_selected_;
		on_click = on_click_;
		
		button.Visible = ! is_selected;
		lineEdit.Visible = is_selected;
	}

	public override void _Process(double delta)
	{
		lineEdit.Size = button.Size;
	}
}
