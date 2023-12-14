namespace InklingAi.Views
{
    using System.Collections.Generic;
    using Avalonia;
    using Avalonia.Controls;
    using InklingAi.ViewModels;

    public partial class DrawingToolbar : ButtonSet
    {
        private Dictionary<Button, DrawPanelMode> _buttonModes;

        public static readonly StyledProperty<VectorImage> VectorImageProperty =
            AvaloniaProperty.Register<DrawingToolbar, VectorImage>(nameof(VectorImage));
        public VectorImage VectorImage
        {
            get => GetValue(VectorImageProperty);
            set => SetValue(VectorImageProperty, value);
        }
        public DrawingToolbar()
        {
            InitializeComponent();
            _buttonModes = new Dictionary<Button, DrawPanelMode>() {
                { btArrow, DrawPanelMode.Arrow },
                { btErase, DrawPanelMode.Erase },
                { btPaint, DrawPanelMode.Paint },
                { btPen, DrawPanelMode.Pen },
                { btResize, DrawPanelMode.ZoomPan },
                { btRect, DrawPanelMode.Rect },
                { btOval, DrawPanelMode.Oval },
                { btStar, DrawPanelMode.Star },
                { btText, DrawPanelMode.Text }
                };
        }

        private void OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (VectorImage != null && sender is Button btn)
            {
                VectorImage.DrawPanelMode = _buttonModes[btn];
            }
        }
    }
}
