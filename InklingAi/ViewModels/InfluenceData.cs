namespace InklingAi.ViewModels
{
    using AIEnv.Clients.StableDiffusion;
    using AIEnv.Utils;
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
                VectorImage = _focusedItem != null ? _focusedItem.VectorImage : null;
                ResetActiveItems();
            }
        }

        private VectorImage? _vectorImage;
        public VectorImage? VectorImage
        {
            get => _vectorImage;
            set
            {
                SetProperty(ref _vectorImage, value);
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

        public Dictionary<string, ImageData> GenerateInputItems(string folder)
        {
            Dictionary<string, ImageData> result = new();
            foreach (var kind in inputKinds.Keys)
            {
                var contraints = inputKinds[kind];
                contraints.ImageKind = kind;
                var imageData = new ImageData(contraints, folder);
                result[kind.Name()] = imageData;
                if (kind.IsControlNet())
                {
                    imageData.ControlNetConfig = new ControlNetConfig(kind);
                }
                else if (kind == ImageKind.Mask) // || kind == ImageKind.ScratchPad)
                {
                    //_maskScratchPbs.Add(pb);
                }
            }
            return result;
        }

        public static readonly Dictionary<ImageKind, BrushSettings> inputKinds = new()
        {
            //{ ImageKind.Main, new (){      Name="Main Image", ColorAllowance = ColorAllowance.None } },
            { ImageKind.Input, new (){     Name="Input Image" } },
            { ImageKind.Mask, new (){      Name="Mask", ColorAllowance = ColorAllowance.Grayscale, DefaultColor=0xFFFFFFFF } },
            { ImageKind.Canny, new (){     Name="Edges", ColorAllowance = ColorAllowance.BlackAndWhite, DefaultColor=0xFFFFFFFF, MaxWidth=1, FeatherMax=0, AlphaMin=255 }},
            { ImageKind.Depth, new (){     Name="Depth M", ColorAllowance = ColorAllowance.Grayscale, DefaultColor=0xFFDDDDDD }},
            { ImageKind.Leres, new (){     Name="Depth LR", ColorAllowance = ColorAllowance.Grayscale, DefaultColor=0xFFDDDDDD }},
            { ImageKind.Hed, new (){       Name="Outline", ColorAllowance = ColorAllowance.Grayscale, DefaultColor=0xFFFFFFFF }},
            { ImageKind.Openpose, new (){  Name="Pose", ColorAllowance = ColorAllowance.None, MaxWidth=4, FeatherMax=0,  AlphaMin=255 }},
            { ImageKind.Normal, new (){    Name="Normals", ColorAllowance = ColorAllowance.Normal, AlphaMin=255 }},
            { ImageKind.Mlsd, new (){      Name="Lines", ColorAllowance = ColorAllowance.Lines, DefaultColor=0xFFFFFFFF, MaxWidth=1, FeatherMax=0, AlphaMin=255 }},
            { ImageKind.Pidinet, new (){   Name="Sketch", ColorAllowance = ColorAllowance.Grayscale, DefaultColor=0xFFFFFFFF }},
            { ImageKind.Uniformer, new (){ Name="Segment", ColorAllowance = ColorAllowance.ColorSeg, FeatherMax=0,  AlphaMin=255 }},
        };
    }
}










