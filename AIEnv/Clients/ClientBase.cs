namespace AIEnv.Clients;

using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AIEnv.Clients.StableDiffusion;
using AIEnv.Embeddings;
using AIEnv.Utils;
using SkiaSharp;

public abstract class ClientBase : IDisposable
{
    public delegate Task<Embedding[]> LoadEmbedding(string[] sentences);

    protected string _hostIP = IOUtils.LocalIPAddress()?.ToString() ?? "127.0.0.1";
    protected int _hostPort = 9000;
    protected string Host => _hostIP + ":" + _hostPort + "/";
    protected string _embeddingRootName = "_emb";

    protected readonly string _apiKey;
    protected string _modelName;
    protected int _hiddenLayers = 512;
    protected int _parseBatchSize = 100;

    protected bool _enabled = false;
    protected readonly HttpClient _httpClient = new HttpClient();

    protected ClientBase(string apiKey, string modelName)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _modelName = modelName;
    }

    public abstract string Route(string method);

    public bool WasAlive { get; protected set; } = false;

    public async Task<bool> IsAlive()
    {
        var result = true;
        var url = Route("is_alive");
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(500);
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
            result = false;
        }
        WasAlive = result;
        return result;
    }

    public virtual async Task<Embedding[]> RequestEmbeddingAsync(string[] sentences)
    {
        var result = new List<Embedding>();

        var url = Route("encode_sentences");
        var binaryData = await RequestBinaryDataAsync(url, sentences);
        var sentenceCount = sentences.Length;
        var tensor = Utils.BytesToJaggedFloats(binaryData, sentenceCount, _hiddenLayers);
        result.AddRange(Embedding.CreateEmbeddings(tensor));
        return result.ToArray();
    }
    public async Task<EmbeddingThemes> RequestTextEmbeddingsAsync(bool makeIsotropic, params string[] filenames)
    {
        var result = new EmbeddingThemes();
        foreach (var filenameFull in filenames)
        {
            var fileroot = Path.GetFileNameWithoutExtension(filenameFull);
            var filename = _embeddingRootName + fileroot;
            var el = EmbeddingTheme.LoadBinary(filename);

            if (el == null)
            {
                el = new EmbeddingTheme();
                await foreach (var embeddings in ReadFileInChunks(RequestEmbeddingAsync, filenameFull, _parseBatchSize))
                {
                    el.AddRange(embeddings);
                }
                el.SaveBinary(filename);
            }
            result.Add(el);
        }

        if (makeIsotropic)
        {
            result.MakeIsotropic();
        }
        return result;
    }

    protected async Task<dynamic?> RequestJsonAsync(string url, string json = "")
    {
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await RequestJsonWithContentAsync(url, content);
    }
    protected async Task<dynamic?> RequestJsonWithContentAsync(string url, HttpContent content)
    {
        dynamic? result = null;
        try
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Server not yet alive. " + ex);
        }
        return result;
    }
    protected async Task<dynamic?> GetJsonAsyncTimeout(string url, int timeout = 500)
    {
        using (var cts = new CancellationTokenSource(timeout))
        {
            return await GetJsonAsync(url, cts.Token);
        }
    }
    protected async Task<dynamic?> GetJsonAsync(string url, CancellationToken cancellationToken)
    {
        dynamic? result = null;
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Server not active. " + ex);
        }
        return result;
    }

    protected async Task<byte[]> RequestBinaryDataAsync(string url, string[] sentences)
    {
        byte[]? result = null;
        var sentencesDict = new Dictionary<string, string[]>() { { "sentences", sentences } };
        string query = JsonConvert.SerializeObject(sentencesDict, Formatting.Indented);
        var content = new StringContent(query, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync(url, content);
            result = await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Could not retreive json from server. " + ex);
        }
        return result == null ? new byte[] { 0 } : result;
    }
    public async Task<string> UploadImageByPath(string imagePath)
    {
        var result = "";
        var url = Route("upload_image");
        var name = Path.GetFileNameWithoutExtension(imagePath);
        using (var content = new MultipartFormDataContent())
        {
            var imageStream = new FileStream(imagePath, FileMode.Open);
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.Add("Content-Type", "image/png");
            content.Add(imageContent, "image", name + ".png");
            var jsonResult = await RequestJsonWithContentAsync(url, content);
            result = jsonResult == null ? "" : jsonResult["filename"];
        }
        return result;
    }
    //public async Task<SKBitmap?> RequestImageByPathAsync(string path)
    //{
    //    var url = Route("get_image_by_path");
    //    var bmp = await RequestImageByPathAsync(url, path);
    //    return bmp;
    //}
    //protected async Task<SKBitmap?> RequestImageByPathAsync(string url, string path)
    //{
    //    SKBitmap? result = null;
    //    var jsonDict = new Dictionary<string, string>();
    //    jsonDict.Add("path", path);
    //    string query = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
    //    var content = new StringContent(query, Encoding.UTF8, "application/json");

    //    try
    //    {
    //        var response = await _httpClient.PostAsync(url, content);
    //        response.EnsureSuccessStatusCode();
    //        var imageData = await response.Content.ReadAsByteArrayAsync();
    //        using (var memoryStream = new MemoryStream(imageData))
    //        {
    //            var image = Image.FromStream(memoryStream);
    //            result = new SKBitmap(image);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Trace.WriteLine("Could not retreive image from server. " + ex);
    //    }

    //    return result;
    //}

    public async Task<string> RunControlNetFilter(ControlNetConfig config, string source, string destination)
    {
        var result = "";
        var jsonDict = new Dictionary<string, object>();
        jsonDict.Add("kind", config.Module);
        jsonDict.Add("source", source);
        jsonDict.Add("destination", destination);
        jsonDict.Add("params", config.GetParams());
        string query = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
        var content = new StringContent(query, Encoding.UTF8, "application/json");

        try
        {
            var url = Route("run_control_net_filter");
            var response = await _httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Could run control net filter. " + ex);
        }
        return result;
    }
    public async Task<string> RunControlNetFilters(IEnumerable<string> kinds)
    {
        var result = "";
        var jsonDict = new Dictionary<string, object>();
        jsonDict.Add("kinds", kinds.ToArray());
        string query = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
        var content = new StringContent(query, Encoding.UTF8, "application/json");

        try
        {
            var url = Route("run_control_net_filters");
            var response = await _httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Could run control net filters. " + ex);
        }
        return result;
    }


    protected string GetJson(string name, string value, string name2 = "", string value2 = "")
    {
        var jsonDict = new Dictionary<string, string>();
        jsonDict.Add(name, value);
        if (name2 != "" && value2 != "")
        {
            jsonDict.Add(name2, value2);
        }
        return JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
    }
    public async static IAsyncEnumerable<Embedding[]> ReadFileInChunks(LoadEmbedding onLoaded, string filename, int chunkSize = 1000)
    {
        string[] sentences = File.ReadAllLines(filename);
        int start = 0;
        int total = sentences.Length;
        for (int i = 0; i < total; i += chunkSize)
        {
            var batch = sentences.Skip(start).Take(chunkSize).ToArray();
            var chunk = await onLoaded(batch);
            yield return chunk;
            start += chunkSize;
        }
    }

    public void Enable()
    {
        _enabled = true;
    }
    public void Disable()
    {
        _enabled = false;
    }

    public void Dispose()
    {
        _enabled = false;
        _httpClient.Dispose();
    }
}
