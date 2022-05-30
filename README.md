# AssemblyDeconstanter

## What is it for ?

A tool to create a copy of an assembly in which **all fields are no longer constant** (static).
The intended usage is for modding in Unity(*), because this way you can **edit all fields value with the use of reflection**.
The modified dll can be used instead of the original dll (without this it's impossible to change the value of the constant properties).

(*) Not tested on game projects other than Unity

## Usage
You can drop your target dll onto the .exe (on Windows) or use the command line.  
The **first argument** is the path to the **target assembly** (absolute or relative).  
The **second argument is optional** and contains the **output path and/or filename**.  
* It can be just a (relative) path like `subdir1\subdir2`  
* It can be just a filename like `CustomFileName.dll`  
* It can be a filename with path like `C:\dir1\dir2\CustomFileName.dll`  
  If omited, it creates the modified assembly in the subdirectory `deconstanted_assemblies`.
  
## Command line options
Usage: AssemblyDeconstanter.exe [Options]+  
An input path must be provided, the other options are optional.  
You can use it without the option identifiers; If so, the first argument is for input and the optional second one for output.  

Options:

|  -short, --long            | Description                                       |
| -------------------------- | ------------------------------------------------- |
|  -i, --input=VALUE         | Path (relative or absolute) to the input assembly |
|  -o, --output=VALUE        | Path/dir/filename for the output assembly         |
|  -e, --exit                | Application should automatically exit             |
|  -h, --help                | Show this message and exit                        |
  
(inspired by AssemblyPublicezer)
