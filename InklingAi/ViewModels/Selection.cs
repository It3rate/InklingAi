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
using SkiaSharp;

public class Selection : IEnumerable<ISelectableEntity>
{
    private readonly List<ISelectableEntity> _internalList = new();
    private PaintableImage _target;
    public Selection(PaintableImage target) 
    {
        _target = target;
    }

    public void Set(params ISelectableEntity[]? items)
    {
        _internalList.Clear();
        if (items != null)
        {
            AddRange(items);
        }
        _target.InvalidateVisual();
    }
    public void SetWith(List<ISelectableEntity> items)
    {
        _internalList.Clear();
        AddRange(items);
        _target.InvalidateVisual();
    }
    public void Add(ISelectableEntity item)
    {
        if (item != null)
        {
            _internalList.Add(item);
            _target.InvalidateVisual();
        }
    }
    public void AddRange(IEnumerable<ISelectableEntity> items)
    {
        _internalList.AddRange(items.Where(item => item != null));
        _target.InvalidateVisual();
    }

    public bool Remove(ISelectableEntity item)
    {
        var result = false;
        if (_internalList.Contains(item))
        {
            result = _internalList.Remove(item);
            _target.InvalidateVisual();
        }
        return result;
    }
    public void RemoveMany(IEnumerable<ISelectableEntity> items)
    {
        _internalList.RemoveMany(items.Where(item => item != null));
        _target.InvalidateVisual();
    }
    public void Clear()
    {
        _internalList.Clear();
        _target.InvalidateVisual();
    }

    public ISelectableEntity[] ToArray() => _internalList.ToArray();
    public List<ISelectableEntity> ToList() => _internalList.ToList();
    public ISelectableEntity this[int index] => _internalList[index];
    public int Count => _internalList.Count;
    public bool Contains(ISelectableEntity item) => _internalList.Contains(item);
    public int IndexOf(ISelectableEntity item) => _internalList.IndexOf(item);

    public void ForEach(Action<ISelectableEntity> action) => _internalList.ForEach(action);
    public IEnumerator<ISelectableEntity> GetEnumerator() => _internalList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _internalList.GetEnumerator();

    public SKMatrix Transform(float dx, float dy, float rotation)
    {
        var matrix = CreateTransformMatrix(Bounds, 45f, 100, 100);
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
    public static SKMatrix CreateTransformMatrix(SKRect boundingBox, float rotationDegrees, float dx, float dy)
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