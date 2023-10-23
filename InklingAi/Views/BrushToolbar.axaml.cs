namespace InklingAi.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaColorPicker;

    public partial class BrushToolbar : UserControl
    {
        public BrushToolbar()
        {
            InitializeComponent();
		_colBtn = GetColorPicker();
	}

	private ColorButton<InklingColorPickerWindow> _colBtn;
	private ColorButton<InklingColorPickerWindow> GetColorPicker()
	{
            var cb = new ColorButton<InklingColorPickerWindow>
            {
                Width = 50,
                Height = 30
            };
            var binding = new Binding("DrawingData.SelColor", BindingMode.TwoWay);
		cb.Bind(ColorButton<InklingColorPickerWindow>.ColorProperty, binding);
		Grid.SetRow(cb, 1);
		Grid.SetColumn(cb, 7);
		//cb.PointerPressed += OnPointerPressed;
		var btg = this.FindControl<Grid>("BrushToolbarGrid");
		btg?.Children.Add(cb);

		//cb.PropertyChanged += colBtn_PropertyChanged;
		return cb;
	}

        //private void colBtn_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) { }
        //private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e) { }


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
