using Avalonia;
using Avalonia.Input;
using InklingAi.CommandEngine;
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
		Engine.Instance.Stack.Undo();
	}
	public void Redo()
	{
		Engine.Instance.Stack.Redo();
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
			}
		}
    }

	public void OnKeyUp(KeyEventArgs e)
	{
		_downKeys.Remove(e.Key);
	}
}

