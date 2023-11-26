
import torch
import numpy as np

from .SchedulerBase import SchedulerBase

class EulerA(SchedulerBase):
    def __init__(self):
        super().__init__()
        self.init_noise_sigma = 1.0
        self.sigmas_full = ((1 - self.alphas_cumprod) / self.alphas_cumprod).sqrt().to('cuda', dtype=torch.float16) #torch.cat((((1 - self.alphas_cumprod) / self.alphas_cumprod).sqrt().flip(0), torch.tensor([0.0])))
        self.zero = torch.tensor([0.0]).to('cuda', dtype=torch.float16)
        
        self.num_inference_steps = 1000
        self.step_ratio = 1
        self.timesteps = None

    def scale_model_input(self, sample: torch.Tensor, timestep) -> torch.Tensor:
        step_index = torch.where(self.timesteps == timestep)[0]
        sigma = self.sigmas[step_index]
        sample = sample / ((sigma**2 + 1) ** 0.5)
        return sample

    def set_timesteps(self, num_inference_steps: int)-> torch.Tensor:
        super().set_timesteps(num_inference_steps)

        self.timesteps = torch.linspace(0, self.num_train_timesteps - 1, num_inference_steps).flip(0).to('cuda', dtype=torch.long)
        long_indexes = self.timesteps.round().long()
        self.sigmas = torch.concat((self.sigmas_full[long_indexes], self.zero))
        
        # timesteps = np.linspace(0, self.num_train_timesteps - 1, num_inference_steps, dtype=float)[::-1].copy()
        # self.timesteps = torch.from_numpy(timesteps).to('cuda')
        # sigmas = np.array(((1 - self.alphas_cumprod) / self.alphas_cumprod) ** 0.5)
        # sigmas = np.interp(timesteps, np.arange(0, len(sigmas)), sigmas) # can't easily do this in pytorch
        # sigmas = np.concatenate([sigmas, [0.0]]).astype(np.float32)
        # self.sigmas = torch.from_numpy(sigmas).to('cuda', dtype=torch.float16)

        return self.timesteps # always returns all steps, start somewhere in middle if using img2img (strength > 0)
    
    def step(self, model_output: torch.FloatTensor, timestep, sample: torch.Tensor, generator):
        step_index = torch.where(self.timesteps == timestep)[0]
        sigma = self.sigmas[step_index]
        pred_original_sample = sample - sigma * model_output
        sigma_from = self.sigmas[step_index]
        sigma_to = self.sigmas[step_index + 1]
        sigma_up = (sigma_to**2 * (sigma_from**2 - sigma_to**2) / sigma_from**2) ** 0.5
        sigma_down = (sigma_to**2 - sigma_up**2) ** 0.5

        # 2. Convert to an ODE derivative
        derivative = (sample - pred_original_sample) / sigma
        dt = sigma_down - sigma
        prev_sample = sample + derivative * dt
        device = model_output.device
        noise = torch.randn(model_output.shape, generator=generator, device=device, dtype=torch.float16, layout=torch.strided).to(device)
        prev_sample = prev_sample + noise * sigma_up
        return prev_sample
    

    def add_noise(self, original_samples: torch.FloatTensor, noise: torch.FloatTensor, timestep: torch.IntTensor) -> torch.FloatTensor:
        #schedule_timesteps = self.timesteps
        step_index = (self.timesteps == timestep).nonzero().item()# [(schedule_timesteps == t).nonzero().item() for t in timesteps]
        sigma = self.sigmas[step_index].flatten()
        while len(sigma.shape) < len(original_samples.shape):
            sigma = sigma.unsqueeze(-1)
        noisy_samples = original_samples + noise * sigma
        return noisy_samples