namespace InklingAi.ViewModels;

using Avalonia;
using Avalonia.Controls;
using InklingAi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public partial class MainViewModel : ViewModelBase
{
    public static string Title => "Inkling";

    //private ObservableCollection<string> _imagePaths = new();
    //public ObservableCollection<string> ImagePaths
    //{
    //    get => _imagePaths;
    //    set => SetProperty(ref _imagePaths, value);
    //}

    private string _mainImagePath;
    public string MainImagePath
    {
        get => _mainImagePath;
        set => SetProperty(ref _mainImagePath, value);
    }

    private ObservableCollection<string[]> _imagePairPaths = new();
    public ObservableCollection<string[]> ImagePairPaths
    {
        get => _imagePairPaths;
        set => SetProperty(ref _imagePairPaths, value);
    }


    private Dictionary<string, InputItem>? _inputMap;
    private ObservableCollection<InputItem> _inputItems = new();
    public ObservableCollection<InputItem> InputItems
    {
        get => _inputItems;
        set => SetProperty(ref _inputItems, value);
    }

    public MainViewModel()
    {
        LoadInputControls();
        LoadNextPage();
}

    private int currentPage = 0;
    private const int itemsPerPage = 30;
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
            if (imagePair.Count == 2)
            {
                ImagePairPaths.Add(imagePair.ToArray());
                imagePair.Clear();
            }
        }
        currentPage++;
    }

    private void LoadInputControls()
    {
        _inputItems.Clear();
        string dirPath = @"C:\Users\Robin\InklingAI\SD\working\";
        _inputMap = InputItem.GenerateInputItems(dirPath);
        foreach (var val in _inputMap.Values)
        {
            InputItems.Add(val);
        }

    }

}

