namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Data;
    using Avalonia.Input;
    using Avalonia.Media;
    using InklingAi.ViewModels;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CompositeImage : BaseImage
    {

        public static readonly StyledProperty<ObservableCollection<ImageData>> ImageDataItemsProperty =
            AvaloniaProperty.Register<SingleImage, ObservableCollection<ImageData>>(nameof(ImageDataItems));
        public ObservableCollection<ImageData> ImageDataItems
        {
            get => GetValue(ImageDataItemsProperty);
            set => SetValue(ImageDataItemsProperty, value);
        }


        public CompositeImage()
        {
        }

        protected override void OnLoaded(object? sender, VisualTreeAttachmentEventArgs e)
        {
            base.OnLoaded(sender, e);

            var mvm = MainViewModel.GetRootDataContext(this);
            if (mvm != null)
            {
                var binding = new Binding("ActiveItems")
                {
                    Mode = BindingMode.OneWay,
                    Source = mvm
                };
                this.Bind(ImageDataItemsProperty, binding);
            }
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var prop = change.Property;
            switch (prop.Name)
            {
                case nameof(ImageDataItems):
                    _imageDataList.Clear();
                    if (change != null && change.NewValue != null && change.NewValue is IEnumerable<ImageData> imageData)
                    {
                        _imageDataList.AddRange(imageData);
                    }
                    InvalidateMeasure();
                    InvalidateVisual();
                    break;
            }
        }

        protected override void OnDrop(object? sender, DragEventArgs e)
        {
        }

        protected override void RenderSkia(SKCanvas canvas)
        {
            if (_imageDataList.Count > 0)
            {
                for (int i = 0; i < _imageDataList.Count; i++)
                {
                    var alpha = 1.0 / (i + 1.0);
                    DrawImage(canvas, _imageDataList[i].ProcessedImage, alpha);
                }
            }
        }
    }
}
