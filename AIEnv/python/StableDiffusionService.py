
from http import client
import torch
import os
from PIL import Image
import numpy as np
import ctypes
from typing import Any, Optional
from flask import Flask, request, Response, jsonify
from Utils import ImageUtils
from Utils import EndpointAction
from StableDiffusionConfig import StableDiffusionConfig, ClipTokens, ClipEmbeddings
from ModelComponents.ControlNetUtils import ControlNetUtils
from ModelComponents import StableDiffusionController
    
class StableDiffusionService:
    def __init__(self):
        self.app = Flask(__name__)
        self.create_endpoints()
        self.client = StableDiffusionController()
        self.is_generating_image = False
        self.is_loading = False
        self.loading_name = ""
        self.loading_inpaint_name = ""
        self.running = True
        self.new_image_available = False
        self.utils = ImageUtils.instance()
        self.root_folder = self.utils.sd_folder

    def add_endpoint(self, endpoint:str, endpoint_name:str, handler=None,  methods=["POST"]):
        self.app.add_url_rule(endpoint, endpoint_name, EndpointAction(handler), methods=methods) 

    def create_endpoints(self):
        #self.add_endpoint(endpoint="/get_latents", endpoint_name="/get_latents", handler=self.get_latents, methods=["POST"])
        self.add_endpoint(endpoint="/is_alive", endpoint_name="/is_alive", handler=self.is_alive, methods=["GET"])
        self.add_endpoint(endpoint="/run_test", endpoint_name="/run_test", handler=self.run_test, methods=["GET"])

        self.add_endpoint(endpoint="/get_tokens", endpoint_name="/get_tokens", handler=self.get_tokens, methods=["POST"])
        self.add_endpoint(endpoint="/get_torch_tokens", endpoint_name="/get_torch_tokens", handler=self.get_torch_tokens, methods=["POST"])
        self.add_endpoint(endpoint="/encode_sentences", endpoint_name="/encode_sentences", handler=self.encode_sentences, methods=["POST"])
        #self.add_endpoint(endpoint="/get_embedding_last", endpoint_name="/get_embedding_last", handler=self.get_embedding_last, methods=["POST"])

        self.add_endpoint(endpoint="/set_model", endpoint_name="/set_model", handler=self.set_model, methods=["POST"])
        self.add_endpoint(endpoint="/get_current_model", endpoint_name="/get_current_model", handler=self.get_current_model, methods=["GET"])
        self.add_endpoint(endpoint="/set_prompt", endpoint_name="/set_prompt", handler=self.set_prompt, methods=["GET"])
        self.add_endpoint(endpoint="/set_vae", endpoint_name="/set_vae", handler=self.set_vae, methods=["GET"])

        self.add_endpoint(endpoint="/get_image_from_diffuser", endpoint_name="/get_image_from_diffuser", handler=self.get_image_from_diffuser, methods=["POST"])
        self.add_endpoint(endpoint="/get_status", endpoint_name="/get_status", handler=self.get_status, methods=["POST"])
        self.add_endpoint(endpoint="/get_image_by_path", endpoint_name="/get_image_by_path", handler=self.get_image_by_path, methods=["POST"])
        self.add_endpoint(endpoint="/upload_image", endpoint_name="/upload_image", handler=self.uploaded_image, methods=["POST"])
        self.add_endpoint(endpoint="/get_schedulers", endpoint_name="/get_schedulers", handler=self.get_schedulers, methods=["POST"])
        self.add_endpoint(endpoint="/set_scheduler", endpoint_name="/set_scheduler", handler=self.set_scheduler, methods=["POST"])
        self.add_endpoint(endpoint="/run_control_net_filter", endpoint_name="/run_control_net_filter", handler=self.run_control_net_filter, methods=["POST"])
        self.add_endpoint(endpoint="/run_control_net_filters", endpoint_name="/run_control_net_filters", handler=self.run_control_net_filters, methods=["POST"])
        self.add_endpoint(endpoint="/stop_running_endpoint", endpoint_name="/stop_running_endpoint", handler=self.stop_running_endpoint)
        #self.add_endpoint(endpoint="/gen_initial_noised", endpoint_name="/gen_initial_noised", handler=self.gen_initial_noised, methods=["POST"])
        
    def is_alive(self):
        return "I am alive."

    def run_control_net_filter(self):
        json:Any = request.get_json(force = False, silent = False, cache = False)
        kind = json['kind']
        source = json['source']
        destination = json['destination']
        params = json['params']

        input_img:Optional[Image.Image] = ImageUtils.load_image(source)
        if input_img != None:
            img = self.client.control_net.process_kind(input_img, kind, params)
            if img != None:
                ImageUtils.save_image(img, destination)
        return "OK"
    
    def run_control_net_filters(self):
        json:Any = request.get_json(force = False, silent = False, cache = False)
        kinds = json['kinds']
        folder = self.client.working_folder

        # note: if 'normal' and 'depth' are present and use the same image, these come from same model, so minor optimization potential
        for kind in kinds:
            input_img:Optional[Image.Image] = ImageUtils.load_image(f"{folder}cn_{kind}_input.png")
            if input_img != None:
                config = {}
                img = self.client.control_net.process_kind(input_img, kind, config)
                if img != None:
                    ImageUtils.save_image(img, f"{folder}cn_{kind}.png")
        return "OK"
    
    def get_tokens(self) -> ClipTokens:
        json:Any = request.get_json(force = False, silent = False, cache = False)
        query = json['query']
        return self.client.pc.get_tokens(query)
    

    def get_torch_tokens(self) -> Response:
        root = ImageUtils.get_root_folder()
        #bridge0 = ctypes.CDLL("C:/Users/Robin/PAIx/python/c10.dll")
        #bridge1 = ctypes.CDLL("C:/Users/Robin/PAIx/python/torch_cpu.dll")
        os.add_dll_directory("F:/source/repos/InklingAI/ModelRunner/x64/Debug")
        bridge = ctypes.WinDLL("F:/source/repos/InklingAI/ModelRunner/x64/Debug/ModelRunner.dll")
        json:Any = request.get_json(force = False, silent = False, cache = False)
        query = json['query']
        clip_tokens = self.client.pc.get_tokens(query)
        tensor = clip_tokens.input_ids
        tensor = tensor.cuda()
        
        data_ptr = ctypes.c_void_p(tensor.data_ptr())
        dims = (ctypes.c_longlong * 2)(3, 3)
        create_tensor = bridge.create_tensor
        create_tensor.argtypes = (ctypes.c_void_p, ctypes.POINTER(ctypes.c_longlong), ctypes.c_int)
        create_tensor.restype = ctypes.c_int
        tensor_id = create_tensor(data_ptr, dims, 2)

        ptr = tensor.data_ptr()
        type = str(tensor.dtype).split(".")[1]
        result = {"tensor_id":ptr, "dtype":type}
        return jsonify(result)
    




    def encode_sentences(self):
        json:Any = request.get_json(force = False, silent = False, cache = False)
        sentences = json['sentences']
        last_hidden_state, pooled_output = self.client.pc.get_text_features(sentences)
        f32 = pooled_output.data.cpu().numpy().astype(np.float32)
        bytes_data = np.ascontiguousarray(f32).tobytes()
        return Response(bytes_data, content_type="application/octet-stream")
        
    # def get_embedding_last(self) -> torch.Tensor:
    #     json:Any = request.get_json(force = False, silent = False, cache = False)
    #     query = json['query']
    #     tokens:ClipTokens = self.client.pc.get_tokens(query)
    #     return self.client.get_embedding(tokens).last_hidden_state
    
    def get_image_from_diffuser(self) -> Response:
        self.is_generating_image = True
        json:Any = request.get_json(force = False, silent = False, cache = False)
        config = StableDiffusionConfig(**json)
        filepath = self.client.generate_image(config)
        result = {"filepath":filepath}
        self.is_generating_image = False
        return jsonify(result)
    
    # def gen_initial_noised(self):
    #     json:Any = request.get_json(force = False, silent = False, cache = False)
    #     config = StableDiffusionConfig(**json)
    #     image:Image.Image = config.initial_image() # type: ignore 
    #     steps = config.num_inference_steps
    #     # todo: batch these in order without cumulative product
        
    #     for i in range(steps):
    #         latent = self.client.nc.add_noise_to_image(image, steps, i)
    #         torch.save(latent, f"{self.client.noise_folder}latent_{i}.pt")
            
    #     org_latent:torch.Tensor = self.client.lc.tensor_from_image(image)
    #     cur_latent = torch.zeros_like(org_latent)
    #     noise_img = self.client.lc.image_from_tensor(cur_latent)
    #     ImageUtils.save_image(noise_img, f"{self.client.noise_folder}noised_image_0.png")

    #     ratio = 1. / float(steps)
    #     step_size = self.client.scheduler.num_train_timesteps // steps # type: ignore

    #     prev_latent = torch.load(f"{self.client.noise_folder}latent_{0}.pt").to(self.client.device)
    #     for i in range(1, steps):
    #         ts = self.client.sc.scheduler.alphas_cumprod[step_size * i]
    #         latent = torch.load(f"{self.client.noise_folder}latent_{i}.pt").to(self.client.device)
    #         new_latent = ((1. - ts) ** 0.5) * org_latent + ((ts) ** 0.5) * latent
    #         diff = new_latent - prev_latent
    #         diff = diff * ratio
    #         cur_latent = cur_latent + diff * 2. # the noise cancel, so really this image is just the org noise's half signal * 2
    #         prev_latent = latent
    #         display = cur_latent - (latent * ts) # for visuals only
    #         noise_img = self.client.lc.image_from_tensor(display)
    #         ImageUtils.save_image(noise_img, f"{self.client.noise_folder}noised_image_{i}.png")
    #     return jsonify("{'status':'Ok'}")

    def uploaded_image(self) -> Response:
        if 'image' not in request.files:
            return Response('No image file in request', status=400)

        file = request.files['image']

        if file.filename == '':
            return Response('No selected file', status=400)
        upload_path = f"{self.client.uploads_folder}{file.filename}"
        ImageUtils.save_image(file, upload_path)

        result = {"filename":upload_path}
        return jsonify(result)
    
    def get_schedulers(self) -> Response:
        keys = list(self.client.sc.scheduler_map.keys())
        result = {"schedulers":keys}
        return jsonify(result)
    
    def set_scheduler(self) -> Response:
        json:Any = request.get_json(force = False, silent = False, cache = False)
        scheduler_id = json["scheduler_id"]
        status = self.client.set_scheduler(scheduler_id)
        return jsonify({"status":status})

    def get_current_model(self):
        result = {"model":self.client.uc.current_model_name, "model_inpaint":self.client.uc.current_model_inpaint_name}
        return jsonify(result)

    def set_model(self):
        result = {}
        self.is_loading = True
        json:Any = request.get_json(force = False, silent = False, cache = False)
        kind = json['kind']
        name = json['name']
        inpaint_name = json['inpaint_name']
        self.loading_name = name
        self.loading_inpaint_name = inpaint_name
        try:
            filepath, inpaint_filepath = self.client.uc.set_model(name, inpaint_name)
            result = {"filepath":filepath, "inpaint_filepath":inpaint_filepath}
        except Exception as e:
            print(f"An error occurred: {e}")
        finally:
            self.is_loading = False
            self.loading_name = ""
            self.loading_inpaint_name = ""
        return jsonify(result)
    
    def set_prompt(self):
        self.client.pc.ensure_model()
        result = {"loaded":self.client.pc.model_loaded}
        return jsonify(result)
    
    def set_vae(self):
        self.client.lc.ensure_model()
        result = {"loaded":self.client.lc.model_loaded}
        return jsonify(result)
    
    def get_status(self) -> Response:
        step = self.client.current_step
        if(self.is_loading):
            status = f"Loading model: {self.loading_name}..."
            result = {"status":status, "loading":True}
        elif self.is_generating_image:
            if step == -2:
                status = "Initializing"
            elif step == -1:
                status = "Ready"
            else:
                status = f"Generating {self.client.latest_step + self.client.uc.start_index} of {self.client.steps}"
            result = {"status":status, "step":f"{self.client.latest_step}", "current_offset_index":f"{self.client.latest_offset_index}", "latent_image":self.client.lc.last_latent_path}
        else:
            status = "Ready"
            result = {"status":status, "model":self.client.uc.current_model_name}

        return jsonify(result)
    
    def get_image_by_path(self) -> Response:
        json:Any = request.get_json(force = False, silent = False, cache = False)
        path = json['path']
        return ImageUtils.get_image_by_path(self.client.image_folder, path)
    
    
    def noise_image_test(self):
        img = ImageUtils.load_image(f"{self.client.uploads_folder}future.png")
        if img is not None:
            latent = self.client.nc.add_noise_to_image(img, 50, 40)
            latent_image = self.client.lc.image_of_latent(latent)
            ImageUtils.save_image(latent_image, f"{self.client.uploads_folder}noised_latent.png")
            noise_img = self.client.lc.image_from_tensor(latent)
            ImageUtils.save_image(noise_img, f"{self.client.uploads_folder}noised_image.png")
        return "OK"
        
    def run_test(self):
        img = ImageUtils.load_image(f"{self.client.uploads_folder}future.png")
        #img = self.client.control_net.process_canny(img, False, 512, 50, 200)
        #ImageUtils.save_image(img, f"{self.client.uploads_folder}canny_img.png")
        
        #img = self.client.control_net.process_hed(img, 512, False)
        #ImageUtils.save_image(img, f"{self.client.uploads_folder}hed_img.png")

        #img = self.client.control_net.process_leres(img, 512, False)
        #ImageUtils.save_image(img, f"{self.client.uploads_folder}leres_img.png")

        # depth, normal = self.client.control_net.process_midas(img, 512)
        # ImageUtils.save_image(depth, f"{self.client.uploads_folder}depth_img.png")
        # ImageUtils.save_image(normal, f"{self.client.uploads_folder}normal_img.png")

        # img = self.client.control_net.process_mlsd(img, 512, True, 0.2, 0.7)
        # ImageUtils.save_image(img, f"{self.client.uploads_folder}mlsd_img.png")

        # img = ImageUtils.load_image(f"{self.client.uploads_folder}pose.jpg")
        # img, info = self.client.control_net.process_openpose(img, 512, False)
        # ImageUtils.save_image(img, f"{self.client.uploads_folder}openpose_img.png")

        img = self.client.control_net.process_pidinet(img, 512)
        ImageUtils.save_image(img, f"{self.client.uploads_folder}pidinet_img.png")


        # img = self.client.control_net.process_uniformer(img, 512)
        # ImageUtils.save_image(img, f"{self.client.uploads_folder}uniformer_img.png")

        return "OK"

    

    def stop_running_endpoint(self):
        self.running = False
        return "Stopped running"


