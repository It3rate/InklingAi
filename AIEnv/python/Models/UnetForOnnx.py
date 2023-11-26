
from turtle import forward
from typing import List
import torch
import torch.nn as nn

from Models.Resnet2D import Resnet2D
from Models.Transformer import Transformer
from Models.Downsample import Downsample
from Models.Upsample import Upsample
from Models.TimestepEmbeds import TimestepEmbeds
from Utils import ImageUtils

class UnetForOnnx(nn.Module):
    """ A Unet inference module for Stable Diffusion."""
    def __init__(self, model_name):
        super().__init__()
        self.sample:torch.Tensor
        self.silu = nn.SiLU()
        self.module_list = self.create_modules()
        
        utils = ImageUtils.instance()
        filepath = f"{utils.sd_model_folder}{model_name}"
        data = torch.load(filepath)
        self.data_len = self.load(data)
        for param in self.parameters():
            param.requires_grad = False
        self.eval()

    @torch.no_grad()
    def forward(self, hidden_states, timestep, encoder_hidden_states, hints:List[torch.Tensor] = []):
        
        (hidden_states, emb, hints) = self.forward_in(hidden_states, timestep, encoder_hidden_states)
        hidden_states = self.forward_mid(hidden_states, emb, encoder_hidden_states)
        hidden_states = self.forward_out_a(hidden_states, emb, encoder_hidden_states, 
                                           hints[6], hints[7], hints[8], hints[9], hints[10], hints[11])
        hidden_states = self.forward_out_b(hidden_states, emb, encoder_hidden_states, hints[2], hints[3], hints[4], hints[5])
        hidden_states = self.forward_out_c(hidden_states, emb, encoder_hidden_states, hints[0], hints[1])
        return hidden_states

    @torch.no_grad()
    def forward_in(self, hidden_states, timestep, encoder_hidden_states):
        emb = self.embedTimestep(timestep, hidden_states)
        hidden_states = self.conv_in(hidden_states)
        skip0 = hidden_states

        hidden_states = self.in_b0_r0 (hidden_states, emb)
        hidden_states = self.in_b0_t0s(hidden_states, encoder_hidden_states)
        skip1 = hidden_states
        hidden_states = self.in_b0_r1 (hidden_states, emb)
        hidden_states = self.in_b0_t1s(hidden_states, encoder_hidden_states)
        skip2 = hidden_states
        self.shape0 = hidden_states.shape[-2:]
        hidden_states = self.in_b0_d1s(hidden_states)
        skip3 = hidden_states
        
        hidden_states = self.in_b1_r0 (hidden_states, emb)
        hidden_states = self.in_b1_t0s(hidden_states, encoder_hidden_states)
        skip4 = hidden_states
        hidden_states = self.in_b1_r1 (hidden_states, emb)
        hidden_states = self.in_b1_t1s(hidden_states, encoder_hidden_states)
        skip5 = hidden_states
        self.shape1 = hidden_states.shape[-2:]
        hidden_states = self.in_b1_d1s(hidden_states)
        skip6 = hidden_states

        hidden_states = self.in_b2_r0 (hidden_states, emb)
        hidden_states = self.in_b2_t0s(hidden_states, encoder_hidden_states)
        skip7 = hidden_states
        hidden_states = self.in_b2_r1 (hidden_states, emb)
        hidden_states = self.in_b2_t1s(hidden_states, encoder_hidden_states)
        skip8 = hidden_states
        self.shape2 = hidden_states.shape[-2:]
        hidden_states = self.in_b2_d1s(hidden_states)
        skip9 = hidden_states

        hidden_states = self.in_b3_r0s(hidden_states, emb)
        skip10 = hidden_states
        hidden_states = self.in_b3_r1s(hidden_states, emb)
        skip11 = hidden_states
        return (hidden_states, emb, [skip0, skip1,  skip2,  skip3,  skip4,  skip5,  
                                skip6, skip7,  skip8,  skip9,  skip10,  skip11])

    @torch.no_grad()
    def forward_mid(self, hidden_states, emb, encoder_hidden_states):
        hidden_states = self.md_b0_r0(hidden_states, emb)
        hidden_states = self.md_b0_t0(hidden_states, encoder_hidden_states)
        hidden_states = self.md_b0_r1(hidden_states, emb)
        # hidden_states = hidden_states + hints.pop() // controlnet
        return hidden_states
    
    @torch.no_grad()
    def forward_out_a(self, hidden_states, emb, encoder_hidden_states, hint0, hint1, hint2, hint3, hint4, hint5):

        hidden_states = torch.cat([hidden_states, hint5], dim=1)
        hidden_states = self.out_b0_r0s (hidden_states, emb)
        hidden_states = torch.cat([hidden_states, hint4], dim=1)
        hidden_states = self.out_b0_r1s (hidden_states, emb)
        hidden_states = torch.cat([hidden_states, hint3], dim=1)
        hidden_states = self.out_b0_r2s (hidden_states, emb)
        hidden_states = self.out_b0_u2  (hidden_states)

        hidden_states = torch.cat([hidden_states, hint2], dim=1)
        hidden_states = self.out_b1_r0s (hidden_states, emb)
        hidden_states = self.out_b1_t0  (hidden_states, encoder_hidden_states)
        hidden_states = torch.cat([hidden_states, hint1], dim=1)
        hidden_states = self.out_b1_r1s (hidden_states, emb)
        hidden_states = self.out_b1_t1  (hidden_states, encoder_hidden_states)
        hidden_states = torch.cat([hidden_states, hint0], dim=1)
        hidden_states = self.out_b1_r2s (hidden_states, emb)
        hidden_states = self.out_b1_t2  (hidden_states, encoder_hidden_states)
        hidden_states = self.out_b1_u2  (hidden_states)
        
        return hidden_states

    @torch.no_grad()
    def forward_out_b(self, hidden_states, emb, encoder_hidden_states, hint0, hint1, hint2, hint3):
        hidden_states = torch.cat([hidden_states, hint3], dim=1)
        hidden_states = self.out_b2_r0s (hidden_states, emb)
        hidden_states = self.out_b2_t0  (hidden_states, encoder_hidden_states)
        hidden_states = torch.cat([hidden_states, hint2], dim=1)
        hidden_states = self.out_b2_r1s (hidden_states, emb)
        hidden_states = self.out_b2_t1  (hidden_states, encoder_hidden_states)
        hidden_states = torch.cat([hidden_states, hint1], dim=1)
        hidden_states = self.out_b2_r2s (hidden_states, emb)
        hidden_states = self.out_b2_t2  (hidden_states, encoder_hidden_states)
        hidden_states = self.out_b2_u2  (hidden_states)
        
        hidden_states = torch.cat([hidden_states, hint0], dim=1)
        hidden_states = self.out_b3_r0s (hidden_states, emb)
        hidden_states = self.out_b3_t0  (hidden_states, encoder_hidden_states)

        return hidden_states

    @torch.no_grad()
    def forward_out_c(self, hidden_states, emb, encoder_hidden_states, hint0, hint1):
        # hidden_states = torch.cat([hidden_states, skips[2]], dim=1)
        # hidden_states = self.out_b3_r0s (hidden_states, emb)
        # hidden_states = self.out_b3_t0  (hidden_states, encoder_hidden_states)
        hidden_states = torch.cat([hidden_states, hint1], dim=1)
        hidden_states = self.out_b3_r1s (hidden_states, emb)
        hidden_states = self.out_b3_t1  (hidden_states, encoder_hidden_states)
        hidden_states = torch.cat([hidden_states, hint0], dim=1)
        hidden_states = self.out_b3_r2s (hidden_states, emb)
        hidden_states = self.out_b3_t2  (hidden_states, encoder_hidden_states)

        hidden_states = self.conv_norm_out(hidden_states)
        hidden_states = self.silu(hidden_states)
        hidden_states = self.conv_out(hidden_states)

        return hidden_states

    def embedTimestep(self, timestep, hidden_states):
        timesteps = timestep[None].to(hidden_states.device, dtype=torch.float16)
        timesteps = timesteps.expand(hidden_states.shape[0])
        temb = self.timestep_embeds.get_timestep_embedding(timesteps)
        return self.timestep_embeds.forward(temb)

    def create_modules(self)->List[nn.Module]:
        self.timestep_embeds = TimestepEmbeds()
        modules:List[nn.Module] = [
            self.timestep_embeds
        ]

        self.conv_in = nn.Conv2d(4, 320, kernel_size=3, padding=(1, 1))
        modules.extend([self.conv_in])

        # input blocks
        self.in_b0_r0  = Resnet2D(320,320)
        self.in_b0_t0s = Transformer(320, skip=1) #*skip
        self.in_b0_r1  = Resnet2D(320,320)
        self.in_b0_t1s = Transformer(320, skip=1) #*skip
        self.in_b0_d1s = Downsample(320, skip=1) #*skip

        self.in_b1_r0  = Resnet2D(320,640, True) # CrossAttnDownBlock2D
        self.in_b1_t0s = Transformer(640, skip=1) #*skip
        self.in_b1_r1  = Resnet2D(640,640)
        self.in_b1_t1s = Transformer(640, skip=1) #*skip
        self.in_b1_d1s = Downsample(640, skip=1) #*skip

        self.in_b2_r0  = Resnet2D(640,1280, True) # CrossAttnDownBlock2D
        self.in_b2_t0s = Transformer(1280, skip=1) #*skip
        self.in_b2_r1  = Resnet2D(1280,1280,)
        self.in_b2_t1s = Transformer(1280, skip=1) #*skip
        self.in_b2_d1s = Downsample(1280, skip=1) #*skip

        self.in_b3_r0s = Resnet2D(1280,1280, skip=1) #*skip DownBlock2D
        self.in_b3_r1s = Resnet2D(1280,1280, skip=1) #*skip
        
        modules.extend([self.in_b0_r0, self.in_b0_t0s, self.in_b0_r1, self.in_b0_t1s, self.in_b0_d1s, 
                        self.in_b1_r0, self.in_b1_t0s, self.in_b1_r1, self.in_b1_t1s, self.in_b1_d1s, 
                        self.in_b2_r0, self.in_b2_t0s, self.in_b2_r1, self.in_b2_t1s, self.in_b2_d1s, 
                        self.in_b3_r0s, self.in_b3_r1s])

        # mid block
        self.md_b0_r0 = Resnet2D(1280,1280)
        self.md_b0_t0 = Transformer(1280)
        self.md_b0_r1 = Resnet2D(1280,1280)
        modules.extend([self.md_b0_r0, self.md_b0_t0, self.md_b0_r1])

        # out block
        self.out_b0_r0s = Resnet2D(1280+1280, 1280, True, skip=-1) #*skip UpBlock2D
        self.out_b0_r1s = Resnet2D(1280+1280, 1280, True, skip=-1) #*skip
        self.out_b0_r2s = Resnet2D(1280+1280, 1280, True, skip=-1) #*skip
        self.out_b0_u2  = Upsample(1280)

        self.out_b1_r0s = Resnet2D(1280+1280, 1280, True, skip=-1) #*skip CrossAttnUpBlock2D
        self.out_b1_t0  = Transformer(1280)
        self.out_b1_r1s = Resnet2D(1280+1280, 1280, True, skip=-1) #*skip
        self.out_b1_t1  = Transformer(1280)
        self.out_b1_r2s = Resnet2D(1280+640, 1280, True, skip=-1) #*skip
        self.out_b1_t2  = Transformer(1280)
        self.out_b1_u2  = Upsample(1280)
        
        self.out_b2_r0s = Resnet2D(1280+640, 640, True, skip=-1) #*skip CrossAttnUpBlock2D
        self.out_b2_t0  = Transformer(640)
        self.out_b2_r1s = Resnet2D(640+640, 640, True, skip=-1) #*skip
        self.out_b2_t1  = Transformer(640)
        self.out_b2_r2s = Resnet2D(640+320, 640, True, skip=-1) #*skip
        self.out_b2_t2  = Transformer(640)
        self.out_b2_u2  = Upsample(640)
        
        self.out_b3_r0s = Resnet2D(640+320, 320, True, skip=-1) #*skip CrossAttnUpBlock2D
        self.out_b3_t0  = Transformer(320)
        self.out_b3_r1s = Resnet2D(320+320, 320, True, skip=-1) #*skip
        self.out_b3_t1  = Transformer(320)
        self.out_b3_r2s = Resnet2D(320+320, 320, True, skip=-1) #*skip
        self.out_b3_t2  = Transformer(320)
        modules.extend([self.out_b0_r0s, self.out_b0_r1s, self.out_b0_r2s, self.out_b0_u2, 
                        self.out_b1_r0s, self.out_b1_t0, self.out_b1_r1s, self.out_b1_t1, self.out_b1_r2s, self.out_b1_t2, self.out_b1_u2,
                        self.out_b2_r0s, self.out_b2_t0, self.out_b2_r1s, self.out_b2_t1, self.out_b2_r2s, self.out_b2_t2, self.out_b2_u2,
                        self.out_b3_r0s, self.out_b3_t0, self.out_b3_r1s, self.out_b3_t1, self.out_b3_r2s, self.out_b3_t2])

        self.conv_norm_out = nn.GroupNorm(num_channels=320, num_groups=32, eps=1e-05)
        self.conv_out = nn.Conv2d(320, 4, kernel_size=3, padding=1)
        modules.extend([self.conv_norm_out, self.conv_out])
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
        
        target_len = 686 #298 if self.skip_encoder else 686
        assert(index == target_len)
        return index

    def save(self, filepath:str='test.bin'):
        ordered_modules = []
        for module in self.module_list:
            if hasattr(module, 'ordered_modules'):
                ordered_modules += module.ordered_modules
            else:
                ordered_modules.append(module)
        tensors = []
        for m in ordered_modules:
            tensors.append(m.weight)
            if m.bias != None:
                tensors.append(m.bias)
        torch.save(tensors, filepath)
        
