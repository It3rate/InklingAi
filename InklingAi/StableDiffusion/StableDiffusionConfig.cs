namespace InklingAi.StableDiffusion
{
    using InklingAi.Utils;
    using InklingAi.ViewModels;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class StableDiffusionConfig : ViewModelBase
    {
        private string _prompt = "Gunslinger, wild west";
        private string _stylePrompt = "colorful, Victorian illustration, decorative border, style of Alphonse Mucha, Turner";
        private string _negativePrompt = "lowres, error, cropped, worst quality, low quality, jpeg artifacts, out of frame, watermark, signature";
        private int _numInferenceSteps = 25;
        private float _guidanceScale = 7.5f;
        private int _seed = -1;
        private int _callbackSteps = 1;
        private string _initialImageName = "";
        private string _initialLatentsName = "";
        private string _generatedImageName = "";
        private string _maskImageName = "";
        private int[] _maskBounds = new int[0];
        private float _imagePreservation = 1f; // noise strength
        private string[] _activeInputs = new string[0];

        private int _startInferenceStep = 0;
        private string _initialImageSource = "";

        [JsonProperty("promptName")]
        public string Prompt { get => _prompt; set => SetProperty(ref _prompt, value); }
        [JsonProperty("style_prompt_name")]
        public string StylePrompt { get => _stylePrompt; set => SetProperty(ref _stylePrompt, value); }
        [JsonProperty("negative_prompt")]
        public string NegativePrompt { get => _negativePrompt; set => SetProperty(ref _negativePrompt, value); }
        [JsonProperty("num_inference_steps")]
        public int NumInferenceSteps { get => _numInferenceSteps; set => SetProperty(ref _numInferenceSteps, value); }
        [JsonProperty("guidance_scale")]
        public float GuidanceScale { get => _guidanceScale; set => SetProperty(ref _guidanceScale, value); }
        // todo: Sampler, batchSize, maskBlur, batchPos 
        [JsonProperty("seed")]
        public int Seed { get => _seed; set => SetProperty(ref _seed, value); }
        [JsonProperty("callback_steps")]
        public int CallbackSteps { get => _callbackSteps; set => SetProperty(ref _callbackSteps, value); }
        [JsonProperty("initial_image_name")]
        public string InitialImageName { get => _initialImageName; set => SetProperty(ref _initialImageName, value); }
        [JsonProperty("initial_latents_name")]
        public string InitialLatentsName { get => _initialLatentsName; set => SetProperty(ref _initialLatentsName, value); }
        [JsonProperty("generated_image_name")]
        public string GeneratedImageName { get => _generatedImageName; set => SetProperty(ref _generatedImageName, value); }
        [JsonProperty("mask_image_name")]
        public string MaskImageName { get => _maskImageName; set => SetProperty(ref _maskImageName, value); }
        [JsonProperty("mask_bounds")]
        public int[] MaskBounds { get => _maskBounds; set => SetProperty(ref _maskBounds, value); }
        [JsonProperty("image_preservation")]
        public float ImagePreservation { get => _imagePreservation; set => SetProperty(ref _imagePreservation, value); }
        [JsonProperty("width")]
        public int Width { get => _aspectSize.Width; set => SetProperty(ref _aspectSize, new IntSize(value, _aspectSize.Height)); }
        [JsonProperty("height")]
        public int Height { get => _aspectSize.Height; set => SetProperty(ref _aspectSize, new IntSize(_aspectSize.Width, value)); }
        [JsonProperty("active_inputs")]
        public string[] ActiveInputs { get => _activeInputs; set => SetProperty(ref _activeInputs, value); }

        [JsonProperty("start_inference_step")]
        public int StartInferenceStep { get => _startInferenceStep; set => SetProperty(ref _startInferenceStep, value); }
        [JsonProperty("initial_image_source")]
        public string InitialImageSource { get => _initialImageSource; set => SetProperty(ref _initialImageSource, value); }


        [JsonIgnore]
        public IntSize _aspectSize = new IntSize(640, 512);
        [JsonProperty("aspect_size")]
        public IntSize AspectSize { get => _aspectSize; set => SetProperty(ref _aspectSize, value); }

        [JsonIgnore]
        private bool _seedUnlocked = true;
        [JsonProperty("seed_unlocked")]
        public bool SeedUnlocked
        {
            get => _seedUnlocked;
            set => SetProperty(ref _seedUnlocked, value);
        }


        [JsonProperty("controlnet")]
        public List<ControlNetConfig> ControlNetConfigs = new List<ControlNetConfig>();

        [JsonIgnore]
        public int[]? GeneratedRegionSize;

        [JsonConstructor]
        public StableDiffusionConfig(
            string prompt = "", string stylePrompt = "", string negativePrompt = "", int steps = 25, int startStep = 0, float guidanceScale = 7.5f,
            int seed = -1, int callbackSteps = 5, float imagePreservation = 1f,
            string initialImageName = "", string initialLatentsName = "",
            string generatedImageName = "", string maskImageName = "", int[]? maskBounds = null,
            int width = 640, int height = 512,
            string[]? activeInputs = null, string initialImageSource = "", ControlNetConfig[]? cnetConfigs = null,
            int[]? generatedRegionSize = null)
        {
            Prompt = prompt != "" ? prompt : Prompt;
            StylePrompt = stylePrompt != "" ? stylePrompt : StylePrompt;
            NegativePrompt = negativePrompt != "" ? negativePrompt : NegativePrompt;
            NumInferenceSteps = steps;
            StartInferenceStep = startStep;
            GuidanceScale = guidanceScale;
            Seed = seed > -1 ? seed : new Random().Next(0, int.MaxValue);
            CallbackSteps = callbackSteps;
            ImagePreservation = initialImageName == "" && initialLatentsName == "" ? 1 : imagePreservation;
            InitialImageName = initialImageName;
            InitialLatentsName = initialLatentsName;
            GeneratedImageName = generatedImageName;
            MaskImageName = maskImageName;
            MaskBounds = maskBounds == null ? new int[0] : maskBounds;
            Width = width;
            Height = height;
            ActiveInputs = activeInputs != null ? activeInputs : ActiveInputs;
            InitialImageSource = initialImageSource;
            if (cnetConfigs != null)
            {
                ControlNetConfigs.AddRange(cnetConfigs);
            }
            GeneratedRegionSize = generatedRegionSize;
        }

        public StableDiffusionConfig(StableDiffusionConfig config)
        {
            CopyPropertiesFrom(config);
        }

        public string GenerateImageName()
        {
            var promptName = Prompt;
            promptName = promptName.Trim();
            promptName = Regex.Replace(promptName, @"[^\w-]+", "_");
            promptName = promptName.Substring(0, Math.Min(64, promptName.Length));
            var filename = IOUtils.SDImageFolder + promptName + Seed.ToString() + ".png";
            return filename;
        }
        public bool HasRegion => MaskBounds.Length == 4 && MaskBounds[2] > 0 && MaskBounds[3] > 0;
        public System.Drawing.Size GeneratedSize()
        {
            return HasRegion && GeneratedRegionSize != null ?
                new System.Drawing.Size(GeneratedRegionSize[2], GeneratedRegionSize[3]) :
                new System.Drawing.Size(Width, Height);
        }

        public Dictionary<string, object>? GetConfigDictionary()
        {
            var jsonString = GetJson();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            return dictionary;
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public void WriteJson(string path)
        {
            File.WriteAllText(path, GetJson());
        }
        public static StableDiffusionConfig? LoadFromJson(string path)
        {
            StableDiffusionConfig? result = null;
            if (File.Exists(path))
            {
                var jsonString = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<StableDiffusionConfig>(jsonString);
            }
            return result;
        }

        public static StableDiffusionConfig? ParseExif(string description)
        {
            StableDiffusionConfig? result = null;

            var controlNetConfigs = new List<ControlNetConfig>();
            // A1111 uses nested text to serialize controlnets (ugh), so pull that text out first.
            int count = 0;
            List<string> nestedValues = new List<string>();
            description = Regex.Replace(description, "\"(.*?)\"", match => {
                nestedValues.Add(match.Groups[1].Value);
                return "#" + count++ + "#";
            });

            string[] separators = { "\r\n", "\r", "\n" };
            string[] lines = description.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string stylePromptTag = "Style promptName: ";
            string negPromptTag = "Negative promptName: ";
            if (lines.Length == 4 && lines[2].StartsWith(negPromptTag))
            {
                var prompt = lines[0];
                var stylePrompt = lines[1].Substring(stylePromptTag.Length);
                var negPrompt = lines[2].Substring(negPromptTag.Length);
                result = new StableDiffusionConfig(prompt, stylePrompt, negPrompt);

                var parseIndex = 0;
                var settings = lines[3].Split(",");
                count = 0;
                foreach (var setting in settings)
                {
                    var sp = setting.Split(":");
                    if (sp.Length == 2)
                    {
                        sp[0] = sp[0].Trim();
                        sp[1] = sp[1].Trim();
                        if (sp[1].StartsWith("#") && sp[1].EndsWith("#"))
                        {
                            sp[1] = nestedValues[count++];
                        }
                        result.AssignFromExifName(sp);
                    }
                }
            }

            return result;
        }
        public string GetExifString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(Prompt + "\n");
            sb.Append("Style promptName: " + StylePrompt + "\n");
            sb.Append("Negative promptName: " + NegativePrompt + "\n");

            sb.Append("Steps: " + NumInferenceSteps + ", ");
            sb.Append("Sampler: " + "DDIM" + ", ");
            sb.Append("CFG scale: " + GuidanceScale.ToString("F1") + ", ");
            sb.Append("Seed: " + Seed + ", ");
            sb.Append("SDSize: " + "512x512" + ", ");
            sb.Append("Model hash: " + "81761151" + ", ");
            sb.Append("Batch size: " + "1" + ", ");
            sb.Append("Batch pos: " + "1" + ", ");
            sb.Append("Denoising strength: " + ImagePreservation.ToString("F2") + ", ");
            sb.Append("Mask blur: " + "0"); // lazily taking care of commas in cnet loop, careful adjusting order here

            for (int i = 0; i < ControlNetConfigs.Count; i++)
            {
                var name = ", ControlNet " + i; // lazy comma here
                var val = "\"" + ControlNetConfigs[i].GetExifString() + "\"";
                sb.Append(name + ": " + val);
            }
            return sb.ToString();
        }
        private void AssignFromExifName(string[] nameValue)
        {
            if (nameValue.Length == 2)
            {
                var name = nameValue[0].Trim();
                var value = nameValue[1].Trim();
                switch (name)
                {
                    case "parameters":
                        Prompt = value;
                        break;
                    case "Style promptName":
                        StylePrompt = value;
                        break;
                    case "Negative promptName":
                        NegativePrompt = value;
                        break;
                    case "Steps":
                        NumInferenceSteps = int.Parse(value);
                        break;
                    case "Sampler":
                        // Sampler = value;
                        break;
                    case "CFG scale":
                        GuidanceScale = float.Parse(value);
                        break;
                    case "Seed":
                        Seed = int.Parse(value);
                        break;
                    case "SDSize":
                        var size = value.ToLower().Split("x");
                        Width = int.Parse(size[0]);
                        Height = int.Parse(size[1]);
                        break;
                    case "Model hash":
                        break;
                    case "Batch size":
                        break;
                    case "Batch pos":
                        break;
                    case "Denoising strength":
                        ImagePreservation = float.Parse(value);
                        break;
                    case "Mask blur":
                        break;
                    default:
                        if (name.ToLowerInvariant().StartsWith("controlnet"))
                        {
                            var cnConfig = ControlNetConfig.ParseExif(value);
                            ControlNetConfigs.Add(cnConfig);
                        }

                        break;

                };
            }
        }

        public StableDiffusionConfig Clone() => new StableDiffusionConfig(this);
        public void CopyPropertiesFrom(StableDiffusionConfig config)
        {
            var json = config.GetJson();
            JsonConvert.PopulateObject(json, this);
        }
    }
}
