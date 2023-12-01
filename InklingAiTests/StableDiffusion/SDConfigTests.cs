namespace InklingAiTests.StableDiffusion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using System.IO;
    using global::InklingAi.StableDiffusion;
    using InklingAi.Utils;
    using System.Collections;
    using AIEnv.Clients.StableDiffusion;

    public class StableDiffusionConfigTests
    {
        [Fact]
        public void GenerateImageName_CreatesExpectedFileName()
        {
            var config = new StableDiffusionConfig
            {
                Prompt = "Test Prompt",
                Seed = 12345
            };
            var expectedFileName = InklingIOUtils.SDImageFolder + "Test_Prompt12345.png";
            Assert.Equal(expectedFileName, config.GenerateImageName());
        }

        [Fact]
        public void GetJson_ReturnsValidJson()
        {
            var config = new StableDiffusionConfig
            {
                Prompt = "Test",
                StylePrompt = "StyleTest",
                NegativePrompt = "NegativeTest",
                NumInferenceSteps = 25,
                GuidanceScale = 7.5f,
                Seed = 12345
            };
            var json = config.GetJson();
            Assert.Contains("Test", json);
            Assert.Contains("StyleTest", json);
            Assert.Contains("NegativeTest", json);
            Assert.Contains("25", json);
            Assert.Contains("7.5", json);
            Assert.Contains("12345", json);
        }

        [Fact]
        public void WriteAndLoadJson_PreservesData()
        {
            var config = new StableDiffusionConfig
            {
                Prompt = "Test",
                StylePrompt = "StyleTest",
                NegativePrompt = "NegativeTest",
                NumInferenceSteps = 25,
                GuidanceScale = 7.5f,
                Seed = 12345
            };
            var path = "testConfigSD.json";
            config.WriteJson(path);
            var loadedConfig = StableDiffusionConfig.LoadFromJson(path);
            File.Delete(path);

            Assert.Equal(config.Prompt, loadedConfig?.Prompt);
            Assert.Equal(config.StylePrompt, loadedConfig?.StylePrompt);
            Assert.Equal(config.NegativePrompt, loadedConfig?.NegativePrompt);
            Assert.Equal(config.NumInferenceSteps, loadedConfig?.NumInferenceSteps);
            Assert.Equal(config.GuidanceScale, loadedConfig?.GuidanceScale);
            Assert.Equal(config.Seed, loadedConfig?.Seed);
        }

        [Fact]
        public void GetExifString_ReturnsExpectedFormat()
        {
            var config = new StableDiffusionConfig
            {
                Prompt = "Test",
                StylePrompt = "StyleTest",
                NegativePrompt = "NegativeTest",
                NumInferenceSteps = 25,
                GuidanceScale = 7.5f,
                Seed = 12345
            };
            var exifString = config.GetExifString();
            Assert.Contains("Test", exifString);
            Assert.Contains("StyleTest", exifString);
            Assert.Contains("NegativeTest", exifString);
            Assert.Contains("25", exifString);
            Assert.Contains("7.5", exifString);
            Assert.Contains("12345", exifString);
        }

        [Fact]
        public void Clone_CreatesIdenticalObject()
        {
            var original = new StableDiffusionConfig
            {
                Prompt = "Original Prompt",
                Seed = 54321
            };
            var clone = original.Clone();

            Assert.Equal(original.Prompt, clone.Prompt);
            Assert.Equal(original.Seed, clone.Seed);
        }

        [Fact]
        public void ParseExif_CreatesConfigFromExifString()
        {
            string exifString = "Test Prompt\nStyle promptName: StyleTest\nNegative promptName: NegativeTest\nSteps: 25, Sampler: DDIM, CFG scale: 6.5, Seed: 12345, SDSize: 512x512, Model hash: 81761151, Batch size: 1, Batch pos: 1, Denoising strength: 1.00, Mask blur: 0";
            var config = StableDiffusionConfig.ParseExif(exifString);

            Assert.NotNull(config);
            Assert.Equal("Test Prompt", config.Prompt);
            Assert.Equal("StyleTest", config.StylePrompt);
            Assert.Equal("NegativeTest", config.NegativePrompt);
            Assert.Equal(25, config.NumInferenceSteps);
            // Assert.Equal("DDIM", config.Sampler); # currently not serialized.
            Assert.Equal(6.5, config.GuidanceScale);
        }

        [Fact]
        public void GetConfigDictionary_ReturnsCorrectKeyValuePairs()
        {
            var config = new StableDiffusionConfig
            {
                Prompt = "Test",
                StylePrompt = "StyleTest",
                NegativePrompt = "NegativeTest",
                NumInferenceSteps = 25,
                GuidanceScale = 7.5f,
                Seed = 12345
            };
            var exif = config.GetExifString();

            var dictionary = config.GetConfigDictionary();
            Assert.NotNull(dictionary);
            Assert.Equal("Test", dictionary["promptName"]);
            Assert.Equal("StyleTest", dictionary["style_prompt_name"]);
            Assert.Equal("NegativeTest", dictionary["negative_prompt"]);
            Assert.Equal(25L, dictionary["num_inference_steps"]);
            Assert.Equal(7.5, dictionary["guidance_scale"]);
            Assert.Equal(12345L, dictionary["seed"]);
        }

        [Fact]
        public void HasRegion_ReturnsExpectedBoolean()
        {
            var configWithRegion = new StableDiffusionConfig
            {
                MaskBounds = new int[] { 0, 0, 100, 100 }
            };
            var configWithoutRegion = new StableDiffusionConfig
            {
                MaskBounds = new int[] { }
            };

            Assert.True(configWithRegion.HasRegion);
            Assert.False(configWithoutRegion.HasRegion);
        }

        [Fact]
        public void GeneratedSize_ReturnsExpectedSize()
        {
            var configWithRegion = new StableDiffusionConfig
            {
                Width = 640,
                Height = 512,
                MaskBounds = new int[] { 100, 100, 320, 256 },
                GeneratedRegionSize = new int[] { 100, 100, 320, 256 }
            };
            var configWithoutRegion = new StableDiffusionConfig
            {
                Width = 640,
                Height = 512
            };

            Assert.Equal(4, configWithRegion.MaskBounds.Length);
            Assert.Equal(320, configWithRegion.MaskBounds[2]);
            Assert.Empty(configWithoutRegion.MaskBounds);

            Assert.Equal(320, configWithRegion.GeneratedSize().Width);
            Assert.Equal(256, configWithRegion.GeneratedSize().Height);
            Assert.Equal(640, configWithoutRegion.GeneratedSize().Width);
            Assert.Equal(512, configWithoutRegion.GeneratedSize().Height);
        }

        [Fact]
        public void CopyPropertiesFrom_CopiesPropertiesCorrectly()
        {
            // Arrange
            var originalConfig = new StableDiffusionConfig
            {
                Prompt = "Original Prompt",
                StylePrompt = "Original StylePrompt",
                NegativePrompt = "Original NegativePrompt",
                NumInferenceSteps = 20,
                GuidanceScale = 5.0f,
                Seed = 42,
                CallbackSteps = 2,
                InitialImageName = "OriginalInitialImageName",
                InitialLatentsName = "OriginalInitialLatentsName",
                GeneratedImageName = "OriginalGeneratedImageName",
                MaskImageName = "OriginalMaskImageName",
                MaskBounds = new int[] { 1, 2, 3, 4 },
                ImagePreservation = 0.5f,
                ActiveInputs = new string[] { "OriginalInput1", "OriginalInput2" },
                StartInferenceStep = 10,
                InitialImageSource = "OriginalInitialImageSource",
                AspectSize = new IntSize(640, 512),
                SeedUnlocked = true
            };

            var newConfig = new StableDiffusionConfig
            {
                Prompt = "New Prompt",
                StylePrompt = "New StylePrompt",
                NegativePrompt = "New NegativePrompt",
                NumInferenceSteps = 25,
                GuidanceScale = 7.5f,
                Seed = 100,
                CallbackSteps = 1,
                InitialImageName = "NewInitialImageName",
                InitialLatentsName = "NewInitialLatentsName",
                GeneratedImageName = "NewGeneratedImageName",
                MaskImageName = "NewMaskImageName",
                MaskBounds = new int[] { 5, 6, 7, 8 },
                ImagePreservation = 1.0f,
                ActiveInputs = new string[] { "NewInput1", "NewInput2" },
                StartInferenceStep = 15,
                InitialImageSource = "NewInitialImageSource",
                AspectSize = new IntSize(800, 600),
                SeedUnlocked = false
            };

            // Act
            originalConfig.CopyPropertiesFrom(newConfig);

            // Assert
            Assert.Equal(newConfig.Prompt, originalConfig.Prompt);
            Assert.Equal(newConfig.StylePrompt, originalConfig.StylePrompt);
            Assert.Equal(newConfig.NegativePrompt, originalConfig.NegativePrompt);
            Assert.Equal(newConfig.NumInferenceSteps, originalConfig.NumInferenceSteps);
            Assert.Equal(newConfig.GuidanceScale, originalConfig.GuidanceScale);
            Assert.Equal(newConfig.Seed, originalConfig.Seed);
            Assert.Equal(newConfig.CallbackSteps, originalConfig.CallbackSteps);
            Assert.Equal(newConfig.InitialImageName, originalConfig.InitialImageName);
            Assert.Equal(newConfig.InitialLatentsName, originalConfig.InitialLatentsName);
            Assert.Equal(newConfig.GeneratedImageName, originalConfig.GeneratedImageName);
            Assert.Equal(newConfig.MaskImageName, originalConfig.MaskImageName);
            Assert.Equal(newConfig.MaskBounds, originalConfig.MaskBounds);
            Assert.Equal(newConfig.ImagePreservation, originalConfig.ImagePreservation);
            Assert.Equal(newConfig.ActiveInputs, originalConfig.ActiveInputs);
            Assert.Equal(newConfig.StartInferenceStep, originalConfig.StartInferenceStep);
            Assert.Equal(newConfig.InitialImageSource, originalConfig.InitialImageSource);
            Assert.Equal(newConfig.AspectSize, originalConfig.AspectSize);
            Assert.Equal(newConfig.SeedUnlocked, originalConfig.SeedUnlocked);
        }
    }
}

