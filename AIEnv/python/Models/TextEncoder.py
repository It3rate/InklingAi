# import torch
# import torch.nn as nn
# from typing import List

# class TextEmbeddings(nn.Module): # CLIPTextTransformer
#     # CLIPTextEmbeddings
#     #   (token_embedding): Embedding(49408, 768)
#     #   (position_embedding): Embedding(77, 768)
#     def forward():
#         # hidden_states = self.embeddings(input_ids=input_ids:int64[]) [1,77]
#         # position_ids = tensor([[ 0,  1,...75, 76]]
#         # inputs_embeds = lookup word embeddings from a vocabsize x 768 table (returns a torch.Size([1, 77, 768]))
#         # position_embeddings = lookup position embeddings, returns a torch.Size([1, 77, 768]) table
#         # hidden_states = inputs_embeds + position_embeddings torch.Size([1, 77, 768])
#         # causal_attn_mask = (77 layers of 0,-65504,-65504... | 0,0,-65504,-65504... etc torch.Size([1, 1, 77, 77]) float16
#         # CLIPEncoder X 12
#                 # layer_outputs = encoder_layer(
#                 #     hidden_states, [1,77,768]
#                 #     attention_mask, (none)
#                 #     causal_attention_mask,
#                 #     output_attentions=output_attentions, false
#                 # )
#         #return last_hidden_state=hidden_states
#         pass

# class EncoderLayer(nn.Module): # 12x CLIPEncoder
#     def create_modules(self)->List[nn.Module]:
#         # CLIPAttention
#         # LayerNorm   (layer_norm1): LayerNorm((768,), eps=1e-05, elementwise_affine=True)
#         # CLIPMLP
#         # LayerNorm   LayerNorm((768,), eps=1e-05, elementwise_affine=True)

#         # residual = hidden_states
#         # hidden_states = self.layer_norm1(hidden_states)
#         # hidden_states, attn_weights = self.self_attn(
#         #     hidden_states=hidden_states,
#         #     attention_mask=attention_mask,
#         #     causal_attention_mask=causal_attention_mask,
#         #     output_attentions=output_attentions,
#         # )
#         # hidden_states = residual + hidden_states
#         # residual = hidden_states
#         # hidden_states = self.layer_norm2(hidden_states)
#         # hidden_states = self.mlp(hidden_states)
#         # hidden_states = residual + hidden_states
#         # return(hidden_states,)

#                     # ModuleList(
#                     # (0-11): 12 x CLIPEncoderLayer(
#                     #     (self_attn): CLIPAttention(
#                     #     (k_proj): Linear(in_features=768, out_features=768, bias=True)
#                     #     (v_proj): Linear(in_features=768, out_features=768, bias=True)
#                     #     (q_proj): Linear(in_features=768, out_features=768, bias=True)
#                     #     (out_proj): Linear(in_features=768, out_features=768, bias=True)
#                     #     )
#                     #     (layer_norm1): LayerNorm((768,), eps=1e-05, elementwise_affine=True)
#                     #     (mlp): CLIPMLP(
#                     #     (activation_fn): QuickGELUActivation()
#                     #     (fc1): Linear(in_features=768, out_features=3072, bias=True)
#                     #     (fc2): Linear(in_features=3072, out_features=768, bias=True)
#                     #     )
#                     #     (layer_norm2): LayerNorm((768,), eps=1e-05, elementwise_affine=True)
#                     # )
#                     # )

#         return []
# class CLIPAttention(nn.Module):
#     def create_modules(self)->List[nn.Module]:
#         # (k_proj): Linear(in_features=768, out_features=768, bias=True)
#         # (v_proj): Linear(in_features=768, out_features=768, bias=True)
#         # (q_proj): Linear(in_features=768, out_features=768, bias=True)
#         # (out_proj): Linear(in_features=768, out_features=768, bias=True)
        
#         # bsz, tgt_len, embed_dim = hidden_states.size()
#         # query_states = self.q_proj(hidden_states) * self.scale
#         # key_states = self._shape(self.k_proj(hidden_states), -1, bsz)
#         # value_states = self._shape(self.v_proj(hidden_states), -1, bsz)
#         # proj_shape = (bsz * self.num_heads, -1, self.head_dim)
#         # query_states = self._shape(query_states, tgt_len, bsz).view(*proj_shape)
#         # key_states = key_states.view(*proj_shape)
#         # value_states = value_states.view(*proj_shape)
#         # src_len = key_states.size(1)
#         # attn_weights = torch.bmm(query_states, key_states.transpose(1, 2))
#         # # apply the causal_attention_mask
#         # attn_weights = attn_weights.view(bsz, self.num_heads, tgt_len, src_len) + causal_attention_mask
#         # attn_weights = attn_weights.view(bsz * self.num_heads, tgt_len, src_len)

#         # attn_weights = nn.functional.softmax(attn_weights, dim=-1)
#         # attn_weights_reshaped = None    

#         # attn_probs = nn.functional.dropout(attn_weights, p=self.dropout, training=self.training)
#         # attn_output = torch.bmm(attn_probs, value_states)
#         # attn_output = attn_output.view(bsz, self.num_heads, tgt_len, self.head_dim)
#         # attn_output = attn_output.transpose(1, 2)
#         # attn_output = attn_output.reshape(bsz, tgt_len, embed_dim)
#         # attn_output = self.out_proj(attn_output)
#         # return attn_output, attn_weights_reshaped

#         return []
# class CLIPMLP(nn.Module):
#     def create_modules(self)->List[nn.Module]:
#         # (fc1): Linear(in_features=768, out_features=3072, bias=True)
#         # (activation_fn): QuickGELUActivation()
#         # (fc2): Linear(in_features=3072, out_features=768, bias=True)
#         return []

# class TextEncoder(nn.Module):
#     def __init__(self, data, skip_encoder = False):
#         super().__init__()
#         self.ordered_modules:List[nn.Module] = []

#     def create_modules(self)->List[nn.Module]:
#         # CLIPTextTransformer
#             # TextEmbeddings
#             # EncoderLayer 12x
#             # LayerNorm
#         return []

#         # CLIPTextConfig(hidden_size=768,intermediate_size=3072, num_attention_heads=12, projection_dim=768, return_dict=True)
# #self.embeddings = CLIPTextEmbeddings(config)
#         # CLIPTextTransformer
#         # embed_dim = config.hidden_size
#         # self.embeddings = CLIPTextEmbeddings(config)
#         # self.encoder = CLIPEncoder(config)
#         # self.final_layer_norm = nn.LayerNorm(embed_dim)
# #self.token_embedding = nn.Embedding(config.vocab_size, embed_dim)
#         # self.layers = nn.ModuleList([CLIPEncoderLayer(config) for _ in range(config.num_hidden_layers)])
#         # self.gradient_checkpointing = False
#             # CLIPEncoderLayer
#             # self.self_attn = CLIPAttention(config)
#             # self.layer_norm1 = nn.LayerNorm(self.embed_dim)
#             # self.mlp = CLIPMLP(config)
#             # self.layer_norm2 = nn.LayerNorm(self.embed_dim)
#                 # CLIPAttention
#                 # self.k_proj = nn.Linear(self.embed_dim, self.embed_dim)
#                 # self.v_proj = nn.Linear(self.embed_dim, self.embed_dim)
#                 # self.q_proj = nn.Linear(self.embed_dim, self.embed_dim)
#                 # self.out_proj = nn.Linear(self.embed_dim, self.embed_dim)
#                 # CLIPMLP
#                 # self.activation_fn = ACT2FN[config.hidden_act]
#                 # self.fc1 = nn.Linear(config.hidden_size, config.intermediate_size)
#                 # self.fc2 = nn.Linear(config.intermediate_size, config.hidden_size)
#         # self.final_layer_norm = nn.LayerNorm(embed_dim)
# #self.position_embedding = nn.Embedding(config.max_position_embeddings, embed_dim)
#         # position_ids (1, len position emb) is contiguous in memory and exported when serialized
# # self.register_buffer("position_ids", torch.arange(config.max_position_embeddings).expand((1, -1)))
# # self.encoder = CLIPEncoder(config)
# # self.final_layer_norm = nn.LayerNorm(embed_dim)


#     @torch.no_grad()
#     def forward(self, hidden_states):#, output_size):
#         pass

#     def load(self, m, index)->int:
#         for module in self.ordered_modules:
#             module.weight = nn.Parameter(m[index])
#             module.bias = nn.Parameter(m[index+1])
#             index += 2
#         return index