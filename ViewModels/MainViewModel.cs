namespace InklingAi.ViewModels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using InklingAi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public partial class MainViewModel : ViewModelBase
{
    public static string Title => "Inkling";

    private Size _aspectSize = new Size(640, 512); //(512, 640); //
    public Size AspectSize
    {
        get => _aspectSize;
        set => SetProperty(ref _aspectSize, value);
    }

    public double AspectRatio => AspectSize.Width / AspectSize.Height;

    private string _mainImagePath = @"C:\Users\Robin\InklingAI\SD\working\main.png";
    public string MainImagePath
    {
        get => _mainImagePath;
        set => SetProperty(ref _mainImagePath, value);
    }

    private ObservableCollection<string[]> _imageTuplePaths = new();
    public ObservableCollection<string[]> ImageTuplePaths
    {
        get => _imageTuplePaths;
        set => SetProperty(ref _imageTuplePaths, value);
    }

    private Dictionary<string, ImageData>? _inputMap;
    private ObservableCollection<ImageData> _inputItems = new();
    public ObservableCollection<ImageData> InputItems
    {
        get => _inputItems;
        set => SetProperty(ref _inputItems, value);
    }
    //public IEnumerable<string> ActiveItems => new List<string>() { @"C:\Users\Robin\InklingAI\SD\images\edit.png" };// InputItems.Where(item => item.IsActive);

    private ObservableCollection<string> _activeItems = new ObservableCollection<string>() { @"C:\Users\Robin\InklingAI\SD\working\edit.png"};
    public List<ImageData> ActiveItems => InputItems.Where(item => item.IsActive).ToList();

    public MainViewModel()
    {
        LoadInputControls();
        LoadNextPage();
}

    private int itemsPerRow = 3;
    private int currentPage = 0;
    private const int itemsPerPage = 31;
    private void LoadNextPage()
    {
        string dirPath = @"C:\Users\Robin\InklingAI\SD\images\"; 
        var allFiles = Directory.GetFiles(dirPath, "*.png")
                         .OrderByDescending(file => new FileInfo(file).LastWriteTime)
                         .ToArray();

        MainImagePath = allFiles.Length > 0 ? allFiles[0] : "";

        var filesToLoad = allFiles.Skip(currentPage * itemsPerPage + 1).Take(itemsPerPage);

        var imagePair = new List<String>();
        foreach (var file in filesToLoad)
        {
            //ImagePaths.Add(file);
            imagePair.Add(file);
            if (imagePair.Count == itemsPerRow)
            {
                ImageTuplePaths.Add(imagePair.ToArray());
                imagePair.Clear();
            }
        }
        currentPage++;
    }

    private void LoadInputControls()
    {
        _inputItems.Clear();
        string dirPath = @"C:\Users\Robin\InklingAI\SD\working\";
        _inputMap = ImageData.GenerateInputItems(dirPath);
        foreach (var val in _inputMap.Values)
        {
            InputItems.Add(val);
        }

    }

    public static MainViewModel? GetRootDataContext(Visual? control)
    {
        MainViewModel? result = null;
        while (control != null && !(control is Window))
        {
            control = control.GetVisualParent();
        }

        if (control is Window window && window.DataContext is MainViewModel mvm)
        {
            result = mvm;
        }
        return result;
    }
}

