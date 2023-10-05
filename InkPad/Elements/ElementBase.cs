namespace InkPad.Elements;

using InkPad.Pads;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InkPad.Agents;

public abstract class ElementBase : IElement
{
    public const int EmptyKeyValue = -99;
    public virtual bool IsEmpty => Key == EmptyKeyValue;
    public static bool IsEmptyKey(int key) => key == EmptyKeyValue;

    public Pad Pad { get; }
    public PadKind PadKind => Pad.PadKind;
    public int Key { get; }
    public abstract List<int> AllKeys { get; }

    public abstract IElement EmptyElement { get; }
    public abstract ElementKind ElementKind { get; }
    public abstract SKPath Path { get; }
    public abstract List<IPoint> Points { get; }
    public abstract SKPoint Center { get; }
    public List<SKPoint> SKPoints
    {
        get
        {
            var pts = Points;
            var result = new List<SKPoint>(pts.Count);
            foreach (var point in pts)
            {
                result.Add(point.Position);
            }
            return result;
        }
    }

    public virtual bool HasArea => false;

    protected bool _isLocked;
    public virtual bool IsLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            if (!ElementKind.IsPoint())
            {
                foreach (var point in Points)
                {
                    point.IsLocked = value;
                }
            }
        }
    }

    protected ElementBase(bool isEmpty) // used to privately create an empty element
    {
        Pad = Agent.Current.PadFor(PadKind.None);
        Key = EmptyKeyValue;
    }
    protected ElementBase(PadKind padKind)
    {
        Pad = Agent.Current.PadFor(padKind);
        Key = Pad.KeyCounter++;
        Pad.AddElement(this);
    }

    public abstract float DistanceToPoint(SKPoint point);

    public virtual void MoveTo(SKPoint position)
    {
        foreach (var point in Points)
        {
            point.Position = position;
        }
    }

    public void SetLock(bool lockStatus)
    {
        IsLocked = lockStatus;
    }

    public abstract IElement Duplicate(bool addElement = true);
}
