namespace AIEnv.Clients.StableDiffusion
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection.Metadata;
    using System.Text;
    using System.Threading.Tasks;
    using AIEnv.Embeddings;
    using AIEnv.Utils;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SkiaSharp;
    using static System.Net.Mime.MediaTypeNames;

    public class DalleClient : ClientBase
    {

        protected string Host => "api.openai.com/v1/images/";
        private int _imgWidth = 1024;
        private int _imgHeight = 1024;
        private int _numImages = 1;
        private string _dall_e_version = "dall-e-3";

        public DalleClient(string modelName = @"generations") : base(IOUtils.OpenAI_ApiKey, modelName)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }
        public override string Route(string method) => @"https://" + Host + _modelName;

        // curl "https://api.openai.com/v1/images/generations"
        // -H "Content-Type: application/json"
        // -H "Authorization: Bearer $OPENAI_API_KEY"
        // -d '{ "model": "dall-e-3", "prompt": "A cute baby sea otter", "n": 1, "size": "1024x1024"}'
        public virtual async Task<string?> RequestImageAsync(StableDiffusionConfig config)
        {
            string? result = null;
            var route = Route(_modelName);
            var jsonContent = GetRequestJson(config.BasePrompt + " " + config.StylePrompt);
            try
            {
                var response = await _httpClient.PostAsync(route, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(responseContent);
                if (jsonObject.ContainsKey("data"))
                {
                    var url = jsonObject["data"][0]["url"].ToString();
                    SKBitmap img = await DownloadImageAsync(url);
                    var name = config.GenerateImageName;
                    var path = IOUtils.SDImageFolder + $"dalle_{name}.png";
                    IOUtils.SaveLocalImage(img, path);
                    result = path;
                }
                else if(jsonObject.ContainsKey("error"))
                {
                    result = jsonObject["error"].ToString();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not retreive image from OpenAI server. " + ex);
            }
            return result;
        }
        private StringContent? GetRequestJson(string prompt)
        {
            var requestData = new
            {
                model = _dall_e_version,
                prompt = prompt, // 4000 char max
                n = _numImages,
                size = $"{_imgWidth}x{_imgHeight}", // dall-e-3: 1024x1024, 1792x1024, 1024x1792     // dall-e-2 256x256, 512x512, 1024x1024
                // quality = "standard" | "hd",
                // response_format = "url" | "b64_json"
                // style = "vivid" | "natural"
            };
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestData),
                Encoding.UTF8,
                "application/json"
            );
            return jsonContent;
        }
    }
}
