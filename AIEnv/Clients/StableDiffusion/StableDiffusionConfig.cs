namespace AIEnv.Clients.StableDiffusion;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

public class StableDiffusionConfig
{
    [JsonProperty("prompt")]
    public string Prompt { get; set; } = "";
    [JsonProperty("negative_prompt")]
    public string NegativePrompt { get; set; } = "";
    [JsonProperty("num_inference_steps")]
    public int NumInferenceSteps { get; set; } = 25;
    [JsonProperty("guidance_scale")]
    public float GuidanceScale { get; set; } = 7.5f;
    // todo: Sampler, batchSize, maskBlur, batchPos 
    [JsonProperty("seed")]
    public int Seed { get; set; } = -1;
    [JsonProperty("callback_steps")]
    public int CallbackSteps { get; set; } = 1;
    [JsonProperty("initial_image_name")]
    public string InitialImageName { get; set; } = "";
    [JsonProperty("initial_latents_name")]
    public string InitialLatentsName { get; set; } = "";
    [JsonProperty("generated_image_name")]
    public string GeneratedImageName { get; set; } = "";
    [JsonProperty("mask_image_name")]
    public string MaskImageName { get; set; } = "";
    [JsonProperty("mask_bounds")]
    public int[] MaskBounds { get; set; } = new int[0];
    [JsonProperty("image_preservation")]
    public float ImagePreservation { get; set; } = 1; // noise strength
    [JsonProperty("width")]
    public int Width { get; set; } = 640;
    [JsonProperty("height")]
    public int Height { get; set; } = 512;
    [JsonProperty("active_inputs")]
    public string[] ActiveInputs { get; set; } = new string[0];

    [JsonProperty("start_inference_step")]
    public int StartInferenceStep { get; set; } = 0;
    [JsonProperty("initial_image_source")]
    public string InitialImageSource { get; set; } = "";


    [JsonProperty("controlnet")]
    public List<ControlNetConfig> ControlNetConfigs = new List<ControlNetConfig>();

    [JsonIgnore]
    public int[]? GeneratedRegionSize;

    [JsonConstructor]
    public StableDiffusionConfig(
        string prompt, string negativePrompt, int steps = 25, int startStep = 0, float guidanceScale = 7.5f,
        int seed = -1, int callbackSteps = 5, float imagePreservation = 1f,
        string initialImageName = "", string initialLatentsName = "",
        string generatedImageName = "", string maskImageName = "", int[]? maskBounds = null,
        int width = 640, int height = 512,
        string[]? activeInputs = null, string initialImageSource = "", ControlNetConfig[]? cnetConfigs = null,
        int[]? generatedRegionSize = null)
    {
        Prompt = prompt;
        NegativePrompt = negativePrompt;
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
    public StableDiffusionConfig Clone() => new StableDiffusionConfig(this);
    public void CopyPropertiesFrom(StableDiffusionConfig config)
    {
        Prompt = config.Prompt;
        NegativePrompt = config.NegativePrompt;
        NumInferenceSteps = config.NumInferenceSteps;
        StartInferenceStep = config.StartInferenceStep;
        GuidanceScale = config.GuidanceScale;
        Seed = config.Seed;
        CallbackSteps = config.CallbackSteps;
        ImagePreservation = config.ImagePreservation;
        InitialImageName = config.InitialImageName;
        InitialLatentsName = config.InitialLatentsName;
        GeneratedImageName = config.GeneratedImageName;
        MaskImageName = config.MaskImageName;
        MaskBounds = (int[])config.MaskBounds.Clone();
        Width = config.Width;
        Height = config.Height;
        ActiveInputs = config.ActiveInputs;
        InitialImageSource = config.InitialImageSource;
        GeneratedRegionSize = config.GeneratedRegionSize;
    }

    public Dictionary<string, object> ConfigDict()
    {
        return new Dictionary<string, object>
        {
            { "prompt", Prompt },
            { "negative_prompt", NegativePrompt },
            { "num_inference_steps", NumInferenceSteps },
            { "start_inference_step", StartInferenceStep },
            { "guidance_scale", GuidanceScale },
            { "seed", Seed },
            { "callback_steps", CallbackSteps },
            { "image_preservation", ImagePreservation },
            { "initial_image_name", InitialImageName },
            { "initial_latents_name", InitialLatentsName },
            { "generated_image_name", GeneratedImageName },
            { "mask_image_name", MaskImageName },
            { "mask_bounds", MaskBounds },
            { "width", Width },
            { "height", Height },
            { "active_inputs", ActiveInputs },
            { "initial_image_source", InitialImageSource },
        };
    }
    public string GetJson()
    {
        var config = ConfigDict();
        return JsonConvert.SerializeObject(config, Formatting.Indented);

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
        description = Regex.Replace(description, "\"(.*?)\"", match =>
        {
            nestedValues.Add(match.Groups[1].Value);
            return "#" + count++ + "#";
        });

        string[] separators = { "\r\n", "\r", "\n" };
        string[] lines = description.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        string negPromptTag = "Negative prompt: ";
        if (lines.Length == 3 && lines[1].StartsWith(negPromptTag))
        {
            var prompt = lines[0];
            var negPrompt = lines[1].Substring(negPromptTag.Length);
            result = new StableDiffusionConfig(prompt, negPrompt);

            var parseIndex = 0;
            var settings = lines[2].Split(",");
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
        var sb = new StringBuilder();
        sb.Append(Prompt + "\n");
        sb.Append("Negative prompt: " + NegativePrompt + "\n");

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
                case "Negative prompt":
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

    public bool HasRegion => MaskBounds.Length == 4 && MaskBounds[2] > 0 && MaskBounds[3] > 0;
    public System.Drawing.Size GeneratedSize()
    {
        return HasRegion ? new System.Drawing.Size(GeneratedRegionSize[2], GeneratedRegionSize[3]) : new System.Drawing.Size(Width, Height);
    }
}
