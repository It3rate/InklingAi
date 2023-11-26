
import socket
import time
from StableDiffusionService import StableDiffusionService
from Models.Unet import Unet

if __name__ == "__main__":
    #unet = Unet()
    fm = StableDiffusionService()
    hostname = socket.gethostname()
    ip_address = socket.gethostbyname(hostname)
    fm.app.run(host=ip_address, port=9000) #"192.168.100.171", port=9000)
    while fm.running:
        time.sleep(1)

