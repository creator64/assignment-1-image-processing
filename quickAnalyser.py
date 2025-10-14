import json
import sys
import os
import subprocess
import threading, time
import matplotlib.pyplot as plt



# Important directories
current_dir     = os.getcwd();
exe_dir         = os.path.join(current_dir, "bin", "Debug", "INFOIBV.exe")
out_dir         = os.path.join(current_dir, "out")

task2_data_dir  = os.path.join(out_dir, "task2", "data")
task2_img_dir   = os.path.join(out_dir, "task2", "images")
task2_plot_dir  = os.path.join(out_dir, "task2", "plots")

task3_data_dir  = os.path.join(out_dir, "task3", "data")
task3_img_dir   = os.path.join(out_dir, "task3", "images")
task3_plot_dir  = os.path.join(out_dir, "task3", "plots")


# Settings of the script
plotData= False
noRun= False
task = None
sigma = "3"

doneLoading = True;

# The actual methods for the script
def parseSettingsFromArgs(argslist):
    global plotData, noRun, task
    for arg in argslist:
        if arg == "--norun":
            noRun = True
        elif arg == "--plotdata":
            plotData = True
        elif (arg == "task1" or arg == "task2" or arg == "task3" or arg == "task4" or "task5"):
            task = arg;

def getImgDataFileNames(imgTag : str, imgCount : int) -> list[str]:
    fileNameList = []
    for i in range(1, imgCount + 1):
        fileNameList.append(f"image_data_{imgTag}{i}.json")
    return fileNameList

def plot_task2():
    datafiles = getImgDataFileNames('C', 5)

    D = [] # the distinct grayscale values of images C1 ... C5
    E = [] # the average of all pixel intenisty values of images C1 ... C5
    sizes = []

    for file in datafiles:
        filewrapper = open(os.path.join(task2_data_dir, file), "r")

        jsonstring = filewrapper.read()

        data = json.loads(jsonstring)

        histogram = data["imgData"]["histogram"]
        D.append(data["imgData"]["amountDistinctValues"])
        E.append(data["imgData"]["averageIntensity"])
        sizes.append(f"{file[-7]}{file[-6]}: {data["filterWidth"]}x{data["filterHeight"]}")

    # Create plot of average grayscale intensity valeus
    plt.figure(0);
    plt.bar(sizes, E)
    plt.ylim([min(E) - 0.50 * (max(E) - min(E)), max(E) + 0.50 * (max(E) - min(E))])
    plt.ylabel("Average Grayscale Intensity Values")
    plt.xlabel("The images C1 until C5 and their respective Filter Dimensions")
    if not (os.path.exists(task2_plot_dir)): os.mkdir(task2_plot_dir)
    plt.savefig(os.path.join(task2_plot_dir, "average_grayscale_values.png"))
    
    # Create plot of # of distinct grayscale intensity values
    plt.figure(1)
    plt.bar(sizes, D)
    plt.ylim([min(D) - 0.50 * (max(D) - min(D)), max(D) + 0.50 * (max(D) - min(D))])
    plt.ylabel("Amount of Distinct Grayscale Intensity Values")
    plt.xlabel("The images C1 until C5 and their respective Filter Dimensions")
    plt.savefig(os.path.join(task2_plot_dir, "amount_of_distinct_grayscale_values.png"))



def plot_task3():
    datafiles = getImgDataFileNames('G', 4)
    
    H = []
    sizes = []
    for file in datafiles:
        filewrapper = open(os.path.join(task3_data_dir, file), "r")

        jsonstring = filewrapper.read()

        data = json.loads(jsonstring)

        H.append(data["imgData"]["amountForegroundPixels"]);
        sizes.append(f"{file[-7]}{file[-6]}: {data["filterWidth"]}x{data["filterHeight"]}")
    
    plt.bar(sizes, H)
    plt.ylim([min(H) - 0.20 * (max(H) - min(H)), max(H) + 0.20 * (max(H) - min(H))])
    plt.ylabel("Amount of Foreground Pixels")
    plt.xlabel("The images G1 until G4 and their respective Filter Dimensions")
    if not (os.path.exists(task3_plot_dir)): os.mkdir(task3_plot_dir)
    plt.savefig(os.path.join(task3_plot_dir, "foreground_pixels_over_filter_size.png"))

# Loading animationf or terminal, just a quality of life feature
def loading_animation(loadingMsg : str):
    global task
    while not doneLoading:
        for i in range(1, 4):
            if doneLoading: break
            print('\r' + loadingMsg + "." * i, end="", flush=True)
            time.sleep(1)
        print("\33[2K\r", end="");


if __name__ == "__main__":
    parseSettingsFromArgs(sys.argv)

    if not task: 
        print("Please specify which task to perform: \n task1, task2, task3, task4")
    else:
        if not noRun: 
            print(f"\033[33mRunning code for {task}, this can take a bit\033[0m")
            t = threading.Thread(target=loading_animation, args=(f"{task} is being executed",))
            doneLoading = False
            t.start()
            if task == "task1": subprocess.call([exe_dir, task, sigma])
            else: subprocess.call([exe_dir, task])
            doneLoading = True
            t.join();
            print(f"\033[32mDone running code for {task}!\033[0m")


        if task == "task2" and plotData:
            print(f"\033[33mPlotting data for {task}, this can take a bit\033[0m")
            t = threading.Thread(target=loading_animation, args=(f"{task} is being analysed and plotted",))
            doneLoading = False
            t.start()
            plot_task2()
            doneLoading = True
            t.join();
            print(f"\033[32mDone analysing {task}!\033[0m")
        elif task == "task3" and plotData:
            print(f"\033[33mPlotting data for {task}, this can take a bit\033[0m")
            t = threading.Thread(target=loading_animation, args=(f"{task} is being analysed and plotted",))
            doneLoading = False
            t.start()
            plot_task3()
            doneLoading = True
            t.join();
            print(f"\033[32mDone analysing {task}!\033[0m")
            

