namespace InklingAi.ViewModels;

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
using AIEnv.Onnx;
using System.ComponentModel;

public partial class InklingModel : ViewModelBase, INotifyPropertyChanged
{
    public static string Title => "Inkling";

	private static InklingModel _instance = new InklingModel();
    public static InklingModel Instance => _instance;

    public StableDiffusionConfig SDConfig { get; } = new();

    public StableDiffusionClient sdController;

    public GalleryData GalleryData { get; } = new();
    public InfluenceData InfluenceData { get; } = new();
	public KeyboardData KeyboardData => KeyboardData.Instance;
	public Engine Engine => Engine.Instance;


    private ImageData? _mainImage;
    public ImageData? MainImage
    {
        get => _mainImage;
        set => SetProperty(ref _mainImage, value);
    }


    public InklingModel() // this needs to be public for design time access in MainView.
    {
        MainImage = new ImageData(IOUtils.ImagePath("main"));
        GalleryData.LoadNextPage();
        InfluenceData.LoadInputControls();

        //sdController = new OnnxSDController();
        //sdController.OnUpdateStep += OnGenerateUpdate;
        sdController = StableDiffusionClient.Instance;
        //sdController.OnUpdateStep += OnGenerateUpdate;

        //ChangeAspectSizeCommand = new RelayCommand<string>(ChangeAspectSize);
        GenerateCommand = new RelayCommand(OnGenerate);
        DeleteCommand = new RelayCommand(OnDelete);
        OnChangeModel("mnRevAnimated");
    }

    #region Commands
    public ICommand GenerateCommand { get; }
    public ICommand DeleteCommand { get; }
    //public ICommand ChangeAspectSizeCommand { get; }

    private DalleClient _dalle = new();

    private bool _useDalle = false;
    public async void OnGenerate()
    {
        MergeInfluenceData();
        RandomizeSeed();
        string? path = null;
        if (_useDalle)
        {
            path = await _dalle.RequestImageAsync(SDConfig); // saves to image folder automatically
        }
        else
        {
            path = await sdController.Generate(SDConfig);
        }

        MainImage = path != null ? new ImageData(path) : null;

        GalleryData.LoadNextPage(0);
    }

    private void MergeInfluenceData()
    {
        foreach (var item in InfluenceData.ActiveItems)
        {
            if (item.ImageKind.IsControlNet())
            {

            }
            else if(item.ImageKind == ImageKind.Input)
            {
                SDConfig.InitialImageSource = item.SourcePath;
                SDConfig.InitialImageName = item.ProcessedPath;
            }
            else if(item.ImageKind == ImageKind.Mask)
            {

            }
        }
    }
    public void OnDelete()
    {
    }

    public void OnChangeModel(string modelName)
    {
        InfluenceData.AllowControlNet(modelName != "mnDalle3");
    }

    public double AspectRatio => SDConfig.AspectSize.Width / SDConfig.AspectSize.Height;
    //private void ChangeAspectSize(string? aspect)
    //{
    //    if (aspect == "Portrait")
    //        SDConfig.AspectSize = new IntSize(512, 640);
    //    else if (aspect == "Square")
    //        SDConfig.AspectSize = new IntSize(512, 512);
    //    else if (aspect == "Landscape")
    //        SDConfig.AspectSize = new IntSize(640, 512);
    //}
    #endregion

    private void OnGenerateUpdate(int currentIndex, SkiaSharp.SKBitmap? img)
    {
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

