import torch
import numpy as np
from transformers import AutoTokenizer, CLIPConfig, CLIPModel, CLIPTextModel
from PIL import Image

class ClipModel:

    def __init__(self):
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.tokenizer = AutoTokenizer.from_pretrained("openai/clip-vit-base-patch32")
        self.model = CLIPTextModel.from_pretrained("openai/clip-vit-base-patch32")
        

    def get_text_features(self, *sentences:str):
        inputs = self.tokenizer(list(sentences), padding=True, truncation=True, return_tensors="pt")
        outputs = self.model(**inputs) # type: ignore
        last_hidden_state = outputs.last_hidden_state
        pooled_output = outputs.pooler_output 
        return last_hidden_state, pooled_output



# with torch.no_grad():
#     image_features = model.encode_image(image)
#     text_features = model.encode_text(text)
    
#     logits_per_image, logits_per_text = model(image, text)
#     probs = logits_per_image.softmax(dim=-1).cpu().numpy()

# print("Label probs:", probs)  # prints: [[0.9927937  0.00421068 0.00299572]]    



# # Initializing a CLIPConfig with openai/clip-vit-base-patch32 style configuration
# configuration = CLIPConfig()
# # Initializing a CLIPModel (with random weights) from the openai/clip-vit-base-patch32 style configuration
# model = CLIPModel(configuration)
# # Accessing the model configuration
# configuration = model.config

# tokenizer = AutoTokenizer.from_pretrained("openai/clip-vit-base-patch32")
# inputs = tokenizer(["a photo of a cat face", "a photo of a dog face"], padding=True, return_tensors="pt")


# model = CLIPModel.from_pretrained("openai/clip-vit-base-patch32")
# text_features = model.get_text_features(**inputs)
# print(text_features.shape)

# text_model = CLIPTextModel.from_pretrained("openai/clip-vit-base-patch32")
# outputs = text_model(**inputs)
# last_hidden_state = outputs.last_hidden_state
# pooled_output = outputs.pooler_output 

# diff = text_features - outputs
# print(diff)

# print(last_hidden_state.shape)
# print(pooled_output.shape)
# print("end")