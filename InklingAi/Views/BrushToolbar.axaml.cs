namespace InklingAi.Views
{
    using Avalonia.Controls;
	using Avalonia.Controls.Primitives;
	using Avalonia.Interactivity;
	using Avalonia.VisualTree;

    public partial class BrushToolbar : UserControl
    {
        public BrushToolbar()
        {
            InitializeComponent();
		}

		//private async void OnColorPicker(object sender, RoutedEventArgs e)
		//{
		//	var colorPicker = new ColorPicker();

		//	var dialog = new Window
		//	{
		//		Content = colorPicker,
		//		Width = 300,
		//		Height = 400,
		//		Title = "Choose a Color"
		//	};
		//	var window = this.GetVisualRoot() as Window;
		//	if (window != null)
		//	{
		//		await dialog.ShowDialog(window);
		//		var chosenColor = colorPicker.Color;
		//	}
		//}
	}
}
