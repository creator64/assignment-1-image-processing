import json
import sys
import os
import subprocess;
import matplotlib

current_dir = os.getcwd();
exe_dir = os.path.join(current_dir, "bin", "Debug", "INFOIBV.exe")

if __name__ == "__main__":
    if len(sys.argv) == 0: 
        print("Please specify which task to perform")
    else:
        subprocess.call([exe_dir, sys.argv[1]])
        