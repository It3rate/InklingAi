namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using ExCSS;
    using InklingAi.ViewModels;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public partial class GalleryControl : UserControl
    {

        public static readonly StyledProperty<int> HalfWidthProperty =
            AvaloniaProperty.Register<GalleryControl, int>(nameof(HalfWidth));
        public int HalfWidth
        {
            get { return GetValue(HalfWidthProperty); }
            set { SetValue(HalfWidthProperty, value); }
        }

        public GalleryControl()
        {
            InitializeComponent();
            lbGallery.SizeChanged += LbGallery_SizeChanged;
        }
        private void LbGallery_SizeChanged(object? sender, EventArgs e)
        {
            UpdateHalfWidth();
        }

        private void UpdateHalfWidth()
        {
            HalfWidth = (int)(Math.Floor(lbGallery.Bounds.Width / 3f)-(8/3f));
        }
        private Avalonia.Point? _startPoint;

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
