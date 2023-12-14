namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum DrawPanelMode
    {
        None,
        Arrow,
        Paint,
        Erase,
        Pen,
        Mask,
        EraseMask,
        Noise,
        FilterBrush,
        Rect,
        Oval,
        Star,
        Text,

        ZoomPan,
        NodeEdit,
        Shadow,
        Contour,
        Envelope,
    }
    public static class DrawPanelModeExtensions
    {
        public static bool IsDrawMode(this DrawPanelMode drawPanelMode)
        {
            bool result = false;
            switch(drawPanelMode)
            {
                case DrawPanelMode.Paint:
                case DrawPanelMode.Pen:
                case DrawPanelMode.Rect:
                case DrawPanelMode.Oval:
                case DrawPanelMode.Star:
                    result = true;
                    break;
            }
            return result;
        }
    }
}
