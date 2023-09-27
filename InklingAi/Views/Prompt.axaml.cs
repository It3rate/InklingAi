namespace InklingAi.Views
{
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using InklingAi.ViewModels;

    public partial class Prompt : UserControl
    {
        public Prompt()
        {
            InitializeComponent();
            _seed.IsCheckedChanged += OnSeedCheckedChanged;
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
