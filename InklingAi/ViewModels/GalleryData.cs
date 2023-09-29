namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GalleryData : ViewModelBase
    {
        private int itemsPerRow = 3;
        private int currentPage = 0;
        private const int itemsPerPage = 31;

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

        public void LoadNextPage()
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
    }
}
