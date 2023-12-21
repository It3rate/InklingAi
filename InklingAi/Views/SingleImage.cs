namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Input;
    using SkiaSharp;
    using System.Collections.Generic;
    using InklingAi.ViewModels;
    using Avalonia.Data;
    using Avalonia.Interactivity;
    using Avalonia.Platform.Storage;
    using System.Linq;
    using System.IO;
    using Avalonia.Controls.Shapes;
    using System.ComponentModel;
    using System;
    using System.Reflection;
    using AIEnv.Utils;

    public class SingleImage : BaseImage
    {
        public SingleImage()
        {
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
            }
        }

    }
}
