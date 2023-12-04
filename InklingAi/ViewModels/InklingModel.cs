namespace InklingAi.ViewModels;

using InklingAi.StableDiffusion;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using InklingAi.CommandEngine;
using InklingAi.Utils;
using InklingAi.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AIEnv.Clients.StableDiffusion;
using AIEnv.Utils;
using Avalonia.Controls.Shapes;
using System.Diagnostics;

public partial class InklingModel : ViewModelBase
{
    public static string Title => "Inkling";

	private static InklingModel _instance = new InklingModel();
    public static InklingModel Instance => _instance;

    public StableDiffusionConfig _sdConfig = new();
    public StableDiffusionController sdController;

    public GalleryData GalleryData { get; } = new();
    public InfluenceData InfluenceData { get; } = new();
	public KeyboardData KeyboardData => KeyboardData.Instance;
	public Engine Engine => Engine.Instance;

    public InklingModel() // this needs to be public for design time access in MainView.
    {
        GalleryData.LoadNextPage();
        InfluenceData.LoadInputControls();
        sdController = new StableDiffusionController();
        sdController.OnUpdateStep += OnGenerateUpdate;

        ChangeAspectSizeCommand = new RelayCommand<string>(ChangeAspectSize);
        GenerateCommand = new RelayCommand(OnGenerate);
        DeleteCommand = new RelayCommand(OnDelete);
        OnChangeModel("mnRevAnimated");
    }

    #region Commands
    public ICommand GenerateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ChangeAspectSizeCommand { get; }

    private DalleClient _dalle = new();
    public async void OnGenerate()
    {
        var path = await _dalle.RequestImageAsync(SDConfig);
        RandomizeSeed();
        //var path = sdController.Generate(SDConfig);
        GalleryData.LoadNextPage(0);
    }
    public void OnDelete()
    {
    }

    public void OnChangeModel(string modelName)
    {
        InfluenceData.AllowControlNet(modelName != "mnDalle3");
    }

    public double AspectRatio => SDConfig.AspectSize.Width / SDConfig.AspectSize.Height;
    private void ChangeAspectSize(string? aspect)
    {
        if (aspect == "Portrait")
            SDConfig.AspectSize = new IntSize(512, 640);
        else if (aspect == "Square")
            SDConfig.AspectSize = new IntSize(512, 512);
        else if (aspect == "Landscape")
            SDConfig.AspectSize = new IntSize(640, 512);
    }
    #endregion

    private void OnGenerateUpdate(int currentIndex, SkiaSharp.SKBitmap? img)
    {
    }

    public StableDiffusionConfig SDConfig
    {
        get => _sdConfig;
        set
        {
            if (_sdConfig != value)
            {
                _sdConfig = value;
                OnPropertyChanged(nameof(SDConfig));
            }
        }
    }

    private Random _random = new Random();
    public void RandomizeSeed()
    {
        if (SDConfig.SeedUnlocked)
        {
            SDConfig.Seed = _random.Next();
        }
    }

    public static InklingModel? GetRootDataContext(Visual? control)
    {
        InklingModel? result = null;
        while (control != null && !(control is Window))
        {
            control = control.GetVisualParent();
        }

        if (control is Window window && window.DataContext is InklingModel mvm)
        {
            result = mvm;
        }
        return result;
    }

    //public void EnableLoadedModelsThreadCheck()
    //{
    //    //if (InvokeRequired) // tofo:check for UI thread
    //    //{
    //    //    this.Invoke((MethodInvoker)delegate
    //    //    {
    //    //        EnableLoadedModels();
    //    //    });
    //    //}
    //    //else
    //    //{
    //    EnableLoadedModels();
    //    //}

    //}
    //public void EnableLoadedModels()
    //{
    //    var modelExists = IOUtils.UnetModelExists(CurrentModelName, pbMask.Checked);
    //    SetCanGenerate(modelExists);

    //    foreach (var pb in _controlnetPbs)
    //    {
    //        if (pb.Checkbox != null)
    //        {
    //            var path = IOUtils.GetModelPath(pb.PBName);
    //            pb.Checkbox.Enabled = File.Exists(path);
    //        }
    //    }
    //}
}

