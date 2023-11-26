
import torch

class SchedulerBase():
    def __init__(self):
        self.num_train_timesteps: int = 1000
        beta_start: float = 0.00085
        beta_end = 0.012
        self.betas = (torch.linspace(beta_start**0.5, beta_end**0.5, self.num_train_timesteps, dtype=torch.float32) ** 2)
        self.alphas = 1.0 - self.betas
        self.alphas_cumprod = torch.cumprod(self.alphas, dim=0)
        self.init_noise_sigma = 1.0
    
        self.num_inference_steps = 1000
        self.step_ratio = 1
        self.timesteps = None
        self.step_offset = 1

    def set_timesteps(self, num_inference_steps: int):
        self.num_inference_steps = num_inference_steps
        self.step_ratio = self.num_train_timesteps // self.num_inference_steps

    def scale_model_input(self, sample: torch.Tensor, timestep) -> torch.Tensor:
        return sample
    
    def step(self, model_output: torch.FloatTensor, timestep, sample: torch.Tensor, generator):
        pass
    
    def add_noise(self, original_samples: torch.FloatTensor, noise: torch.FloatTensor, timesteps: torch.IntTensor) -> torch.FloatTensor:
        pass