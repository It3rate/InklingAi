namespace InklingAi.ViewModels
{
    using Avalonia;
	using InklingAi.Utils;
	using InklingAi.Views;
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

        private ObservableCollection<string[]> _imageTuplePaths = new();
        public ObservableCollection<string[]> ImageTuplePaths
        {
            get => _imageTuplePaths;
            set => SetProperty(ref _imageTuplePaths, value);
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

            var imageTuplePaths = new ObservableCollection<string[]>();
            var imagePair = new List<String>();
            foreach (var file in filesToLoad)
            {
                imagePair.Add(file);
                if (imagePair.Count == itemsPerRow)
                {
                    imageTuplePaths.Add(imagePair.ToArray());
                    imagePair.Clear();
                }
            }
            ImageTuplePaths = imageTuplePaths;
            currentPage++;
        }
    }
}
