
import sys
import os
import onnx
import torch
import shutil
import argparse
from pathlib import Path
from torch.onnx import export
from typing import Optional

sys.path.append("F:\\source\\repos\\PersonalAI\\PAI2\\python")
from Utils import ImageUtils
from ModelComponents import PromptComponent, LatentConversion, NoiseComponent, SchedulerComponent, UnetComponent
from Models.UnetForOnnx import UnetForOnnx

class ConvertToOnnx():

    def __init__(self, output_path:str):
        self.utils = ImageUtils.instance()
        self.sc:Optional[SchedulerComponent] = None
        self.pc:Optional[PromptComponent] = None
        self.lc:Optional[LatentConversion] = None
        self.nc:Optional[NoiseComponent] = None
        self.uc:Optional[UnetComponent] = None

        self.dtype = torch.float16
        self.device = "cuda"
        self.path = Path(output_path)
        self.opset = 14

    @torch.no_grad()
    def convert_unet(self):
        # self.pc = PromptComponent(self.utils.sd_model_folder)# type: ignore
        # self.nc = NoiseComponent(self.lc, self.sc)
        default_model_name = "revAnimated_v122_f16.bin"
        default_model_inpaint_name = "revAnimated_v121Inp_inpainting_f16.bin"

        self.sc = SchedulerComponent("ddim")
        self.pc = PromptComponent(self.utils.sd_model_folder)# type: ignore
        self.lc = LatentConversion(self.utils.sd_model_folder, self.sc)# type: ignore
        self.nc = NoiseComponent(self.lc, self.sc)

        self.uc = UnetComponent(self.utils.sd_model_folder, default_model_name, 
                                 default_model_inpaint_name, self.pc, self.lc, self.nc, self.sc, None)# type: ignore
        num_tokens = self.pc.text_encoder.config.max_position_embeddings
        text_hidden_size = self.pc.text_encoder.config.hidden_size
        unet_in_channels = 4 #self.uc.unet.config.in_channels
        unet_sample_width = 80 #self.uc.unet.config.sample_size
        unet_sample_height = 64 #self.uc.unet.config.sample_size
        model_path = self.path / "unet" / "model.onnx"

        self.onnx_export(
            self.uc.unet,
            model_args=(
                torch.randn(1, unet_in_channels, unet_sample_width, unet_sample_height).to(device=self.device, dtype=self.dtype),
                torch.tensor(973, device=self.device), #torch.randn(2).to(device=device, dtype=dtype),
                torch.randn(1, num_tokens, text_hidden_size).to(device=self.device, dtype=self.dtype),
                [],
            ),
            output_path=model_path,
            ordered_input_names=["sample", "timestep", "encoder_hidden_states", "hints"],
            output_names=["out_sample"],  # has to be different from "sample" for correct tracing
            dynamic_axes={
                "sample": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                #"timestep": {0: "batch"},
                "encoder_hidden_states": {0: "batch", 1: "sequence"},
            },
            use_external_data_format=True,  # UNet is > 2GB, so the weights need to be split
        )
        self.save_model(model_path)

    @torch.no_grad()
    def convert_vae_decoder(self):
        self.sc = SchedulerComponent("ddim")
        self.lc = LatentConversion(self.utils.sd_model_folder, self.sc)# type: ignore
        vae_decoder = self.lc.vae
        vae_latent_channels = 4#vae_decoder.config.latent_channels
        vae_out_channels = 3#vae_decoder.config.out_channels
        model_path = self.path / "vae_decoder" / "model.onnx"
        vae_decoder.forward = vae_decoder.decode_latent_onnx
        self.onnx_export(
            vae_decoder,
            model_args=(
                torch.randn(1, vae_latent_channels, 64, 80).to(device=self.device, dtype=self.dtype),
                #False,
            ),
            output_path = model_path,
            ordered_input_names=["latent_sample", "return_dict"],
            output_names=["sample"],
            dynamic_axes={
                "latent_sample": {0: "batch", 1: "channels", 2: "height", 3: "width"},
            },
        )

    @torch.no_grad()
    def convert_vae_encoder(self):
        self.sc = SchedulerComponent("ddim")
        self.lc = LatentConversion(self.utils.sd_model_folder, self.sc)# type: ignore
        vae_encoder = self.lc.vae

        model_path = self.path / "vae_encoder" / "model.onnx"
        vae_encoder.forward = vae_encoder.encode_latent_onnx
        #vae_encoder.forward = lambda sample, return_dict: vae_encoder.encode_latent_onnx(sample, return_dict)[0].sample()
        self.onnx_export(
            vae_encoder,
            model_args=(
                (torch.rand(512, 640, 3) * 255).byte().to(self.device),
            ),
            output_path = model_path,
            ordered_input_names=["sample"],
            output_names=["latent_sample"],
            dynamic_axes={
                #"sample": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                "sample": {0: "channels", 1: "height", 2: "width"},
            },
        )

    @torch.no_grad()
    def convert_tokenizer(self):
        tokenizer = PromptComponent(self.utils.sd_model_folder)# type: ignore
        model_path = self.path / "tokenizer" / "model.onnx"
        tokenizer.forward = tokenizer.onnxTokenize
        self.onnx_export(
            tokenizer,
            model_args=(
                (torch.rand(512, 640, 3) * 255).byte().to(self.device),
            ),
            output_path = model_path,
            ordered_input_names=["text_to_encode"],
            output_names=["text_input_tokens"],
            dynamic_axes={
            #     "sample": {0: "channels", 1: "height", 2: "width"},
            },
        )
    @torch.no_grad()
    def convert_text_encoder(self):
        self.opset = 16
        pc = PromptComponent(self.utils.sd_model_folder)# type: ignore
        text_encoder = pc.text_encoder
        tokenized_input = pc.onnxTokenize("worm shaped ROBOT creature. goldfish. Style of mucha, turner")
        #text_encoder.forward = text_encoder.onnxTextEncode
        self.onnx_export(
            text_encoder,
            model_args=(tokenized_input),
            output_path=self.path / "text_encoder" / "model.onnx",
            ordered_input_names=["input_ids"],
            output_names=["last_hidden_state"],
            dynamic_axes={
                "input_ids": {0: "batch", 1: "sequence"},
            },
        )

    def onnx_export(self,
        model,
        model_args: tuple,
        output_path: Path,
        ordered_input_names,
        output_names,
        dynamic_axes,
        use_external_data_format=False,
    ):
        output_path.parent.mkdir(parents=True, exist_ok=True)
        export(
                model,
                model_args,
                f=output_path.as_posix(),
                input_names=ordered_input_names,
                output_names=output_names,
                dynamic_axes=dynamic_axes,
                do_constant_folding=True,
                opset_version=self.opset,
            )

    def save_model(self, path:Path):
        model_path = str(path.absolute().as_posix())
        model_dir = os.path.dirname(model_path)
        model = onnx.load(model_path)
        shutil.rmtree(model_dir)
        os.mkdir(model_dir)
        onnx.save_model(
            model,
            model_path,
            save_as_external_data=True,
            all_tensors_to_one_file=True,
            location="weights.pb",
            convert_attribute=False,
        )

    @torch.no_grad()
    def convert_unet_in(self):
        default_model_name = "revAnimated_v122_f16.bin"

        unet = UnetForOnnx(default_model_name)
        unet.forward = unet.forward_in
        model_path = self.path / "unet_in" / "model.onnx"

        self.onnx_export(
            unet,
            model_args=(
                torch.randn(1, 4, 64, 80).to(device=self.device, dtype=self.dtype),
                torch.tensor(973, device=self.device, dtype=torch.float32), #torch.randn(2).to(device=device, dtype=dtype),
                torch.randn(1, 77, 768).to(device=self.device, dtype=self.dtype)
            ),
            output_path=model_path,
            ordered_input_names=["sample", "timestep", "encoder_hidden_states"],
            output_names=["out_sample", "emb", "skips"],  # has to be different from "sample" for correct tracing
            dynamic_axes={
                "sample": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                #"timestep": {0: "batch"},
                "encoder_hidden_states": {0: "batch", 1: "sequence"},
            }
        )
        self.save_model(model_path)
        
    @torch.no_grad()
    def convert_unet_mid(self):
        default_model_name = "revAnimated_v122_f16.bin"

        unet = UnetForOnnx(default_model_name)
        unet.forward = unet.forward_mid
        model_path = self.path / "unet_mid" / "model.onnx"

        self.onnx_export(
            unet,
            model_args=(
                torch.randn(2, 1280, 8, 10).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 77, 768).to(device=self.device, dtype=self.dtype)
            ),
            output_path=model_path,
            ordered_input_names=["hidden_states", "emb", "encoder_hidden_states"],
            output_names=["out_hidden_states"], 
            dynamic_axes={
                "hidden_states": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                "encoder_hidden_states": {0: "batch", 1: "sequence"},
            }
        )
        self.save_model(model_path)

    @torch.no_grad()
    def convert_unet_out_a(self):
        default_model_name = "revAnimated_v122_f16.bin"

        unet = UnetForOnnx(default_model_name)
        unet.forward = unet.forward_out_a
        model_path = self.path / "unet_out_a" / "model.onnx"

        self.onnx_export(
            unet,
            model_args=(
                torch.randn(2, 1280, 8, 10).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 77, 768).to(device=self.device, dtype=self.dtype),
                
                torch.randn(2, 640, 16, 20).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280, 16, 20).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280, 16, 20).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280, 8, 10).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280, 8, 10).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280, 8, 10).to(device=self.device, dtype=self.dtype)
                
            ),
            output_path=model_path,
            ordered_input_names=["hidden_states", "emb", "encoder_hidden_states", "hint0", "hint1", "hint2", "hint3", "hint4", "hint5"],
            output_names=["out_hidden_states"], 
            dynamic_axes={
                "hidden_states": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                "encoder_hidden_states": {0: "batch", 1: "sequence"},

                "hint0": {0: "batch", 2: "height", 3: "width"},
                "hint1": {0: "batch", 2: "height", 3: "width"},
                "hint2": {0: "batch", 2: "height", 3: "width"},
                "hint3": {0: "batch", 2: "height", 3: "width"},
                "hint4": {0: "batch", 2: "height", 3: "width"},
                "hint5": {0: "batch", 2: "height", 3: "width"},
            },
            use_external_data_format=True,
        )
        self.save_model(model_path)

    @torch.no_grad()
    def convert_unet_out_b(self):
        default_model_name = "revAnimated_v122_f16.bin"

        unet = UnetForOnnx(default_model_name)
        unet.forward = unet.forward_out_b
        model_path = self.path / "unet_out_b" / "model.onnx"

        self.onnx_export(
            unet,
            model_args=(
                torch.randn(2, 1280, 32, 40).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 77, 768).to(device=self.device, dtype=self.dtype),
                
                torch.randn(2, 320, 64, 80).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 320, 32, 40).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 640, 32, 40).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 640, 32, 40).to(device=self.device, dtype=self.dtype)
                
            ),
            output_path=model_path,
            ordered_input_names=["hidden_states", "emb", "encoder_hidden_states", "hint0", "hint1", "hint2", "hint3"],
            output_names=["out_hidden_states"], 
            dynamic_axes={
                "hidden_states": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                "encoder_hidden_states": {0: "batch", 1: "sequence"},
                "hint0": {0: "batch", 2: "height", 3: "width"},
                "hint1": {0: "batch", 2: "height", 3: "width"},
                "hint2": {0: "batch", 2: "height", 3: "width"},
                "hint3": {0: "batch", 2: "height", 3: "width"},
            },
            use_external_data_format=True,
        )
        self.save_model(model_path)

    @torch.no_grad()
    def convert_unet_out_c(self):
        default_model_name = "revAnimated_v122_f16.bin"

        unet = UnetForOnnx(default_model_name)
        unet.forward = unet.forward_out_c
        model_path = self.path / "unet_out_c" / "model.onnx"

        self.onnx_export(
            unet,
            model_args=(
                torch.randn(2, 320, 64, 80).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 1280).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 77, 768).to(device=self.device, dtype=self.dtype),
                
                torch.randn(2, 320, 64, 80).to(device=self.device, dtype=self.dtype),
                torch.randn(2, 320, 64, 80).to(device=self.device, dtype=self.dtype),
                
            ),
            output_path=model_path,
            ordered_input_names=["hidden_states", "emb", "encoder_hidden_states", "hint0", "hint1"],
            output_names=["out_hidden_states"], 
            dynamic_axes={
                "hidden_states": {0: "batch", 1: "channels", 2: "height", 3: "width"},
                "encoder_hidden_states": {0: "batch", 1: "sequence"},
                "hint0": {0: "batch", 2: "height", 3: "width"},
                "hint1": {0: "batch", 2: "height", 3: "width"},
            },
            use_external_data_format=True,
        )
        self.save_model(model_path)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        parser = argparse.ArgumentParser(description='Process a path.')
        parser.add_argument('path', type=str, help='The path to process')
        args = parser.parse_args()
        target = args.path
    convertToOnnx = ConvertToOnnx("F:/tmp/vae/sd_ink")
    convertToOnnx.convert_vae_decoder()
    # convertToOnnx.convert_vae_encoder()
    # convertToOnnx.convert_tokenizer() # not working, needs module
    # convertToOnnx.convert_text_encoder()
    # convertToOnnx.convert_unet()
    # convertToOnnx.convert_unet_in()
    # convertToOnnx.convert_unet_mid()
    # convertToOnnx.convert_unet_out_a()
    # convertToOnnx.convert_unet_out_b()
    # convertToOnnx.convert_unet_out_c()

# copy to autoencoder_kl.py
    # def encode_latent_onnx(self, image_bytes:torch.ByteTensor)->torch.FloatTensor:
    #     image_bytes = image_bytes.to(dtype=torch.float16)
    #     image_bytes = (image_bytes / 127.5) - 1.0
    #     image_bytes = image_bytes.permute(2, 0, 1) # convert from [h,w,c] to [c,h,w]
    #     image_bytes = image_bytes[[2,1,0], :, :]
    #     image_bytes = image_bytes.unsqueeze(0) # ensure a batch of 1 for encode
        
    #     #init_latents = self.vae.encode(image_bytes)
    #     h = self.encoder(image_bytes)
    #     moments = self.quant_conv(h)
    #     posterior = DiagonalGaussianDistribution(moments)
    #     init_latents = posterior.sample() 
    #     init_latents = init_latents.squeeze(0)
    #     init_latents = 0.18215 * init_latents
    #     return init_latents
    
    # def decode_latent_onnx(self, latents: torch.FloatTensor, return_dict: bool = True)->torch.ByteTensor:
    #     latents = 1.0 / 0.18215 * latents
    #     image = self.decode(latents).sample
    #     image = image.squeeze(0) 
    #     image = (image / 2.0 + 0.5).clamp(0, 1)
    #     image *= 255
    #     image = image.byte()
    #     image = image.permute(1, 2, 0) # convert from [c,h,w] to [h,w,c]

    #     image = torch.cat((image, 255 * torch.ones_like(image[..., :1])), dim=2)
    #     return image