namespace InkPad.Elements;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IConstraint
{
    int Key { get; }
    IElement StartElement { get; }
    bool HasElement(int key);
    List<int> AffectedKeys { get; }

    void OnAddConstraint();
    void OnRemoveConstraint();
    void OnElementChanged(IElement changedElement, Dictionary<int, SKPoint> _changes);
}

public interface ITwoElementConstraint : IConstraint
{
    IElement EndElement { get; }

    IElement OtherElement(int originalKey);
    void OnStartChanged(Dictionary<int, SKPoint> adjustedElements);
    void OnEndChanged(Dictionary<int, SKPoint> adjustedElements);
}

public enum ConstraintTarget
{
    None,
    Full,
    X,
    Y,
    T,
    Length,
    StartPoint,
    EndPoint,
    Collinear,
    Parallel,
    Perpendicular,
    Horizontal,
    Vertical,
    Tangent,
    Angle,
}
