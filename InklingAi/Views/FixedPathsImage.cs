namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.ViewModels;
    using SkiaSharp;

    public class FixedPathsImage : BaseImage
    {
        public override ImageData? Data { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void RenderSkia(SKCanvas canvas)
        {
            throw new NotImplementedException();
        }
    }
}
