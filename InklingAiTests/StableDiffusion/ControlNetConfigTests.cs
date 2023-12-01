namespace InklingAiTests.StableDiffusion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIEnv.Clients.StableDiffusion;
using global::InklingAi.StableDiffusion;
using Newtonsoft.Json;

public class ControlNetConfigTests
{

    [Theory]
    [InlineData("canny", new float[] { 100, 200 }, new string[] { "Low Threshold", "High Threshold" }, new string[] { "low", "high" })]
    [InlineData("leres", new float[] { 0.0f, 0.0f }, new string[] { "Remove Near", "Remove Background" }, new string[] { "near", "bkg" })]
    [InlineData("mlsd", new float[] { 0.2f, 0.7f }, new string[] { "Value Threshold", "Distance Threshold" }, new string[] { "hough_value", "hough_thresh" })]
    [InlineData("openpose", new float[] { 0f }, new string[] { "Show Hands" }, new string[] { "hand" })]
    [InlineData("normal", new float[] { 6.283f, 0.1f }, new string[] { "Init", "Remove Background" }, new string[] { "init", "bkg_thresh" })]
    [InlineData("tile", new float[] { 1 }, new string[] { "Down Sampling Rate" }, new string[] { })]
    [InlineData("reference", new float[] { 0.5f }, new string[] { "Style Fidelity" }, new string[] { })]
    public void ControlNetConfig_InitializesCorrectly(string moduleName, float[] expectedExtraParamValues, string[] expectedExtraParamNames, string[] expectedExtraParamAPINames)
    {
        // Act
        var config = new ControlNetConfig(moduleName);

        // Assert
        Assert.Equal(expectedExtraParamValues, config.ExtraParamValues);
        Assert.Equal(expectedExtraParamNames, config.ExtraParamNames);
        Assert.Equal(expectedExtraParamAPINames, config.ExtraParamAPINames);
    }

    [Fact]
    public void ControlNetConfig_InitializesWithDefaults_OnUnknownModule()
    {
        // Arrange
        var unknownModuleName = "unknown";

        // Act
        var config = new ControlNetConfig(unknownModuleName);

        // Assert
        Assert.Empty(config.ExtraParamValues);
        Assert.Empty(config.ExtraParamNames);
        Assert.Empty(config.ExtraParamAPINames);
    }

    [Fact]
    public void CopyPropertiesFrom_CopiesPropertiesCorrectly()
    {
        // Arrange
        var originalConfig = new ControlNetConfig("mlsd")
        {
            Enabled = false,
            Module = "mlsd",
            Model = "control_mlsd",
            Weight = 1f,
            GuidanceStart = 0f,
            GuidanceEnd = 1f,
            InputPath = "InputPathOriginal",
            ImagePath = "ImagePathOriginal",
            UploadPath = "UploadPathOriginal"
        };

        var newConfig = new ControlNetConfig("canny")
        {
            Enabled = true,
            Module = "canny",
            Model = "control_canny",
            Weight = 2f,
            GuidanceStart = 0.5f,
            GuidanceEnd = 1.5f,
            InputPath = "InputPathNew",
            ImagePath = "ImagePathNew",
            UploadPath = "UploadPathNew"
        };

        // Act
        originalConfig.CopyPropertiesFrom(newConfig);

        // Assert
        Assert.Equal(newConfig.Enabled, originalConfig.Enabled);
        Assert.Equal(newConfig.Module, originalConfig.Module);
        Assert.Equal(newConfig.Model, originalConfig.Model);
        Assert.Equal(newConfig.Weight, originalConfig.Weight);
        Assert.Equal(newConfig.GuidanceStart, originalConfig.GuidanceStart);
        Assert.Equal(newConfig.GuidanceEnd, originalConfig.GuidanceEnd);
        Assert.Equal(newConfig.InputPath, originalConfig.InputPath);
        Assert.Equal(newConfig.ImagePath, originalConfig.ImagePath);
        Assert.Equal(newConfig.UploadPath, originalConfig.UploadPath);
    }

    [Fact]
    public void SerializationAndDeserialization_WorksCorrectly()
    {
        // Arrange
        var originalConfig = new ControlNetConfig("mlsd")
        {
            Enabled = true,
            Module = "mlsd",
            Model = "control_mlsd",
            Weight = 1f,
            GuidanceStart = 0f,
            GuidanceEnd = 1f,
            InputPath = "InputPathOriginal",
            ImagePath = "ImagePathOriginal",
            UploadPath = "UploadPathOriginal"
        };

        // Act
        var json = originalConfig.GetJson();
        var deserializedConfig = JsonConvert.DeserializeObject<ControlNetConfig>(json);

        // Assert
        Assert.NotNull(deserializedConfig);
        Assert.Equal(originalConfig.Enabled, deserializedConfig.Enabled);
        Assert.Equal(originalConfig.Module, deserializedConfig.Module);
        Assert.Equal(originalConfig.Model, deserializedConfig.Model);
        Assert.Equal(originalConfig.Weight, deserializedConfig.Weight);
        Assert.Equal(originalConfig.GuidanceStart, deserializedConfig.GuidanceStart);
        Assert.Equal(originalConfig.GuidanceEnd, deserializedConfig.GuidanceEnd);
        Assert.Equal(originalConfig.InputPath, deserializedConfig.InputPath);
        Assert.Equal(originalConfig.ImagePath, deserializedConfig.ImagePath);
        Assert.Equal(originalConfig.UploadPath, deserializedConfig.UploadPath);
    }

    [Fact]
    public void GetConfigDictionary_ReturnsCorrectDictionary()
    {
        // Arrange
        var config = new ControlNetConfig("mlsd")
        {
            Enabled = true,
            Module = "mlsd",
            Model = "control_mlsd",
            Weight = 1f,
            GuidanceStart = 0f,
            GuidanceEnd = 1f,
            InputPath = "InputPath",
            ImagePath = "ImagePath",
            UploadPath = "UploadPath"
        };

        // Act
        var dictionary = config.GetConfigDictionary();

        // Assert
        Assert.NotNull(dictionary);
        Assert.Equal(config.Enabled, dictionary["enabled"]);
        Assert.Equal(config.Module, dictionary["module"]);
        Assert.Equal(config.Model, dictionary["model"]);
        Assert.Equal(config.Weight, (float)(double)dictionary["weight"]);
        Assert.Equal(config.GuidanceStart, (float)(double)dictionary["guidance_start"]);
        Assert.Equal(config.GuidanceEnd, (float)(double)dictionary["guidance_end"]);
        Assert.Equal(config.InputPath, dictionary["input_path"]);
        Assert.Equal(config.ImagePath, dictionary["image_path"]);
        Assert.Equal(config.UploadPath, dictionary["upload_path"]);
    }

    [Fact]
    public void WriteAndLoadJson_WorksCorrectly()
    {
        // Arrange
        var originalConfig = new ControlNetConfig("mlsd")
        {
            Enabled = true,
            Module = "mlsd",
            Model = "control_mlsd",
            Weight = 1f,
            GuidanceStart = 0f,
            GuidanceEnd = 1f,
            InputPath = "InputPathOriginal",
            ImagePath = "ImagePathOriginal",
            UploadPath = "UploadPathOriginal"
        };
        var path = "testConfig.json";

        // Act
        originalConfig.WriteJson(path);
        var loadedConfig = ControlNetConfig.LoadFromJson(path);

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(originalConfig.Enabled, loadedConfig.Enabled);
        Assert.Equal(originalConfig.Module, loadedConfig.Module);
        Assert.Equal(originalConfig.Model, loadedConfig.Model);
        Assert.Equal(originalConfig.Weight, loadedConfig.Weight);
        Assert.Equal(originalConfig.GuidanceStart, loadedConfig.GuidanceStart);
        Assert.Equal(originalConfig.GuidanceEnd, loadedConfig.GuidanceEnd);
        Assert.Equal(originalConfig.InputPath, loadedConfig.InputPath);
        Assert.Equal(originalConfig.ImagePath, loadedConfig.ImagePath);
        Assert.Equal(originalConfig.UploadPath, loadedConfig.UploadPath);

        // Cleanup
        File.Delete(path);
    }
}

