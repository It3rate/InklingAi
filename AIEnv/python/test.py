from numpy import dtype
from safetensors import safe_open
import torch
import io
import os

folder = 'R:/SD/models/stable-diffusion-v1-5/'
enc_file = f'{folder}text_encoder/model.safetensors'
enc16_file = f'{folder}text_encoder/pytorch_model.safetensors'
vae_file = f'{folder}vae/diffusion_pytorch_model.safetensors'
v15_file = f'{folder}v1-5-pruned-emaonly.safetensors'
v15_del_file = f'{folder}deliberate_v2.safetensors'
lora_RA_file = f'{folder}Lora/LowRA.safetensors'
lora_more_details_file = f'{folder}Lora/more_details.safetensors'
lora_3DMM_V11_file = f'{folder}Lora/3DMM_V11.safetensors'
lora_epi_noiseoffset2_file = f'{folder}Lora/epi_noiseoffset2.safetensors'

target = v15_file

print("\033[97m")
diffusion_key = "model.diffusion_model."
unet = {"input_blocks":{},"middle_block":{}, "output_blocks":{}, "time_embed":{}, "out":{}}
with safe_open(target, framework="pt", device=0) as st:
    txt_file = target.replace(".safetensors", ".txt")
    with open(txt_file, 'w') as file:
        for key in st.keys():
            if(key.startswith(diffusion_key)):
                tensor = st.get_tensor(key)
                tensor = tensor.to('cuda', dtype=torch.float16)
                file.write("{:<40} {}\n".format(str(tensor.shape), key))
                tail = key[len(diffusion_key):]
                sp = tail.split(".")
                for block in unet.keys():
                    if(block == sp[0]):
                        unet[block][tail[len(block) + 1:]] = tensor
                        break
                