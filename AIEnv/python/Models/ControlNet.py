from typing import OrderedDict
from numpy import dtype
import torch
import torch.nn as nn
from PIL import Image
import gc

from ModelComponents.LatentConversion import LatentConversion
from Models.Unet import Unet
from Utils import ImageUtils
from Models.Resnet2D import Resnet2D
from Models.Transformer import Transformer
from Models.Downsample import Downsample
from Models.Upsample import Upsample
from Models.TimestepEmbeds import TimestepEmbeds


class ControlNet():
    def __init__(self, kind:str, latent_conversion:LatentConversion) :
        self.utils = ImageUtils.instance()
        self.kind = kind
        self.latent_conversion = latent_conversion
        
        self.unet = None
        self.zero_convst = None
        self.input_hint_blockt = None

        self.control_net_folder = self.utils.sd_control_net_model_folder
        self.annotator_kinds = ['canny', 'depth', 'openpose', 'seg', 'softedge', 'scribble', 'normalbae', 'mlsd', 'lineart', 'inpaint', 'tile', 'shuffle', 'ip2p', 'lineart']
        self.model_map = {"canny":"canny", "depth":"depth", "leres":"depth", "hed":"lineart_anime", 
                          "openpose":"openpose", "normal":"normalbae", "mlsd":"mlsd", "pidinet":"softedge", "uniformer":"seg"}
        self.model_paths = {name:f"{self.control_net_folder}controlNet_{name}_f16_cuda.bin" for name in list(self.model_map.values())}
        self.model_name = self.model_map[kind]  

        self.silu = nn.SiLU()
        self.path = self.model_paths[self.model_name]
        # data = torch.load(self.path)
        # self.unet = Unet(data, True)
        # self.loadx(data, self.unet.data_len) # 298

    def prepare_input(self, image:Image.Image):
        result = self.latent_conversion.preprocess(image, 0.0, 1.0, False)
        last_index = len(self.input_hint_block) - 1
        for i, conv in enumerate(self.input_hint_block):
            result = conv.forward(result)
            if i < last_index:
                result = self.silu(result)
        return result

    @torch.no_grad()
    def generate_outs(self, hint, hidden_states, timestep, encoder_hidden_states)->list[torch.Tensor]:
        skip_tensors = []
        emb = self.unet.embedTimestep(timestep, hidden_states)

        hidden_states = self.unet.conv_in(hidden_states)
        hidden_states += hint
        skip_tensors.append(self.zero_convs[0](hidden_states))

        modules = self.unet.input_blocks + self.unet.mid_blocks
        zero_index = 1
        for module in modules:
            hidden_states = self.unet.callModule(module, hidden_states, emb, encoder_hidden_states)
            if module.skip > 0:
                skip_tensors.append(self.zero_convs[zero_index](hidden_states))
                zero_index += 1

        skip_tensors.append(self.zero_convs[zero_index](hidden_states))
        return skip_tensors

    def create_modules(self)->tuple[list, list]:
        zero_convs = [
            nn.Conv2d(320, 320,   kernel_size=1),
            nn.Conv2d(320, 320,   kernel_size=1),
            nn.Conv2d(320, 320,   kernel_size=1),
            nn.Conv2d(320, 320,   kernel_size=1),
            nn.Conv2d(640, 640,   kernel_size=1),
            nn.Conv2d(640, 640,   kernel_size=1),
            nn.Conv2d(640, 640,   kernel_size=1),
            nn.Conv2d(1280, 1280, kernel_size=1),
            nn.Conv2d(1280, 1280, kernel_size=1),
            nn.Conv2d(1280, 1280, kernel_size=1),
            nn.Conv2d(1280, 1280, kernel_size=1),
            nn.Conv2d(1280, 1280, kernel_size=1),

            nn.Conv2d(1280, 1280, kernel_size=1), # middle_block_out
            ]
        input_hint_block = [
            nn.Conv2d(3, 16,    kernel_size=3, padding=1),
            nn.Conv2d(16, 16,   kernel_size=3, padding=1),
            nn.Conv2d(16, 32,   kernel_size=3, padding=1, stride=2),
            nn.Conv2d(32, 32,   kernel_size=3, padding=1),
            nn.Conv2d(32, 96,   kernel_size=3, padding=1, stride=2),
            nn.Conv2d(96, 96,   kernel_size=3, padding=1),
            nn.Conv2d(96, 256,  kernel_size=3, padding=1, stride=2),
            nn.Conv2d(256, 320, kernel_size=3, padding=1),
        ]
        return (zero_convs, input_hint_block)

    # def loadx(self, m, index)->int:
    #     index = self.load_zero_convs(m, index)
    #     index = self.load_input_hint_block(m, index)
    #     return index
    
    def load(self):
        if(self.unet == None):
            self.zero_convs, self.input_hint_block = self.create_modules()
            data = torch.load(self.path)
            self.unet = Unet(data, True)
            # self.load(data, self.unet.data_len)
            index = self.unet.data_len
            index = self.load_zero_convs(data, index)
            index = self.load_input_hint_block(data, index)
    
    def unload(self):
        if self.unet != None:
            del self.unet
            del self.zero_convs
            del self.input_hint_block
            torch.cuda.empty_cache()
            gc.collect()
            self.unet = None
    
    def load_zero_convs(self, data, index:int):
        for module in self.zero_convs:
            module.weight = nn.Parameter(data[index])
            module.bias = nn.Parameter(data[index+1])
            index += 2
        return index

    def load_input_hint_block(self, data, index:int) :
        for module in self.input_hint_block:
            module.weight = nn.Parameter(data[index])
            module.bias = nn.Parameter(data[index+1])
            index += 2
        return index