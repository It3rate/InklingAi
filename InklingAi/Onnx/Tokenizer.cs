namespace InklingAi.Onnx;

using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class Tokenizer
{
    public InferenceSession inferenceSession;
    public int DeviceId = 0;

    public Tokenizer(string modelPath = @"F:\tmp\vae\sd_ink\clean\tokenizer\cliptokenizer.onnx")
    {
        inferenceSession = OnnxUtils.GetCudaSession(modelPath);
    }

    public int[] TokenizeText(string text)
    {
        return text == "" ? CreateEmptyTokens() : CreateTokens(text);
    }

    private int[] CreateTokens(string text)
    {
        var result = new int[0];
        var inputTensor = new DenseTensor<string>(new string[] { text }, new int[] { 1 });
        var inputString = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor<string>("string_input", inputTensor) };
        var tokens = inferenceSession.Run(inputString);
        if (tokens != null && tokens.Count > 0)
        {
            var inputIds = (tokens.ToList().First().Value as IEnumerable<long>).ToList();
            inputIds.RemoveAt(inputIds.Count - 1);
            inputIds.RemoveAt(inputIds.Count - 1);
            result = inputIds.Select(x => (int)x).ToArray();
            var modelMaxLength = 77;
            // Pad array with 49407 until length is modelMaxLength
            if (result.Length < modelMaxLength)
            {
                var pad = Enumerable.Repeat(49407, 77 - result.Length).ToArray();
                result = result.Concat(pad).ToArray();
            }
        }

        return result;
    }

    private static int[] CreateEmptyTokens()
    {
        var blankTokenValue = 49407;
        var modelMaxLength = 77;
        var inputIds = new List<Int32>();
        inputIds.Add(49406);
        var pad = Enumerable.Repeat(blankTokenValue, modelMaxLength - inputIds.Count()).ToArray();
        inputIds.AddRange(pad);

        return inputIds.ToArray();
    }
}
