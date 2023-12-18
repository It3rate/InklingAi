namespace InklingAi.StableDiffusion
{
    using AIEnv.Clients.StableDiffusion;
    using AIEnv.Utils;
    using InklingAi.Utils;
    using InklingAi.ViewModels;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class StableDiffusionConfigUI : ViewModelBase
    {
        public StableDiffusionConfig Config = new StableDiffusionConfig();

        public string Prompt
        {
            get => Config.BasePrompt;
            set
            {
                if (Config.BasePrompt != value)
                {
                    Config.BasePrompt = value;
                    OnPropertyChanged(nameof(Prompt));
                }
            }
        }
        public string StylePrompt
        {
            get => Config.StylePrompt;
            set
            {
                if (Config.StylePrompt != value)
                {
                    Config.StylePrompt = value;
                    OnPropertyChanged(nameof(StylePrompt));
                }
            }
        }
        public string NegativePrompt
        {
            get => Config.NegativePrompt;
            set
            {
                if (Config.NegativePrompt != value)
                {
                    Config.NegativePrompt = value;
                    OnPropertyChanged(nameof(NegativePrompt));
                }
            }
        }
        public int NumInferenceSteps
        {
            get => Config.NumInferenceSteps;
            set
            {
                if (Config.NumInferenceSteps != value)
                {
                    Config.NumInferenceSteps = value;
                    OnPropertyChanged(nameof(NumInferenceSteps));
                }
            }
        }
        public float GuidanceScale
        {
            get => Config.GuidanceScale;
            set
            {
                if (Config.GuidanceScale != value)
                {
                    Config.GuidanceScale = value;
                    OnPropertyChanged(nameof(GuidanceScale));
                }
            }
        }

        public int Seed
        {
            get => Config.Seed;
            set
            {
                if (Config.Seed != value)
                {
                    Config.Seed = value;
                    OnPropertyChanged(nameof(Seed));
                }
            }
        }
        public int CallbackSteps
        {
            get => Config.CallbackSteps;
            set
            {
                if (Config.CallbackSteps != value)
                {
                    Config.CallbackSteps = value;
                    OnPropertyChanged(nameof(CallbackSteps));
                }
            }
        }
        public string InitialImageName
        {
            get => Config.InitialImageName;
            set
            {
                if (Config.InitialImageName != value)
                {
                    Config.InitialImageName = value;
                    OnPropertyChanged(nameof(InitialImageName));
                }
            }
        }
        public string InitialLatentsName
        {
            get => Config.InitialLatentsName;
            set
            {
                if (Config.InitialLatentsName != value)
                {
                    Config.InitialLatentsName = value;
                    OnPropertyChanged(nameof(InitialLatentsName));
                }
            }
        }
        public string GeneratedImageName
        {
            get => Config.GeneratedImageName;
            set
            {
                if (Config.GeneratedImageName != value)
                {
                    Config.GeneratedImageName = value;
                    OnPropertyChanged(nameof(GeneratedImageName));
                }
            }
        }
        public string MaskImageName
        {
            get => Config.MaskImageName;
            set
            {
                if (Config.MaskImageName != value)
                {
                    Config.MaskImageName = value;
                    OnPropertyChanged(nameof(MaskImageName));
                }
            }
        }
        public int[] MaskBounds
        {
            get => Config.MaskBounds;
            set
            {
                if (Config.MaskBounds != value)
                {
                    Config.MaskBounds = value;
                    OnPropertyChanged(nameof(MaskBounds));
                }
            }
        }
        public float ImagePreservation
        {
            get => Config.ImagePreservation;
            set
            {
                if (Config.ImagePreservation != value)
                {
                    Config.ImagePreservation = value;
                    OnPropertyChanged(nameof(ImagePreservation));
                }
            }
        }
        public int Width
        {
            get => Config.Width;
            set
            {
                if (Config.Width != value)
                {
                    Config.Width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }
        public int Height
        {
            get => Config.Height;
            set
            {
                if (Config.Height != value)
                {
                    Config.Height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }
        public string[] ActiveInputs
        {
            get => Config.ActiveInputs;
            set
            {
                if (Config.ActiveInputs != value)
                {
                    Config.ActiveInputs = value;
                    OnPropertyChanged(nameof(ActiveInputs));
                }
            }
        }
        public int StartInferenceStep
        {
            get => Config.StartInferenceStep;
            set
            {
                if (Config.StartInferenceStep != value)
                {
                    Config.StartInferenceStep = value;
                    OnPropertyChanged(nameof(StartInferenceStep));
                }
            }
        }
        public string InitialImageSource
        {
            get => Config.InitialImageSource;
            set
            {
                if (Config.InitialImageSource != value)
                {
                    Config.InitialImageSource = value;
                    OnPropertyChanged(nameof(InitialImageSource));
                }
            }
        }
        public IntSize AspectSize
        {
            get => Config.AspectSize;
            set
            {
                if (Config.AspectSize != value)
                {
                    Config.AspectSize = value;
                    OnPropertyChanged(nameof(AspectSize));
                }
            }
        }
        public bool SeedUnlocked
        {
            get => Config.SeedUnlocked;
            set
            {
                if (Config.SeedUnlocked != value)
                {
                    Config.SeedUnlocked = value;
                    OnPropertyChanged(nameof(SeedUnlocked));
                }
            }
        }

    }
}
