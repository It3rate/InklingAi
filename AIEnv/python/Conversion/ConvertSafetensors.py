from numpy import dtype
from safetensors import safe_open
import torch
import argparse
import sys
import re
import gc
import json

folder = 'R:/SD/models/stable-diffusion-v1-5/'
#folder = '/home/debreuil/SD/models/stable-diffusion-v1-5/'
enc_file = f'{folder}text_encoder/model.safetensors'
enc16_file = f'{folder}text_encoder/pytorch_model.safetensors'
vae_file_d = f'{folder}vae/diffusion_pytorch_model.fp16.safetensors'
vae_file = f'{folder}vae/vae-ft-mse-840000-ema-pruned.safetensors'

v15_file = f'{folder}v1-5-pruned-emaonly.safetensors'
v15_deliberate_v2_file = f'{folder}deliberate_v2.safetensors'
realisticVision = f'{folder}realisticVisionV30_v30VAE.safetensors'
revAnimated_v122 = f'{folder}revAnimated_v122.safetensors'
dreamshaper_7 = f'{folder}dreamshaper_7.safetensors'

v15_deliberate_v2_inpaint = f'{folder}Deliberate-inpainting.safetensors'
realisticVisionV40_v30Novae_inpaint = f'{folder}realisticVisionV40_v30Novae-inpainting.safetensors'
revAnimated_v121Inp_inpaint = f'{folder}revAnimated_v121Inp-inpainting.safetensors'
dreamshaper_7_inpaint = f'{folder}dreamshaper_7-inpainting.safetensors'

lora_RA_file = f'{folder}Lora/LowRA.safetensors'
lora_more_details_file = f'{folder}Lora/more_details.safetensors'
lora_3DMM_V11_file = f'{folder}Lora/3DMM_V11.safetensors'
lora_epi_noiseoffset2_file = f'{folder}Lora/epi_noiseoffset2.safetensors'

control_canny_file = f'{folder}controlnet/control_canny-fp16.safetensors'
control_depth_file = f'{folder}controlnet/control_depth-fp16.safetensors'

target = vae_file# revAnimated_v121Inp_inpaint

class ConvertSafetensors:
    
    def __init__(self):
        self.json_file = None
        self.json_keys = []

        self.is_vae = False
        self.is_model = False
        self.is_controlnet = False
        self.is_lora = False
        
        self.diffusion_key = "model.diffusion_model."
        self.lora_key = "lora_"
        self.unet_tensors = {"input_blocks":{},"middle_block":{}, "output_blocks":{}, "time_embed":{}, "out":{}, 
                             'zero_convs':{}, 'input_hint_block':{}, 'middle_block_out':{},} # controlnet
        
        self.encoder_tensors = {}#{"conv_in":{},"down_blocks":{}, "mid_block":{}, "conv_norm_out":{}, "conv_out":{}, 
                             #'quant_conv':{}} 
        self.decoder_tensors = {}#{"post_quant_conv":{}, "conv_in":{},"conv_norm_out":{},"conv_out":{},"mid_block":{}, "mid_block":{}, "conv_norm_out":{}, "conv_out":{}, 
                             #'up_blocks':{}} 
        self.lora_tensors = {"te_text_model_encoder_layers":{}, 
                             "unet_down_blocks":{}, "unet_mid_block":{}, "unet_up_blocks":{}}

        self.total_len = 0
        self.converted = []
        self.paths = {}

    def parse(self, path:str):
        if self.converted != None and len(self.converted) > 0:
            del self.converted
            torch.cuda.empty_cache()
            gc.collect()
            self.unet_inpaint = None
        self.converted = []

        self.is_vae = "/vae/" in path

        if self.is_vae:
            self.get_and_transform_vae(path)
        elif self.is_model:
            self.get_tensors(path)
            self.transform_full_sd(path)
            assert len(self.converted) == 686, f"tensor len should be 686 but is {len(self.converted)}."
        elif self.is_lora:
            self.get_tensors(path)
            self.transform_lora_sd(path)
            assert len(self.converted) == 792, f"tensor len should be 792 but is {len(self.converted)}."
        elif self.is_controlnet:
            self.get_tensors(path)
            self.transform_controlnet(path)
            assert len(self.converted) == 340, f"tensor len should be 340 but is {len(self.converted)}."
        
    def save(self, path:str):
        saveAsTensors = False
        if saveAsTensors:
            out_file_path = path.replace(".safetensors", "_f16.bin")
            torch.save(self.converted, out_file_path)
        else:
            out_file_path = path.replace(".safetensors", ".bytes")
            with open(out_file_path, 'wb') as f:
                for tensor in self.converted:
                    tensor_data = tensor.half().cpu().numpy().tobytes()
                    f.write(tensor_data)
        del self.converted

    def get_tensors(self, path:str, out_txt:str = ""):
        is_enc = out_txt.endswith("_enc.safetensors")
        with safe_open(target, framework="pt", device=0) as st:
            out_txt = target if out_txt == "" else out_txt
            txt_file = out_txt.replace(".safetensors", "_parse.txt")
            with open(txt_file, 'w') as file:
                for key in st.keys():
                    if self.is_vae :
                        tail = key
                    elif key.startswith(self.diffusion_key) :
                        self.is_model = True
                        tail = key[len(self.diffusion_key):]

                    elif key.startswith(self.lora_key):
                        self.is_lora = True
                        tail = key[len(self.lora_key):]

                    else:
                        self.is_controlnet = True
                        tail = key

                    tensor = st.get_tensor(key)
                    tensor = tensor.to('cuda', dtype=torch.float16)
                    if(self.is_lora):
                        self.parse_lora_key(tail, tensor, file)
                    elif(self.is_vae):
                        self.parse_vae_key(key, tensor, file, is_enc)
                    else:
                        self.parse_model_key(tail, tensor, file)

    def set_nested_item(self, dict_obj:dict, path:list, value):
        for key in path[:-1]:
            if key not in dict_obj:
                dict_obj[key] = {}
            dict_obj = dict_obj[key]
        dict_obj[path[-1]] = value

    def parse_vae_key(self, key, tensor, file, is_enc:bool):
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
        self.set_nested_item(target, new_parts, tensor)
        file.write("{:<40} {}\n".format(str(tensor.shape), new_parts))
        
    def parse_model_key(self, tail, tensor, file):
        sp = tail.split(".")
        for block in self.unet_tensors.keys():
            if(block == sp[0]):
                sub_key = tail[len(block) + 1:]

                match = re.search(r'\d+(\.\d+)?', sub_key)
                block_num = match.group()
                rem = sub_key[len(block_num) + 1:]
                if rem.startswith("transformer_blocks.0."):
                    rem = rem[len("transformer_blocks.0."):]
                if block_num not in self.unet_tensors[block]:
                    self.unet_tensors[block][block_num] = {}
                self.unet_tensors[block][block_num][rem] = tensor
                file.write("{:<40} {} {} {}\n".format(str(tensor.shape), block, block_num, rem))
                break

    def parse_lora_key(self, tail, tensor, file):
        for block in self.lora_tensors.keys():
            if tail.startswith(block):
                sub_key = tail[len(block) + 1:]
                match = re.search(r'^\d+', sub_key)
                if match:
                    block_num = match.group()
                    rem = sub_key[len(block_num) + 1:]
                else:
                    block_num = "*"
                    rem = sub_key

                # if rem.startswith("transformer_blocks.0."):
                #     rem = rem[len("transformer_blocks.0."):]
                if block_num not in self.lora_tensors[block]:
                    self.lora_tensors[block][block_num] = {}
                self.lora_tensors[block][block_num][rem] = tensor

                file.write("{:<30} {} {} {}\n".format(str(tensor.shape), block, block_num, rem))
                break
    
    def transform_full_sd(self, path:str):
        self.transform_timesteps()
        self.transform_input_blocks()
        self.transform_middle_block()
        self.transform_output_blocks()
        self.transform_out()
        self.save(path)
        
    def transform_controlnet(self, path:str):
        self.transform_timesteps()
        self.transform_input_blocks()
        self.transform_middle_block()
        self.transform_zero_convs()
        self.transform_middle_block_out()
        self.transform_input_hint_block()
        self.save(path)

    def transform_lora_sd(self, path:str):
        self.transform_lora_text_encoder_layers()
        assert len(self.converted) == 216, f"text encoder tensors len should be 216 but is {len(self.converted)}."
        self.transform_lora_unet()
        self.save(path)
        
    def get_and_transform_vae(self, path:str):
        self.total_len = 0
        self.converted = []
        self.paths = {}
        enc_path = target.replace(".safetensors", "_enc.safetensors")
        self.get_tensors(path, enc_path)
        self.transform_encoder()
        assert len(self.converted) == 108, f"encoder tensor len should be 108 but is {len(self.converted)}."
        json_path = enc_path.replace(".safetensors", ".json")
        with open(json_path, 'w') as json_file:
            json.dump(self.paths, json_file)
        self.save(enc_path)

        self.total_len = 0
        self.converted = []
        self.paths = {}
        dec_path = target.replace(".safetensors", "_dec.safetensors")
        self.get_tensors(path, dec_path)
        self.transform_decoder()
        assert len(self.converted) == 140, f"decoder tensor len should be 140 but is {len(self.converted)}."
        json_path = dec_path.replace(".safetensors", ".json")
        with open(json_path, 'w') as json_file:
            json.dump(self.paths, json_file)
        self.save(dec_path)

    def map_vae_name(self, name:str):
        result = name
        if name == "block":
            result = 'resnet'
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
        return result
    
    def set_order(self, block:dict, names:list, keys:list, space:str = ""):
        for name in names:
            keys.append(self.map_vae_name(name))
            if name == 'weight' or name == 'bias':
                tensor = block[name]
                length = tensor.element_size() * tensor.numel()
                self.paths['.'.join(keys)] = {"start":self.total_len, "length":length, "shape":block[name].shape}
                self.converted.append(block[name])
                self.total_len += length
            else:
                if name == "down":
                    down_block = block[name]
                    for dn_index in down_block:
                        keys.append(str(dn_index))
                        next_names = []
                        next_names.append("block")
                        if "downsample" in down_block[dn_index] : next_names.append("downsample")
                        self.set_order(down_block[dn_index], next_names, keys)
                        keys.pop()
                elif name == "mid":
                    mid_block = block[name]
                    counter = 0
                    for mid_index in mid_block:
                        keys.append(str(counter))
                        counter += 1
                        cur_mid_block = mid_block[mid_index]
                        self.set_order(cur_mid_block, ["block"], keys)
                        keys.pop()
                elif name == "up":
                    up_block = block[name]
                    for up_index in up_block:
                        keys.append(str(up_index))
                        next_names = []
                        next_names.append("block")
                        if "upsample" in up_block[up_index] : next_names.append("upsample")
                        self.set_order(up_block[up_index], next_names, keys)
                        keys.pop()
                elif name == "block":
                    resnet_block = block[name]
                    counter = 0
                    for rn_index in resnet_block:
                        next_names = []
                        keys.append(str(counter))
                        rn = resnet_block[rn_index]
                        next_names.append("norm1")
                        next_names.append("conv1")
                        next_names.append("norm2")
                        next_names.append("conv2")
                        if len(rn) == 5: next_names.append("nin_shortcut")
                        self.set_order(rn, next_names, keys)
                        if "attn" in block and rn_index in block['attn']:
                            bindex = keys.pop()
                            bname = keys.pop()
                            self.set_order(block['attn'][rn_index], [f"attn.{counter}"], keys)
                            keys.append(bname)
                            keys.append(bindex)
                        keys.pop()
                        counter += 1

                elif name.startswith("attn"):
                    self.set_order(block, ["norm","q","k","v","proj_out"], keys)
                elif name == "upsample":
                    self.set_order(block[name], ["conv"], keys)
                elif name == "downsample":
                    self.set_order(block[name], ["conv"], keys)
                else:
                    next_names = ["weight", "bias"]
                    sub_block = block[name]
                    self.set_order(sub_block, next_names, keys)
            keys.pop()

    def transform_encoder(self):
        self.set_order(self.encoder_tensors,["conv_in", "down", "mid", "norm_out", "conv_out", "quant_conv"], ["encoder"])
        # self.add_weight_bias(self.encoder_tensors["conv_in"])
        # self.add_vae_blocks( self.encoder_tensors["down"])
        # self.add_vae_blocks( self.encoder_tensors["mid"])
        # self.add_weight_bias(self.encoder_tensors["norm_out"])
        # self.add_weight_bias(self.encoder_tensors["conv_out"])
        # self.add_weight_bias(self.encoder_tensors["quant_conv"])

    def transform_decoder(self):
        self.set_order(self.decoder_tensors,["post_quant_conv", "conv_in", "mid", "up", "norm_out", "conv_out"], ["decoder"])
        # self.add_weight_bias(self.decoder_tensors["post_quant_conv"])
        # self.add_weight_bias(self.decoder_tensors["conv_in"])
        # self.add_vae_blocks(self.decoder_tensors["mid"])
        # self.add_vae_blocks(self.decoder_tensors["up"])
        # self.add_weight_bias(self.decoder_tensors["norm_out"])
        # self.add_weight_bias(self.decoder_tensors["conv_out"])


    def add_vae_blocks(self, blocks):
        for block_index in blocks:
            block = blocks[block_index]

            # order is resnet0, [attn0, resnet1]*
            if 'block' in block:
                rns = block['block']
                for rn_index in rns:
                    rn = rns[rn_index]
                    self.add_vae_resnet(rn)

                    if 'attn' in block and rn_index in block['attn']:
                        atn = block['attn'][rn_index]
                        self.add_vae_attention(atn)

            if 'upsample' in block:
                uss = block['upsample']
                for us_index in uss:
                    us = uss[us_index]
                    self.add_vae_upsample(us)

            if 'downsample' in block:
                dss = block['downsample']
                for ds_index in dss:
                    ds = dss[ds_index]
                    self.add_vae_downsample(ds)

    def add_vae_downsample(self, block:dict):
        self.add_weight_bias(block)

    def add_vae_upsample(self, block:dict):
        self.add_weight_bias(block)

    def add_vae_resnet(self, block:dict):
        assert len(block) == 4 or len(block) == 5, "resnet length error"
        start_len = len(self.converted)
        self.add_weight_bias(block["norm1"])
        self.add_weight_bias(block["conv1"])
        self.add_weight_bias(block["norm2"])
        self.add_weight_bias(block["conv2"])
        
        if len(block) == 5:
            self.add_weight_bias(block["nin_shortcut"])
        assert  len(self.converted) - start_len == len(block) * 2, "Not all tensors parsed in resnet."

    def add_vae_attention(self, block:dict):
        assert len(block) == 5, "attention length error"
        start_len = len(self.converted)
        self.add_weight_bias(block["norm"])
        self.add_weight_bias(block["q"])
        self.add_weight_bias(block["k"])
        self.add_weight_bias(block["v"])
        self.add_weight_bias(block["proj_out"])
        
        assert  len(self.converted) - start_len == len(block) * 2, "Not all tensors parsed in attention."













    def transform_lora_text_encoder_layers(self):
        tensor_dict  = self.lora_tensors["te_text_model_encoder_layers"]
        self.add_MLPTransformer(tensor_dict["0"])
        self.add_MLPTransformer(tensor_dict["1"])
        self.add_MLPTransformer(tensor_dict["2"])
        self.add_MLPTransformer(tensor_dict["3"])
        self.add_MLPTransformer(tensor_dict["4"])
        self.add_MLPTransformer(tensor_dict["5"])
        self.add_MLPTransformer(tensor_dict["6"])
        self.add_MLPTransformer(tensor_dict["7"])
        self.add_MLPTransformer(tensor_dict["8"])
        self.add_MLPTransformer(tensor_dict["9"])
        self.add_MLPTransformer(tensor_dict["10"])
        self.add_MLPTransformer(tensor_dict["11"])

    def transform_lora_unet(self):
        down_dict  = self.lora_tensors["unet_down_blocks"]
        self.add_lora_unet_block(down_dict["0"], 2)
        self.add_lora_unet_block(down_dict["1"], 2)
        self.add_lora_unet_block(down_dict["2"], 2)
        mid_dict  = self.lora_tensors["unet_mid_block"]
        self.add_lora_unet_block(mid_dict["*"], 1)
        up_dict  = self.lora_tensors["unet_up_blocks"]
        self.add_lora_unet_block(up_dict["1"], 3)
        self.add_lora_unet_block(up_dict["2"], 3)
        self.add_lora_unet_block(up_dict["3"], 3)


    def transform_zero_convs(self):
        tensor_dict  = self.unet_tensors["zero_convs"]
        self.add_weight_bias(tensor_dict["0.0"])
        self.add_weight_bias(tensor_dict["1.0"])
        self.add_weight_bias(tensor_dict["2.0"])
        self.add_weight_bias(tensor_dict["3.0"])
        self.add_weight_bias(tensor_dict["4.0"])
        self.add_weight_bias(tensor_dict["5.0"])
        self.add_weight_bias(tensor_dict["6.0"])
        self.add_weight_bias(tensor_dict["7.0"])
        self.add_weight_bias(tensor_dict["8.0"])
        self.add_weight_bias(tensor_dict["9.0"])
        self.add_weight_bias(tensor_dict["10.0"])
        self.add_weight_bias(tensor_dict["11.0"])

    def transform_middle_block_out(self):
        tensor_dict  = self.unet_tensors["middle_block_out"]
        self.add_weight_bias(tensor_dict["0"])

    def transform_input_hint_block(self):
        tensor_dict  = self.unet_tensors["input_hint_block"]
        self.add_weight_bias(tensor_dict["0"])
        self.add_weight_bias(tensor_dict["2"])
        self.add_weight_bias(tensor_dict["4"])
        self.add_weight_bias(tensor_dict["6"])
        self.add_weight_bias(tensor_dict["8"])
        self.add_weight_bias(tensor_dict["10"])
        self.add_weight_bias(tensor_dict["12"])
        self.add_weight_bias(tensor_dict["14"])

    def transform_timesteps(self):
        t_temb = self.unet_tensors["time_embed"]
        self.add_timesteps(t_temb["0"])
        self.add_timesteps(t_temb["2"])

    def transform_input_blocks(self):
        t_input  = self.unet_tensors["input_blocks"]

        self.add_weight_bias(t_input["0.0"])

        self.add_resnet(t_input["1.0"])
        self.add_transformer(t_input["1.1"])
        self.add_resnet(t_input["2.0"])
        self.add_transformer(t_input["2.1"])
        self.add_downsample(t_input["3.0"])

        self.add_resnet(t_input["4.0"])
        self.add_transformer(t_input["4.1"])
        self.add_resnet(t_input["5.0"])
        self.add_transformer(t_input["5.1"])
        self.add_downsample(t_input["6.0"])

        self.add_resnet(t_input["7.0"])
        self.add_transformer(t_input["7.1"])
        self.add_resnet(t_input["8.0"])
        self.add_transformer(t_input["8.1"])
        self.add_downsample(t_input["9.0"])
        
        self.add_resnet(t_input["10.0"])
        self.add_resnet(t_input["11.0"])

    def transform_middle_block(self):
        t_middle = self.unet_tensors["middle_block"]
        self.add_resnet(t_middle["0"])
        self.add_transformer(t_middle["1"])
        self.add_resnet(t_middle["2"])
        
    def transform_output_blocks(self):
        t_output = self.unet_tensors["output_blocks"]
        self.add_resnet(t_output["0.0"])
        self.add_resnet(t_output["1.0"])
        self.add_resnet(t_output["2.0"])
        self.add_upsample(t_output["2.1"])
        
        self.add_resnet(t_output["3.0"])
        self.add_transformer(t_output["3.1"])
        self.add_resnet(t_output["4.0"])
        self.add_transformer(t_output["4.1"])
        self.add_resnet(t_output["5.0"])
        self.add_transformer(t_output["5.1"])
        self.add_upsample(t_output["5.2"])
        
        self.add_resnet(t_output["6.0"])
        self.add_transformer(t_output["6.1"])
        self.add_resnet(t_output["7.0"])
        self.add_transformer(t_output["7.1"])
        self.add_resnet(t_output["8.0"])
        self.add_transformer(t_output["8.1"])
        self.add_upsample(t_output["8.2"])
        
        self.add_resnet(t_output["9.0"])
        self.add_transformer(t_output["9.1"])
        self.add_resnet(t_output["10.0"])
        self.add_transformer(t_output["10.1"])
        self.add_resnet(t_output["11.0"])
        self.add_transformer(t_output["11.1"])

    def transform_out(self):
        t_out    = self.unet_tensors["out"]
        self.add_weight_bias(t_out["0"])
        self.add_weight_bias(t_out["2"])





    def add_timesteps(self, block:dict):
        assert len(block) == 2, "timestep length error"
        # self.ordered_modules = [self.linear_1, self.linear_2]
        self.converted.append(block["weight"])
        self.converted.append(block["bias"])

    def add_weight_bias(self, block:dict):
        assert len(block) == 2, "weight_bias length error"
        self.converted.append(block["weight"])
        self.converted.append(block["bias"])

    def add_resnet(self, block:dict):
        assert len(block) == 10 or len(block) == 12, "resnet length error"
        start_len = len(self.converted)
        # self.ordered_modules = [self.norm1, self.conv1, self.time_emb_proj, self.norm2, self.conv2]
        self.converted.append(block["in_layers.0.weight"])
        self.converted.append(block["in_layers.0.bias"])
        self.converted.append(block["in_layers.2.weight"])
        self.converted.append(block["in_layers.2.bias"])
        self.converted.append(block["emb_layers.1.weight"])
        self.converted.append(block["emb_layers.1.bias"])
        self.converted.append(block["out_layers.0.weight"])
        self.converted.append(block["out_layers.0.bias"])
        self.converted.append(block["out_layers.3.weight"])
        self.converted.append(block["out_layers.3.bias"])
        if "skip_connection.bias" in block.keys():
            self.converted.append(block["skip_connection.weight"])
            self.converted.append(block["skip_connection.bias"])

        assert  len(self.converted) - start_len == len(block), "Not all tensors parsed in resnet."


    def add_transformer(self, block:dict):
        assert len(block) == 26, "transform length error"
        start_len = len(self.converted)
        # self.ordered_modules = [
        #     self.norm, self.proj_in,
        #     self.norm1, self.cross0_q, self.cross0_k, self.cross0_v, self.cross0_out,
        #     self.norm2, self.cross1_q, self.cross1_k, self.cross1_v, self.cross1_out,
        #     self.norm3, self.ff_GEGLU.proj, self.ff_out,
        #     self.proj_out]
        self.converted.append(block["norm.weight"])
        self.converted.append(block["norm.bias"])
        self.converted.append(block["proj_in.weight"])
        self.converted.append(block["proj_in.bias"])

        self.converted.append(block["norm1.weight"])
        self.converted.append(block["norm1.bias"])
        self.converted.append(block["attn1.to_q.weight"])
        self.converted.append(block["attn1.to_k.weight"])
        self.converted.append(block["attn1.to_v.weight"])
        self.converted.append(block["attn1.to_out.0.weight"])
        self.converted.append(block["attn1.to_out.0.bias"])

        self.converted.append(block["norm2.weight"])
        self.converted.append(block["norm2.bias"])
        self.converted.append(block["attn2.to_q.weight"])
        self.converted.append(block["attn2.to_k.weight"])
        self.converted.append(block["attn2.to_v.weight"])
        self.converted.append(block["attn2.to_out.0.weight"])
        self.converted.append(block["attn2.to_out.0.bias"])

        self.converted.append(block["norm3.weight"])
        self.converted.append(block["norm3.bias"])

        self.converted.append(block["ff.net.0.proj.weight"])
        self.converted.append(block["ff.net.0.proj.bias"])
        self.converted.append(block["ff.net.2.weight"])
        self.converted.append(block["ff.net.2.bias"])

        self.converted.append(block["proj_out.weight"])
        self.converted.append(block["proj_out.bias"])
        
        assert  len(self.converted) - start_len == len(block), "Not all tensors parsed in transformer."

    def add_downsample(self, block:dict):
        assert len(block) == 2, "downsample length error"
        self.converted.append(block["op.weight"])
        self.converted.append(block["op.bias"])

    def add_upsample(self, block:dict):
        assert len(block) == 2, "upsample length error"
        self.converted.append(block["conv.weight"])
        self.converted.append(block["conv.bias"])

    def append_up_down_alpha(self, block:dict, prefix:str):
        self.converted.append(block[f"{prefix}.lora_up.weight"])
        self.converted.append(block[f"{prefix}.lora_down.weight"])
        self.converted.append(block[f"{prefix}.alpha"])       

    def add_MLPTransformer(self, block:dict):
        assert len(block) == 18, "mlp transform length error"
        start_len = len(self.converted)
        self.append_up_down_alpha(block, "mlp_fc1")
        self.append_up_down_alpha(block, "mlp_fc2")
        self.append_up_down_alpha(block, "self_attn_q_proj")
        self.append_up_down_alpha(block, "self_attn_k_proj")
        self.append_up_down_alpha(block, "self_attn_v_proj")
        self.append_up_down_alpha(block, "self_attn_out_proj")
        
        assert  len(self.converted) - start_len == len(block), "Not all tensors parsed in mlp transformer."

    def add_lora_unet_block(self, block:dict, count):
        assert len(block) == 36*count, "lora transform length error"
        start_len = len(self.converted)
        for i in range(count):
            prefix = f"attentions_{i}_"
            self.append_up_down_alpha(block, f"{prefix}proj_in")
            for j in range(2):
                tprefix = f"{prefix}transformer_blocks_0_attn{j+1}_"
                self.append_up_down_alpha(block, f"{tprefix}to_q")
                self.append_up_down_alpha(block, f"{tprefix}to_k")
                self.append_up_down_alpha(block, f"{tprefix}to_v")
                self.append_up_down_alpha(block, f"{tprefix}to_out_0")

            self.append_up_down_alpha(block, f"{prefix}transformer_blocks_0_ff_net_0_proj")
            self.append_up_down_alpha(block, f"{prefix}transformer_blocks_0_ff_net_2")
            self.append_up_down_alpha(block, f"{prefix}proj_out")

        
        assert  len(self.converted) - start_len == len(block), "Not all tensors parsed in mlp transformer."


if __name__ == "__main__":
    if len(sys.argv) > 1:
        parser = argparse.ArgumentParser(description='Process a path.')
        parser.add_argument('path', type=str, help='The path to process')
        args = parser.parse_args()
        target = args.path
    convertSD = ConvertSafetensors()
    convertSD.parse(target)
