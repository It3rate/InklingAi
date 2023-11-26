import torch
import torch.onnx
import os
import json
import numpy as np
from PIL import Image,ImageFilter
from typing import cast, Tuple
from diffusers.utils import randn_tensor
from diffusers import AutoencoderKL
from diffusers.models.autoencoder_kl import AutoencoderKLOutput
from StableDiffusionConfig import StableDiffusionConfig
from .SchedulerComponent import SchedulerComponent
from Utils import ImageUtils

class LatentConversion():

    def __init__(self, models_folder:str, scheduler_component:SchedulerComponent):
        # todo: have an IO class
        self.model_loaded = False
        self.models_folder = models_folder
        self.utils = ImageUtils.instance()
        self.root_folder = self.utils.sd_folder
        self.image_folder =  self.utils.ensure_folder_from_root(self.root_folder, "images") 
        self.working_folder =  self.utils.ensure_folder_from_root(self.root_folder, "working")
        self.uploads_folder =  self.utils.ensure_folder_from_root(self.root_folder, "uploads") 

        self.vae = self.create_vae()
        self.scheduler_component = scheduler_component

        self.vae_scale_factor = 2 ** (len(self.vae.config.block_out_channels) - 1) # type: ignore 
        self.last_image_width = 512
        self.last_image_height = 512
        self.last_latent_path = ""

    def create_vae(self) -> AutoencoderKL:
        db_types = ("DownEncoderBlock2D", "DownEncoderBlock2D", "DownEncoderBlock2D", "DownEncoderBlock2D")
        ub_types = ("UpDecoderBlock2D", "UpDecoderBlock2D", "UpDecoderBlock2D", "UpDecoderBlock2D")
        self.vae = AutoencoderKL(block_out_channels=(128, 256, 512, 512), down_block_types=db_types, up_block_types=ub_types, layers_per_block=2, sample_size=256)# type: ignore
        self.vae.to(torch.device('cuda'), torch.float16)
        self.load_data()
        for param in self.vae.parameters():
            param.requires_grad = False
        self.vae.eval()

        return self.vae
    
    def load_data(self):
        filepath = f"{self.models_folder}vae_f16_cuda.bin"
        if os.path.exists(filepath):
            te_dict = torch.load(filepath)
            self.vae.load_state_dict(te_dict)
            self.model_loaded = True

            # for name, param in self.vae.named_parameters():
            #     parameter_bytes = param.data.numpy().tobytes()
            #     start_byte = parameter_bytes[0]
            #     end_byte = parameter_bytes[-1]

            # conv1_weight = self.vae.state_dict()["decoder.conv_in.weight"]
            # weights = conv1_weight.data
            # weights.cpu().numpy().tofile("vae_conv1_filters.bin")

    def ensure_model(self):
        if not self.model_loaded:
            self.load_data()
        return self.model_loaded

    def check_inputs(self):
        return True

    def set_with_config(self, config:StableDiffusionConfig):
        self.set_size(config.width, config.height)

    def set_size(self, w:int, h:int):
        self.last_image_width = w - w % 8
        self.last_image_height = h - h % 8

    @torch.no_grad()
    def preprocess(self, image:Image.Image, min:float = -1.0, max:float = 1.0, add_batch_dim=True)->torch.FloatTensor:
        w, h = image.size
        self.set_size(w, h)
        resized_image = image.resize((self.last_image_width, self.last_image_height), resample=Image.LANCZOS)

        if min == -1:
            np_image = (np.array(resized_image.convert("RGB")).astype(np.float16) / 127.5) - 1.0
        else:
            np_image = (np.array(resized_image.convert("RGB")).astype(np.float16) / 255.0)
        np_image = np_image.transpose(2, 0, 1)

        tensor_image = torch.from_numpy(np_image).to('cuda')
        if add_batch_dim:
            tensor_image = tensor_image.unsqueeze(0) # ensure a batch of 1 for encode
        return cast(torch.FloatTensor, tensor_image)
    
    @torch.no_grad()
    def process_mask(self, mask_image:Image.Image, input_image:Image.Image, width:int, height:int)->Tuple[torch.FloatTensor, torch.FloatTensor]:
        #self.mask_latents = self.noise_component.get_mask_latent(self.input_image)
        mask = mask_image.convert("L")
        mask = mask.resize(input_image.size)
        mask = mask.filter(ImageFilter.GaussianBlur(4)) # without filtering you get artifacts.
        mask = mask.point(lambda x: 0 if x < 128 else 1, mode='1') # [512,512]
        mask = np.array(mask)[None, None, :] # [1,1,512,512]
        mask_tensor = torch.from_numpy(mask).to('cuda', dtype=torch.float16)

        image = np.array(input_image.convert("RGB"))[None, :] 
        image = image.transpose(0, 3, 1, 2)
        image = torch.from_numpy(image).to('cuda', dtype=torch.float16) / 127.5 - 1.0
        masked_image = cast(torch.FloatTensor, image * (mask_tensor < 0.5))

        mask_tensor = torch.nn.functional.interpolate(mask_tensor, size=(height, width))
        mask_tensor = mask_tensor.to('cuda', dtype=torch.float16)
        mask_tensor = torch.cat([mask_tensor] * 2)

        mask_latents = self.tensor_from_image_tensor(masked_image)
        mask_latents = mask_latents.unsqueeze(0)
        mask_latents = torch.cat([mask_latents] * 2)
        return (cast(torch.FloatTensor, mask_tensor), cast(torch.FloatTensor, mask_latents))

    @torch.no_grad()
    def latent_from_image(self, image:Image.Image, iter:int)->AutoencoderKLOutput:
        tensor_image = self.preprocess(image)
        init_latents = self.vae.encode(tensor_image)
        return init_latents
    
    @torch.no_grad()
    def tensor_from_image(self, image:Image.Image)->torch.FloatTensor:
        tensor_image = self.preprocess(image)
        init_latents = self.vae.encode(tensor_image)
        #init_latents = init_latents.latent_dist.sample(self.scheduler_component.generator) 
        init_latents = init_latents.latent_dist.sample() 
        init_latents = init_latents.squeeze(0)
        init_latents = 0.18215 * init_latents
        return cast(torch.FloatTensor, init_latents)
    
    @torch.no_grad()
    def tensor_from_image_tensor(self, tensor:torch.FloatTensor)->torch.FloatTensor:
        init_latents = self.vae.encode(tensor)
        #init_latents = init_latents.latent_dist.sample(self.scheduler_component.generator) 
        init_latents = init_latents.latent_dist.sample() 
        init_latents = init_latents.squeeze(0)
        init_latents = 0.18215 * init_latents
        return cast(torch.FloatTensor, init_latents)
        
    @torch.no_grad() #todo: convert to torchvision transforms
    def image_from_tensor(self, latents:torch.Tensor)->Image.Image:
        image = self.decode_latent(latents)
        pil_image = self.numpy_to_pil(image)

        imagex = self.vae.decode_latent_onnx( latents ).cpu().numpy()
        pil_imagex = Image.fromarray(imagex)
        path = f"{self.working_folder}_testx.png"
        ImageUtils.save_image(pil_imagex, path)
        # path = f"{self.working_folder}_test.png"
        # ImageUtils.save_image(pil_image, path)

        return pil_image
    
    @torch.no_grad() #todo: convert to torchvision transforms
    def image_of_latent(self, latent:torch.Tensor)->Image.Image:
        min = latent.min()
        max = latent.max()
        range = max - min
        latent = latent.clone()
        latent = latent - min
        latent = (latent / range).clamp(0, 1)
        latent = latent.cpu().permute(0, 2, 3, 1).float().numpy()
        pil_image = self.numpy_to_pil(latent)
        return pil_image

    @torch.no_grad()
    def decode_latent(self, latents):
        latents = 1.0 / 0.18215 * latents
        if latents.dim() == 3:
            latents = latents.unsqueeze(0) 
        image = self.vae.decode(latents).sample# type: ignore
        image = image.squeeze(0) 
        image = (image / 2.0 + 0.5).clamp(0, 1)
        image = image.cpu().permute(1, 2, 0).float().numpy()
        return image
    
    def numpy_to_pil(self, np_image):
        image = (np_image * 255).round().astype("uint8")
        if image.shape[-1] == 1:
            # special case for grayscale (single channel) images
            pil_image = Image.fromarray(image.squeeze(), mode="L")
        else:
            pil_image = Image.fromarray(image)

        return pil_image

    def post_generate(self, config:StableDiffusionConfig, image:Image.Image, has_region):        
        latent = self.tensor_from_image(image)
        self.save_latent_iteration(config.num_inference_steps, latent, image.copy(), has_region)
        
        filename = ImageUtils.make_safe_filename(f"{config.prompt}")
        filename = f"{filename}{config.seed}.png"
        filepath = f"{self.image_folder}{filename}"
        ImageUtils.save_image(image, filepath)
        config.generated_image_name = filepath
        self.prev_latents:torch.Tensor = torch.zeros(1)
        return filepath

    def load_latent(self, folder:str, name:str, shape = None) -> torch.FloatTensor:
        # byteArray = np.fromfile(f"{self.uploads_folder}input.bin")
        with open(f"{folder}{name}.bin", 'rb') as f:
            byteArray = f.read()
        data = np.frombuffer(byteArray, dtype=np.float16)

        if shape == None:
            with open(f"{folder}{name}_shape.json", "r") as file:
                shape = json.load(file)

        reshaped_data = data.reshape(shape).copy()
        tensor = torch.from_numpy(reshaped_data)
        return cast(torch.FloatTensor, tensor.cuda())
    
    def save_latent(self, filepath:str, latent):
        # flat = latent.flatten()
        # flat = flat.cpu()
        # numpy_array = flat.numpy()
        # numpy_array.tofile(filepath)
        byte_data = latent.cpu().numpy().tobytes()
        with open(filepath, 'wb') as f:
            f.write(byte_data)
    
    def save_latent_iteration(self, iter, latent, image, has_region):
        torch.save(latent, f"{self.working_folder}latent_{iter}.pt")
        path_targ = "region_" if has_region else "iter_"
        path = f"{self.working_folder}{path_targ}{iter}.png"
        ImageUtils.save_image(image, path)
        self.last_latent_path = path # save before updating path to avoid passing path prematurely

        flat = latent.flatten()
        flat = flat.cpu()
        numpy_array = flat.numpy()
        numpy_array.tofile(f"{self.working_folder}latent_{iter}.bin")