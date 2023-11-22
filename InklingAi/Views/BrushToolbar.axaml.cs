namespace InklingAi.Views;

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaColorPicker;
using DynamicData;
using InklingAi.CommandEngine;
using InklingAi.Commands;
using InklingAi.Tasks;
using InklingAi.ViewModels;

public partial class BrushToolbar : UserControl
{

	public static readonly StyledProperty<PaintableImage> PaintableImageProperty =
		AvaloniaProperty.Register<BrushToolbar, PaintableImage>(nameof(PaintableImage));
	public PaintableImage PaintableImage
	{
		get => GetValue(PaintableImageProperty);
		set => SetValue(PaintableImageProperty, value);
	}

	public bool GenCommandsOnChange = false;
	public BrushToolbar()
    {
		InitializeComponent();
		_colBtn = GetColorPicker();
		Loaded += BrushToolbar_Loaded;
	}

	private void BrushToolbar_Loaded(object? sender, RoutedEventArgs e)
	{
		GenCommandsOnChange = true;
	}

	private ColorButton<InklingColorPickerWindow> _colBtn;
	private ColorButton<InklingColorPickerWindow> GetColorPicker()
	{
		var cb = new ColorButton<InklingColorPickerWindow>
		{
			Width = 50,
			Height = 30
		};
		var binding = new Binding("PaintableImage.DrawingData.SelColor")
		{
			Mode = BindingMode.TwoWay,
			RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
			{
				AncestorType = typeof(UserControl),
				AncestorLevel = 1
			}
		};
		cb.Bind(ColorButton<InklingColorPickerWindow>.ColorProperty, binding);
		Grid.SetRow(cb, 1);
		Grid.SetColumn(cb, 7);
		var btg = this.FindControl<Grid>("BrushToolbarGrid");
		btg?.Children.Add(cb);

		//cb.PointerPressed += OnPointerPressed;
		cb.PropertyChanged += OnPropertyChanged;
		return cb;
	}

	private void OnValueSpinned(object? sender, SpinEventArgs change)
	{
		if (GenCommandsOnChange)
		{
			if (sender == nudBrushSize)
			{
				//ChangeBrushSizeCommand(change.);
			}
			else if (sender == nudFeather)
			{
				//Trace.WriteLine("nudFeather");
				//DrawingData.Instance.BrushSize = 
			}
			else if (sender == nudTransparent) // taken care of by color
			{
			}
		}
	}

     private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (GenCommandsOnChange && (sender == _colBtn || sender == nudTransparent))
		{
			var prop = change.Property;
			switch (prop.Name)
			{
				case nameof(_colBtn.Color):
					if (change.OldValue is Color col && change.OldValue != change.NewValue)
					{
						ChangeColorCommand(col);
					}
					break;
			}
		}
	}

	private void ChangeBrushSizeCommand(float oldBrushSize)
	{
		var task = new ChangeBrushParamsTask(PaintableImage.DrawingDataSource, oldBrushSize);
		RunCommand(task);
	}
	private void ChangeColorCommand(Color oldColor)
	{
		var task = new ChangeBrushParamsTask(PaintableImage.DrawingDataSource, oldColor.ToUInt32());
		RunCommand(task);
	}
	private void RunCommand(ChangeBrushParamsTask task)
	{
		var cmd = new ChangeBrushParamsCommand(task, this);
		Engine.Instance.Stack.Do(cmd);
	}

    private void OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e) 
	{
		if(sender is NumericUpDown numericUpDown)
		{
			switch (numericUpDown.Name)
			{
				case "brushSize": PaintableImage.DrawingDataSource.BrushSize = (float)(numericUpDown.Value ?? 20); break;
			}
		}
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
