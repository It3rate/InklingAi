namespace InklingAi.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.Pads;
    using SkiaSharp;

    public interface IPoint : IElement
    {
        //PointKind SelectionKind { get; }
        SKPoint Position { get; set; }
        //bool ReplaceWith(IPoint pt);
        //void CopyValuesFrom(IPoint from);
        bool CanMergeWith(IPoint point);
        IPoint TargetPoint { get; }
        int MergeInto(IPoint point);
    }

}
