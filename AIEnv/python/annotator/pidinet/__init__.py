import os
import torch
import numpy as np
from einops import rearrange
from annotator.pidinet.model import pidinet

netNetwork = None
base_model_path = "./python/annotator/pidinet" 
remote_model_path = "https://github.com/TencentARC/T2I-Adapter/raw/main/models/table5_pidinet.pth"

def load_state_dict(ckpt_path, location='cpu'):
    _, extension = os.path.splitext(ckpt_path)
    if extension.lower() == ".safetensors":
        import safetensors.torch
        state_dict = safetensors.torch.load_file(ckpt_path, device=location)
    else:
        state_dict = get_state_dict(torch.load(
            ckpt_path, map_location=torch.device(location)))
    state_dict = get_state_dict(state_dict)
    print(f'Loaded state_dict from [{ckpt_path}]')
    return state_dict

def get_state_dict(d):
    return d.get('state_dict', d)

def apply_pidinet(input_image):
    global netNetwork
    if netNetwork is None:
        modelpath = os.path.join(base_model_path, "table5_pidinet.pth")
        netNetwork = pidinet()
        ckp = load_state_dict(modelpath)
        netNetwork.load_state_dict({k.replace('module.',''):v for k, v in ckp.items()})
        
    netNetwork.cuda().eval()
    assert input_image.ndim == 3
    input_image = input_image[:, :, ::-1].copy()
    with torch.no_grad():
        image_pidi = torch.from_numpy(input_image).float().cuda()
        image_pidi = image_pidi / 255.0
        image_pidi = rearrange(image_pidi, 'h w c -> 1 c h w')
        edge = netNetwork(image_pidi)[-1]
        edge = edge>0.5
        edge = (edge * 255.0).clip(0, 255).cpu().numpy().astype(np.uint8)
        
    return edge[0][0] 

def unload_pid_model():
    global netNetwork
    if netNetwork is not None:
        netNetwork.cpu()