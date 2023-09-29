namespace InklingAi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class InfluenceData : ViewModelBase
    {
        private Dictionary<string, ImageData>? _inputMap;
        private ObservableCollection<ImageData> _inputItems = new();
        public ObservableCollection<ImageData> InputItems
        {
            get => _inputItems;
            set => SetProperty(ref _inputItems, value);
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

        public void LoadInputControls()
        {
            _inputItems.Clear();
            string dirPath = @"C:\Users\Robin\InklingAI\SD\working\";
            _inputMap = ImageData.GenerateInputItems(dirPath);
            foreach (var val in _inputMap.Values)
            {
                InputItems.Add(val);
            }

        }
    }
}
