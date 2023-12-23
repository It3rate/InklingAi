import cv2
from PIL import Image
import numpy as np
from annotator import canny, hed, leres, midas, mlsd, openpose, uniformer, pidinet
from annotator.util import resize_image, HWC3

class ControlNetUtils:
    def __init__(self):
        self.proc_map = {
            "canny" : self.process_canny,
            "hed" : self.process_hed,
            "leres" : self.process_leres,
            "depth" : self.process_depth,
            "normal" : self.process_normal,
            "mlsd" : self.process_mlsd,
            "openpose" : self.process_openpose,
            "pidinet" : self.process_pidinet,
            "uniformer" : self.process_uniformer, # seg 
        }

    def img_to_mat(self, img): 
        img = HWC3(np.asarray(img))
        return img
        
    def mat_to_img(self, mat, invert=False):
        if invert:
            mat = (255-mat)
        return Image.fromarray(mat)
    
    def process_kind(self, img, kind:str, params={}):
        result = None
        if kind in self.proc_map:
            proc = self.proc_map[kind]
            result = proc(img, **params)
        return result
    
    
    # single shot
    def process_canny(self, img, invert=False, low=100, high=200): # edges
        img = self.img_to_mat(img)
        result = canny.apply_canny(img, low, high)
        result = self.mat_to_img(result, invert)
        return result
    
    def process_hed(self, img, invert=False): # outlines
        img = self.img_to_mat(img)
        result = hed.apply_hed(img)
        result = self.mat_to_img(result, invert)
        return result
    
    def process_leres(self, img, invert=False, near=0, bkg=0): # depthLR #near and bkg are percents, 0-100
        img = self.img_to_mat(img)
        enableMDE = False ## Enable optimized monocular depth estimation 
        result = leres.apply_leres(img, near, bkg, enableMDE)
        result = self.mat_to_img(result, invert)
        return result
    
    def process_depth(self, img, init=6.2830, bkg_thresh=0.1): # depth M
        img = self.img_to_mat(img)
        depth, _ = midas.apply_midas(img, init, bkg_thresh)
        depth = self.mat_to_img(depth)
        return depth
    
    def process_normal(self, img, init=6.2830, bkg_thresh=0.1): # depth, normal
        img = self.img_to_mat(img)
        _, normal = midas.apply_midas(img, init, bkg_thresh)
        normal = self.mat_to_img(normal)
        return normal
    
    def process_mlsd(self, img, invert=False, hough_value=0.2, hough_thresh=0.7): # straight lines
        img = self.img_to_mat(img)
        result = mlsd.apply_mlsd(img, hough_value, hough_thresh)
        result = self.mat_to_img(result, invert)
        return result
    
    def process_openpose(self, img, hand=False): # pose
        img = self.img_to_mat(img)
        result, _ = openpose.apply_openpose(img, hand)
        result = self.mat_to_img(result)
        return result
    
    def process_pidinet(self, img):  # sketch
        img = self.img_to_mat(img)
        result = pidinet.apply_pidinet(img)
        result = self.mat_to_img(result)
        return result
    
    def process_uniformer(self, img): # segment
        img = self.img_to_mat(img)
        result = uniformer.apply_uniformer(img)
        result = self.mat_to_img(result)
        return result
    

    # tuple returns
    def process_midas(self, img, init=6.2830, bkg_thresh=0.1): # depth, normal
        img = self.img_to_mat(img)
        depth, normal = midas.apply_midas(img, init, bkg_thresh)
        depth = self.mat_to_img(depth)
        normal = self.mat_to_img(normal)
        return depth, normal
    
    def process_openpose_candidates(self, img, hand=False): # pose
        img = self.img_to_mat(img)
        result, candidate_subset = openpose.apply_openpose(img, hand)
        result = self.mat_to_img(result)
        return result, candidate_subset