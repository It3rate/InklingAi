namespace InklingAi.Views
{
    using System.Collections.Generic;
    using Avalonia;
    using Avalonia.Controls;
    using InklingAi.ViewModels;

    public partial class DrawingToolbar : ButtonSet
    {
        private Dictionary<Button, DrawMode> _buttonModes;

        public static readonly StyledProperty<PaintableImage> PaintableImageProperty =
            AvaloniaProperty.Register<DrawingToolbar, PaintableImage>(nameof(PaintableImage));
        public PaintableImage PaintableImage
        {
            get => GetValue(PaintableImageProperty);
            set => SetValue(PaintableImageProperty, value);
        }
        public DrawingToolbar()
        {
            InitializeComponent();
            _buttonModes = new Dictionary<Button, DrawMode>() {
                { btArrow, DrawMode.Arrow },
                { btErase, DrawMode.Erase },
                { btPaint, DrawMode.Paint },
                { btPen, DrawMode.Pen },
                { btResize, DrawMode.ZoomPan }
                };
        }

        private void OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (PaintableImage != null && sender is Button btn)
            {
                PaintableImage.DrawMode = _buttonModes[btn];
            }
        }
    }
}
