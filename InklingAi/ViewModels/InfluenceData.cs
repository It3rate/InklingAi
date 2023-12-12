using InklingAi.ViewModels;

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
            _allItemsMap = GenerateInputItems(dirPath);
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

        public static Dictionary<string, ImageData> GenerateInputItems(string folder)
        {
            Dictionary<string, ImageData> result = new();
            foreach (var kind in inputKinds.Keys)
            {
                var contraints = inputKinds[kind];
                contraints.ImageKind = kind;
                result[kind.Name()] = new ImageData(contraints, folder);
            }
            return result;
        }

        public static readonly Dictionary<ImageKind, ImageConstraints> inputKinds = new()
        {
            //{ "main", "Main Image" },
            { ImageKind.Input, new (){     Name="Input Image" } },
            { ImageKind.Mask, new (){      Name="Mask", ColorAllowance = ColorAllowance.Grayscale } },
            { ImageKind.Canny, new (){     Name="Edges", ColorAllowance = ColorAllowance.BlackAndWhite, MaxWidth=1, FeatherMax=0, AlphaMin=255 }},
            { ImageKind.Depth, new (){     Name="Depth M", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Leres, new (){     Name="Depth LR", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Hed, new (){       Name="Outline", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Openpose, new (){  Name="Pose", ColorAllowance = ColorAllowance.None, MaxWidth=0, FeatherMax=0,  AlphaMin=255 }},
            { ImageKind.Normal, new (){    Name="Normals", ColorAllowance = ColorAllowance.Normal, AlphaMin=255 }},
            { ImageKind.Mlsd, new (){      Name="Lines", ColorAllowance = ColorAllowance.Lines, MaxWidth=1, FeatherMax=0, AlphaMin=255 }},
            { ImageKind.Pidinet, new (){   Name="Sketch", ColorAllowance = ColorAllowance.Grayscale }},
            { ImageKind.Uniformer, new (){ Name="Segment", ColorAllowance = ColorAllowance.ColorSeg, FeatherMax=0,  AlphaMin=255 }},
        };
    }
}










