
import torch
import torch.nn as nn

class Downsample(nn.Module):
    def __init__(self, out_channels, skip = 0):
        super().__init__()
        self.skip = skip
        self.conv = nn.Conv2d(out_channels, out_channels, kernel_size=3, stride=2, padding=1)
        self.ordered_modules = [self.conv]

    @torch.no_grad()
    def forward(self, hidden_states):
        hidden_states = self.conv(hidden_states)
        return hidden_states

    def load(self, m, index)->int:
        for module in self.ordered_modules:
            module.weight = nn.Parameter(m[index])
            module.bias = nn.Parameter(m[index+1])
            index += 2
        return index
    
        keys = list(m.keys())
        
        self.conv.weight = nn.Parameter(m[keys[index + 0]])
        self.conv.bias = nn.Parameter(m[keys[index + 1]])
        return index + 2