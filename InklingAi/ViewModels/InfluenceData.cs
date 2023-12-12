namespace InklingAi.ViewModels
{
    using InklingAi.Utils;
    using InklingAi.Views;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class InfluenceData : ViewModelBase
    {
        private Dictionary<string, ImageData> _allItemsMap = new();

        private ObservableCollection<ImageData> _inputItems = new();
        public ObservableCollection<ImageData> InputItems
        {
            get => _inputItems;
            set => SetProperty(ref _inputItems, value);
        }
        public void ResetInputItems()
        {
            InputItems = new ObservableCollection<ImageData>(_allItemsMap.Values.Where(item => item.IsApplicable));
        }

        private ObservableCollection<ImageData> _activeItems = new();
        public ObservableCollection<ImageData> ActiveItems
        {
            get => _activeItems;
            set => SetProperty(ref _activeItems, value);    
        }
        public void ResetActiveItems()
        {
            ActiveItems = new ObservableCollection<ImageData>(InputItems.Where(item => item.IsActive));
        }
        private ImageData? _focusedItem;
        public ImageData? FocusedItem
        {
            get => _focusedItem;
            set
            {
                SetProperty(ref _focusedItem, value);
                PaintableImage = _focusedItem != null ? _focusedItem.PaintableImage : null;
                ResetActiveItems();
            }
        }
        //public PaintableImage? PaintableImage => FocusedItem != null ? FocusedItem.PaintableImage : null;
        private PaintableImage? _paintableImage;
        public PaintableImage? PaintableImage
        {
            get => _paintableImage;
            set
            {
                SetProperty(ref _paintableImage, value);
            }
        }

        public void LoadInputControls()
        {
            _inputItems.Clear();
            string dirPath = InklingIOUtils.SDWorkingFolder;// @"C:\Users\Robin\InklingAI\SD\working\";
            _allItemsMap = ImageData.GenerateInputItems(dirPath);
            ResetInputItems();
        }
        public void AllowControlNet(bool allowControlNet)
        {
            foreach(var item in _allItemsMap.Values)
            {
                if (item.ImageKind.IsControlNet())
                {
                    item.IsApplicable = allowControlNet;
                }
            }
            ResetInputItems();
            ResetActiveItems();
        }
    }
}
