namespace InklingAi.Elements;

using InklingAi.Pads;
using InklingAi.Renderer;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class PointBase : ElementBase, IPoint
{
    public abstract SKPoint Position { get; set; }
    public override SKPoint Center => Position;

    protected PointBase(bool isEmpty) : base(isEmpty) { }
    protected PointBase(PadKind padKind) : base(padKind) { }

    public override SKPath Path
    {
        get
        {
            var path = new SKPath();
            path.AddCircle(Position.X, Position.Y, 1);
            return path;
        }
    }
    public override float DistanceToPoint(SKPoint point)
    {
        return point.DistanceTo(Position);
    }

    public abstract bool CanMergeWith(IPoint point);
    public virtual IPoint TargetPoint => this;

    public virtual int MergeInto(IPoint point)
    {
        Pad.SetPointAt(Key, point);
        return point.Key;
    }

    //public static bool operator ==(PointBase left, IPoint right) => left.Key == right.Key;
    //public static bool operator !=(PointBase left, IPoint right) => left.Key != right.Key;
    //public override bool Equals(object obj) => obj is PointBase value && this == value;
    //public override int GetHashCode() => Key.GetHashCode();
}
