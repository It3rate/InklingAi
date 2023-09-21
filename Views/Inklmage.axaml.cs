namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using System;
    using System.ComponentModel;
    using System.Xml.Linq;

    public partial class InkImage : UserControl
    {
        public float _widthExtra = 2f;
        private Avalonia.Point? _startPoint;

        public static readonly StyledProperty<string> SourceProperty =
            AvaloniaProperty.Register<GalleryControl, string>(nameof(Source), @"C:\Users\Robin\InklingAI\SD\working\input.png");
        public string Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public InkImage()
        {
            InitializeComponent();
        }

        private void OnImagePointerPressed(object sender, PointerPressedEventArgs e)
        {
            _startPoint = e.GetPosition(this);
        }

        private void OnImagePointerMoved(object sender, PointerEventArgs e)
        {
            if (!_startPoint.HasValue)
                return;

            var currentPosition = e.GetPosition(this);
            var diff = currentPosition - _startPoint.Value;

            //if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            //    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            //{
            //    // Start the drag operation
            //    DragDrop.DoDragDrop(new DataObject(DataFormats.FileDrop, new string[] { /* paths */ }), DragDropEffects.Copy);
            //    _startPoint = null;
            //}
        }

        private void OnImagePointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _startPoint = null;
        }

    }
}
