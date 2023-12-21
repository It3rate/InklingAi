namespace InklingAi.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Avalonia.Data;
    using Avalonia;
    using SkiaSharp;
    using System.IO;
    using InklingAi.Utils;
    using AIEnv.Utils;
    using InklingAi.ViewModels;
    using Avalonia.Threading;

    public class FileImage : BaseImage
    {

        public FileImage()
        {
            Data = new ImageData("");
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(Source):
                    if (change.NewValue is string source && Data != null)
                    {
                        Data.SourcePath = source;
                        //Data = _data;
                        //SetValue(DataProperty, imgData);
                    }
                    break;
            }
        }
    }
}
