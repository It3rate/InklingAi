namespace InklingAi.StableDiffusion
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using InklingAi.Utils;

    public enum ModuleKind
    {
        ControlNet,
        T2ia,
        Lora,
        TextualInversion,
        Custom,
    }
    public abstract class SideModuleConfig
    {
        public ModuleKind ModuleKind { get; set; } = ModuleKind.ControlNet;

        [JsonProperty("extra_params")]
        public float[] ExtraParamValues { get; set; } = new float[0];
        [JsonProperty("extra_paramNames")]
        public string[] ExtraParamNames { get; set; } = new string[0];

        public float[] ExtraParamDefaults { get; set; } = new float[0];
        public float[] ExtraParamMins { get; set; } = new float[0];
        public float[] ExtraParamMaxs { get; set; } = new float[0];
        public float[] ExtraParamSteps { get; set; } = new float[0];
        public string[] ExtraParamAPINames { get; set; } = new string[0];

        //public abstract Task<string> RunFilter();
        //public abstract Task<string> RunFilter(string source, string destination);
    }

    public class ControlNetConfig : SideModuleConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = false;
        [JsonProperty("module")]
        public string Module { get; set; } = ""; // mlsd
        [JsonProperty("model")]
        public string Model { get; set; } = ""; // control_mlsd - fp16[e3705cfa]
        [JsonProperty("weight")]
        public float Weight { get; set; } = 1f;
        [JsonProperty("guidance_start")]
        public float GuidanceStart { get; set; } = 0f;
        [JsonProperty("guidance_end")]
        public float GuidanceEnd { get; set; } = 1f;

        [JsonProperty("input_path")]
        public string InputPath { get; set; } = "";
        [JsonProperty("image_path")]
        public string ImagePath { get; set; } = "";
        [JsonProperty("upload_path")]
        public string UploadPath { get; set; } = "";

        [JsonConstructor]
        public ControlNetConfig(
            string module, string model, float weight = 1f, float guidanceStart = 0f, float guidanceEnd = 1f,
            float param0 = 0f, float param1 = 0f, params float[] extraParams)
        {
            Module = module;
            Model = model;
            Weight = weight;
            GuidanceStart = guidanceStart;
            GuidanceEnd = guidanceEnd;
            ExtraParamValues = extraParams;

            InputPath = IOUtils.ImagePath(Module, true);
            ImagePath = IOUtils.ImagePath(Module, false);
            UploadPath = IOUtils.SDUploadsFolder + IOUtils.ImageName(Module, false);
        }
        public ControlNetConfig(string moduleName) : this(moduleName, "x")
        {
            switch (moduleName)
            {
                case "canny":
                    ExtraParamValues = new float[] { 100, 200 };
                    ExtraParamMins = new float[] { 0, 0 };
                    ExtraParamMaxs = new float[] { 255, 255 };
                    ExtraParamSteps = new float[] { 1, 1 };
                    ExtraParamNames = new string[] { "Low Threshold", "High Threshold" };
                    ExtraParamAPINames = new string[] { "low", "high" };
                    break;
                case "leres":
                    ExtraParamValues = new float[] { 0.0f, 0.0f };
                    ExtraParamMins = new float[] { 0.00f, 0.00f };
                    ExtraParamMaxs = new float[] { 100f, 100f };
                    ExtraParamSteps = new float[] { 1f, 1f };
                    ExtraParamNames = new string[] { "Remove Near", "Remove Background" };
                    ExtraParamAPINames = new string[] { "near", "bkg" };
                    break;
                case "mlsd":
                    ExtraParamValues = new float[] { 0.2f, 0.7f };
                    ExtraParamMins = new float[] { 0.0f, 0.0f };
                    ExtraParamMaxs = new float[] { 1f, 40f };
                    ExtraParamSteps = new float[] { 0.01f, 0.1f };
                    ExtraParamNames = new string[] { "Value Threshold", "Distance Threshold" };
                    ExtraParamAPINames = new string[] { "hough_value", "hough_thresh" };
                    break;
                case "openpose":
                    ExtraParamValues = new float[] { 0f };
                    ExtraParamMins = new float[] { 0f };
                    ExtraParamMaxs = new float[] { 1f };
                    ExtraParamSteps = new float[] { 1f };
                    ExtraParamNames = new string[] { "Show Hands" };
                    ExtraParamAPINames = new string[] { "hand" };
                    break;
                case "normal":
                    ExtraParamValues = new float[] { 6.283f, 0.1f };
                    ExtraParamMins = new float[] { 0f, 0.0f };
                    ExtraParamMaxs = new float[] { 12.57f, 1f };
                    ExtraParamSteps = new float[] { .314f, 0.01f };
                    ExtraParamNames = new string[] { "Init", "Remove Background" };
                    ExtraParamAPINames = new string[] { "init", "bkg_thresh" };
                    break;
                case "tile":
                    ExtraParamValues = new float[] { 1 };
                    ExtraParamMins = new float[] { 1f };
                    ExtraParamMaxs = new float[] { 8f };
                    ExtraParamSteps = new float[] { 0.01f };
                    ExtraParamNames = new string[] { "Down Sampling Rate" };
                    break;
                case "reference":
                    ExtraParamValues = new float[] { 0.5f };
                    ExtraParamMins = new float[] { 0f };
                    ExtraParamMaxs = new float[] { 1f };
                    ExtraParamSteps = new float[] { 0.01f };
                    ExtraParamNames = new string[] { "Style Fidelity" };
                    break;
                default:
                    break;
            }
            ExtraParamDefaults = (float[])ExtraParamValues.Clone();
        }

        public ControlNetConfig(ControlNetConfig config)
        {
            CopyPropertiesFrom(config);
        }
        private ControlNetConfig()
        {
        }
        public ControlNetConfig Clone() => new ControlNetConfig(this);
        public void CopyPropertiesFrom(ControlNetConfig config)
        {
            Enabled = config.Enabled;
            Module = config.Module;
            Model = config.Model;
            Weight = config.Weight;
            GuidanceStart = config.GuidanceStart;
            GuidanceEnd = config.GuidanceEnd;
            ExtraParamValues = config.ExtraParamValues;
            ExtraParamNames = config.ExtraParamNames;
            InputPath = config.InputPath;
            ImagePath = config.ImagePath;
            UploadPath = config.UploadPath;
        }

        public void SetParam(int index, float value)
        {
            if (index >= ExtraParamValues.Length)
            {
                float[] extendedArray = new float[index];
                Array.Copy(ExtraParamValues, extendedArray, ExtraParamValues.Length);
                ExtraParamValues = extendedArray;
            }
            ExtraParamValues[index] = value;
        }
        public Dictionary<string, float> GetParams()
        {
            var result = new Dictionary<string, float>();
            for (int i = 0; i < ExtraParamValues.Length; i++)
            {
                result.Add(ExtraParamAPINames[i], ExtraParamValues[i]);
            }
            return result;
        }

        public Dictionary<string, object> ConfigDict()
        {
            return new Dictionary<string, object>
            {
                {"enabled", Enabled },
                {"module", Module },
                {"model", Model },
                {"weight", Weight },
                {"guidance_start", GuidanceStart },
                {"guidance_end", GuidanceEnd },
                {"extra_params", ExtraParamValues },
                {"extra_param_names", ExtraParamNames },
                {"input_path", InputPath },
                {"image_path", ImagePath },
                {"upload_path", UploadPath },
            };
        }
        public string GetJson()
        {
            var config = ConfigDict();
            return JsonConvert.SerializeObject(config, Formatting.Indented);

        }

        public static ControlNetConfig ParseExif(string description)
        {
            ControlNetConfig result = new ControlNetConfig();

            var controlNetConfigs = new List<ControlNetConfig>();
            // A1111 uses nested values like "(0, 1)" to serialize controlnets, so pull that out first.
            int count = 0;
            List<string> nestedValues = new List<string>();
            description = Regex.Replace(description, "\\((.*?)\\)", match => {
                nestedValues.Add(match.Groups[1].Value);
                return "#" + count++ + "#";
            });

            string[] separators = { "\r\n", "\r", "\n" };
            string[] lines = description.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 1)
            {
                var settings = lines[0].Split(",");
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
                    }
                    result.AssignFromExifName(sp);
                }

            }

            return result;
        }
        public string GetExifString()
        {
            var sb = new StringBuilder();
            sb.Append("preprocessor: " + Module + ", ");
            sb.Append("model: " + Model + ", ");
            sb.Append("weight: " + Weight.ToString() + ", ");
            sb.Append("starting/ending: (" + GuidanceStart + ", " + GuidanceEnd + "), ");
            sb.Append("preprocessor params: (" + string.Join(",", ExtraParamValues) + ")");
            //sb.Append("resize mode: 0");
            //sb.Append("pixel perfect: 0");
            //sb.Append("control mode: 0");
            return sb.ToString();
        }
        private void AssignFromExifName(string[] nameValue)
        {
            try
            {
                if (nameValue.Length == 2)
                {
                    var name = nameValue[0].Trim();
                    var value = nameValue[1].Trim();
                    switch (name.ToLowerInvariant())
                    {
                        case "preprocessor":
                            Module = value;
                            break;
                        case "model":
                            Model = value;
                            break;
                        case "weight":
                            Weight = float.Parse(value);
                            break;
                        case "starting/ending":
                            var vals = value.Split(',');
                            if (vals.Length == 2)
                            {
                                GuidanceStart = float.Parse(vals[0]);
                                GuidanceEnd = float.Parse(vals[1]);
                            }
                            break;
                        case "preprocessor params":
                            var ppVals = value.Split(',');
                            for (int i = 0; i < ppVals.Length; i++)
                            {
                                if (ppVals[i] != "")
                                {
                                    SetParam(i, float.Parse(ppVals[i]));
                                }
                            }
                            break;
                        case "resize mode":
                        case "pixel perfect":
                        case "control mode":
                        default:
                            break;

                    };
                }

            }
            catch (Exception ex) { }
        }

        //public override async Task<string> RunFilter(string source, string destination)
        //{
        //    var client = StableDiffusionClient.Instance;
        //    var success = await client.RunControlNetFilter(this, source, destination);
        //    return success;
        //}
        //public override async Task<string> RunFilter()
        //{
        //    var client = StableDiffusionClient.Instance;
        //    return await client.RunControlNetFilters(new string[] { Module });
        //}
        //public void WriteJson(string path)
        //{
        //    File.WriteAllText(path, GetJson());
        //}
        //public static ControlNetConfig? LoadFromJson(string path)
        //{
        //    ControlNetConfig? result = null;
        //    if (File.Exists(path))
        //    {
        //        var jsonString = File.ReadAllText(path);
        //        result = JsonConvert.DeserializeObject<ControlNetConfig>(jsonString);
        //    }
        //    return result;
        //}








    }
}
