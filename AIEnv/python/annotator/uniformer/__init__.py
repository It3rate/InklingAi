from annotator.uniformer.mmseg.apis import init_segmentor, inference_segmentor, show_result_pyplot
from annotator.uniformer.mmseg.core.evaluation import get_palette
import os

model = None
base_model_path = "./python/annotator/uniformer" 
config_file = "./python/annotator/uniformer/exp/upernet_global_small/config.py" 
checkpoint_file = "./python/annotator/uniformer/upernet_global_small.pth"

def unload_uniformer_model():
    global model
    if model is not None:
        model = model.cpu()

def apply_uniformer(img):
    global model
    if model is None:
        model = init_segmentor(config_file, checkpoint_file)
    model = model.cuda()
    result = inference_segmentor(model, img)
    
    res_img = show_result_pyplot(model, img, result, get_palette('ade'), opacity=1)
    return res_img
