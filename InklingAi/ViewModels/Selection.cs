namespace InklingAi.ViewModels;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using InklingAi.Views;
using InkPad.Adapters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SkiaSharp;

public class Selection : IEnumerable<ISelectableElement>
{
    private readonly List<ISelectableElement> _internalList = new();
    protected PaintableImage _target;
	public TransformKind TransformKind { get; set; } = TransformKind.Scale;

	public Selection(PaintableImage target) 
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

    int[] _prevIds = new int[0];
    private void ContentsChanged()
	{
        if (!GetIds().SequenceEqual(_prevIds))
        {
            TransformKind = TransformKind.Scale;
            _prevIds = GetIds();
        }
		_target.InvalidateVisual();
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

    public SKMatrix Transform(float dx, float dy, float rotation)
    {
        var matrix = CreateTransformMatrix(Bounds, dx, dy, rotation);
        ApplyMatrixToPaths(matrix);
        return matrix;
    }
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
    public static SKMatrix CreateTransformMatrix(SKRect boundingBox, float dx, float dy, float rotationDegrees)
    {
        var matrix = SKMatrix.CreateIdentity();
        matrix = matrix.PreConcat(SKMatrix.CreateTranslation(-boundingBox.MidX, -boundingBox.MidY));
        matrix = matrix.PreConcat(SKMatrix.CreateRotationDegrees(rotationDegrees));
        matrix = matrix.PreConcat(SKMatrix.CreateTranslation(boundingBox.MidX + dx, boundingBox.MidY + dy));
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