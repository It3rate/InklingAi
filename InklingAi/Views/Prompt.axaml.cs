namespace InklingAi.Views
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using Avalonia.Input;
    using InklingAi.ViewModels;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public partial class Prompt : UserControl
    {
        public Prompt()
        {
            InitializeComponent();
            _seed.IsCheckedChanged += OnSeedCheckedChanged;
        }

        private string _previousText = string.Empty;
        private void OnNumericOnly(object? sender, TextChangingEventArgs e)
        {
            var tbx = sender as TextBox;
            if (tbx != null && !string.IsNullOrEmpty(tbx.Text))
            {
                if (tbx.Text.Any(ch => ch < '0' || ch > '9') || !int.TryParse(tbx.Text, NumberStyles.Integer, null, out int result))
                {
                    tbx.Text = _previousText;
                }
                else
                {
                    _previousText = tbx.Text;
                }
            }
            else
            {
                _previousText = string.Empty;
            }
        }


        private void OnSeedCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(sender is ToggleButton tb && tb?.IsChecked == false)
            {
                var root = MainViewModel.GetRootDataContext(this);
                root?.RandomizeSeed();
            }
        }

    }
}
