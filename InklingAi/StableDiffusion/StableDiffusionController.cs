namespace InklingAi.StableDiffusion
{
    using Microsoft.ML.OnnxRuntime.Tensors;
    using Microsoft.ML.OnnxRuntime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InklingAi.Utils;
    using SkiaSharp;
    using InklingAi.ViewModels;
    using AIEnv.Clients.StableDiffusion;
    using AIEnv.Utils;
    using AIEnv.Onnx;

    public class StableDiffusionController
    {
        public Tokenizer _tokenizer;
        public TextEncoder _textEncoder;
        public VaeEncoder _vaeEncoder;
        public Unet _unet;
        public VaeDecoder _vaeDecoder;
        public SchedulerBase _scheduler;

        public delegate void UpdateStepCallback(int currentIndex, SKBitmap? img);
        public event UpdateStepCallback? OnUpdateStep;

        public StableDiffusionController(UpdateStepCallback? callback = null)
        {
            OnUpdateStep = callback;
            //CreateModels();
        }
        public void CreateModels()
        {
            _tokenizer = new Tokenizer();
            _textEncoder = new TextEncoder();
            _scheduler = new EulerA();
            //_scheduler = new LMSDiscreteScheduler();
            _unet = new Unet(_scheduler);
            _vaeEncoder = new VaeEncoder();
            _vaeDecoder = new VaeDecoder();
        }

        public SKBitmap? Generate(StableDiffusionConfig config)
        {
            var latents = GenerateLatents(config);
            var image = _vaeDecoder.Decode(latents);
            //var path = IOUtils.IterationStepPath(config.NumInferenceSteps);
            //IOUtils.SaveLocalImage(image, path);

            var path = config.GenerateImageName();
            InklingAi.Utils.InklingIOUtils.SaveLocalImage(image, path);
            //var path = IOUtils.IterationStepPath(999);
            //IOUtils.SaveLocalImage(image, path);
            return image;
        }

        private DenseTensor<Float16> GenerateLatents(StableDiffusionConfig config)
        {
            //return IOUtils.LoadEncoding(@"C:\Users\Robin\InklingAI\SD\working\latent_10.bin", new int[] { 1, 4, 80, 64 });
            var positiveTokens = _tokenizer.TokenizeText(config.Prompt + "," + config.StylePrompt);
            var positiveEmbed = _textEncoder.EncodeText(positiveTokens);

            var negativeTokens = _tokenizer.TokenizeText(config.NegativePrompt);
            var negativeEmbed = _textEncoder.EncodeText(negativeTokens);

            DenseTensor<Float16> embeds = new DenseTensor<Float16>(new[] { 2, 77, 768 });
            for (var i = 0; i < positiveEmbed.Length; i++)
            {
                embeds[0, i / 768, i % 768] = negativeEmbed[i];
                embeds[1, i / 768, i % 768] = positiveEmbed[i];
            }

            var timesteps = _scheduler.SetTimesteps(config.NumInferenceSteps);
            var seed = config.Seed;

            var latents = GenerateLatentSample(config.Height, config.Width, seed, _scheduler.InitNoiseSigma);
            List<NamedOnnxValue> input;
            SKBitmap? image = null;

            int t;
            for (t = 0; t < timesteps.Length; t++)
            {
                latents = _unet.Step(latents, embeds, timesteps[t], config.GuidanceScale);
                if (OnUpdateStep != null)
                {
                    //image = _vaeDecoder.Decode(latents);
                    //latents = IOUtils.LoadEncoding(IOUtils.SDWorkingFolder + "latent_6.bin", latents.Dimensions.ToArray());
                    //OnUpdateStep(t + 1, image);
                }
                //var path = IOUtils.IterationStepPath(100 + t);
                //IOUtils.SaveEncoding(latents, path + ".bin");
                //IOUtils.SaveLocalImage(image, path);
            }
            return latents;
        }

        public DenseTensor<Float16> GenerateLatentSample(int height, int width, int seed, float initNoiseSigma)
        {
            OnnxUtils.random = new Random(seed);
            var batchSize = 1;
            var channels = 4;
            var latentHeight = height / 8;
            var latentWidth = width / 8;
            var latentsArray = new Float16[batchSize * channels * latentHeight * latentWidth];

            for (int i = 0; i < latentsArray.Length; i++)
            {
                var u1 = OnnxUtils.random.NextDouble();
                var u2 = OnnxUtils.random.NextDouble();
                var radius = Math.Sqrt(-2.0 * Math.Log(u1)); // Radius of polar coordinates
                var theta = 2.0 * Math.PI * u2; // Angle of polar coordinates
                var standardNormalRand = radius * Math.Cos(theta); // Standard normal random number
                latentsArray[i] = ((float)standardNormalRand * initNoiseSigma).ToFloat16();
            }

            var latents = OnnxUtils.CreateTensor(latentsArray, new[] { batchSize, channels, latentHeight, latentWidth });
            return latents;
        }

        public SKBitmap? DecodePath(string latentPath, IntSize size)
        {
            return _vaeDecoder.DecodePath(latentPath, size);
        }
        public DenseTensor<Float16> EncodeImage(SKBitmap sourceBmp)
        {
            return _vaeEncoder.EncodeImage(sourceBmp);
        }

    }
}
