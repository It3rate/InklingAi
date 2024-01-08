namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
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
            var itemsPerRow = 4f;// (float)lbGallery.Items.Count;
            HalfWidth = (int)(Math.Floor(lbGallery.Bounds.Width / itemsPerRow) - 4);
        }

    }
}
