import torch
import inspect
import random
from typing import Optional, cast
from Schedulers.SchedulerBase import SchedulerBase
from StableDiffusionConfig import StableDiffusionConfig
from diffusers import DDPMScheduler,DDIMScheduler,PNDMScheduler,LMSDiscreteScheduler,EulerDiscreteScheduler,EulerAncestralDiscreteScheduler,DPMSolverMultistepScheduler

from Schedulers.DDIM import DDIM
from Schedulers.EulerA import EulerA

class SchedulerComponent():

    def __init__(self, scheduler_id = "euler_anc", num_inference_steps:int = 20, image_preservation:float = 0, seed:int=-1):
        self.device = "cuda"
        self.generator:torch.Generator = torch.Generator(device=self.device)
        #self.model_version = model_version
        self.num_inference_steps = num_inference_steps
        self.image_preservation = image_preservation
        self.eta = 0
        self.seed = seed
        self.last_seed = seed
        self.set_seed(self.seed)

        self.ddim = DDIM()
        self.euler_anc = EulerA()

        # self.ddpm = DDPMScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.ddim = DDIMScheduler(beta_start=0.0001, beta_end=0.02,beta_schedule="scaled_linear",set_alpha_to_one=False,steps_offset=1,clip_sample=False)  #DDIMScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.pndm = PNDMScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.lms = LMSDiscreteScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.euler_anc = EulerAncestralDiscreteScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.euler = EulerDiscreteScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.dpm = DPMSolverMultistepScheduler.from_pretrained(self.model_version, subfolder="scheduler")# type: ignore
        # self.scheduler_map = {"ddpm":self.ddpm, "ddim":self.ddim, "pndm":self.pndm , "lms":self.lms, "euler_anc":self.euler_anc, "euler":self.euler, "dpm":self.dpm}

        self.scheduler_map = {"ddim":self.ddim,"euler_anc":self.euler_anc,}
        self.scheduler:SchedulerBase = self.scheduler_map[scheduler_id]

    def init_noise_sigma(self):
        return self.scheduler.init_noise_sigma
    
    def set_with_config(self, config:StableDiffusionConfig):
        self.set_seed(config.seed)
        if(config.initial_image_name == '' and config.initial_latents_name == ''):
            config.image_preservation = 0
        self.set_steps_and_image_preservation(config.num_inference_steps, config.image_preservation)

    def check_inputs(self):
        return True

    def set_seed(self, seed):
        self.seed = seed
        self.last_seed = seed if seed > -1 else random.randint(0, 0xFFFF)
        self.generator.manual_seed(self.last_seed)
        return self.last_seed

    def set_steps_and_image_preservation(self, steps:int, image_preservation:float = 0):
        self.steps = steps
        self.image_preservation = image_preservation

    def set_scheduler(self, scheduler_id:str):
        if scheduler_id in self.scheduler_map:
            self.scheduler = self.scheduler_map[scheduler_id]
            result = scheduler_id
        else:
            result = "Error setting scheduler"
        return result

    @torch.no_grad()
    def get_timesteps(self, num_inference_steps)->tuple[torch.Tensor,int]:
        result:torch.Tensor
        self.num_inference_steps = num_inference_steps
        self.scheduler.set_timesteps(self.num_inference_steps)
        t_start = max(0,min(self.num_inference_steps - 1, int(self.num_inference_steps * self.image_preservation)))  # always take at least one step, but not more than num_inf
        result = cast(torch.Tensor, self.scheduler.timesteps)[t_start:]
        steps =self.num_inference_steps - t_start
        return (result, steps)
    
    def scale_model_input(self, sample: torch.Tensor, timestep) -> torch.Tensor:
        return self.scheduler.scale_model_input(sample, timestep)
    
    def step(self, model_output: torch.FloatTensor, timestep, sample: torch.Tensor):
        return self.scheduler.step(model_output, timestep, sample, generator=self.generator)

    @torch.no_grad()
    def get_timesteps_partial(self):
        init_timestep = min(int(self.num_inference_steps * self.image_preservation), self.num_inference_steps)
        t_start = max(self.num_inference_steps - init_timestep, 0)
        timesteps = self.scheduler.timesteps[t_start:] # type: ignore
        return timesteps, self.num_inference_steps - t_start
    

    def add_eta_if_needed(self, eta):
        # eta corresponds to η in DDIM paper: https://arxiv.org/abs/2010.02502 and should be between [0, 1]
        accepts_eta = "eta" in set(inspect.signature(self.scheduler.step).parameters.keys())
        extra_step_kwargs = {}
        if accepts_eta:
            self.eta = eta
            extra_step_kwargs["eta"] = eta

        # check if the scheduler accepts generator
        accepts_generator = "generator" in set(inspect.signature(self.scheduler.step).parameters.keys())
        if accepts_generator:
            extra_step_kwargs["generator"] = self.generator
        return extra_step_kwargs