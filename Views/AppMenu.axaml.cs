namespace InklingAi.Views
{
    using Avalonia.Controls;
    using System.Diagnostics;

    public partial class AppMenu : UserControl
    {
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
    }
}
