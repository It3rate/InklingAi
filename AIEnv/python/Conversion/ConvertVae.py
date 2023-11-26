from numpy import dtype
from safetensors import safe_open
import torch
import argparse
import sys
import re
import gc
import json

from diffusers import AutoencoderKL
from safetensors.torch import load_model
from diffusers import StableDiffusionPipeline

class ConvertVae:
    
    def __init__(self):
        self.encoder_tensors = {}
        self.decoder_tensors = {}
        self.total_len = 0
        self.converted = []
        self.paths = {}
        self.vae = None
        self.parent_dict = {}

    def parse(self, path:str):
        if self.converted != None and len(self.converted) > 0:
            del self.converted
            torch.cuda.empty_cache()
            gc.collect()
            
        self.get_and_transform_vae(path)
        
    def get_and_transform_vae(self, path:str):
        self.transform_encoder(path)
        self.transform_decoder(path)


    def transform_encoder(self, path:str):
        self.get_tensors(path, True)
        self.set_order(self.encoder_tensors,["conv_in", "down_blocks", "mid_block", "conv_norm_out", "conv_out", "quant_conv"], ["encoder"])
        assert len(self.converted) == 108, f"encoder tensor len should be 108 but is {len(self.converted)}."
        self.save(path.replace(".safetensors", "_enc"))

    def transform_decoder(self, path:str):
        self.get_tensors(path, False)
        self.set_order(self.decoder_tensors,["post_quant_conv", "conv_in", "mid_block", "up_blocks", "conv_norm_out", "conv_out"], ["decoder"])
        assert len(self.converted) == 140, f"decoder tensor len should be 140 but is {len(self.converted)}."
        self.save(path.replace(".safetensors", "_dec"))


    def get_tensors(self, path:str, is_enc:bool):
        self.total_len = 0
        self.converted = []
        self.paths = {}

        db_types = ("DownEncoderBlock2D", "DownEncoderBlock2D", "DownEncoderBlock2D", "DownEncoderBlock2D")
        ub_types = ("UpDecoderBlock2D", "UpDecoderBlock2D", "UpDecoderBlock2D", "UpDecoderBlock2D")
        self.vae = AutoencoderKL(block_out_channels=(128, 256, 512, 512), down_block_types=db_types, up_block_types=ub_types, layers_per_block=2, sample_size=256)# type: ignore
        checkpoint = torch.load('R:/SD/models/stable-diffusion-v1-5/vae/diffusion_pytorch_model_1_7.bin')
        self.vae.load_state_dict(checkpoint)
 
        with safe_open(path, framework="pt", device=0) as st:
            for key in st.keys():
                tensor = st.get_tensor(key)
                tensor = tensor.to('cuda', dtype=torch.float16)
                self.parse_vae_key(key, tensor, is_enc)

    def parse_vae_key(self, key, tensor, is_enc:bool):
        tail = ""
        target:dict = {}
        if key.startswith("encoder") and is_enc:
            tail = key[len("encoder") + 1:]
            target = self.encoder_tensors
        elif key.startswith("decoder") and not is_enc :
            tail = key[len("decoder") + 1:]
            target = self.decoder_tensors
        elif key.startswith("quant_conv") and is_enc:
            tail = key
            target = self.encoder_tensors
        elif key.startswith("post_quant_conv") and not is_enc:
            tail = key
            target = self.decoder_tensors
        else:
            #print(f"missing key {key} {tensor}")
            return

        word_dash_digit = r'^(.+?)_(\d+)$'
        parts = tail.split(".")
        new_parts = []
        for i, part in enumerate(parts[:-2]):
            match = re.match(word_dash_digit, part)
            if part.isdigit():
                new_parts.append(int(part))
            elif match:
                new_parts.append(match[1])
                new_parts.append(int(match[2]))
            else:
                if i > 0 and not parts[i-1].isdigit():
                    new_parts.append(0)
                new_parts.append(part)
                if i == 0 and not parts[i+1].isdigit():
                    new_parts.append(0)
        new_parts.extend(parts[-2:])

        parent_key = key.rsplit('.', 1)[0] 
        parent = self.get_nested_attr(self.vae, parent_key)
        self.set_nested_item(target, new_parts, {"tensor" : tensor, "parent":parent})
        

        
    def set_nested_item(self, dict_obj:dict, path:list, value):
        for key in path[:-1]:
            if key not in dict_obj:
                dict_obj[key] = {}
            dict_obj = dict_obj[key]
        dict_obj[path[-1]] = value


    def map_vae_name(self, name:str):
        result = name
        if name == "resnets":
            result = 'resnet'
        elif name == "attentions":
            result = 'attention'
        elif name == "block":
            result = 'resnet'
        elif name == "down_blocks":
            result = 'down'
        elif name == "up_blocks":
            result = 'up'
        elif name == "mid_block":
            result = 'mid'
        elif name == "downsamplers":
            result = 'downsample.0'
        elif name == "upsamplers":
            result = 'upsample.0'
        elif name == "norm":
            result = 'norm.0'
        elif name == "norm1":
            result = 'norm.0'
        elif name == "norm2":
            result = 'norm.1'
        elif name == "conv":
            result = 'conv.0'
        elif name == "conv1":
            result = 'conv.0'
        elif name == "conv2":
            result = 'conv.1'
        elif name == "nin_shortcut":
            result = 'shortcut.0'
        elif name == "conv_shortcut":
            result = 'shortcut.0'
        return result
    
    def find_tensor_by_key(self, model, key):
        for name, param in model.named_parameters():
            if name == key:
                return param
        return None
    
    def get_nested_attr(self, obj, attr_string):
        attrs = attr_string.split('.')
        mapping = {
            "to_k": "key",
            "to_q": "query",
            "to_v": "value",
            "to_out": "proj_attn"
        }
        for attr in attrs:
            attr = mapping.get(attr, attr)
            if attr.isdigit():  # Check if the attribute is an index
                try:
                    obj = obj[int(attr)]
                except:
                    pass
            else:
                obj = getattr(obj, attr)
        return obj

    def set_order(self, block:dict, names:list, keys:list, space:str = ""):
        for name in names:
            keys.append(self.map_vae_name(name))
            if name == 'weight' or name == 'bias':
                tensor = block[name]["tensor"]
                parent = block[name]["parent"]
                length = tensor.element_size() * tensor.numel()
                key = '.'.join(keys)
                value = {"start":self.total_len, "length":length, "shape":tensor.shape}
                if hasattr(parent, "padding"):
                    value["padding"] = parent.padding
                if hasattr(parent, "padding_mode"):
                    value["padding_mode"] = parent.padding_mode
                if hasattr(parent, "stride"):
                    value["stride"] = parent.stride
                if hasattr(parent, "dilation"):
                    value["dilation"] = parent.dilation
                if hasattr(parent, "num_groups"):
                    value["num_groups"] = parent.num_groups
                if hasattr(parent, "eps"):
                    value["eps"] = parent.eps

                self.paths[key] = value
                self.converted.append(tensor)
                self.total_len += length
                self.total_len = (self.total_len + 255) & ~255 # round to nearest 256 for alignment
            else:
                if name == "down_blocks":
                    down_block =  block[name]
                    for dn_index in down_block:
                        keys.append(str(dn_index))
                        next_names = []
                        next_names.append("resnets")
                        if "downsamplers" in down_block[dn_index] : next_names.append("downsamplers")
                        self.set_order(down_block[dn_index], next_names, keys)
                        keys.pop()
                elif name == "mid_block":
                    mid_block =  block[name]
                    counter = 0
                    for mid_index in mid_block:
                        keys.append(str(counter))
                        counter += 1
                        cur_mid_block = mid_block[mid_index][0]
                        self.set_order(cur_mid_block, ["resnets"], keys)
                        keys.pop()
                elif name == "up_blocks":
                    up_block =  block[name]
                    for up_index in up_block:
                        keys.append(str(up_index))
                        next_names = []
                        next_names.append("resnets")
                        if "upsamplers" in up_block[up_index] : next_names.append("upsamplers")
                        self.set_order(up_block[up_index], next_names, keys)
                        keys.pop()
                elif name == "resnets":
                    resnet_block =  block[name]
                    counter = 0
                    for rn_index in resnet_block:
                        next_names = []
                        keys.append(str(counter))
                        rn = resnet_block[rn_index]
                        next_names.append("norm1")
                        next_names.append("conv1")
                        next_names.append("norm2")
                        next_names.append("conv2")
                        if len(rn) == 5: next_names.append("conv_shortcut")
                        self.set_order(rn, next_names, keys)
                        if "attentions" in block and rn_index in block['attentions']:
                            bindex = keys.pop()
                            bname = keys.pop()
                            attn_name = self.map_vae_name('attentions')
                            self.set_order(block['attentions'][rn_index], [f"{attn_name}.{counter}"], keys)
                            keys.append(bname)
                            keys.append(bindex)
                        keys.pop()
                        counter += 1

                elif name.startswith("attention"):
                    self.set_order(block, ["group_norm","to_q","to_k","to_v","to_out"], keys)
                elif name.startswith("upsamplers"):
                    self.set_order( block[name][0], ["conv"], keys)
                elif name.startswith("downsamplers"):
                    self.set_order( block[name][0], ["conv"], keys)
                else:
                    next_names = ["weight", "bias"]
                    sub_block =  block[name]['0'] if name == "to_out" else  block[name]
                    self.set_order(sub_block, next_names, keys)
            keys.pop()

    def save(self, filename:str):
        with open(f"{filename}.json", 'w') as json_file:
            json.dump(self.paths, json_file)

        path = f"{filename}.bytes"
        
        byte_count = 0
        with open(path, 'wb') as f:
            num_tensors = len(self.converted)
            idx = 0
            for tensor in self.converted:
                idx += 1
                tensor_data = tensor.half().cpu().numpy().tobytes()
                f.write(tensor_data)
                unpadded = byte_count + len(tensor_data)
                byte_count = (unpadded + 255) & ~255
                dif = byte_count - unpadded
                if dif > 0 and idx < num_tensors:    
                    padding = bytes([0] * dif)
                    f.write(padding)

        assert byte_count == self.total_len
        del self.converted


if __name__ == "__main__":

    folder = 'R:/SD/models/stable-diffusion-v1-5/'
    #vae_file = f'{folder}vae/vae-ft-mse-840000-ema-pruned.safetensors'
    vae_file = f'{folder}vae/diffusion_pytorch_model.fp16.safetensors'
    target = vae_file

    if len(sys.argv) > 1:
        parser = argparse.ArgumentParser(description='Process a path.')
        parser.add_argument('path', type=str, help='The path to process')
        args = parser.parse_args()
        target = args.path
    convert = ConvertVae()
    convert.parse(target)
