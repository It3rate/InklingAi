from typing import OrderedDict
from typing import List
import torch
import torch.nn as nn
from Models.Resnet2D import Resnet2D
from Models.Transformer import Transformer
from Models.Downsample import Downsample
from Models.Upsample import Upsample
from Models.TimestepEmbeds import TimestepEmbeds

class VaeDecoder(nn.Module):
    """ A Vae inference module for Stable Diffusion."""
    def __init__(self, data, skip_encoder = False):
        super().__init__()
        self.sample:torch.Tensor
        self.silu = nn.SiLU()
        self.post_quant_conv
        self.conv_in
        self.mid_block = []
        self.up_blocks = []
        self.conv_out
        self.norm_out

        self.module_list = self.create_modules()
        self.data_len = self.load(data)
        
    def create_modules(self)->List[nn.Module]:
        self.timestep_embeds = TimestepEmbeds()
        modules:List[nn.Module] = [
            self.timestep_embeds
        ]

        groups = 32
        eps = 1e-05

        self.post_quant_conv = nn.Conv2d(4, 4, kernel_size=1, padding=(1, 1))
        modules.append(self.post_quant_conv)

        self.conv_in = nn.Conv2d(4, 512, kernel_size=1, padding=(1, 1))
        modules.append(self.conv_in)
        # post_quant_conv
        # conv_in
        # mid  (UNetMidBlock2D)
        #   resnet0    ResnetBlock2D
        #   attn0      AttentionBlock
        #   resnet1
        # up (UpDecoderBlock2D x4)
        #   resnet 012
        #   resnet 012, upsample (ResnetBlock2D, Upsample2D)
        #   resnet 012, upsample
        #   resnet 012, upsample
        # norm_out
        # conv_out
        
        self.mid_blocks = [
            Resnet2D(512,512),
            Transformer(512),
            Resnet2D(512,512)
        ]
        modules.extend(self.mid_blocks)

        self.up_blocks = [
            Resnet2D(512,512),
            Resnet2D(512,512),
            Resnet2D(512,512),
            Upsample(512),

            Resnet2D(512,256),
            Resnet2D(256,256),
            Resnet2D(256,256),
            Upsample(256),

            Resnet2D(256,128),
            Resnet2D(128,128),
            Resnet2D(128,128),
            Upsample(128),
        ]
        modules.extend(self.up_blocks)

        self.norm_out = torch.nn.GroupNorm(num_groups=groups, num_channels=128, eps=eps, affine=True)
        modules.append(self.norm_out)
        #silu
        self.conv_out = nn.Conv2d(128, 3, kernel_size=1, padding=(1, 1))
        modules.append(self.conv_out)
        return modules
    

    def load(self, m):
        index = 0
        for i, module in enumerate(self.module_list):
            if hasattr(module, 'ordered_modules'):
                index = module.load(m, index)
            else:
                module.weight = nn.Parameter(m[index])
                module.bias = nn.Parameter(m[index+1])
                index += 2
        
        target_len = 248
        assert(index == target_len)
        return index