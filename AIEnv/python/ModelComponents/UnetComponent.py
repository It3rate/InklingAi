import torch
import numpy as np
import gc
import os
from PIL import Image
from torchvision.transforms import ToTensor
from typing import cast, Optional
from diffusers import UNet2DConditionModel
#from Conversion.ConvertSafetensors import ConvertSafetensors
from Models.Unet import Unet
from Models.UnetForOnnx import UnetForOnnx
from Models.UnetFromOnnx import UnetFromOnnx
from Schedulers.DDIM import DDIM
from Schedulers.EulerA import EulerA

from Utils import ImageUtils
from Models.ControlNet import ControlNet
from ModelComponents.ControlNetUtils import ControlNetUtils
from StableDiffusionConfig import StableDiffusionConfig
from .PromptComponent import PromptComponent
from .LatentConversion import LatentConversion
from .NoiseComponent import NoiseComponent
from .SchedulerComponent import SchedulerComponent


class UnetComponent():
    def __init__(self, models_folder:str, start_model:str, start_inpaint_model:str, prompt_component:PromptComponent, latent_conversion:LatentConversion, noise_component:NoiseComponent, scheduler_component:SchedulerComponent, callback=None):
        self.device = torch.device('cuda')
        #self.convert_safetensors = ConvertSafetensors()
        self.unet = None
        self.unet_inpaint = None
        self.models_folder = models_folder
        self.current_model_name = ""
        self.current_model_inpaint_name = ""
        self.set_model(start_model, start_inpaint_model)

        self.prompt_component = prompt_component
        self.latent_conversion = latent_conversion
        self.noise_component = noise_component
        self.scheduler_component = scheduler_component
        self.num_inference_steps = 20
        self.guidance_scale = 7.5
        self.callback = callback
        self.callback_steps = 0
        
        self.start_index = 0
        self.current_step = -1
        self.current_offset_index = 0
        self.input_image:Optional[Image.Image] = None
        self.input_latents:Optional[torch.FloatTensor] = None
        self.mask_image:Optional[Image.Image] = None
        self.mask_bounds:list = []
        self.mask_latents:Optional[torch.FloatTensor] = None

        self.control_net = ControlNetUtils()
        self.active_inputs:list = []
        self.previous_active_inputs:list = []
        self.control_net_upload_paths:dict = {}
        self.control_net_image_paths:dict = {}
        self.control_net_input_paths:dict = {}
        self.control_nets = {}
        self.control_net_names = ["canny", "depth", "leres", "hed", "openpose", "normal", "mlsd", "pidinet", "uniformer"]
        for name in self.control_net_names:
            model = ControlNet(name, self.latent_conversion)
            #model.load()
            self.control_nets[model.kind] = model
            self.control_net_upload_paths[name] = f"{self.latent_conversion.uploads_folder}cn_{name}.png"
            self.control_net_image_paths[name] = f"{self.latent_conversion.working_folder}cn_{name}.png"
            self.control_net_input_paths[name] = f"{self.latent_conversion.working_folder}cn_{name}_input.png"
        
    
    @property
    def width(self):
        return self.latent_conversion.last_image_width
    @property
    def height(self):
        return self.latent_conversion.last_image_height
    
    @property
    def scheduler(self):
        return self.scheduler_component.scheduler
    
    @property
    def steps(self):
        return self.scheduler_component.steps
    

    def set_model(self, model_name:str, model_inpaint_name:str):
        filepath = ""
        filepath_inpaint = ""

        if model_name != self.current_model_name:
            filepath = self.replace_unet(model_name)

        if model_inpaint_name != self.current_model_inpaint_name:
            filepath_inpaint = self.replace_inpaint(model_inpaint_name)

        return (filepath, filepath_inpaint)

    def del_unet(self):
        if self.unet != None:
            del self.unet
            torch.cuda.empty_cache()
            gc.collect()
            self.unet = None

    def replace_unet(self, model_name:str):
        self.del_unet()
        filepath = f"{self.models_folder}{model_name}"
        data = self.get_model(filepath)
        if data != None:
            self.unet = Unet(data)
            self.current_model_name = model_name
        else:
            self.current_model_name = ""
        
        # # if data != None:
        #     #self.unet = Unet(data)
        # self.unet = UnetForOnnx(model_name)
        # #self.unet = UnetFromOnnx()
        # self.current_model_name = model_name
        # # else:
        # #     self.current_model_name = ""

        return filepath

    def del_inpaint(self):
        if self.unet_inpaint != None:
            del self.unet_inpaint
            torch.cuda.empty_cache()
            gc.collect()
            self.unet_inpaint = None

    def replace_inpaint(self, model_inpaint_name:str):
        self.del_inpaint()
        filepath = f"{self.models_folder}{model_inpaint_name}"
        data = self.get_model(filepath)
        if data != None:
            self.unet_inpaint = Unet(data)
            self.current_model_inpaint_name = model_inpaint_name
        else:
            self.current_model_inpaint_name = ""
        return filepath
    
    def get_model(self, filepath:str):
        data = None
        if os.path.exists(filepath):
            data = torch.load(filepath)
        return data

    def set_with_config(self, config:StableDiffusionConfig):
        self.callback_steps = config.callback_steps
        #should probably be a component for image inputs, or part of configs
        self.input_latents = None
        self.input_image = None
        if config.initial_latents_name != "":
            # self.input_latents =  torch.load(config.initial_latents_name).to(self.device)
            self.input_latents = self.latent_conversion.load_latent(self.latent_conversion.uploads_folder, "input")
        elif config.initial_image_name != "":
            self.input_image = ImageUtils.load_image(config.initial_image_name) # need timesteps to convert to latents

        self.mask_latents = None
        self.mask_image = None
        self.mask_bounds = []
        if(self.input_image != None):
            # if config.mask_latents_name != "":
            #     self.mask_latents = torch.load(config.mask_latents_name).to(self.device)
            if config.mask_image_name != "":
                self.mask_image = ImageUtils.load_image(config.mask_image_name) # need timesteps to convert to latents
                self.mask_image = self.mask_image.resize(self.input_image.size)
                self.mask_bounds = config.mask_bounds

        self.active_inputs = config.active_inputs

    def has_region(self):
        return self.mask_bounds != None and len(self.mask_bounds) == 4
    
    def check_inputs(self):
        pcs = self.prompt_component.check_inputs()
        lcs = self.latent_conversion.check_inputs()
        ncs = self.noise_component.check_inputs()
        scs = self.scheduler_component.check_inputs()
        return pcs or lcs or ncs or scs
    
    def ensure_controlnet_models(self):
        cur_set = set(self.active_inputs)
        need_set = set(self.previous_active_inputs)
        to_unload = list(need_set - cur_set)
        to_load = list(cur_set - need_set)
        for unload_name in to_unload:
            if unload_name in self.control_nets.keys():
                self.control_nets[unload_name].unload()
        for load_name in to_load:
            if load_name in self.control_nets.keys():
                self.control_nets[load_name].load()


    @torch.no_grad()
    def generate_image(self,num_inference_steps = 20, guidance_scale = 7.5)->Image.Image:
        self.scheduler_component.num_inference_steps = num_inference_steps
        self.guidance_scale = guidance_scale
        do_classifier_free_guidance = True #guidance_scale > 1.0

        prompt_embeds = self.prompt_component.encode_prompts()
        self.latent_conversion.save_latent(f"{self.latent_conversion.working_folder}_py_text_encoding.bin", prompt_embeds)
        
        timesteps, final_step_count = self.scheduler_component.get_timesteps(num_inference_steps)
        self.start_index = num_inference_steps - final_step_count
        if self.input_latents == None:
            self.input_latents = self.noise_component.get_noised_latent(self.input_image, timesteps[0])
        else:
            self.input_latents = self.noise_component.add_noise_to_latent(self.input_latents, timesteps[0])

        # test
        # self.input_latents = self.latent_conversion.load_latent(self.latent_conversion.uploads_folder, "latent_test", self.input_latents.shape)
        # prompt_embeds = self.latent_conversion.load_latent(self.latent_conversion.uploads_folder, "prompt_test", prompt_embeds.shape)

        batched_latent = self.input_latents.unsqueeze(0) # make noised tensor into a batch size of 1
        
        mask_tensor = None
        cn_latent = None
        uses_mask = self.mask_image != None and self.input_image != None
        if uses_mask:
            cur_unet = self.unet_inpaint
            if self.mask_latents == None:
                w = self.input_latents.shape[2]
                h = self.input_latents.shape[1]
                mask_tensor, self.mask_latents = self.latent_conversion.process_mask(self.mask_image, self.input_image, w, h)
        else:
            cur_unet = self.unet

        #if len(self.active_inputs) > 0:
        cn_model = None
        used_count = 0.0
        if len(self.active_inputs) > 0:
            self.ensure_controlnet_models()
            self.previous_active_inputs = self.active_inputs.copy()
            for cn_name in self.active_inputs:
                if cn_name in self.control_net_names:
                    cn_model = self.control_nets[cn_name]
                    cn_image = ImageUtils.load_image(self.control_net_upload_paths[cn_name])
                    needs_reprocess = ["canny", "openpose", "normal", "uniformer"] # will come from cnet config eventually
                    if(self.has_region() and cn_name in needs_reprocess): # must reprocess cnet if a subregion (eg canny cant be blurred, so can't have scaled up)
                        config = {}
                        cn_image = self.control_net.process_kind(cn_image, cn_name, config)
                        ImageUtils.save_image(cn_image.copy(), self.control_net_upload_paths[cn_name])
                    cur_cn_latent = cn_model.prepare_input(cn_image)
                    if cn_latent == None:
                        cn_latent = cur_cn_latent
                    else:
                        cn_latent += cur_cn_latent
                    used_count += 1.0

        if used_count > 0:
            cn_latent = cn_latent / used_count # just average for now, will add control net params which will include weights over time.

        for i, t in enumerate(timesteps):
            latent_model_input = torch.cat([batched_latent] * 2) if do_classifier_free_guidance else batched_latent # goes from [1, 4, 64, 64] to [2, 4, 64, 64]
            latent_model_input = cast(torch.FloatTensor, latent_model_input)
            latent_model_input = self.scheduler_component.scale_model_input(latent_model_input, t) # nop for ddim

            cn_outs = []
            if cn_model != None:
                cn_outs = cn_model.generate_outs(cn_latent, latent_model_input, t, prompt_embeds)

            if self.mask_latents != None:
                latent_model_input = torch.cat([latent_model_input, mask_tensor, self.mask_latents], dim=1)

            if uses_mask:
                cn_outs_masked = cn_outs
                noise_pred = cur_unet.forward(latent_model_input, t, prompt_embeds, cn_outs_masked) # has 9 channels, probably can consolidate these
            else:
                noise_pred = cur_unet.forward(latent_model_input, t, prompt_embeds, cn_outs)

            if do_classifier_free_guidance:
                noise_pred_uncond, noise_pred_text = noise_pred.chunk(2)
                noise_pred = noise_pred_uncond + self.guidance_scale * (noise_pred_text - noise_pred_uncond)
            batched_latent = self.scheduler_component.step(noise_pred, t, batched_latent)

            if self.callback is not None and i % self.callback_steps == 0:
                self.callback(i, t, batched_latent)

        image = self.latent_conversion.image_from_tensor(batched_latent)

        if self.callback is not None:
            self.callback(len(timesteps), timesteps[-1], batched_latent)
        return image
