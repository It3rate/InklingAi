
import torch
from .SchedulerBase import SchedulerBase
from typing import cast

class Dpmpp2MKeras(SchedulerBase):
    def __init__(self):
        super().__init__()
        # extra_args = {} if extra_args is None else extra_args # dict_keys(['cond', 'image_cond', 'uncond', 'cond_scale', 's_min_uncond'])
        # s_in = x.new_ones([x.shape[0]])
        # sigma_fn = lambda t: t.neg().exp()
        # t_fn = lambda sigma: sigma.log().neg()
        # old_denoised = None


    def scale_model_input(self, sample: torch.Tensor, timestep) -> torch.Tensor:
        return sample
    
    #def set_timesteps(self, num_inference_steps: int)-> torch.Tensor:
        super().set_timesteps(num_inference_steps)
        # start = min((self.num_inference_steps-0) * self.step_ratio + self.step_offset, 999)
        # self.timesteps = torch.arange(start, -1, -self.step_ratio, device='cuda', dtype=torch.long)
        # return self.timesteps 
    
    #def step(self, model_output: torch.FloatTensor, timestep, sample: torch.Tensor, generator):
        # prev_timestep = max(0, timestep - self.step_ratio)
        # alpha_prod_t = self.alphas_cumprod[timestep]
        # alpha_prod_t_prev = self.alphas_cumprod[prev_timestep]
        # beta_prod_t = 1 - alpha_prod_t
        # beta_prod_t_prev = 1 - alpha_prod_t_prev

        # pred_original_sample = (sample - beta_prod_t ** (0.5) * model_output) / alpha_prod_t ** (0.5)
        # #variance = (beta_prod_t_prev / beta_prod_t) * (1 - alpha_prod_t / alpha_prod_t_prev)
        # pred_sample_direction = beta_prod_t_prev ** (0.5) * model_output
        # prev_sample = alpha_prod_t_prev ** (0.5) * pred_original_sample + pred_sample_direction
        # return prev_sample

    #def add_noise(self, original_samples: torch.FloatTensor, noise: torch.FloatTensor, timesteps: torch.IntTensor) -> torch.FloatTensor:
        # sqrt_alpha_prod = self.alphas_cumprod[timesteps] ** 0.5
        # sqrt_alpha_prod = sqrt_alpha_prod.flatten().to('cuda', dtype=torch.float16)
        # while len(sqrt_alpha_prod.shape) < len(original_samples.shape):
        #     sqrt_alpha_prod = sqrt_alpha_prod.unsqueeze(-1)

        # sqrt_one_minus_alpha_prod = (1 - self.alphas_cumprod[timesteps]) ** 0.5
        # sqrt_one_minus_alpha_prod = sqrt_one_minus_alpha_prod.flatten().to('cuda', dtype=torch.float16) 
        # while len(sqrt_one_minus_alpha_prod.shape) < len(original_samples.shape):
        #     sqrt_one_minus_alpha_prod = sqrt_one_minus_alpha_prod.unsqueeze(-1)

        # noisy_samples = sqrt_alpha_prod * original_samples + sqrt_one_minus_alpha_prod* noise
        # noisy_samples = noisy_samples.to(dtype=original_samples.dtype)
        # return cast(torch.FloatTensor, noisy_samples)

