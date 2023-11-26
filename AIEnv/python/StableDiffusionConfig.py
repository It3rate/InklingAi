import sys
import random
from typing import Any, Union, TypedDict, Dict
from torch import Tensor
from PIL import Image

from Utils import ImageUtils

class StableDiffusionConfig():

    def __init__(self, prompt:str, negative_prompt:str = "", 
                 num_inference_steps:int = 30, start_inference_step:int = 0, guidance_scale:float = 7.5, 
                 seed:int = -1, callback_steps:int=5, image_preservation = 0, 
                 initial_image_name:str = "", initial_latents_name = "", generated_image_name:str = "", 
                 mask_image_name:str = "", mask_bounds:list = [], mask_latents_name:str = "", 
                 width:int = 512, height:int = 512, prompt_embeds=None,negative_prompt_embeds=None,
                 active_inputs:list = [], initial_image_source = ""):
        self.prompt = prompt
        self.prompt_embeds = prompt_embeds
        self.negative_prompt = negative_prompt
        self.negative_prompt_embeds = negative_prompt_embeds

        self.num_inference_steps = num_inference_steps
        self.start_inference_step = start_inference_step
        self.guidance_scale = guidance_scale
        self.seed = seed if seed > -1 else random.randint(0, sys.maxsize)
        self.callback_steps = callback_steps
        self.image_preservation = image_preservation
        self.initial_image_name  = initial_image_name
        self.initial_latents_name  = initial_latents_name
        self.generated_image_name  = generated_image_name
        self.mask_image_name = mask_image_name
        self.mask_bounds = mask_bounds
        self.mask_latents_name  = mask_latents_name
        self.width = width
        self.height = height
        self.active_inputs = active_inputs
        self.initial_image_source = initial_image_source
    
    def initial_image(self):
        return ImageUtils.load_image(self.initial_image_name)
    
    def check_inputs(self):
        if self.height % 8 != 0 or self.width % 8 != 0:
            raise ValueError(f"`height` and `width` have to be divisible by 8 but are {self.height} and {self.width}.")

        if (self.callback_steps is None) or (
            self.callback_steps is not None and (not isinstance(self.callback_steps, int) or self.callback_steps <= 0)
        ):
            raise ValueError(
                f"`self.callback_steps` has to be a positive integer but is {self.callback_steps} of type"
                f" {type(self.callback_steps)}."
            )

        if self.prompt is not None and self.prompt_embeds is not None:
            raise ValueError(
                f"Cannot forward both `prompt`: {self.prompt} and `prompt_embeds`: {self.prompt_embeds}. Please make sure to"
                " only forward one of the two."
            )
        elif self.prompt is None and self.prompt_embeds is None:
            raise ValueError(
                "Provide either `prompt` or `prompt_embeds`. Cannot leave both `prompt` and `prompt_embeds` undefined."
            )
        elif self.prompt is not None and (not isinstance(self.prompt, str) and not isinstance(self.prompt, list)):
            raise ValueError(f"`prompt` has to be of type `str` or `list` but is {type(self.prompt)}")

        if self.negative_prompt is not None and self.negative_prompt_embeds is not None:
            raise ValueError(
                f"Cannot forward both `negative_prompt`: {self.negative_prompt} and `negative_prompt_embeds`:"
                f" {self.negative_prompt_embeds}. Please make sure to only forward one of the two."
            )

        if self.prompt_embeds is not None and self.negative_prompt_embeds is not None:
            if self.prompt_embeds.shape != self.negative_prompt_embeds.shape:
                raise ValueError(
                    "`prompt_embeds` and `negative_prompt_embeds` must have the same shape when passed directly, but"
                    f" got: `prompt_embeds` {self.prompt_embeds.shape} != `negative_prompt_embeds`"
                    f" {self.negative_prompt_embeds.shape}."
                )
        if self.image_preservation < 0 or self.image_preservation > 1:
            raise ValueError(f"The value of image_preservation should in [0.0, 1.0] but is {self.image_preservation}")
	    

class StableDiffusionInput():

    def __init__(self, num_inference_steps:int = 30, guidance_scale:float = 7.5, 
                 seed:int = -1, callback_steps:int=5, image_preservation = 0, 
                 initial_image_name:str = "", initial_latents_name = "", generated_image_name:str = "", mask_image_name:str = "", 
                 width:int = 512, height:int = 512, prompt_embeds=None,negative_prompt_embeds=None):

        self.num_inference_steps = num_inference_steps
        self.guidance_scale = guidance_scale
        self.seed = seed if seed > -1 else random.randint(0, sys.maxsize)
        self.image_preservation = image_preservation

        self.prompt_embeds = prompt_embeds
        self.negative_prompt_embeds = negative_prompt_embeds
        self.latents = None
        self.image = None
        self.mask = None
        self.mask_bounds = []

        self.callback_steps = callback_steps
        self.callback = None






# these classes are strictly for type inference

class ClipTokens():
    def __init__(self):
        self.input_ids:Tensor
        self.attention_mask:Tensor

class ClipEmbeddings():
    def __init__(self):
        self.last_hidden_state:Tensor
        self.pooler_output:Tensor
        self.hidden_states:Any
        self.attentions:Any
        

