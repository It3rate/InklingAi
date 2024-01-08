namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia;
    using AIEnv.Utils;
    using Avalonia.Media;
    using Avalonia.Interactivity;

    public class FileImage : BaseImage
    {
        public FileImage()
        {
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            Size sz = new Size(512, 512);// AspectSize.ToAvaloniaSize();
            var result = Stretch.CalculateSize(availableSize, sz, StretchDirection.Both);
            return result;
        }
    }
}
