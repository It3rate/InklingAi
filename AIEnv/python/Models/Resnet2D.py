
import torch
import torch.nn as nn

class Resnet2D(nn.Module):
    def __init__(self, in_channels, out_channels, use_shortcut = False, skip = 0):
        super().__init__()
        self.use_shortcut = use_shortcut
        self.skip = skip
        groups = 32
        eps = 1e-05
        temb_channels=1280
        self.nonlinearity = nn.SiLU()

        self.norm1 = torch.nn.GroupNorm(num_groups=groups, num_channels=in_channels, eps=eps, affine=True)
        self.conv1 = torch.nn.Conv2d(in_channels, out_channels, kernel_size=3, stride=1, padding=1)
        self.time_emb_proj = torch.nn.Linear(temb_channels, out_channels)
        self.norm2 = torch.nn.GroupNorm(num_groups=groups, num_channels=out_channels, eps=eps, affine=True)
        self.conv2 = torch.nn.Conv2d(out_channels, out_channels, kernel_size=3, stride=1, padding=1)

        self.ordered_modules = [self.norm1, self.conv1, self.time_emb_proj, self.norm2, self.conv2]

        self.conv_shortcut = None
        if self.use_shortcut:
            self.conv_shortcut = torch.nn.Conv2d(in_channels, out_channels, kernel_size=1, stride=1, padding=0)
            self.ordered_modules.append(self.conv_shortcut)
        

    @torch.no_grad()
    def forward(self, input_tensor:torch.Tensor, temb):
        hidden_states = self.norm1(input_tensor)
        hidden_states = self.nonlinearity(hidden_states)
        hidden_states = self.conv1(hidden_states)
        
        temb = self.nonlinearity(temb)
        temb = self.time_emb_proj(temb)[:, :, None, None]
        hidden_states = hidden_states + temb

        hidden_states = self.norm2(hidden_states)
        hidden_states = self.nonlinearity(hidden_states)
        hidden_states = self.conv2(hidden_states)
        
        if self.conv_shortcut is not None:
            input_tensor = self.conv_shortcut(input_tensor)

        return (input_tensor + hidden_states)

    def load(self, m, index)->int:
        
        for module in self.ordered_modules:
            module.weight = nn.Parameter(m[index])
            module.bias = nn.Parameter(m[index+1])
            index += 2
        return index

        keys = list(m.keys())
        self.norm1.weight = nn.Parameter(m[keys[index + 0]])
        self.norm1.bias = nn.Parameter(m[keys[index + 1]])
        self.conv1.weight = nn.Parameter(m[keys[index + 2]])
        self.conv1.bias = nn.Parameter(m[keys[index + 3]])

        self.time_emb_proj.weight = nn.Parameter(m[keys[index + 4]])
        self.time_emb_proj.bias = nn.Parameter(m[keys[index + 5]])

        self.norm2.weight = nn.Parameter(m[keys[index + 6]])
        self.norm2.bias = nn.Parameter(m[keys[index + 7]])
        self.conv2.weight = nn.Parameter(m[keys[index + 8]])
        self.conv2.bias = nn.Parameter(m[keys[index + 9]])
        offset = 10
        if self.use_shortcut:
            self.conv_shortcut.weight = nn.Parameter(m[keys[index + 10]])
            self.conv_shortcut.bias = nn.Parameter(m[keys[index + 11]])
            offset += 2
        return index + offset


