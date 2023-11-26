import torch
import math
import torch.nn as nn


class TimestepEmbeds(nn.Module):
    def __init__(self):
        super().__init__()
        self.embedding_dim = 320
        self.linear_1 = nn.Linear(self.embedding_dim, 1280)
        self.silu = nn.SiLU()
        self.linear_2 = nn.Linear(1280, 1280)
        self.ordered_modules = [self.linear_1, self.linear_2]

    def get_timestep_embedding(self, timesteps:torch.Tensor): # timesteps is 1D, step index for each batch eg: tensor([951, 951])
        half_dim = self.embedding_dim // 2
        exponent = -math.log(10000) * torch.arange(start=0, end=half_dim, dtype=torch.float32, device=timesteps.device)
        exponent = exponent / half_dim

        emb = torch.exp(exponent)
        emb = timesteps[:, None].float() * emb[None, :]
        emb = torch.cat([torch.sin(emb), torch.cos(emb)], dim=-1)
        emb = torch.cat([emb[:, half_dim:], emb[:, :half_dim]], dim=-1)
        emb = emb.to(dtype=torch.float16)
        return emb


    @torch.no_grad()
    def forward(self, sample): 
        sample = self.linear_1(sample)
        sample = self.silu(sample)
        sample = self.linear_2(sample)
        return sample
    
    def load(self, m, index)->int:
        for module in self.ordered_modules:
            module.weight = nn.Parameter(m[index])
            module.bias = nn.Parameter(m[index+1])
            index += 2
        return index
    
        keys = list(m.keys())
        self.linear_1.weight = nn.Parameter(m[keys[index + 0]])
        self.linear_1.bias = nn.Parameter(m[keys[index + 1]])
        self.linear_2.weight = nn.Parameter(m[keys[index + 2]])
        self.linear_2.bias = nn.Parameter(m[keys[index + 3]])
        
        return index + 4