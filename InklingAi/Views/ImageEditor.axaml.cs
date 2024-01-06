using System;
using AIEnv.Clients.StableDiffusion;
using Avalonia;
using Avalonia.Controls;
using InklingAi.ViewModels;

namespace InklingAi.Views;

public partial class ImageEditor : UserControl
{
    private InklingModel _model = InklingModel.Instance;
    public ImageEditor()
    {
        InitializeComponent();
        _model.SDConfig.AspectChanged += OnAspectChanged;
    }
    private void OnAspectChanged()
    {
        var aspectSize = _model.SDConfig.AspectSize;
        _editImageControl.AspectSize = aspectSize;
        InvalidateVisual();
        InvalidateMeasure();
        InvalidateArrange();
    }
}
