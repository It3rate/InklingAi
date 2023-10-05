namespace InklingAi.Elements
{
    using InklingAi.Pads;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IElement
    {
        PadKind PadKind { get; }
        Pad Pad { get; }
        ElementKind ElementKind { get; }
        int Key { get; }
        IElement EmptyElement { get; }
        bool IsLocked { get; set; }
        bool IsEmpty { get; }
        bool HasArea { get; }
        SKPath Path { get; }
        SKPoint Center { get; }

        //IPoint[] TerminalPoints { get; }
        List<IPoint> Points { get; }
        List<int> AllKeys { get; }
        List<SKPoint> SKPoints { get; }
        float DistanceToPoint(SKPoint point);

        void MoveTo(SKPoint position);
        void SetLock(bool lockStatus);
        IElement Duplicate(bool addElement = true);
    }
    public interface IAreaElement : IElement
    {
        bool ContainsPosition(SKPoint point);
    }
    public interface ISlugElement : IElement
    {
        float StartT { get; set; }
        float EndT { get; set; }
        //Slug LocalRatio { get; set; }
        float TRatio { get; }
    }
    public interface ISegmentElement
    {
        IPoint StartPoint { get; }
        IPoint EndPoint { get; }
        SKPoint StartPosition { get; }
        SKPoint EndPosition { get; }
    }

    public interface ITValue
    {
        float T { get; set; }
    }

    public interface IMidpointSettable
    {
        void SetMidpoint(SKPoint midPoint);
    }
}
