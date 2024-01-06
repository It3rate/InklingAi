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

	public static readonly StyledProperty<VectorImage> VectorImageProperty =
		AvaloniaProperty.Register<BrushToolbar, VectorImage>(nameof(VectorImage));
	public VectorImage VectorImage
	{
		get => GetValue(VectorImageProperty);
		set => SetValue(VectorImageProperty, value);
	}

	//private ColorButton<InklingColorPickerWindow> _colBtn;
	public BrushToolbar()
    {
		InitializeComponent();
        GetColorPicker();
	}

	private ColorButton<InklingColorPickerWindow> GetColorPicker()
	{
		var cb = new ColorButton<InklingColorPickerWindow>
		{
			Width = 50,
			Height = 30
		};
		var binding = new Binding("VectorImage.BrushState.SelColor")
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

		return cb;
	}

}
