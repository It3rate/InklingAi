namespace AIEnv.Clients.StableDiffusion
{
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

        public abstract Task<string> RunFilter();
        public abstract Task<string> RunFilter(string source, string destination);
    }

}
