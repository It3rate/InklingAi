namespace AIEnv.Utils
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using System.Net.Http;
    using System.Runtime.InteropServices;

    //using Microsoft.ML.OnnxRuntime.Tensors;
    //using PAI2.Onnx;

    public class IOUtils
    {
        public static char Sep = Path.DirectorySeparatorChar;

        public static string DevUserFolder = "";
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Sep;

        public static string InstallFolder
        {
            get
            {
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var result = Path.GetDirectoryName(appPath) + Sep;
                if (DevUserFolder != "" && result.Contains($"\\bin\\") && result.Contains("PAI2"))
                {
                    result = DevUserFolder;
                }
                return result;
            }
        }

        public static string DataFolder = UserFolder + @"InklingAI" + Sep;
        public static string DataDrive = Directory.GetDirectoryRoot(DataFolder).Substring(0, DataFolder.IndexOf(Sep));
        public static string CurrentDirectory = Directory.GetCurrentDirectory() + Sep;

        public static string PythonFolder = DataFolder + "python" + Sep;

        public static string SDData = "SD";
        public static string SDModelsName = "models";
        public static string SDImagesName = "images";
        public static string SDWorkingName = "working";
        public static string SDUploadsName = "uploads";

        public static string SDDataFolder = DataFolder + SDData + Sep;
        public static string SDModelsFolder = SDDataFolder + "models" + Sep;
        public static string SDOnnxFolder = SDDataFolder + "onnx" + Sep;
        public static string SDImageFolder = SDDataFolder + "images" + Sep;
        public static string SDWorkingFolder = SDDataFolder + "working" + Sep;
        public static string SDUploadsFolder = SDDataFolder + "uploads" + Sep;
        public static string SDLogFilePath = SDDataFolder + "log.txt";

        public static string ConfigFilemame = "config.json";

        public static string OpenAI_ApiKey = DataFolder + @"openAI.apikey";
        public static string TextRoot = "text";
        public static string TextData = TextRoot + Sep + "data";
        public static string TextParsed = TextRoot + Sep + "parsed";
        public static string TextRootFolder = DataFolder + TextRoot + Sep;
        public static string TextDataFolder = DataFolder + TextData + Sep;
        public static string TextParsedFolder = DataFolder + TextParsed + Sep;

        static IOUtils()
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(SDDataFolder);
            Directory.CreateDirectory(SDModelsFolder);
            Directory.CreateDirectory(SDImageFolder);
            Directory.CreateDirectory(SDWorkingFolder);
            Directory.CreateDirectory(SDUploadsFolder);
        }

        private static string[] defaultKinds = new[] { "main", "input", "mask", "original", "edit", "editmask" };
        public static string ImageName(string name, bool isInput = false, string ext = "png")
        {
            var prefix = defaultKinds.Contains(name) || name.StartsWith("scratch") ? "" : "cn_";
            var suffix = isInput ? "_input" : "";
            var fileType = "." + ext;
            return prefix + name + suffix + fileType;
        }
        public static string ImagePath(string name, bool isInput = false, string ext = "png") =>
            SDWorkingFolder + ImageName(name, isInput, ext);

        public static string IterationStepName(int stepIndex, string prefix = "", string ext = "png")
        {
            prefix = prefix == "" ? "iter" : prefix;
            return prefix + "_" + stepIndex + "." + ext;
        }

        public static string LatentStepName(int stepIndex) => IterationStepName(stepIndex, "latent", "bin");
        public static string IterationStepPath(int stepIndex, string prefix = "") =>
                                    SDWorkingFolder + IterationStepName(stepIndex, prefix);
        public static string RegionStepPath(int stepIndex) => SDWorkingFolder + IterationStepName(stepIndex, "region");
        public static string LatentStepPath(int stepIndex) => SDWorkingFolder + LatentStepName(stepIndex);

        public static string CachedImageFolderName(string imagePath) => Path.GetDirectoryName(imagePath) + Sep + "_" + Path.GetFileNameWithoutExtension(imagePath) + Sep;

        public static IPAddress? LocalIPAddress()
        {
            IPAddress? result = null;
            string hostName = Dns.GetHostName();

            IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
            foreach (IPAddress localIP in localIPs)
            {
                if (localIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    result = localIP;
                    Console.WriteLine("Local IP address: " + localIP.ToString());
                    break;
                }
            }
            return result;
        }

        public static bool CopyFile(string fromPath, string toPath)
        {
            var result = false;
            if (File.Exists(fromPath))
            {
                try
                {
                    File.Copy(fromPath, toPath, true);
                    result = true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error copying file. " + ex.Message);
                }
            }
            return result;
        }

        //public static bool CopyCacheFolder(string fromFolder, string toFolder)
        //{
        //    bool result = fromFolder == toFolder;
        //    if (!result)
        //    {
        //        try
        //        {
        //            var config = StableDiffusionConfig.LoadFromJson(fromFolder + ConfigFilemame);
        //            if (config != null)
        //            {
        //                Directory.CreateDirectory(toFolder);

        //                File.Copy(fromFolder + ConfigFilemame, toFolder + ConfigFilemame, true);
        //                for (int i = 0; i < config.NumInferenceSteps + 1; i++) // include final image, generated as "iter{steps}.png"
        //                {
        //                    CopyIfExists(fromFolder, toFolder, IterationStepName(i));
        //                    CopyIfExists(fromFolder, toFolder, LatentStepName(i));
        //                }
        //                foreach (var root in config.ActiveInputs) // includes controlNet, mask and input image
        //                {
        //                    var inputFilename = ImageName(root, true);
        //                    CopyIfExists(fromFolder, toFolder, inputFilename);
        //                    var outputFilename = ImageName(root, false);
        //                    CopyIfExists(fromFolder, toFolder, outputFilename);
        //                }
        //                result = true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Trace.WriteLine("Error copying folder. " + ex.Message);
        //        }
        //    }
        //    return result;
        //}
        private static bool CopyIfExists(string inputFolder, string outputFolder, string filename)
        {
            bool result = false;
            if (File.Exists(inputFolder + filename) && Directory.Exists(outputFolder))
            {
                File.Copy(inputFolder + filename, outputFolder + filename, true);
                result = true;
            }
            return result;
        }

        //public static StableDiffusionConfig? GetExifData(string path)
        //{
        //    StableDiffusionConfig? result = null;

        //    if (path != null && path != "")
        //    {
        //        var file = ImageFile.FromFile(path);
        //        var pngTag = file.Properties.Get(ExifTag.PNGText);
        //        if (pngTag != null)
        //        {
        //            var desc = pngTag.Value.ToString();
        //            result = StableDiffusionConfig.ParseExif(desc);
        //        }
        //    }
        //    return result;
        //}
        //public static void SetExifData(string path, StableDiffusionConfig config)
        //{
        //    if (path != null && path != "")
        //    {
        //        var file = ImageFile.FromFile(path);
        //        if (file != null)
        //        {
        //            var data = config.GetExifString();
        //            var prop = new PNGText(ExifTag.PNGText, "parameters", data, false);
        //            file.Properties.Add(prop);
        //            file.Save(path);
        //        }
        //    }
        //}




        static readonly HttpClient httpClient = new HttpClient();

        public static async Task DownloadFileAsync(string url, string outputPath, IProgress<double> progress = null)
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error occurred while downloading file. HTTP {response.StatusCode}");
            }

            var contentLength = response.Content.Headers.ContentLength.GetValueOrDefault(-1L);
            var totalBytes = 0L;

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var output = File.Create(outputPath))
            {
                var buffer = new byte[65536];
                var bytesRead = 0;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await output.WriteAsync(buffer, 0, bytesRead);
                    totalBytes += bytesRead;

                    if (contentLength != -1L)
                    {
                        var percentage = (double)totalBytes / contentLength;
                        progress?.Report(percentage * 100);
                    }
                }
            }
        }
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

        public static bool UnetModelExists(string modelName, bool inpaintVersion = false)
        {
            bool result = false;
            var index = ModelFiles.IndexOf(modelName);
            if (index > -1)
            {
                var name = inpaintVersion ? ModelFiles[index + 2] : ModelFiles[index + 0];
                var path = SDModelsFolder + name;
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
                    folder = SDModelsFolder + ControlNetFolder + Sep;
                }
                else if (modelName.EndsWith("_input"))
                {
                    var cnDirectory = modelName.Substring(0, modelName.IndexOf("_"));
                    folder = PythonFolder + "annotator" + Sep + cnDirectory + Sep;
                }
                else
                {
                    folder = SDModelsFolder;
                }
                result = folder + filename;
            }
            return result;
        }





        //public static async Task EnsureModelsLoaded()
        //{
        //    Directory.CreateDirectory(SDModelsFolder + ControlNetFolder);

        //    var filename = "";
        //    var progress = new Progress<double>();
        //    progress.ProgressChanged += (sender, value) =>
        //    {
        //        InklingAI.GenInstance.SetStatusText($"Downloaded {value.ToString("0.0")}% of {filename}");
        //    };

        //    int index = 0;
        //    InklingAI.GenInstance.EnableLoadedModelsThreadCheck();
        //    for (int i = 0; i < ModelFiles.Count; i += 2)
        //    {
        //        var modelName = ModelFiles[i];
        //        filename = ModelFiles[i + 1];
        //        var urlPath = ModelUrlFolder + filename;
        //        var outputPath = GetModelPath(modelName);
        //        var tempPath = SDModelsFolder + "downloading.bin";
        //        if (File.Exists(tempPath))
        //        {
        //            File.Delete(tempPath);
        //        }
        //        if (!File.Exists(outputPath))
        //        {
        //            await DownloadFileAsync(urlPath, tempPath, progress); // guarding for partial downloads
        //            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        //            CopyFile(tempPath, outputPath);
        //            File.Delete(tempPath);
        //            if (index == 0)
        //            {
        //                StableDiffusionClient.Instance.SetVae();
        //            }
        //            if (index == 1)
        //            {
        //                StableDiffusionClient.Instance.SetPrompt();
        //            }
        //            else if (index == 2) // allow gen without mask, then make sure mask is also loaded.
        //            {
        //                await StableDiffusionClient.Instance.SetModel(filename, "");
        //            }
        //            else if (index == 3) // allow gen without mask, then make sure mask is also loaded.
        //            {
        //                await StableDiffusionClient.Instance.SetModel(ModelFiles[i - 1], filename);
        //            }
        //            InklingAI.GenInstance.EnableLoadedModelsThreadCheck();
        //        }
        //        index++;
        //    }
        //    InklingAI.GenInstance.EnableLoadedModelsThreadCheck();

        //    InklingAI.GenInstance.SetStatusText($"Finished loading models. Ready to generate.");
        //}



        //public async static Task<bool> SaveWorkingFolder(Window window)
        //{
        //    bool result = false;
        //    var config = StableDiffusionConfig.LoadFromJson(SDWorkingFolder + ConfigFilemame);
        //    if (config != null)
        //    {
        //        var storage = window.StorageProvider;

        //        FilePickerSaveOptions saveOptions = new FilePickerSaveOptions();
        //        saveOptions.FileTypeChoices = new List<FilePickerFileType>() { FilePickerFileTypes.ImagePng };
        //        saveOptions.DefaultExtension = "PNG Image|*.png";
        //        saveOptions.Title = "Save Generated Folder";
        //        saveOptions.SuggestedFileName = config.GeneratedImageName;
        //        var dialogResult = await storage.SaveFilePickerAsync(saveOptions);
        //        var outputFilePath = dialogResult?.Path.ToString();

        //        if (dialogResult != null && outputFilePath != null && outputFilePath != "")
        //        {
        //            try
        //            {
        //                var bitmap = GetLocalImage(SDImageFolder + config.GeneratedImageName);
        //                if (bitmap == null)
        //                {
        //                    bitmap = GetLocalImage(ImagePath("main"));
        //                }
        //                bitmap?.Save(outputFilePath);
        //                var outputFolderPath = CachedImageFolderName(outputFilePath);
        //                result = CopyCacheFolder(SDWorkingFolder, outputFolderPath);
        //            }
        //            catch (Exception ex)
        //            {
        //                Trace.WriteLine("Error saving working folder. " + ex.Message);
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static Bitmap? GetLocalImage(string folder, string filename = "")
        //{
        //    Bitmap? result = null;
        //    var path = folder + filename; // folder can hold the full file path is filename is ""
        //    try
        //    {
        //        if (File.Exists(path))
        //        {
        //            result = new Bitmap(path);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine("Error getting local image. " + ex.Message);
        //    }
        //    return result;
        //}
        //public static string SaveLocalImage(SKBitmap? img, string? filename, StableDiffusionConfig? exifData = null)
        //{
        //    var success = false;
        //    if (img != null && filename != null && filename != "")
        //    {
        //        try
        //        {
        //            using (var image = SKImage.FromBitmap(img))
        //            using (var data = image.Encode())
        //            {
        //                using (var stream = File.OpenWrite(filename))
        //                {
        //                    data.SaveTo(stream);
        //                }
        //            }

        //            if (exifData != null)
        //            {
        //                SetExifData(filename, exifData);
        //            }

        //            success = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            Trace.WriteLine(ex.Message);
        //        }
        //    }
        //    return success ? filename : "";
        //}

        //public static DenseTensor<byte> GetTensorFromImage(SKBitmap sourceBmp)
        //{
        //    //var bmp = GraphicUtils.ConvertToRGBIfNeeded(sourceBmp);
        //    //var tensorShape = new int[] { bmp.Height, bmp.Width, 3 };
        //    //var bytes = GraphicUtils.BytesFromBitmap(bmp);
        //    //var result = new DenseTensor<byte>(bytes, tensorShape);
        //    //return result;
        //    return new DenseTensor<byte>(0);
        //}
        //unsafe public static DenseTensor<Float16>? GetLatentTensor(string path, int[] tensorShape)
        //{
        //    DenseTensor<Float16>? result = null;
        //    try
        //    {
        //        if (File.Exists(path))
        //        {
        //            byte[] fileBytes = File.ReadAllBytes(path);
        //            int tensorSize = fileBytes.Length / 2;
        //            Float16[] f16_org;
        //            fixed (byte* byteDataPtr = fileBytes)
        //            {
        //                f16_org = new Span<Float16>(byteDataPtr, tensorSize).ToArray();
        //            }
        //            var mem = new Memory<Float16>(f16_org);
        //            result = new DenseTensor<Float16>(mem, tensorShape);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine("Error getting latent data. " + ex.Message);
        //    }
        //    return result;
        //}
        //unsafe public static DenseTensor<Float16> LoadEncoding(string filename, int[] dimensions)
        //{
        //    var data = File.ReadAllBytes(filename);
        //    return OnnxUtils.CreateFloat16TensorFromBytes(data, dimensions);
        //}
        //unsafe public static void SaveEncoding(DenseTensor<Float16> tensor, string filename)
        //{
        //    Span<Float16> tensorSpan = tensor.Buffer.Span;
        //    byte[] byteArray = MemoryMarshal.AsBytes(tensorSpan).ToArray();

        //    // Write bytes to a file
        //    File.WriteAllBytes(filename, byteArray);

        //    //var data = tensor.ToArray();
        //    //byte[] byteArray = new byte[data.Length * 2];

        //    //for (int i = 0; i < data.Length; i++)
        //    //{
        //    //    ushort val = (ushort)data[i].value;
        //    //    byteArray[i * 2] = (byte)(val & 0xFF);
        //    //    byteArray[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        //    //}
        //    //File.WriteAllBytes(filename, byteArray);
        //}
        //public static void SaveShape(DenseTensor<Float16> tensor, string filePath)
        //{
        //    var data = tensor.Dimensions.ToArray();

        //    string json = JsonConvert.SerializeObject(data);
        //    File.WriteAllText(filePath, json);

        //    //using (FileStream fs = new FileStream(filePath, FileMode.Create))
        //    //using (BinaryWriter writer = new BinaryWriter(fs))
        //    //{
        //    //    foreach (int value in data)
        //    //    {
        //    //        writer.Write(value);
        //    //    }
        //    //}
        //}

        //static async Task DownloadFileAsync(string url, string outputPath)
        //{
        //    InklingAI.GenInstance.SetStatusText($"Starting download: {url}");
        //    using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
        //    using (var stream = await response.Content.ReadAsStreamAsync())
        //    using (var output = File.Create(outputPath))
        //    {
        //        await stream.CopyToAsync(output);
        //    }
        //    InklingAI.GenInstance.SetStatusText($"Completed download: {url}");
        //}



    }
}
