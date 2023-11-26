
from turtle import forward
from typing import List
import torch
import torch.nn as nn
import onnxruntime as ort
import numpy as np

from Models.Resnet2D import Resnet2D
from Models.Transformer import Transformer
from Models.Downsample import Downsample
from Models.Upsample import Upsample
from Models.TimestepEmbeds import TimestepEmbeds
from Utils import ImageUtils

class UnetFromOnnx(nn.Module):

    def __init__(self):
        super().__init__()
        folder = "F:/tmp/vae/sd_ink/"
        self.inference_session_in =    self.load_model(folder + "unet_in/model.onnx")
        self.inference_session_mid =   self.load_model(folder + "unet_mid/model.onnx")
        self.inference_session_out_a = self.load_model(folder + "unet_out_a/model.onnx")
        self.inference_session_out_b = self.load_model(folder + "unet_out_b/model.onnx")
        self.inference_session_out_c = self.load_model(folder + "unet_out_c/model.onnx")
        
    def load_model(self, path):
        return ort.InferenceSession(path, providers=["CUDAExecutionProvider"])


    @torch.no_grad()
    def forward(self, hidden_states, timestep, encoder_hidden_states, hints:List[torch.Tensor] = []):
        hidden_states_np = hidden_states.cpu().numpy()
        timestep_np = timestep.cpu().numpy()  # If timestep is a scalar. Adjust if it's already a tensor.
        encoder_hidden_states_np = encoder_hidden_states.cpu().numpy()

        # Prepare input dictionary
        inputs = {
            "encoder_hidden_states": encoder_hidden_states_np,
            "sample": hidden_states_np,
            "timestep": timestep_np
        }
        outputs = self.inference_session_in.run(None, inputs)
        hidden_states_np = outputs[0]
        emb = outputs[1]
        hints = outputs[2:14]

        inputs = {
            "encoder_hidden_states": encoder_hidden_states_np,
            "hidden_states": hidden_states_np,
            "emb": emb
        }
        outputs = self.inference_session_mid.run(None, inputs)
        hidden_states_np = outputs[0]

        inputs = {
            "encoder_hidden_states": encoder_hidden_states_np,
            "hidden_states": hidden_states_np,
            "emb": emb,
            "hint0":hints[6],
            "hint1":hints[7],
            "hint2":hints[8],
            "hint3":hints[9],
            "hint4":hints[10],
            "hint5":hints[11],
        }
        outputs = self.inference_session_out_a.run(None, inputs)
        hidden_states_np = outputs[0]

        inputs = {
            "encoder_hidden_states": encoder_hidden_states_np,
            "hidden_states": hidden_states_np,
            "emb": emb,
            "hint0":hints[2],
            "hint1":hints[3],
            "hint2":hints[4],
            "hint3":hints[5],
        }
        outputs = self.inference_session_out_b.run(None, inputs)
        hidden_states_np = outputs[0]
        
        inputs = {
            "encoder_hidden_states": encoder_hidden_states_np,
            "hidden_states": hidden_states_np,
            "emb": emb,
            "hint0":hints[0],
            "hint1":hints[1],
        }   
        outputs = self.inference_session_out_c.run(None, inputs)
        result = torch.tensor(outputs[0]).cuda()
        return result

        # inputs = create_end_input(output_tensor, emb, text_embeddings, hints[6:12])
        # outputs = run_model(inference_session_out_a, inputs)
        # output_tensor = outputs[0]

        # inputs = create_end_input(output_tensor, emb, text_embeddings, hints[2:6])
        # outputs = run_model(inference_session_out_b, inputs)
        # output_tensor = outputs[0]

        # inputs = create_end_input(output_tensor, emb, text_embeddings, hints[0:2])
        # outputs = run_model(inference_session_out_c, inputs)
        # output_tensor = outputs[0]