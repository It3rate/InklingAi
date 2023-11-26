import torch
import os
from transformers import CLIPTextModel, CLIPTokenizer, CLIPTextConfig
from diffusers.utils import logging
from typing import List, Optional, Union
from StableDiffusionConfig import ClipTokens, StableDiffusionConfig
from annotator.midas.api import load_model
from Models.TextEncoderFromOnnx import TextEncoderFromOnnx


logger = logging.get_logger(__name__)  # pylint: disable=invalid-name

#todo: add weightings, perhaps as separate class
class PromptComponent():

    def __init__(self, models_folder:str, prompt:str = "", negative_prompt:str = "", prompt_embeds=None, negative_prompt_embeds=None):
        self.model_loaded = False
        self.models_folder = models_folder
        self.tokenizer = self.create_tokenizer()
        self.text_encoder = self.create_encoder()

        self.prompt = prompt
        self.positive_tokens:Optional[torch.Tensor] = None
        self.prompt_embeds:Optional[torch.Tensor] = prompt_embeds

        self.negative_prompt = negative_prompt
        self.negative_tokens:Optional[torch.Tensor] = None
        self.negative_prompt_embeds:Optional[torch.Tensor] = negative_prompt_embeds
        
        self.concat_prompt:Optional[torch.Tensor] = None

        self._use_attention_mask:bool= False
        self.device = "cuda"

    def modules(self):
        return {}
    def onnxTokenize(self, text_to_encode:str):
        text_inputs = self.tokenizer(text_to_encode, padding="max_length", max_length=77, truncation=True, return_tensors="pt")
        text_input_tokens = text_inputs.input_ids
        return text_input_tokens.to(device=self.device, dtype=torch.int32)
    
    def onnxEncode(self, text_input_tokens):
        embeds = self.text_encoder(text_input_tokens.to(self.device), attention_mask=None)
        return embeds 

    def create_tokenizer(self) -> CLIPTokenizer:
        vocab = f"python/data/vocab.json"
        merges = f"python/data/merges.txt"
        tokenizer = CLIPTokenizer(vocab, merges)
        return tokenizer

    def create_encoder(self):# -> CLIPTextModel:
        te_config = CLIPTextConfig(hidden_size=768,intermediate_size=3072, num_attention_heads=12, projection_dim=768, return_dict=True)
        self.text_encoder = CLIPTextModel(te_config)
        self.text_encoder.to('cuda', dtype=torch.float16)
        self.load_data()
        # self.text_encoder = TextEncoderFromOnnx()
        return self.text_encoder

    def load_data(self):
        filepath = f"{self.models_folder}text_encoder_f16_cuda.bin"
        if os.path.exists(filepath):
            te_dict = torch.load(filepath)
            self.text_encoder.load_state_dict(te_dict)
            self.model_loaded = True

    def ensure_model(self):
        if not self.model_loaded:
            self.load_data()
        return self.model_loaded
    
    def set_with_config(self, config:StableDiffusionConfig):
        pos:Optional[str] = None
        neg:Optional[str] = None
        if config.prompt_embeds == None:
            pos = config.prompt
        else:
            self.prompt_embeds = config.prompt_embeds

        if config.negative_prompt_embeds == None:
            neg = config.negative_prompt
        else:
            self.negative_prompt_embeds = config.negative_prompt_embeds

        self.set_prompt_text(pos, neg)
            
    def check_inputs(self):
        return True
    
    def get_batched_encodings(self, batch_size:int = 1, number_of_images:int = 1):
        pass

    def get_tokens(self, prompt_or_len:Union[str, int, tuple]) -> ClipTokens:
        input =  [''] * int(prompt_or_len) if isinstance(prompt_or_len, int) else prompt_or_len
        input =  list(input) if isinstance(input, tuple) else input
        return self.tokenizer(input, padding='max_length', max_length=self.tokenizer.model_max_length, truncation=True, return_tensors='pt')# type: ignore

    def set_prompt_text(self, positive_prompt:Optional[str], negative_prompt:Optional[str]):
        if positive_prompt != None and positive_prompt != self.prompt:
            self.prompt = positive_prompt
            self.positive_tokens, embeds = self._encode_text(self.prompt)
            self.prompt_embeds = embeds.last_hidden_state

        if negative_prompt != None and negative_prompt != self.negative_prompt:
            self.negative_prompt = negative_prompt
            self.negative_tokens, embeds = self._encode_text(self.negative_prompt)
            self.negative_prompt_embeds = embeds.last_hidden_state

        if self.negative_prompt_embeds != None and self.prompt_embeds != None:
            self.concat_prompt = torch.cat([self.negative_prompt_embeds, self.prompt_embeds])

        return self.concat_prompt
    
    def set_embeds(self, positive_embed:Optional[torch.Tensor], negative_embed:Optional[torch.Tensor]):
        if positive_embed != None:
            self.prompt = ""
            self.prompt_embeds = positive_embed

        if negative_embed != None:
            self.negative_prompt = ""
            self.negative_prompt_embeds = negative_embed

        if self.negative_prompt_embeds != None and self.prompt_embeds != None:
            self.concat_prompt = torch.cat([self.negative_prompt_embeds, self.prompt_embeds])
            
        return self.concat_prompt

    def use_attention_mask(self, use_attention_mask:bool):
        self._use_attention_mask = use_attention_mask

    def encode_prompts(self, overwrite_embeds = True):
        if (self.prompt_embeds is None) or overwrite_embeds:
            self.positive_tokens, embeds = self._encode_text(self.prompt)
            self.prompt_embeds = embeds.last_hidden_state

        if (self.negative_prompt_embeds is None) or overwrite_embeds:
            self.negative_tokens, embeds = self._encode_text(self.negative_prompt)
            self.negative_prompt_embeds = embeds.last_hidden_state

        self.concat_prompt = torch.cat([self.negative_prompt_embeds, self.prompt_embeds])
        return self.concat_prompt

    def _encode_text(self, text_to_encode:Union[str,List[str]]):
        if text_to_encode is None:
            text_to_encode = ['']
        elif text_to_encode is str:
            text_to_encode = [str(text_to_encode)]

        max_length = 77 #self.tokenizer.model_max_length
        text_inputs = self.tokenizer(text_to_encode, padding="max_length", max_length=max_length, truncation=True, return_tensors="pt")
        text_input_tokens = text_inputs.input_ids
        masks = text_inputs.attention_mask
        for i, mask in enumerate(masks):
            if mask[-1] > 0:
                logger.warning(f"Your input may have been clipped, the max token length is {77} tokens: {text_to_encode[i]}") # self.tokenizer.max_model_input_sizes['openai/clip-vit-base-patch32']

        #final_masks = text_inputs.attention_mask.to(self.device) if self._use_attention_mask else None
        embeds = self.text_encoder(text_input_tokens.to(self.device))#, attention_mask=final_masks)
        return text_input_tokens, embeds 

    def get_text_features(self, sentences:List[str]): # for token and embedding experimenting
        _, embeds = self._encode_text(sentences)
        return embeds.last_hidden_state, embeds.pooler_output
    

