namespace InklingAi.ViewModels;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using InklingAi.Utils;
using InklingAi.Views;
using InkPad.Adapters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SkiaSharp;

public class Selection : IEnumerable<ISelectableElement>
{
    public event EventHandler ContentChanged;
    private readonly List<ISelectableElement> _internalList = new();
    protected VectorImage _target;
	public TransformKind TransformKind { get; set; } = TransformKind.Scale;
    public bool HasSelection => _internalList.Count > 0;
	int[] _prevIds = new int[0];

	public Selection(VectorImage target) 
    {
        _target = target;
    }

    public void Set(params ISelectableElement[]? items)
    {
        _internalList.Clear();
        if (items != null)
        {
            AddRange(items);
        }
        _target.InvalidateVisual();
    }
    public void SetWith(List<ISelectableElement> items)
    {
        _internalList.Clear();
        AddRange(items);
        _target.InvalidateVisual();
    }
    public void Add(ISelectableElement item)
    {
        if (item != null)
        {
            _internalList.Add(item);
			ContentsChanged();
        }
    }
    public void AddRange(IEnumerable<ISelectableElement> items)
    {
        _internalList.AddRange(items.Where(item => item != null));
		ContentsChanged();
	}

    public bool Remove(ISelectableElement item)
    {
        var result = false;
        if (_internalList.Contains(item))
        {
            result = _internalList.Remove(item);
			ContentsChanged();
		}
        return result;
    }
    public void RemoveMany(IEnumerable<ISelectableElement> items)
    {
        _internalList.RemoveMany(items.Where(item => item != null));
		ContentsChanged();
	}
    public void Clear()
    {
        _internalList.Clear();
		ContentsChanged();
	}

    private void ContentsChanged()
	{
        if (!GetIds().SequenceEqual(_prevIds))
        {
            TransformKind = TransformKind.Scale;
            _prevIds = GetIds();
            OnContentChanged(EventArgs.Empty);
        }
		_target.InvalidateVisual();
    }
    protected virtual void OnContentChanged(EventArgs e)
    {
        ContentChanged?.Invoke(this, e);
    }

    public int Count => _internalList.Count;
    public ISelectableElement[] ToArray() => _internalList.ToArray();
    public List<ISelectableElement> ToList() => _internalList.ToList();
    public ISelectableElement this[int index] => _internalList[index];
    public bool Contains(ISelectableElement item) => _internalList.Contains(item);
    public int IndexOf(ISelectableElement item) => _internalList.IndexOf(item);

    public void ForEach(Action<ISelectableElement> action) => _internalList.ForEach(action);
    public IEnumerator<ISelectableElement> GetEnumerator() => _internalList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _internalList.GetEnumerator();
    public int[] GetIds() => _internalList.Select(item => item.Id).ToArray();

    public SKRect Bounds
    {
        get
        {
            SKRect totalBounds = SKRect.Empty;

            foreach (var path in _internalList)
            {
                if (totalBounds.IsEmpty)
                    totalBounds = path.Bounds;
                else
                    totalBounds.Union(path.Bounds);
            }

            return totalBounds;
        }
    }
    public SKMatrix Transform(float dx, float dy, float rotation)
    {
        var matrix = Bounds.CreateTransformMatrix(dx, dy, rotation);
        ApplyMatrixToPaths(matrix);
        return matrix;
    }
    public void ApplyMatrixToPaths(SKMatrix matrix)
    {
        foreach (var path in _internalList)
        {
            path.Matrix = matrix; // need to merge matricies
        }
        _target.InvalidateVisual();
    }

}

public enum SelectionTransformKind
{
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    ScaleUp,
    ScaleDown,
    RotateRight,
    RotateLeft,
}