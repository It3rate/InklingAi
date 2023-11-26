
import torch
import torch.nn as nn
import onnxruntime as ort
import numpy as np

class TextEncoderFromOnnx(nn.Module):

    def __init__(self):
        super().__init__()
        print(ort.__version__)
        folder = "F:/tmp/vae/sd_ink/"
        path = folder + "text_encoder/model.onnx"
        self.inference_session = ort.InferenceSession(path, providers=["CUDAExecutionProvider"]) 
        
    @torch.no_grad()
    def forward(self, input_ids):
        input_ids_np = input_ids.cpu().numpy().astype(np.int32)
        inputs = {
            "input_ids": input_ids_np
        }
        outputs = self.inference_session.run(None, inputs)
        
        flat = outputs[0].flatten()
        #flat.tofile(f"{self.working_folder}py_text_encoding.bin")
        flat.tofile("C:/Users/Robin/InklingAI/SD/working/py_text_encoding.bin")

        result = torch.tensor(outputs[0]).cuda()
        return result