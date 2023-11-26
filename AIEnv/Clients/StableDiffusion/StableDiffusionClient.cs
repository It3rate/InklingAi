namespace AIEnv.Clients.StableDiffusion;

using AIEnv.Clients;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

public class StableDiffusionClient : ClientBase
{
    private static StableDiffusionClient? _instance;
    public static StableDiffusionClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new StableDiffusionClient();
            }
            return _instance;
        }
    }

    private StableDiffusionClient(string apiKey = "", string modelName = @"runwayml/stable-diffusion-v1-5") : base(apiKey, modelName)
    {
        _hostPort = 9000;
        _embeddingRootName = "clip_";
    }
    public override string Route(string method) => @"http://" + Host + method;

    public async Task<string> GetTokens(string text)
    {
        var url = Route("/get_tokens");
        var json = await RequestJsonAsync(url, GetJson("query", text));
        return json ?? "";
    }
    public async Task<string> GetEmbedding(string text)
    {
        var url = Route("/get_embedding");
        var json = await RequestJsonAsync(url, GetJson("sentences", text));
        return json ?? "";
    }

    public async void SetPrompt()
    {
        var url = Route("/set_prompt");
        var json = await GetJsonAsyncTimeout(url);
    }
    public async void SetVae()
    {
        var url = Route("/set_vae");
        var json = await GetJsonAsyncTimeout(url);
    }
    public async Task<string> GetModel()
    {
        var url = Route("/get_current_model");
        var json = await GetJsonAsyncTimeout(url);
        var modelName = json?["model"].ToString() ?? "";
        return modelName;
    }

    public async Task<string> SetModel(string? name, string? inpaintName, string kind = "unet")
    {
        var result = "";
        if (name != null)
        {
            var url = Route("/set_model");
            var jsonDict = new Dictionary<string, string>();
            jsonDict.Add("kind", kind);
            jsonDict.Add("name", name);
            jsonDict.Add("inpaint_name", inpaintName);
            string query = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
            var fileInfo = await RequestJsonAsync(url, query);
            result = fileInfo?["filepath"].ToString() ?? "";
        }
        return result;
    }

    public async Task<string> Generate(StableDiffusionConfig config)
    {
        var url = Route("/get_image_from_diffuser");
        var fileInfo = await RequestJsonAsync(url, config.GetJson());
        return fileInfo?["filepath"].ToString() ?? "";
    }

    public async void GenerateInitialNoised(StableDiffusionConfig config)
    {
        var url = Route("/gen_initial_noised");
        await RequestJsonAsync(url, config.GetJson());
    }

    public async Task<string> GetSchedulers()
    {
        var url = Route("/get_schedulers");
        var json = await RequestJsonAsync(url);
        return json?["schedulers"] ?? "";
    }
    public async Task<dynamic?> SetScheduler(string schedulerId)
    {
        var url = Route("/set_scheduler");
        var json = await RequestJsonAsync(url, GetJson("scheduler_id", schedulerId));
        return json ?? null;
    }

    public async Task<dynamic?> GetStatusAsync()
    {
        var url = Route("/get_status");
        var json = await RequestJsonAsync(url);
        WasAlive = json != null;
        return json ?? null;
    }

    //var url = Route("/get_image_from_diffuser");
    //var url = Route("/get_status");
    //var url = Route("/get_image_by_id");
    //var url = Route("/get_image_by_path");
    //var url = Route("/stop_running_endpoint");


}
