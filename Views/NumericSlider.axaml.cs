namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using System;

    public partial class NumericSlider : UserControl
    {
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
