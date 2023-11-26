
import torch
from .SchedulerBase import SchedulerBase
from typing import cast

class DDIM(SchedulerBase):
    def __init__(self):
        super().__init__()
        self.init_noise_sigma = 1.0
        
        self.num_inference_steps = 1000
        self.step_ratio = 1
        self.timesteps = None

    def scale_model_input(self, sample: torch.Tensor, timestep) -> torch.Tensor:
        return sample
    
    def set_timesteps(self, num_inference_steps: int)-> torch.Tensor:
        super().set_timesteps(num_inference_steps)
        start = min((self.num_inference_steps-0) * self.step_ratio + self.step_offset, 999)
        self.timesteps = torch.arange(start, -1, -self.step_ratio, device='cuda', dtype=torch.long)
        return self.timesteps # always returns all steps, start somewhere in middle if using img2img (strength > 0)
    
    def step(self, model_output: torch.FloatTensor, timestep, sample: torch.Tensor, generator):
        prev_timestep = max(0, timestep - self.step_ratio)
        alpha_prod_t = self.alphas_cumprod[timestep]
        alpha_prod_t_prev = self.alphas_cumprod[prev_timestep]
        beta_prod_t = 1 - alpha_prod_t
        beta_prod_t_prev = 1 - alpha_prod_t_prev

        pred_original_sample = (sample - beta_prod_t ** (0.5) * model_output) / alpha_prod_t ** (0.5)
        pred_sample_direction = beta_prod_t_prev ** (0.5) * model_output
        prev_sample = alpha_prod_t_prev ** (0.5) * pred_original_sample + pred_sample_direction
        return prev_sample

    def add_noise(self, original_samples: torch.FloatTensor, noise: torch.FloatTensor, timesteps: torch.IntTensor) -> torch.FloatTensor:
        sqrt_alpha_prod = self.alphas_cumprod[timesteps] ** 0.5
        sqrt_alpha_prod = sqrt_alpha_prod.flatten().to('cuda', dtype=torch.float16)
        while len(sqrt_alpha_prod.shape) < len(original_samples.shape):
            sqrt_alpha_prod = sqrt_alpha_prod.unsqueeze(-1)

        sqrt_one_minus_alpha_prod = (1 - self.alphas_cumprod[timesteps]) ** 0.5
        sqrt_one_minus_alpha_prod = sqrt_one_minus_alpha_prod.flatten().to('cuda', dtype=torch.float16) 
        while len(sqrt_one_minus_alpha_prod.shape) < len(original_samples.shape):
            sqrt_one_minus_alpha_prod = sqrt_one_minus_alpha_prod.unsqueeze(-1)

        noisy_samples = sqrt_alpha_prod * original_samples + sqrt_one_minus_alpha_prod* noise
        noisy_samples = noisy_samples.to(dtype=original_samples.dtype)
        return cast(torch.FloatTensor, noisy_samples)

