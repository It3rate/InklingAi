import time
from torch import Tensor
import numpy as np
from typing import Any
from flask import Flask, request, Response, jsonify, send_file
from ClipModel import ClipModel
from PIL import Image
from Utils import ImageUtils
from Utils import EndpointAction

    
class ClipService:
    def __init__(self):
        self.app = Flask(__name__)
        self.create_endpoints()
        self.client = ClipModel()
        self.running = True
        self.new_image_available = False


    def create_endpoints(self):
        self.add_endpoint(endpoint="/encode_sentences", endpoint_name="/encode_sentences", handler=self.encode_sentences)
        self.add_endpoint(endpoint="/check_new_image", endpoint_name="/check_new_image", handler=self.check_new_image)
        #self.add_endpoint("/get_image/<id>", "get_image", handler=self.get_image, methods=["POST"])
        #self.add_endpoint(endpoint="/get_image", endpoint_name="/get_image", handler=ImageUtils.get_image_by_id, methods=["POST"])
        self.add_endpoint(endpoint="/stop_running_endpoint", endpoint_name="/stop_running_endpoint", handler=self.stop_running_endpoint)

    def add_endpoint(self, endpoint:str, endpoint_name:str, handler=None,  methods=["POST"]):
        self.app.add_url_rule(endpoint, endpoint_name, EndpointAction(handler), methods=methods) 

    def get_text_features(self, sentences:list[str]):
        self.last_hidden_state, self.pooled_output = self.client.get_text_features(*sentences)
        return [self.last_hidden_state, self.pooled_output]

    def encode_sentences(self):
        json:Any = request.get_json(force = False, silent = False, cache = False)
        sentences = json['sentences']
        last_hidden_state, pooled_output = self.get_text_features(sentences)
        f32 = pooled_output.data.numpy().astype(np.float32)
        bytes_data = np.ascontiguousarray(f32).tobytes()
        return Response(bytes_data, content_type="application/octet-stream")

    def stop_running_endpoint(self):
        self.running = False
        return "Stopped running"

    def check_new_image(self):
        response = {'new_image_available': self.new_image_available}
        new_image_available = False 
        return jsonify(response)



if __name__ == "__main__":
    fm = ClipService()
    fm.app.run(host="192.168.100.171", port=9000)
    while fm.running:
        time.sleep(1)
