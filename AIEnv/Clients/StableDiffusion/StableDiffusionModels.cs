namespace AIEnv.Clients.StableDiffusion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AIEnv.Utils;
    using SkiaSharp;

    public class StableDiffusionModels
    {
        public static string ModelUrlFolder = @"https://huggingface.co/debreuil/SD_PureDataModels/resolve/main/";

        public static List<string> ModelFiles = new List<string>()
        {
            "vae",                          "vae_f16_cuda.bin",
            "text_encoder",                 "text_encoder_f16_cuda.bin",

            "RevAnimated V122",             "revAnimated_v122_f16.bin",
            "RevAnimated V122 inpainting",  "revAnimated_v121Inp_inpainting_f16.bin",

            "canny",                        "controlNet_canny_f16_cuda.bin",

            "midas_input",                  "dpt_hybrid-midas-501f0c75.pt",
            "depth",                        "controlNet_depth_f16_cuda.bin",
            "normal",                       "controlNet_normalbae_f16_cuda.bin",

            "leres_input",                  "res101.pth",
            "leres",                        "controlNet_depth_f16_cuda.bin",

            "hed_input",                    "network-bsds500.pth",
            "hed",                          "controlNet_lineart_anime_f16_cuda.bin",

            "openpose_body_input",          "body_pose_model.pth",
            "openpose_hand_input",          "hand_pose_model.pth",
            "openpose",                     "controlNet_openpose_f16_cuda.bin",

            "mlsd_input",                   "mlsd_large_512_fp32.pth",
            "mlsd",                         "controlNet_mlsd_f16_cuda.bin",

            "pidinet_input",                "table5_pidinet.pth",
            "pidinet",                      "controlNet_softedge_f16_cuda.bin",

            "uniformer_input",              "upernet_global_small.pth",
            "uniformer",                    "controlNet_seg_f16_cuda.bin",

            "Realistic Vision V30",         "realisticVisionV30_v30VAE_f16.bin",
            "Realistic Vision V30 inpainting", "realisticVisionV40_v30Novae_inpainting_f16.bin",
            "Deliberate V2",                "deliberate_v2_f16.bin",
            "Deliberate V2 inpainting",     "deliberate_v2_inpainting_f16.bin",
            "Dreamshaper 7",                "dreamshaper_7_f16.bin",
            "Dreamshaper 7 inpainting",     "dreamshaper_7_inpainting_f16.bin",
            "Default",                      "unet_f16_cuda.bin",
            "Default inpainting",           "unet_inpainting_f16_cuda.bin",
        };

        public async Task EnsureModelsLoaded()
        {
            Directory.CreateDirectory(IOUtils.SDModelsFolder + ControlNetFolder);

            var filename = "";
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, value) =>
            {
                //InklingAI.GenInstance.SetStatusText($"Downloaded {value.ToString("0.0")}% of {filename}");
            };

            int index = 0;
            //InklingAI.GenInstance.EnableLoadedModelsThreadCheck();
            for (int i = 0; i < ModelFiles.Count; i += 2)
            {
                var modelName = ModelFiles[i];
                filename = ModelFiles[i + 1];
                var urlPath = ModelUrlFolder + filename;
                var outputPath = GetModelPath(modelName);
                var tempPath = IOUtils.SDModelsFolder + "downloading.bin";
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                if (!File.Exists(outputPath))
                {
                    await IOUtils.DownloadFileAsync(urlPath, tempPath, progress); // guarding for partial downloads
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    IOUtils.CopyFile(tempPath, outputPath);
                    File.Delete(tempPath);
                    if (index == 0)
                    {
                        StableDiffusionClient.Instance.SetVae();
                    }
                    if (index == 1)
                    {
                        StableDiffusionClient.Instance.SetPrompt();
                    }
                    else if (index == 2) // allow gen without mask, then make sure mask is also loaded.
                    {
                        await StableDiffusionClient.Instance.SetModel(filename, "");
                    }
                    else if (index == 3) // allow gen without mask, then make sure mask is also loaded.
                    {
                        await StableDiffusionClient.Instance.SetModel(ModelFiles[i - 1], filename);
                    }
                    //InklingAI.GenInstance.EnableLoadedModelsThreadCheck();
                }
                index++;
            }
            //InklingAI.GenInstance.EnableLoadedModelsThreadCheck();

            //InklingAI.GenInstance.SetStatusText($"Finished loading models. Ready to generate.");
        }

        public static bool UnetModelExists(string modelName, bool inpaintVersion = false)
        {
            bool result = false;
            var index = ModelFiles.IndexOf(modelName);
            if (index > -1)
            {
                var name = inpaintVersion ? ModelFiles[index + 2] : ModelFiles[index + 0];
                var path = IOUtils.SDModelsFolder + name;
                result = File.Exists(path);
            }
            return result;
        }

        public static string ControlNetFolder = "control_net";
        public static string GetModelPath(string modelName)
        {
            var result = "";
            var index = ModelFiles.IndexOf(modelName);
            if (index > -1 && index < ModelFiles.Count - 1)
            {
                var filename = ModelFiles[index + 1];
                var urlPath = ModelUrlFolder + filename;
                var folder = "";
                if (filename.StartsWith("controlNet"))
                {
                    folder = IOUtils.SDModelsFolder + ControlNetFolder + IOUtils.Sep;
                }
                else if (modelName.EndsWith("_input"))
                {
                    var cnDirectory = modelName.Substring(0, modelName.IndexOf("_"));
                    folder = IOUtils.PythonFolder + "annotator" + IOUtils.Sep + cnDirectory + IOUtils.Sep;
                }
                else
                {
                    folder = IOUtils.SDModelsFolder;
                }
                result = folder + filename;
            }
            return result;
        }

    }
}
