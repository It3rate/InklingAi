namespace InklingAi.ViewModels
{
    using Avalonia;
	using InklingAi.Utils;
	using InklingAi.Views;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GalleryData : ViewModelBase
    {
        private int itemsPerRow = 4;
        private int currentPage = 0;
        private const int itemsPerPage = 50;

        //private ObservableCollection<string[]> _imageTuplePaths = new();
        //public ObservableCollection<string[]> ImageTuplePaths
        //{
        //    get => _imageTuplePaths;
        //    set => SetProperty(ref _imageTuplePaths, value);
        //}

        //private ObservableCollection<SKBitmap[]> _imageTuples = new();
        //public ObservableCollection<SKBitmap[]> ImageTuples
        //{
        //    get => _imageTuples;
        //    set => SetProperty(ref _imageTuples, value);
        //}

        private ObservableCollection<ProcessedImageData[]> _imageData = new();
        public ObservableCollection<ProcessedImageData[]> ImageData
        {
            get => _imageData;
            set => SetProperty(ref _imageData, value);
        }

        public void LoadNextPage(int startIndex = -1)
        {
            if(startIndex >= 0)
            {
                currentPage = startIndex;
            }
            string dirPath = InklingIOUtils.SDImageFolder;
            var allFiles = Directory.GetFiles(dirPath, "*.png")
                             .OrderByDescending(file => new FileInfo(file).LastWriteTime)
                             .ToArray();

            var filesToLoad = allFiles.Skip(currentPage * itemsPerPage + 1).Take(itemsPerPage);

            var imageData = new ObservableCollection<ProcessedImageData[]>();
            var imageDataSet = new List<ProcessedImageData>();
            //var imageTuplePaths = new ObservableCollection<string[]>();
            //var imageTuples = new ObservableCollection<SKBitmap[]>();
            //var imagePathSet = new List<String>();
            //var imageSet = new List<SKBitmap>();
            foreach (var file in filesToLoad)
            {
                imageDataSet.Add(new ProcessedImageData(file));
                if (imageDataSet.Count == itemsPerRow)
                {
                    imageData.Add(imageDataSet.ToArray());
                    imageDataSet.Clear();
                }
                //imagePathSet.Add(file);
                //var img = InklingIOUtils.LoadSKImage(file);
                //if(img != null)
                //{
                //    imageSet.Add(img);
                //}

                //if (imagePathSet.Count == itemsPerRow)
                //{
                //    imageTuplePaths.Add(imagePathSet.ToArray());
                //    imageTuples.Add(imageSet.ToArray());
                //    imagePathSet.Clear();
                //    imageSet.Clear();
                //}
            }
            //ImageTuplePaths = imageTuplePaths;
            //ImageTuples = imageTuples;
            ImageData = imageData;
            currentPage++;
        }
    }
}
