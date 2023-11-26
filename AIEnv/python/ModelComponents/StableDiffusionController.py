from xmlrpc.client import Boolean
import torch
from typing import Optional
from PIL import Image
from diffusers import StableDiffusionPipeline, StableDiffusionImg2ImgPipeline, StableDiffusionInpaintPipeline
from diffusers.models import UNet2DConditionModel, AutoencoderKL
from diffusers import DDIMScheduler

from StableDiffusionConfig import StableDiffusionConfig
from Utils import ImageUtils
from ModelComponents import PromptComponent, LatentConversion, NoiseComponent, SchedulerComponent, UnetComponent
from Models.Unet import Unet

class StableDiffusionController():

    def __init__(self):
        self.utils = ImageUtils.instance()
        self.root_folder = self.utils.sd_folder
        self.image_folder =  self.utils.ensure_folder_from_root(self.root_folder, "images") 
        self.working_folder =  self.utils.ensure_folder_from_root(self.root_folder, "working") 
        self.uploads_folder =  self.utils.ensure_folder_from_root(self.root_folder, "uploads") 

        self.sc = SchedulerComponent("ddim")
        self.pc = PromptComponent(self.utils.sd_model_folder)# type: ignore
        self.lc = LatentConversion(self.utils.sd_model_folder, self.sc)# type: ignore
        self.nc = NoiseComponent(self.lc, self.sc)
        self.default_model_name = "revAnimated_v122_f16.bin"
        self.default_model_inpaint_name = "revAnimated_v121Inp_inpainting_f16.bin"
        self.uc = UnetComponent(self.utils.sd_model_folder, self.default_model_name, self.default_model_inpaint_name, self.pc, self.lc, self.nc, self.sc, self.callback)# type: ignore
        self.control_net = self.uc.control_net

        self.current_latents:Optional[torch.Tensor] = None
        self.prev_latents:Optional[torch.Tensor] = None

        self.latest_step = -1
        self.latest_offset_index = -1

        #self.test()

    def test(self):
        self.inpaint = StableDiffusionInpaintPipeline.from_pretrained("runwayml/stable-diffusion-inpainting",revision="fp16",torch_dtype=torch.float16,device_map="auto")
        # self.uc.unet = self.inpaintX.unet
        # self.lc.vae = self.inpaintX.vae
        #self.img2img = StableDiffusionImg2ImgPipeline(m.vae, m.text_encoder, m.tokenizer, m.unet, self.sc.scheduler, None, None, False)

        # torch.save(unet_inpaint.state_dict(), "ip_unet.bin")
        # torch.save(self.model.text_encoder.state_dict(), "text_encoder_f16.bin")
        # torch.save(self.model.vae.state_dict(), "vae_f16.bin")


    @property
    def steps(self):
        return self.sc.steps 
    @property
    def current_step(self):
        return self.uc.current_step
    
    def set_with_config(self, config:StableDiffusionConfig):
        self.pc.set_with_config(config)
        self.sc.set_with_config(config)
        self.lc.set_with_config(config)
        self.uc.set_with_config(config)
        self.current_latents = torch.zeros(1) # todo; Latents can come from outside
    
    # prompts
    def set_prompts(self, positive_prompt:str, negative_prompt:str):
        return self.pc.set_prompt_text(positive_prompt, negative_prompt)
    
    def set_text_embeds(self, positive_embed:Optional[torch.Tensor], negative_embed:Optional[torch.Tensor]):
        return self.pc.set_embeds(positive_embed, negative_embed)
    
    def use_attention_mask(self, use_attention_mask:bool):
        self.pc.use_attention_mask(use_attention_mask)

    # scheduler
    def set_scheduler(self, scheduler_id:str)->str:
        return self.sc.set_scheduler(scheduler_id)
    
    def set_seed(self, seed:int):
        self.sc.set_seed(seed)
        
    # unet
    def set_callback_steps(self, cb_steps):
        self.uc.callback_steps = cb_steps
    
    def set_size(self, w:int, h:int):
        self.lc.last_image_width = w
        self.lc.last_image_height = h
        return (self.lc.last_image_width, self.lc.last_image_height) # rounded vals may change

    def generate_image(self, config:StableDiffusionConfig)->str:
        self.lc.ensure_model()
        self.pc.ensure_model()
        self.set_with_config(config)

        image = self.uc.generate_image(config.num_inference_steps, config.guidance_scale)
        filepath = self.lc.post_generate(config, image, self.uc.has_region)
        self.uc.current_step = -1
        self.latest_step = -1
        self.latest_offset_index = -1
        return filepath


    def callback(self, iter, t, latents):
        # t_index = torch.where(self.scheduler.timesteps == t)[0][0].item()
        # print('\n',self.start_t_index + iter, t_index)
        self.uc.current_step = iter
        self.uc.current_offset_index = self.uc.start_index + iter

        if self.uc.current_step > 0 and self.prev_latents.shape == latents.shape:
            self.current_latents += latents - self.prev_latents
            self.prev_latents = latents.clone()

            frac = (self.uc.current_offset_index ) / float(self.uc.steps)# type: ignore 
            interpolated_latents = self.current_latents * (1.0-frac) + latents * frac
        else:
            if self.current_latents.shape != latents.shape:
                self.current_latents = torch.zeros_like(latents)
            self.prev_latents = latents.clone()
            interpolated_latents = self.current_latents.clone()

        #with torch.no_grad():
        image = self.lc.image_from_tensor(interpolated_latents)
        self.lc.save_latent_iteration(self.uc.current_offset_index, interpolated_latents, image, self.uc.has_region())
            
        self.latest_step = self.uc.current_step
        self.latest_offset_index = self.uc.current_offset_index