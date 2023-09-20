namespace InklingAi.ViewModels;

using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public partial class MainViewModel : ViewModelBase
{
    public static string Title => "Inkling";

    private ObservableCollection<string> _imagePaths = new();
    public ObservableCollection<string> ImagePaths
    {
        get => _imagePaths;
        set => SetProperty(ref _imagePaths, value);
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
    private const int itemsPerPage = 10;
    private void LoadNextPage()
    {
        string dirPath = @"C:\Users\Robin\InklingAI\SD\images\"; 
        var allFiles = Directory.GetFiles(dirPath, "*.png")
                         .OrderByDescending(file => new FileInfo(file).LastWriteTime)
                         .ToArray();
        var filesToLoad = allFiles.Skip(currentPage * itemsPerPage).Take(itemsPerPage);

        foreach (var file in filesToLoad)
        {
            ImagePaths.Add(file);
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

