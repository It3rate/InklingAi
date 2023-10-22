using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaColorPicker;
using InklingAi.ViewModels;

namespace InklingAi.Views;

public partial class MainWindow : Window
{
	public MainWindow()
    {
        InitializeComponent();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		KeyboardData.Instance.OnKeyDown(e);
	}
	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		KeyboardData.Instance.OnKeyUp(e);
	}
}
