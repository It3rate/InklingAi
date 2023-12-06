namespace InklingAi.ViewModels
{
    using DynamicData;
    using InklingAi.Utils;
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
            if(FocusedItem != null)
            {
                CompositedItems = new ObservableCollection<ImageData>(new ImageData[] { FocusedItem });
            }
        }

        private ObservableCollection<ImageData> _compositedItems = new();
        public ObservableCollection<ImageData> CompositedItems
        {
            get => _compositedItems;
            set => SetProperty(ref _compositedItems, value);
        }
        private ImageData? _focusedItem;
        public ImageData? FocusedItem
        {
            get => _focusedItem;
            set
            {
                _focusedItem = value;
                ResetActiveItems();
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
