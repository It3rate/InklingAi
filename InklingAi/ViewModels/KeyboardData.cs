using Avalonia;
using Avalonia.Input;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InklingAi.ViewModels;

public class KeyboardData
{
	public static KeyboardData Instance = new KeyboardData();

    private InklingModel _model => InklingModel.Instance;
    private Engine _engine => _model.Engine;

    private HashSet<Key> _downKeys { get; } = new HashSet<Key>();
	public int Count(Key key) => _downKeys.Count();
	public bool IsDown(Key key) => _downKeys.Contains(key);

	public bool IsShiftDown => _downKeys.Contains(Key.LeftShift) || _downKeys.Contains(Key.RightShift);
	public bool IsControlDown => _downKeys.Contains(Key.LeftCtrl) || _downKeys.Contains(Key.RightCtrl);
	public bool IsAltDown => _downKeys.Contains(Key.LeftAlt) || _downKeys.Contains(Key.RightAlt);

	private KeyboardData()
	{
	}

	public void Cut()
	{
	}
	public void Copy()
	{
	}
	public void Paste()
	{
	}
	public void Undo()
	{
		_engine.Stack.Undo();
	}
	public void Redo()
	{
		_engine.Stack.Redo();
    }
    public void Delete()
    {
        _engine.FocusedCommandTarget?.DeleteSelected();
    }
    public void Move(SelectionTransformKind transformKind)
    {
        _engine.FocusedCommandTarget?.Move(transformKind, IsControlDown, IsShiftDown);
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        _downKeys.Add(e.Key);
        if (IsControlDown && IsShiftDown)
        {
            switch (e.Key)
            {
                case Key.Z: Redo(); break;
            }
        }
        else if (IsControlDown)
        {
            switch (e.Key)
            {
                case Key.C: Copy(); break;
                case Key.V: Paste(); break;
                case Key.X: Cut(); break;
                case Key.Y: Redo(); break;
                case Key.Z: Undo(); break;
                case Key.Left: Move(SelectionTransformKind.MoveLeft); break;
                case Key.Right: Move(SelectionTransformKind.MoveRight); break;
                case Key.Up: Move(SelectionTransformKind.MoveUp); break;
                case Key.Down: Move(SelectionTransformKind.MoveDown); break;
            }
        }
        else if (IsShiftDown)
        {
            switch (e.Key)
            {
                case Key.Left: Move(SelectionTransformKind.MoveLeft); break;
                case Key.Right: Move(SelectionTransformKind.MoveRight); break;
                case Key.Up: Move(SelectionTransformKind.MoveUp); break;
                case Key.Down: Move(SelectionTransformKind.MoveDown); break;
            }
        }
        else
        {
            switch (e.Key)
            {
                case Key.Delete: Delete(); break;
                case Key.Back: Delete(); break;
                case Key.Left: Move(SelectionTransformKind.MoveLeft); break;
                case Key.Right: Move(SelectionTransformKind.MoveRight); break;
                case Key.Up: Move(SelectionTransformKind.MoveUp); break;
                case Key.Down: Move(SelectionTransformKind.MoveDown); break;
            }
        }
    }

	public void OnKeyUp(KeyEventArgs e)
	{
		_downKeys.Remove(e.Key);
	}
}

