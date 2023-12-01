
namespace AIEnv.Onnx;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public abstract class SchedulerBase
{
    public List<int> Timesteps { get; set; }

    public float[] Sigmas { get; set; }
    public float InitNoiseSigma { get; set; }
    public float StepRatio { get; set; }

    protected readonly int _numTrainTimesteps = 1000;
    protected List<float> _alphas;
    protected List<float> _betas;
    protected List<float> _alphasCumulativeProducts;
    protected List<float> _sigmas;

    protected int _numInferenceSteps;

    // self.betas = torch.linspace(beta_start, beta_end, num_train_timesteps, dtype=torch.float32)
    // self.alphas = 1.0 - self.betas
    // self.alphas_cumprod = torch.cumprod(self.alphas, dim= 0)
    // sigmas = np.array(((1 - self.alphas_cumprod) / self.alphas_cumprod) ** 0.5)
    // sigmas = np.concatenate([sigmas[::- 1], [0.0]]).astype(np.float32)
    // self.sigmas = torch.from_numpy(sigmas)


    public SchedulerBase()
    {
        var betaStart = 0.00085f;
        var betaEnd = 0.012f;
        var start = (float)Math.Sqrt(betaStart);
        var end = (float)Math.Sqrt(betaEnd);
        _betas = np.linspace(start, end, _numTrainTimesteps).ToArray<float>().Select(x => x * x).ToList();
        _alphas = _betas.Select(beta => 1 - beta).ToList();
        _alphasCumulativeProducts = _alphas.Select((alpha, i) => _alphas.Take(i + 1).Aggregate((a, b) => a * b)).ToList();
        _sigmas = _alphasCumulativeProducts.Select(alpha_prod => ((float)Math.Sqrt((1 - alpha_prod) / alpha_prod))).Reverse().ToList();
        InitNoiseSigma = _sigmas.Max(); // 1.0f; //  
        StepRatio = 1f;
    }

    public virtual int[] SetTimesteps(int numInferenceSteps)
    {
        _numInferenceSteps = numInferenceSteps;
        StepRatio = _numTrainTimesteps;
        return new int[0];
    }

    public virtual DenseTensor<Float16> ScaleInput(DenseTensor<Float16> sample, int timestep)
    {
        return sample;
    }

    public abstract DenseTensor<Float16> Step(Tensor<Float16> modelOutput, int timestep, Tensor<Float16> sample);
    //public abstract DenseTensor<Float16> AddNoise(Tensor<Float16> originalSample, int timestep, Tensor<Float16> noise);
}















//        public bool is_scale_input_called;

//        public abstract List<int> Timesteps { get; set; }

//        //public SchedulerBase(int numTrainTimesteps = 1000)
//        //{
//        //    _numTrainTimesteps = numTrainTimesteps;
//        //}

//        public static double[] Interpolate(double[] timesteps, double[] range, List<double> sigmas)
//        {
//            // Create an output array with the same shape as timesteps
//            var result = np.zeros(timesteps.Length + 1);

//            // Loop over each element of timesteps
//            for (int i = 0; i < timesteps.Length; i++)
//            {
//                // Find the index of the first element in range that is greater than or equal to timesteps[i]
//                int index = Array.BinarySearch(range, timesteps[i]);

//                // If timesteps[i] is exactly equal to an element in range, use the corresponding value in sigma
//                if (index >= 0)
//                {
//                    result[i] = sigmas[index];
//                }

//                // If timesteps[i] is less than the first element in range, use the first value in sigmas
//                else if (index == -1)
//                {
//                    result[i] = sigmas[0];
//                }

//                // If timesteps[i] is greater than the last element in range, use the last value in sigmas
//                else if (index == -range.Length - 1)
//                {
//                    result[i] = sigmas[-1];
//                }

//                // Otherwise, interpolate linearly between two adjacent values in sigmas
//                else
//                {
//                    index = ~index; // bitwise complement of j gives the insertion point of x[i]
//                    double t = (timesteps[i] - range[index - 1]) / (range[index] - range[index - 1]); // fractional distance between two points
//                    result[i] = sigmas[index - 1] + t * (sigmas[index] - sigmas[index - 1]); // linear interpolation formula
//                }

//            }
//            //  add 0.000 to the end of the result
//            result = np.add(result, 0.000f);

//            return result.ToArray<double>();
//        }

//        public DenseTensor<Float16> ScaleInput(DenseTensor<Float16> sample, int timestep)
//        {
//            // Get step index of timestep from TimeSteps
//            int stepIndex = Timesteps.IndexOf(timestep);
//            // Get sigma at stepIndex
//            var sigma = Sigmas[stepIndex];
//            sigma = (float)Math.Sqrt((Math.Pow(sigma, 2) + 1));

//            // Divide sample tensor shape {2,4,64,64} by sigma
//            var result = OnnxUtils.DivideTensorByFloat(sample.ToFloatTensor().ToArray(), sigma, sample.Dimensions.ToArray());
//            is_scale_input_called = true;
//            return result.ToFloat16Tensor();
//        }
//        public abstract int[] SetTimesteps(int num_inference_steps);

//        public abstract DenseTensor<Float16> Step(
//               Tensor<Float16> modelOutput16,
//               int timestep,
//               Tensor<Float16> sample16,
//               int order = 4);
//    }
//}