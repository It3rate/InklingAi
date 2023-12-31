namespace AIEnv.Clients.StableDiffusion
{
    using AIEnv.Utils;
    using System.Reflection;
    using Newtonsoft.Json;

    public enum ModuleKind
    {
        Mask,
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

        [JsonProperty("module")]
        public string Module { get; set; } = ""; // mlsd

        [JsonProperty("input_path")]
        public string InputPath { get; set; } = "";
        [JsonProperty("image_path")]
        public string ImagePath { get; set; } = "";
        [JsonProperty("upload_path")]
        public string UploadPath { get; set; } = "";

        public SideModuleConfig(string module)
        {
            SetPaths(module);
        }
        protected void SetPaths(string moduleName)
        {
            Module = moduleName;
            InputPath = IOUtils.ImagePath(Module, true);
            ImagePath = IOUtils.ImagePath(Module, false);
            UploadPath = IOUtils.SDUploadsFolder + IOUtils.ImageName(Module, false);
        }
        public abstract Task<string> RunFilter();
        public abstract Task<string> RunFilter(string source, string destination);
    }

}
