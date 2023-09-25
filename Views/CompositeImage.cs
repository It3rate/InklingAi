namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Media;
    using InklingAi.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CompositeImage : BaseImage
    {
        //protected override void OnRender(DrawingContext context)
        //{
        //    base.Render(context);

        //    // Draw a red border around the image
        //    var pen = new Pen(Brushes.Red, 2);
        //    context.DrawRectangle(pen, new Rect(0, 0, Bounds.Width, Bounds.Height));
        //}
        public override ImageData PrimarySource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
