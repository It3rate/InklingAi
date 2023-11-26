
import torch.nn as nn

class LoraUpDown(nn.Module):
    def __init__(self):
        self.up = None
        self.down = None
        self.alpha = None

    def load(self, m, index)->int:
        self.up = nn.Parameter(m[index])
        index += 1
        self.down = nn.Parameter(m[index])
        index += 1
        self.alpha = nn.Parameter(m[index])
        index += 1
        return index


class LoraMLPTransformer(nn.Module):
    def __init__(self):
        self.proj_in = LoraUpDown()
        self.cross0_q = LoraUpDown()
        self.cross0_k = LoraUpDown()
        self.cross0_v = LoraUpDown()
        self.cross0_out = LoraUpDown()
        self.proj_out = LoraUpDown()
        self.ordered_modules = [self.proj_in, 
                        self.cross0_q, self.cross0_k, self.cross0_v, self.cross0_out,
                        self.proj_out]
        
    def load(self, m, index)->int:
        for module in self.ordered_modules:
            index = module.load(m, index)
        return index

class LoraTransformer(nn.Module):
    def __init__(self):
        self.proj_in = LoraUpDown()
        self.cross0_q = LoraUpDown()
        self.cross0_k = LoraUpDown()
        self.cross0_v = LoraUpDown()
        self.cross0_out = LoraUpDown()
        self.cross1_q = LoraUpDown()
        self.cross1_k = LoraUpDown()
        self.cross1_v = LoraUpDown()
        self.cross1_out = LoraUpDown()
        self.ff_GEGLU  = LoraUpDown()
        self.ff_out  = LoraUpDown()
        self.proj_out = LoraUpDown()
        self.ordered_modules = [self.proj_in, 
                        self.cross0_q, self.cross0_k, self.cross0_v, self.cross0_out,
                        self.cross1_q, self.cross1_k, self.cross1_v, self.cross1_out,
                        self.ff_GEGLU, self.ff_out, self.proj_out]

    def load(self, m, index)->int:
        for module in self.ordered_modules:
            index = module.load(m, index)
        return index

class LoraLayer(nn.Module):
    pass

class Lora(nn.Module):
    def __init__(self, channels:int, skip = 0):
        super().__init__()


    def load(self, m, index)->int:
        
        for module in self.ordered_modules:
            module.weight = nn.Parameter(m[index])
            index += 1
            if module.bias != None:
                module.bias = nn.Parameter(m[index])
                index += 1
        return index