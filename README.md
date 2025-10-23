# Assignment 2
Greetings Corrector(s),

Like last time, the tasks can be run with both the GUI as well as via the terminal using `quickAnalyser.py`.
The Binary Pipeline can be run via:
```bash
python quickAnalyser.py task4
```

The Grayscale Pipeline meanwhile, can be run via:
```bash
python quickAnalyser.py task5
```

The output of which can be seen in the `out/task4` and `out/task5` respectively. We figured this would be much more
ergonomical than manually typing 

For quality of life purposes, both on our and your part, we've made it so that the configurations
of each of the respective images can be found inside of the `data/` folder. These configurations are
in json format, making them easy to import in your own program to cross-check for instance.

Most of the code important for this project can be found in the `ProcessingImage.cs`, `HoughTransform.cs`, and `Pipelines.cs` files.

P.S: One of the parameters that you often see is "thetaDetail" and "rDetail" which both go from 1 to 3. We admit that these names hence this small explanation.
ThetaDetail and rDetail refer to the factor by which the input image's width and height are respectively multiplied to arrive at the dimensions for the accumulator array.

## Line Intersections
Depending on certain what image you feed to the line intersections function. Sometimes the hough transform can detect many smaller lines in what is 
actually one (thick) line on the input edge map. These lines sometimes overlap, and as a result will show up as intersections when Drawing the Hough Intersection Points.

# Assignment 1

## Run the Program

The program consists out of two subprograms.  
The pre made GUI and a subprogram to automatically execute the tasks

to run the GUI with the following commands, assuming dotnet 
is correctly installed
```
dotnet build
bin/debug/INFOIBV.exe
```

to run the subprogram you can run
```
dotnet build
bin/debug/INFOIBV.exe [taskname] [sigma]
```
with `taskname` being simply _task1_, _task2_ or _task3_
and sigma only applying to task1

This will create the output images for the tasks in the 
output folder `/out`

to analyse the images and create plots with the requested data,
you can run the additional made python script as follows

```
python quickAnalyser.py [task_name] [--plotdata] [--norun]
```
this script will run the subprogram and thereby creating the output images  
if `--plotdata` is included, it will also create the plots in the output folder  
if `--norun` is included, it will not again create the output images

## Code Modifications
There has been done some refactoring in order to make it possible to directly call the 
processing functions via the command line. The processing functions have been moved to a separate
folder called `Core` in a static class called `ProcessingFunctions`. This class contains only
static functional methods for processing grayscale images. Also some operations like converting images to grayscale and
converting bitmaps to arrays have been extracted from a couple of functions in the form designer and moved 
them to the class `ConverterMethods`. This again to make them reusable for the subprogram.
