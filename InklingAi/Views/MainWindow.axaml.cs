using Avalonia.Controls;
using Avalonia.Input;
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
