import torch
import numpy as np
from typing import cast
from diffusers import AutoencoderKL
from diffusers.models.autoencoder_kl import AutoencoderKLOutput
from PIL import Image
from diffusers.utils import randn_tensor
from typing import Optional
from .LatentConversion import LatentConversion
from .SchedulerComponent import SchedulerComponent

class NoiseComponent():

    def __init__(self, latent_conversion:LatentConversion, scheduler_component:SchedulerComponent):
        self.latent_conversion = latent_conversion
        self.scheduler_component = scheduler_component
        self.num_channels_latents = 4

    @property
    def device(self):
        return self.latent_conversion.device
    
    @property
    def dtype(self):
        return self.latent_conversion.vae.dtype
    
    @property
    def vae(self):
        return self.latent_conversion.vae
    
    @property
    def scheduler(self):
        return self.scheduler_component.scheduler
    
    @property
    def generator(self):
        return self.scheduler_component.generator
    
    @property
    def latent_shape(self):
        vae_scale = self.latent_conversion.vae_scale_factor
        width = self.latent_conversion.last_image_width
        height = self.latent_conversion.last_image_height
        shape = (self.num_channels_latents, height // vae_scale, width // vae_scale)
        return shape

    def check_inputs(self):
        return True

    def get_noised_latent(self, image:Optional[Image.Image] = None, noise_amount:Optional[torch.Tensor] = None) -> torch.FloatTensor:
        if image is None:
            result = randn_tensor(self.latent_shape, generator=self.generator, device=self.vae.device, dtype=self.dtype)
        else:
            result = self.add_noise_to_image(image, noise_amount)
        # scale the initial noise by the standard deviation required by the scheduler
        result = result * self.scheduler_component.init_noise_sigma()
        return cast(torch.FloatTensor, result)
    
    def add_noise_to_latent(self, latent:torch.Tensor, noise_amount:Optional[torch.Tensor] = None) -> torch.FloatTensor:
        result = self.add_noise_to_tensor(latent, noise_amount)
        result = result * self.scheduler_component.init_noise_sigma()
        return cast(torch.FloatTensor, result)
    
    def get_mask_latent(self, img_mask:Image.Image) -> torch.FloatTensor:
        mask = np.array(img_mask.convert("L"))
        mask = mask.astype(np.float32) / 255.0
        # mask[mask < 0.5] = 0
        # mask[mask >= 0.5] = 1
        tmask = torch.from_numpy(mask)
        tmask = tmask.unsqueeze(0).unsqueeze(0)
        tmask = torch.nn.functional.interpolate(tmask, size=(64,64))#self.latent_shape[1:])
        tmask = tmask.to(device=self.device, dtype=self.dtype)
        return cast(torch.FloatTensor, tmask)
    
    @torch.no_grad()
    def add_noise_to_image(self, image:Image.Image, noise_amount:Optional[torch.Tensor])->torch.Tensor:
        latents:torch.Tensor = self.latent_conversion.tensor_from_image(image)
        return self.add_noise_to_tensor(latents, noise_amount)
    
    @torch.no_grad()
    def add_noise_to_tensor(self, latents:torch.Tensor, noise_amount:Optional[torch.Tensor])->torch.Tensor:
        noise = randn_tensor(latents.shape, dtype=latents.dtype, device=latents.device, generator=self.generator)
        # noised_amount is the noising value from 0 to 1000, this requires the correct set_timesteps has been completed on the scheduler
        noised_image = self.scheduler.add_noise(latents, noise, noise_amount) # type: ignore
        return noised_image

