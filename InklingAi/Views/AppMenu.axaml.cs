namespace InklingAi.Views
{
    using Avalonia.Controls;
    using InklingAi.ViewModels;
    using System.Diagnostics;

    public partial class AppMenu : UserControl
    {
        private InklingModel _model => InklingModel.Instance;

        public AppMenu()
        {
            InitializeComponent();
        }
        private void OnMenuItemClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(sender is MenuItem menuItem)
            {
                Trace.WriteLine(menuItem.Name);
            }
        }
        private void OnModelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(sender is MenuItem menuItem)
            {
                _model.OnChangeModel(menuItem.Name);
                InvalidateVisual();
            }
        }
    }
}
