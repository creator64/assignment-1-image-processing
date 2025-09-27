import json
import sys
import os
import subprocess
import threading, time
import matplotlib



# Important directories
current_dir     = os.getcwd();
exe_dir         = os.path.join(current_dir, "bin", "Debug", "INFOIBV.exe")
out_dir         = os.path.join(current_dir, "out")

task2_data_dir  = os.path.join(out_dir, "task2", "data")
task2_img_dir   = os.path.join(out_dir, "task2", "images")

task3_data_dir  = os.path.join(out_dir, "task3", "data")
task3_img_dir   = os.path.join(out_dir, "task3", "images")

# settings of the script
plotData= False
noRun= False
task = None

doneLoading = True;

# code
def parseSettingsFromArgs(argslist):
    global plotData, noRun, task
    for arg in argslist:
        if arg == "--norun":
            noRun = True
        elif arg == "--plotdata":
            plotData = True
        elif (arg == "task1" or arg == "task2" or arg == "task3"):
            task = arg;

def getImgDataFileNames(imgTag : str, imgCount : int) -> list[str]:
    fileNameList = []
    for i in range(1, imgCount + 1):
        fileNameList.append(f"image_data_{imgTag}{i}.json")
    return fileNameList

def plot_task1():
    return None;

def plot_task2():
    return None

def plot_task3():
    imgData = getImgDataFileNames('G', 4)
    for dat in imgData: print(dat)

def loading_animation():
    global task
    while not doneLoading:
        for i in range(1, 4):
            print('\r' + f"{task} is being executed" + "." * i, end="", flush=True)
            time.sleep(1)
        print("\33[2K\r", end="");



if __name__ == "__main__":
    parseSettingsFromArgs(sys.argv)

    if not task: 
        print("Please specify which task to perform: \n task1, task2 or task3")
    else:
        if not noRun: 
            print(f"\033[33mRunning code for {task}, this can take a bit\033[0m")
            t = threading.Thread(target=loading_animation)
            doneLoading = False
            t.start()
            subprocess.call([exe_dir, task])
            doneLoading = True
            t.join();
            print(f"\033[32mDone running code for {task}!\033[0m")


        if (task == "task1") and plotData:
            plot_task1()
        elif task == "task2" and plotData:
            plot_task2()
        elif task == "task3" and plotData:
            plot_task3()
            

