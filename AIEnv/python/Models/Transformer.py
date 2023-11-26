
import torch
import torch.nn as nn
from diffusers.models.attention import GEGLU
from diffusers.models.cross_attention import CrossAttnProcessor
#from diffusers.models.attention_processor import CrossAttnProcessor

class Transformer(nn.Module):
    def __init__(self, channels:int, skip = 0):
        super().__init__()
        self.skip = skip
        norm_num_groups: int = 32
        self.num_attention_heads: int = 8
        attention_head_dim:int = channels // self.num_attention_heads
        query_dim:int = self.num_attention_heads * attention_head_dim
        inner_dim:int = self.num_attention_heads * attention_head_dim # same as query_dim in this case
        cross_attention_dim:int = 768
        self.scale = (channels / self.num_attention_heads)**-0.5

        self.processor = CrossAttnProcessor()

    # Pre
        self.norm = torch.nn.GroupNorm(num_groups=norm_num_groups, num_channels=channels, eps=1e-6)
        self.proj_in = nn.Conv2d(channels, inner_dim, kernel_size=1, stride=1, padding=0)

    # BasicTransformerBlock
        self.norm1 = nn.LayerNorm(inner_dim)
        self.cross0_q = nn.Linear(query_dim, inner_dim, bias=False)
        self.cross0_k = nn.Linear(query_dim, inner_dim, bias=False)
        self.cross0_v = nn.Linear(query_dim, inner_dim, bias=False)
        self.cross0_out = nn.Linear(inner_dim, query_dim)
        self.cross0 = {"q":self.cross0_q, "k":self.cross0_k, "v":self.cross0_v, "o":self.cross0_out}

        self.norm2 = nn.LayerNorm(inner_dim)
        self.cross1_q = nn.Linear(query_dim, inner_dim, bias=False)
        self.cross1_k = nn.Linear(cross_attention_dim, inner_dim, bias=False)
        self.cross1_v = nn.Linear(cross_attention_dim, inner_dim, bias=False)
        self.cross1_out = nn.Linear(inner_dim, query_dim)
        self.cross1 = {"q":self.cross1_q, "k":self.cross1_k, "v":self.cross1_v, "o":self.cross1_out}

        self.norm3 = nn.LayerNorm(inner_dim)
        self.ff_GEGLU = GEGLU(inner_dim, inner_dim * 4)
        self.ff_out = nn.Linear(inner_dim, inner_dim)
        
        self.proj_out = nn.Conv2d(inner_dim, channels, kernel_size=1, stride=1, padding=0)

        self.ordered_modules = [self.norm, self.proj_in, self.norm1, self.cross0_q, self.cross0_k, self.cross0_v, self.cross0_out,
                         self.norm2, self.cross1_q, self.cross1_k, self.cross1_v, self.cross1_out,
                         self.norm3, self.ff_GEGLU.proj, self.ff_out,
                         self.proj_out]



    @torch.no_grad()
    def forward(self, hidden_states:torch.Tensor, encoder_hidden_states):
        batch, _, height, width = hidden_states.shape
        residual = hidden_states

        # Pre
        hidden_states = self.norm(hidden_states)
        hidden_states = self.proj_in(hidden_states)
        inner_dim = hidden_states.shape[1]
        hidden_states = hidden_states.permute(0, 2, 3, 1).reshape(batch, height * width, inner_dim)

        #Down
        norm_hidden_states = self.norm1(hidden_states)
        attn_output = self.apply_cross_attention(self.cross0, norm_hidden_states, norm_hidden_states)
        hidden_states = attn_output + hidden_states
        
        #Up
        norm_hidden_states = self.norm2(hidden_states)
        attn_output = self.apply_cross_attention(self.cross1, norm_hidden_states, encoder_hidden_states)
        hidden_states = attn_output + hidden_states

        # FF
        norm_hidden_states = self.norm3(hidden_states)
        ff_output = self.ff_GEGLU(norm_hidden_states)
        # hidden_states, gate = self.proj(hidden_states).chunk(2, dim=-1)
        # hidden_states = hidden_states * F.gelu(gate)
        ff_output = self.ff_out(ff_output)
        hidden_states = ff_output + hidden_states
        
        # Out
        hidden_states = hidden_states.reshape(batch, height, width, inner_dim).permute(0, 3, 1, 2).contiguous()
        hidden_states = self.proj_out(hidden_states)
        sample = hidden_states + residual

        return sample



    def apply_cross_attention(self, tensor_dict, hidden_states, target_hidden_states): # target_hidden_states should be same as hidden_states for self attention
        #batch_size, sequence_length, _ = hidden_states.shape
        q:nn.Linear = tensor_dict['q']
        k:nn.Linear = tensor_dict['k']
        v:nn.Linear = tensor_dict['v']
        o:nn.Linear = tensor_dict['o']
        query = q(hidden_states)
        query = self.head_to_batch_dim(query)

        key = k(target_hidden_states)
        value = v(target_hidden_states)
        key = self.head_to_batch_dim(key)
        value = self.head_to_batch_dim(value)

        attention_probs = self.get_attention_scores(query, key)
        hidden_states = torch.bmm(attention_probs, value)
        hidden_states = self.batch_to_head_dim(hidden_states)

        hidden_states = o(hidden_states)
        return hidden_states
    
    def get_attention_scores(self, query, key):
        attention_scores = torch.baddbmm(
            torch.empty(query.shape[0], query.shape[1], key.shape[1], dtype=query.dtype, device=query.device),
            query,
            key.transpose(-1, -2),
            beta=0,
            alpha=self.scale,
        )
        attention_probs = attention_scores.softmax(dim=-1)
        #attention_probs = attention_probs.to(torch.float16)
        return attention_probs

    def batch_to_head_dim(self, tensor):
        head_size = self.num_attention_heads
        batch_size, seq_len, dim = tensor.shape
        tensor = tensor.reshape(batch_size // head_size, head_size, seq_len, dim)
        tensor = tensor.permute(0, 2, 1, 3).reshape(batch_size // head_size, seq_len, dim * head_size)
        return tensor
    
    def head_to_batch_dim(self, tensor):
        head_size = self.num_attention_heads
        batch_size, seq_len, dim = tensor.shape
        tensor = tensor.reshape(batch_size, seq_len, head_size, dim // head_size)
        tensor = tensor.permute(0, 2, 1, 3).reshape(batch_size * head_size, seq_len, dim // head_size)
        return tensor
    
    def load(self, m, index)->int:
        
        for module in self.ordered_modules:
            module.weight = nn.Parameter(m[index])
            index += 1
            if module.bias != None:
                module.bias = nn.Parameter(m[index])
                index += 1
        return index
        
        keys = list(m.keys())
    # Pre
        self.norm.weight = nn.Parameter(m[keys[index + 0]])
        self.norm.bias = nn.Parameter(m[keys[index + 1]])
        self.proj_in.weight = nn.Parameter(m[keys[index + 2]])
        self.proj_in.bias = nn.Parameter(m[keys[index + 3]])

    # BasicTransformerBlock
        self.cross0_q.weight = nn.Parameter(m[keys[index + 4]])
        self.cross0_k.weight = nn.Parameter(m[keys[index + 5]])
        self.cross0_v.weight = nn.Parameter(m[keys[index + 6]])
        self.cross0_out.weight = nn.Parameter(m[keys[index + 7]])
        self.cross0_out.bias = nn.Parameter(m[keys[index + 8]])

        self.ff_GEGLU.proj.weight = nn.Parameter(m[keys[index + 9]])
        self.ff_GEGLU.proj.bias = nn.Parameter(m[keys[index + 10]])
        self.ff_out.weight = nn.Parameter(m[keys[index + 11]])
        self.ff_out.bias = nn.Parameter(m[keys[index + 12]])
        
        self.cross1_q.weight = nn.Parameter(m[keys[index + 13]])
        self.cross1_k.weight = nn.Parameter(m[keys[index + 14]])
        self.cross1_v.weight = nn.Parameter(m[keys[index + 15]])
        self.cross1_out.weight = nn.Parameter(m[keys[index + 16]])
        self.cross1_out.bias = nn.Parameter(m[keys[index + 17]])

        # Norms
        self.norm1.weight = nn.Parameter(m[keys[index + 18]])
        self.norm1.bias = nn.Parameter(m[keys[index + 19]])
        self.norm2.weight = nn.Parameter(m[keys[index + 20]])
        self.norm2.bias = nn.Parameter(m[keys[index + 21]])
        self.norm3.weight = nn.Parameter(m[keys[index + 22]])
        self.norm3.bias = nn.Parameter(m[keys[index + 23]])
        # Out
        self.proj_out.weight = nn.Parameter(m[keys[index + 24]])
        self.proj_out.bias = nn.Parameter(m[keys[index + 25]])
        return index + 26