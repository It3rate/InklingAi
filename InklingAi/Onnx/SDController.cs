namespace InklingAi.Onnx;

using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InklingAi.StableDiffusion;
using InklingAi.Utils;
using Avalonia.Media;
using Avalonia;
using SkiaSharp;

public class SDController
{
    public Tokenizer _tokenizer;
    public TextEncoder _textEncoder;
    public VaeEncoder _vaeEncoder;
    public Unet _unet;
    public VaeDecoder _vaeDecoder;
    public SchedulerBase _scheduler;

    public delegate void UpdateStepCallback(int currentIndex, SKBitmap? img);
    public event UpdateStepCallback? OnUpdateStep;

    public SDController(UpdateStepCallback? callback = null) 
    { 
        OnUpdateStep = callback;

        _tokenizer = new Tokenizer();
        _textEncoder = new TextEncoder();
        _scheduler = new EulerA();
        //_scheduler = new LMSDiscreteScheduler();
        _unet = new Unet(_scheduler);
        _vaeEncoder = new VaeEncoder();
        _vaeDecoder = new VaeDecoder();
    }

    public string Generate(StableDiffusionConfig config)
    {
        var latents = GenerateLatents(config);
        var image = _vaeDecoder.Decode(latents);
        var path = IOUtils.IterationStepPath(config.NumInferenceSteps);
        IOUtils.SaveLocalImage(image, path);

        //var path = IOUtils.IterationStepPath(999);
        //IOUtils.SaveLocalImage(image, path);
        return path;
    }

    private DenseTensor<Float16> GenerateLatents(StableDiffusionConfig config)
    {
        var positiveTokens = _tokenizer.TokenizeText(config.Prompt);
        var positiveEmbed = _textEncoder.EncodeText(positiveTokens);

        var negativeTokens = _tokenizer.TokenizeText(config.NegativePrompt);
        var negativeEmbed = _textEncoder.EncodeText(negativeTokens);

        DenseTensor<Float16> embeds = new DenseTensor<Float16>(new[] { 2, 77, 768 });
        for (var i = 0; i < positiveEmbed.Length; i++)
        {
            embeds[0, i / 768, i % 768] = negativeEmbed[i];
            embeds[1, i / 768, i % 768] = positiveEmbed[i];
        }

        //var pos = OnnxUtils.CreateTensor(positiveEmbed, new[] { 1, 77, 768 });
        //var neg = OnnxUtils.CreateTensor(negativeEmbed, new[] { 1, 77, 768 });
        //var embeds2 = pos.AddTensors(neg);
        //IOUtils.SaveEncoding(embeds, IOUtils.SDWorkingFolder + "_cs_text_encoding.bin");
        //embeds = IOUtils.LoadEncoding(IOUtils.SDWorkingFolder + "_py_text_encoding.bin", embeds.Dimensions.ToArray());

        var timesteps = _scheduler.SetTimesteps(config.NumInferenceSteps);
        var seed = config.Seed;
        var latents = GenerateLatentSample(config.Height, config.Width, seed, _scheduler.InitNoiseSigma);
        //IOUtils.SaveEncoding(latents, IOUtils.SDWorkingFolder + "latent_test.bin");

        List<NamedOnnxValue> input;
        SKBitmap? image = null;
        int t;
        for (t = 0; t < timesteps.Length; t++)
        {
            latents = _unet.Step(latents, embeds, timesteps[t], config.GuidanceScale);
            image = _vaeDecoder.Decode(latents);
            //latents = IOUtils.LoadEncoding(IOUtils.SDWorkingFolder + "latent_6.bin", latents.Dimensions.ToArray());
            if (OnUpdateStep != null)
            {
                OnUpdateStep(t + 1, image);
            }
            //var path = IOUtils.IterationStepPath(100 + t);
            //IOUtils.SaveEncoding(latents, path + ".bin");
            //IOUtils.SaveLocalImage(image, path);
        }
        return latents;
    }

    public DenseTensor<Float16> GenerateLatentSample(int height, int width, int seed, float initNoiseSigma)
    {
        var random = new Random(seed);
        var batchSize = 1;
        var channels = 4;
        var latentHeight = height / 8;
        var latentWidth = width / 8;
        var latentsArray = new Float16[batchSize * channels * latentHeight * latentWidth];

        for (int i = 0; i < latentsArray.Length; i++)
        {
            // Generate a random number from a normal distribution with mean 0 and variance 1
            var u1 = random.NextDouble(); // Uniform(0,1) random number
            var u2 = random.NextDouble(); // Uniform(0,1) random number
            var radius = Math.Sqrt(-2.0 * Math.Log(u1)); // Radius of polar coordinates
            var theta = 2.0 * Math.PI * u2; // Angle of polar coordinates
            var standardNormalRand = radius * Math.Cos(theta); // Standard normal random number
            // add noise to latents with * scheduler.init_noise_sigma
            // generate randoms that are negative and positive
            //latentsArray[i] = ((float)standardNormalRand * initNoiseSigma).ToFloat16();
            latentsArray[i] = ((float)standardNormalRand).ToFloat16();
        }

        var latents = OnnxUtils.CreateTensor(latentsArray, new[] { batchSize, channels, latentHeight, latentWidth });
        return latents;
    }

    public SKBitmap? DecodePath(string latentPath, Size size)
    {
        return _vaeDecoder.DecodePath(latentPath, size);
    }
    public DenseTensor<Float16> EncodeImage(SKBitmap sourceBmp)
    {
        return _vaeEncoder.EncodeImage(sourceBmp);
    }

}
