namespace AIEnv.Clients.StableDiffusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using AIEnv.Clients;

public class ClipClient : ClientBase
{
    public ClipClient(string apiKey = "", string modelName = @"openai/clip-vit-base-patch32") : base(apiKey, modelName)
    {
        _hostPort = 9000;
        _embeddingRootName = "clip_";
        _hiddenLayers = 77;
    }
    public override string Route(string method) => @"http://" + Host + method;

}
