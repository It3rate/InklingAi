
import io
import os
from typing import Any
from flask import request, Response
from PIL import Image
from typing import Optional
import re
import json
from typing import Type, TypeVar, Generic

class ImageUtils:
    
    __instance:'ImageUtils' = None  # type: ignore
    @classmethod
    def instance(cls) -> 'ImageUtils':
        if cls.__instance == None:
            cls.__instance = cls()
        return cls.__instance
    
    @classmethod
    def get_root_folder(cls) -> str:
        current_directory = os.getcwd()
        # for testing, use the default install location rather than the bin/debug folder.
        if "InklingAi" in current_directory and "AIEnv" in current_directory:
            home_dir = os.path.expanduser("~")
            converted_path = home_dir.replace("\\", "/")
            current_directory = f"{converted_path}/InklingAI"

        if not os.path.exists(current_directory):
            os.makedirs(current_directory)
        os.chdir(current_directory)
        return current_directory

    def __init__(self):
        self._root_folder = ImageUtils.get_root_folder() # "C:/Users/Robin/InklingAI"
        self._sd_folder_name = "SD"
        self._text_folder_name = "Text"
        self.ensure_folder(self.root_folder)
        self.ensure_folder(self.sd_folder)
        self.ensure_folder(self.text_folder)
        self.ensure_folder(self.sd_model_folder)

    # __root_folder = "./"
    # __sd_folder = __root_folder + "SD/"
    # __text_folder = __root_folder + "Text/"

    @property
    def root_folder(self):
        return f"{self._root_folder }/"
    @root_folder.setter
    def root_folder(self, value):
        self._root_folder = value
        self.ensure_folder(self.root_folder)
        self.ensure_folder(self.sd_folder)
        self.ensure_folder(self.text_folder)

    @property
    def sd_folder(self):
        return f"{self.root_folder}{self._sd_folder_name }/"
    @property
    def sd_model_folder(self):
        return f"{self.root_folder}{self._sd_folder_name}/models/"
    @property
    def sd_control_net_model_folder(self):
        return f"{self.root_folder}{self._sd_folder_name}/models/control_net/"
    @property
    def sd_folder_name(self):
        return self._sd_folder_name
    @sd_folder_name.setter
    def sd_folder_name(self, value):
        self._sd_folder_name = value
        self.ensure_folder(self.sd_folder)

    @property
    def text_folder(self):
        return f"{self.root_folder}{self._text_folder_name }/"
    @property
    def text_folder_name(self):
        return self._text_folder_name
    @text_folder_name.setter
    def text_folder_name(self, value):
        self._text_folder_name = value
        self.ensure_folder(self.text_folder)

    def ensure_folder(self, full_folder_name:str)->str:
        path = f"{full_folder_name}/"
        if not os.path.exists(path):
            os.makedirs(path)
        return path
    
    def ensure_folder_from_root(self, root:str, folder_name:str)->str:
        path = f"{root}{folder_name}/"
        if not os.path.exists(path):
            os.makedirs(path)
        return path
    
    @classmethod
    def save_image(cls, img, path):
        try:
            img.save(path)
        except Exception as e:
            print(f"Error saving image: {e}")
        finally:
            if img != None:
                img.close()
                
    @classmethod
    def load_image(cls, path) -> Optional[Image.Image]:
        img:Optional[Image.Image] = None
        if path is None or len(path) < 3:
            return img
        try:    
            img = Image.open(path).convert("RGB")
        except Exception as e:
            print(f"Error loading image: {e}")
        return img
    
    @classmethod
    def save_json(cls, obj, path):
        try:
            with open(path, 'w') as file:
                json.dump(obj, file, indent=4)
        except Exception as e:
            print(f"Error saving json: {e}")

    @classmethod
    def load_json(cls, path):
        obj = {}
        try:
            with open(path, 'r') as file:
                obj = json.load(file)
        except Exception as e:
            print(f"Error loading json: {e}")
        return obj

    @classmethod
    def get_image_by_path(cls, folder:str, filename:str=""):
        filepath = f"{folder}{filename}"
        img_bytes = io.BytesIO()
        with open(filepath, 'rb') as file:
            image = Image.open(file)
            image.save(img_bytes, format='PNG')
            img_bytes = img_bytes.getvalue()
        return Response(img_bytes, content_type="image/png")
    
    @classmethod
    def make_safe_filename(cls, filename):
        filename = filename.strip()
        filename = re.sub(r'[^\w-]+', '_', filename)
        filename = filename[:64]
        return filename
    

class EndpointAction(object):
    def __init__(self, action):
        self.action = action
    def __call__(self, *args):
        return self.action()