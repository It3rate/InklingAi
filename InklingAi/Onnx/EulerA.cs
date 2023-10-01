namespace InklingAi.Onnx;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Linq;

public class EulerA : SchedulerBase
{
    protected List<float> _sigmasFull;
    public EulerA()
    {
        _sigmasFull = _alphasCumulativeProducts.Select(alpha => (float)Math.Sqrt((1 - alpha) / alpha)).ToList();
        //self.zero = torch.tensor([0.0]).to('cuda', dtype = torch.float16)
    }

    public override int[] SetTimesteps(int numInferenceSteps)
    {
        base.SetTimesteps(numInferenceSteps);

        //self.timesteps = torch.linspace(0, self.num_train_timesteps - 1, num_inference_steps).flip(0).to('cuda', dtype = torch.long)
        //long_indexes = self.timesteps.round().long()
        //self.sigmas = torch.concat((self.sigmas_full[long_indexes], self.zero))


        double start = 0;
        double stop = _numTrainTimesteps - 1;
        double[] timesteps = np.linspace(start, stop, _numInferenceSteps).ToArray<double>();
        Timesteps = timesteps.Select(x => (int)x).Reverse().ToList(); // to int and reverse
        Sigmas = Timesteps.Select(t => _sigmasFull[t]).ToArray();

        return Timesteps.ToArray();
    }

    public override DenseTensor<Float16> ScaleInput(DenseTensor<Float16> sample, int timestep)
    {
        //step_index = torch.where(self.timesteps == timestep)[0]
        //sigma = self.sigmas[step_index]
        //sample = sample / ((sigma * *2 + 1) * *0.5)
        //return sample

        int stepIndex = this.Timesteps.IndexOf(timestep);
        var sigma = this.Sigmas[stepIndex];
        sigma = (float)Math.Sqrt(sigma*sigma + 1);

        var div = OnnxUtils.DivideTensorByFloat(sample, sigma);
        return div;
    }

    public override DenseTensor<Float16> Step(Tensor<Float16> modelOutput16,int timestep,Tensor<Float16> sample16 )
    {
        var dims = sample16.Dimensions.ToArray();
        var modelOutput = modelOutput16.ToFloatTensor();
        var sample = sample16.ToFloatTensor();

        int stepIndex = Timesteps.IndexOf((int)timestep);
        var sigma = Sigmas[stepIndex];

        // 1. compute predicted original sample (x_0) from sigma-scaled predicted noise
        //  pred_original_sample = sample - sigma * model_output
        var predOriginalSample = OnnxUtils.SubtractTensors(sample, OnnxUtils.MultipleTensorByFloat(modelOutput, sigma));

        float sigmaFrom = Sigmas[stepIndex];
        float sigmaTo = Sigmas.Length > stepIndex + 1 ? Sigmas[stepIndex + 1] : 0;

        var sigmaFromSq = sigmaFrom * sigmaFrom;
        var sigmaToSq = sigmaTo * sigmaTo;
        var sigmaFromLessSigmaTo = sigmaFromSq - sigmaToSq;

        var sigmaUpResult = (sigmaToSq * sigmaFromLessSigmaTo) / sigmaFromSq;
        var sigmaUp = sigmaUpResult < 0 ? -MathF.Sqrt(MathF.Abs(sigmaUpResult)) : MathF.Sqrt(sigmaUpResult);

        var sigmaDownResult = sigmaToSq - sigmaUp*sigmaUp;
        var sigmaDown = sigmaDownResult < 0 ? -MathF.Sqrt(MathF.Abs(sigmaDownResult)) : MathF.Sqrt(sigmaDownResult);

        // 2. Convert to an ODE derivative
        var sampleMinusPredOriginalSample = OnnxUtils.SubtractTensors(sample, predOriginalSample);
        DenseTensor<float> derivative = OnnxUtils.DivideTensorByFloat(sampleMinusPredOriginalSample, sigma);// (sample - predOriginalSample) / sigma;

        float dt = sigmaDown - sigma;

        DenseTensor<float> prevSample = OnnxUtils.AddTensors(sample, OnnxUtils.MultipleTensorByFloat(derivative, dt));// sample + derivative * dt;

        //var noise = generator == null ? np.random.randn(modelOutput.shape) : np.random.RandomState(generator).randn(modelOutput.shape);
        var noise = OnnxUtils.GetRandomTensor(prevSample.Dimensions);
        prevSample = OnnxUtils.AddTensors(prevSample, OnnxUtils.MultipleTensorByFloat(noise, sigmaUp));// prevSample + noise * sigmaUp;

        return prevSample.ToFloat16Tensor();
    }

}