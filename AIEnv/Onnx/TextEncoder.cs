namespace AIEnv.Onnx;

using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TextEncoder
{
    public InferenceSession inferenceSession;

    public TextEncoder(string modelPath = @"F:\tmp\vae\sd_ink\text_encoder\model.onnx")
    //public TextEncoder(string modelPath = @"F:\tmp\vae\sd_ink\opt\text_encoder\model.onnx")
    //public TextEncoder(string modelPath = @"F:\tmp\vae\sd_ink\clean\text_encoder\model.onnx")
    //public TextEncoder(string modelPath = @"F:\tmp\vae\sd_v1_5\fp16\text_encoder\model.onnx")
    {
        tokenizer = new Tokenizer();

        var deviceId = 0;
        var sessionOptions = new SessionOptions();
        sessionOptions.AppendExecutionProvider_CUDA(deviceId);
        //sessionOptions.AppendExecutionProvider_CPU();
        inferenceSession = new InferenceSession(modelPath, sessionOptions);


        //inferenceSession = OnnxUtils.GetCudaSession(modelPath);//,@"F:\tmp\vae\sd_ink\clean\text_encoder\model.onnx");
    }

    //public static DenseTensor<float> EncodeText(String prompt, String negPrompt, StableDiffusionConfig config)
    //{
    //    // Load the tokenizer and text encoder to tokenize and encode the text.
    //    var textTokenized = EncodeTextTokens(prompt, config);
    //    var textPromptEmbeddings = TextEncoder(textTokenized, config).ToArray();

    //    // Create uncond_input of blank tokens
    //    var uncondInputTokens = CreateUncondInput();
    //    var uncondEmbedding = TextEncoder(uncondInputTokens, config).ToArray();

    //    // Concant textEmeddings and uncondEmbedding
    //    DenseTensor<float> textEmbeddings = new DenseTensor<float>(new[] { 2, 77, 768 });

    //    for (var i = 0; i < textPromptEmbeddings.Length; i++)
    //    {
    //        textEmbeddings[0, i / 768, i % 768] = uncondEmbedding[i];
    //        textEmbeddings[1, i / 768, i % 768] = textPromptEmbeddings[i];
    //    }
    //    return textEmbeddings;

    //}

    public Float16[] EncodeText(int[] tokenizedInput)
    {
        var input_ids = OnnxUtils.CreateTensor(tokenizedInput, new[] { 1, tokenizedInput.Count() });
        var input = new List<NamedOnnxValue> {
            NamedOnnxValue.CreateFromTensor<int>("input_ids", input_ids)
        };

        Environment.SetEnvironmentVariable("ORT_LOGGING_LEVEL", "VERBOSE");
        RunOptions ro = new RunOptions();
        ro.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_VERBOSE;
        ro.LogVerbosityLevel = 10;
        var encoded = inferenceSession.Run(input, new string[] { "last_hidden_state" }, ro);

        var lastHiddenState = (encoded.ToList().First().Value as IEnumerable<Float16>).ToArray();
        return lastHiddenState;
    }

    public static int[] CreateUncondInput()
    {
        // Create an array of empty tokens for the unconditional input.
        var blankTokenValue = 49407;
        var modelMaxLength = 77;
        var inputIds = new List<Int32>();
        inputIds.Add(49406);
        var pad = Enumerable.Repeat(blankTokenValue, modelMaxLength - inputIds.Count()).ToArray();
        inputIds.AddRange(pad);

        return inputIds.ToArray();
    }

    Tokenizer tokenizer;
    public DenseTensor<Float16> CreateEmbedding(string inputText)
    {
        var inputIds = tokenizer.TokenizeText(inputText);
        var encodedText = EncodeText(inputIds);
        var result = OnnxUtils.CreateTensor(encodedText, new[] { 1, 77, 768 });
        return result;
    }
}
