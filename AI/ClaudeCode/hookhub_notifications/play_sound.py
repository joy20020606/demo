import os
import platform
import subprocess


def play_sound():
    sound_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), "ulala.wav")
    system = platform.system()
    if system == "Windows":
        import winsound
        winsound.PlaySound(sound_file, winsound.SND_FILENAME)
    elif system == "Darwin":
        subprocess.run(["afplay", sound_file])
    else:
        subprocess.run(["aplay", sound_file])


if __name__ == "__main__":
    play_sound()
