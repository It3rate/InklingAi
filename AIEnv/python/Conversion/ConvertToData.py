import torch
import json
import numpy as np
import os
from safetensors import safe_open

class ConvertToData:

    def __init__(self):
        self.models_folder = 'R:\\SD\\models\\stable-diffusion-v1-5\\'
        os.chdir(self.models_folder) 
        if not os.path.exists('out'):
            os.mkdir('out')


    def load(self):
        params_info = {}
        all_data = b''  # This will hold all of our parameters data as bytes
        target = f"{self.models_folder}vae\\diffusion_pytorch_model.fp16.safetensors"
        with safe_open(target, framework="pt", device=0) as model:

            for key in model.keys():
                tensor = model.get_tensor(key)
                param_data = tensor.half().data.cpu().numpy().tobytes()  # Make sure to move data to cpu before converting to numpy
                start_byte = len(all_data)  # The start byte is now the current length of our byte array
                all_data += param_data  # Add parameter bytes to our byte array
                end_byte = len(all_data)  # The end byte is now the new length of our byte array
          
                params_info[key] = {
                'startByte': start_byte,
                'endByte': end_byte,
                'shape' : list(tensor.shape)
            }

            # Write byte data to a file
            with open('out/vae.bin', 'wb') as f:
                f.write(all_data)

            # Write parameter info to a json file
            with open('out/vae.json', 'w') as f:
                json.dump(params_info, f)#, indent=4)

if __name__ == "__main__":
    convert = ConvertToData()
    convert.load()