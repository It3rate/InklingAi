from typing import OrderedDict
from numpy import dtype
from typing import List
import torch
import torch.nn as nn

from Utils import ImageUtils
from Models.Resnet2D import Resnet2D
from Models.Transformer import Transformer
from Models.Downsample import Downsample
from Models.Upsample import Upsample
from Models.TimestepEmbeds import TimestepEmbeds

from diffusers.models.embeddings import Timesteps, TimestepEmbedding
from diffusers.models.resnet import ResnetBlock2D, Downsample2D, Upsample2D
from diffusers.models.transformer_2d import Transformer2DModel

class Unet(nn.Module):
    """ A Unet inference module for Stable Diffusion."""
    def __init__(self, data, skip_encoder = False):
        super().__init__()
        self.skip_encoder = skip_encoder
        self.sample:torch.Tensor
        self.silu = nn.SiLU()
        self.input_blocks = []
        self.mid_blocks = []
        self.output_blocks = []

        self.module_list = self.create_modules()
        self.data_len = self.load(data)
        #self.save(f"{filepath}")
        for param in self.parameters():
            param.requires_grad = False
        self.eval()


    @torch.no_grad()
    def forward(self, hidden_states, timestep, encoder_hidden_states, hints:List[torch.Tensor] = []):
        # assumes full unet, controlNet should use decode()
        skip_tensors = []
        uses_hints = len(hints) > 0
        emb = self.embedTimestep(timestep, hidden_states)

        hidden_states = self.conv_in(hidden_states)
        skip_tensors.append(hidden_states)
        upsample_shapes = []
        for module in self.input_blocks:
            if isinstance(module, Downsample):
                upsample_shapes.append(hidden_states.shape[-2:])
            hidden_states = self.callModule(module, hidden_states, emb, encoder_hidden_states)
            if module.skip > 0: # type:ignore
                skip_tensors.append(hidden_states)
                
        for module in self.mid_blocks:
            hidden_states = self.callModule(module, hidden_states, emb, encoder_hidden_states)
        if uses_hints:
            hidden_states = hidden_states + hints.pop()

        for module in self.output_blocks:
            if module.skip < 0: # type:ignore
                hs = skip_tensors.pop()
                if uses_hints:
                    hs = hs + hints.pop()
                hidden_states = torch.cat([hidden_states, hs], dim=1)
            output_size = upsample_shapes.pop() if (isinstance(module, Upsample) and len(upsample_shapes) > 0) else None # fixes rounding issues on upsample
            hidden_states = self.callModule(module, hidden_states, emb, encoder_hidden_states, output_size)


        # modules = self.input_blocks + self.mid_blocks + self.output_blocks
        # for module in modules:
        #     if module.skip < 0: # type:ignore
        #         hs = skip_tensors.pop() + hints.pop()
        #         hidden_states = torch.cat([hidden_states, hs], dim=1)

        #     hidden_states = self.callModule(module, hidden_states, emb, encoder_hidden_states)

        #     if module.skip > 0: # type:ignore
        #         skip_tensors.append(hidden_states)

        hidden_states = self.conv_norm_out(hidden_states)
        hidden_states = self.silu(hidden_states)
        hidden_states = self.conv_out(hidden_states)

        self.sample = hidden_states
        return self.sample
    
    def callModule(self, module, hidden_states, emb, encoder_hidden_states, sz=None)->torch.Tensor:
        if isinstance(module, Transformer):
            hidden_states = module(hidden_states, encoder_hidden_states)
        elif isinstance(module, Resnet2D):
            hidden_states = module(hidden_states, emb)
        elif isinstance(module, Upsample):
            hidden_states = module(hidden_states, sz)
        else:
            hidden_states = module(hidden_states)
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
        modules.append(self.conv_in)
        self.input_blocks = nn.ModuleList([
            Resnet2D(320,320), # CrossAttnDownBlock2D
            Transformer(320, skip=1), #*skip
            Resnet2D(320,320),
            Transformer(320, skip=1), #*skip
            Downsample(320, skip=1), #*skip

            Resnet2D(320,640, True), # CrossAttnDownBlock2D
            Transformer(640, skip=1), #*skip
            Resnet2D(640,640),
            Transformer(640, skip=1), #*skip
            Downsample(640, skip=1), #*skip
            
            Resnet2D(640,1280, True), # CrossAttnDownBlock2D
            Transformer(1280, skip=1), #*skip
            Resnet2D(1280,1280,),
            Transformer(1280, skip=1), #*skip
            Downsample(1280, skip=1), #*skip

            Resnet2D(1280,1280, skip=1), #*skip DownBlock2D
            Resnet2D(1280,1280, skip=1), #*skip
        ])
        modules.extend(self.input_blocks)
        self.mid_blocks = nn.ModuleList([
            Resnet2D(1280,1280), #20
            Transformer(1280), #21
            Resnet2D(1280,1280), #22
        ])
        modules.extend(self.mid_blocks)

        if not self.skip_encoder:
            self.output_blocks = nn.ModuleList([
                Resnet2D(1280+1280, 1280, True, skip=-1), #*skip UpBlock2D
                Resnet2D(1280+1280, 1280, True, skip=-1), #*skip
                Resnet2D(1280+1280, 1280, True, skip=-1), #*skip
                Upsample(1280),

                Resnet2D(1280+1280, 1280, True, skip=-1), #*skip CrossAttnUpBlock2D
                Transformer(1280),
                Resnet2D(1280+1280, 1280, True, skip=-1), #*skip
                Transformer(1280),
                Resnet2D(1280+640, 1280, True, skip=-1), #*skip
                Transformer(1280),
                Upsample(1280),
                
                Resnet2D(1280+640, 640, True, skip=-1), #*skip CrossAttnUpBlock2D
                Transformer(640),
                Resnet2D(640+640, 640, True, skip=-1), #*skip
                Transformer(640),
                Resnet2D(640+320, 640, True, skip=-1), #*skip
                Transformer(640),
                Upsample(640),  
                
                Resnet2D(640+320, 320, True, skip=-1), #*skip CrossAttnUpBlock2D
                Transformer(320),
                Resnet2D(320+320, 320, True, skip=-1), #*skip
                Transformer(320),
                Resnet2D(320+320, 320, True, skip=-1), #*skip
                Transformer(320),
            ])
            modules.extend(self.output_blocks)

            self.conv_norm_out = nn.GroupNorm(num_channels=320, num_groups=32, eps=1e-05)
            self.conv_out = nn.Conv2d(320, 4, kernel_size=3, padding=1)
            modules.append(self.conv_norm_out)
            modules.append(self.conv_out)

        return modules
    
        # modules = [
        #     self.timestep_embeds.linear_1, # TimestepEmbedding
        #     self.timestep_embeds.linear_2,

        #     self.conv_in, #*skip

        #     #downblocks
        #     Resnet2D(320,320), # CrossAttnDownBlock2D
        #     Transformer(320, skip=1), #*skip
        #     Resnet2D(320,320),
        #     Transformer(320, skip=1), #*skip
        #     Downsample(320, skip=1), #*skip

        #     Resnet2D(320,640, True), # CrossAttnDownBlock2D
        #     Transformer(640, skip=1), #*skip
        #     Resnet2D(640,640),
        #     Transformer(640, skip=1), #*skip
        #     Downsample(640, skip=1), #*skip
            
        #     Resnet2D(640,1280, True), # CrossAttnDownBlock2D
        #     Transformer(1280, skip=1), #*skip
        #     Resnet2D(1280,1280,),
        #     Transformer(1280, skip=1), #*skip
        #     Downsample(1280, skip=1), #*skip

        #     Resnet2D(1280,1280, skip=1), #*skip DownBlock2D
        #     Resnet2D(1280,1280, skip=1), #*skip

        #     #midblock
        #     Resnet2D(1280,1280), #20
        #     Transformer(1280), #21
        #     Resnet2D(1280,1280), #22
        # ]
        # if not self.skip_encoder:
        #     modules += [
        #         #upblocks
        #         Resnet2D(1280+1280, 1280, True, skip=-1), #*skip UpBlock2D
        #         Resnet2D(1280+1280, 1280, True, skip=-1), #*skip
        #         Resnet2D(1280+1280, 1280, True, skip=-1), #*skip
        #         Upsample(1280),

        #         Resnet2D(1280+1280, 1280, True, skip=-1), #*skip CrossAttnUpBlock2D
        #         Transformer(1280),
        #         Resnet2D(1280+1280, 1280, True, skip=-1), #*skip
        #         Transformer(1280),
        #         Resnet2D(1280+640, 1280, True, skip=-1), #*skip
        #         Transformer(1280),
        #         Upsample(1280),
                
        #         Resnet2D(1280+640, 640, True, skip=-1), #*skip CrossAttnUpBlock2D
        #         Transformer(640),
        #         Resnet2D(640+640, 640, True, skip=-1), #*skip
        #         Transformer(640),
        #         Resnet2D(640+320, 640, True, skip=-1), #*skip
        #         Transformer(640),
        #         Upsample(640),  
                
        #         Resnet2D(640+320, 320, True, skip=-1), #*skip CrossAttnUpBlock2D
        #         Transformer(320),
        #         Resnet2D(320+320, 320, True, skip=-1), #*skip
        #         Transformer(320),
        #         Resnet2D(320+320, 320, True, skip=-1), #*skip
        #         Transformer(320),

        #         self.conv_norm_out,
        #         self.conv_out,
        #     ]
        # return modules

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
        


    def load(self, m):
        index = 0
        for i, module in enumerate(self.module_list):
            if hasattr(module, 'ordered_modules'):
                index = module.load(m, index)
            else:
                module.weight = nn.Parameter(m[index])
                module.bias = nn.Parameter(m[index+1])
                index += 2
        
        target_len = 298 if self.skip_encoder else 686
        assert(index == target_len)
        return index

        keys = list(m.keys())
        self.conv_in.weight = nn.Parameter(m[keys[0]])
        self.conv_in.bias = nn.Parameter(m[keys[1]])
        
        index = 2
        index = self.timestep_embeds.load(m, index)

        # The torch UNet is not stored in execution order
        load_order = [4,6,3,5,7,  9,11,8,10,12,  14,16,13,15,17,  18,19, # down
                      23,24,25,26,  28,30,32,27,29,31,33,  35,37,39,34,36,38,40,  42,44,46,41,43,45, # up
                      21,20,22] # mid
        for m_index in load_order:
            index = self.module_list[m_index].load(m, index)
        
        self.conv_norm_out.weight = nn.Parameter(m[keys[index + 0]])
        self.conv_norm_out.bias = nn.Parameter(m[keys[index + 1]])
        self.conv_out.weight = nn.Parameter(m[keys[index + 2]])
        self.conv_out.bias = nn.Parameter(m[keys[index + 3]])

        assert(index + 4 == 686)
        return index