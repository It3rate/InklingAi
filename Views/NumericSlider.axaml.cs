using Avalonia;
using ExCSS;
using InklingAi.Views;

namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using ExCSS;
    using System;

    public partial class NumericSlider : UserControl
    {
        public static readonly StyledProperty<string> NumberFormatProperty =
            AvaloniaProperty.Register<GalleryControl, string>(nameof(NumberFormat), "0");
        public string NumberFormat
        {
            get => GetValue(NumberFormatProperty);
            set => SetValue(NumberFormatProperty, value);
        }

        public static readonly StyledProperty<int> MinimumProperty =
            AvaloniaProperty.Register<GalleryControl, int>(nameof(Minimum), 0);
        public int Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }


        public static readonly StyledProperty<int> MaximumProperty =
            AvaloniaProperty.Register<GalleryControl, int>(nameof(Maximum), 100);
        public int Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly StyledProperty<double> TickFrequencyProperty =
            AvaloniaProperty.Register<GalleryControl, double>(nameof(TickFrequency), 1);
        public double TickFrequency
        {
            get => GetValue(TickFrequencyProperty);
            set => SetValue(TickFrequencyProperty, value);
        }

        public NumericSlider()
        {
            InitializeComponent();

            _slider.ValueChanged += NumericSlider_ValueChanged;
        }

        private void NumericSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider _slider)
            {
                _slider.Value = Math.Round(_slider.Value * 2) / 2;
            }
        }
    }
}
