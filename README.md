# FastDL-Generator
Generates a Fast DL directory with the lua and compressed files

## How to use
 <details>
     <summary>Windows</summary>
     
Drag & drop the directories onto the executable
     
![](preview.gif)
 </details>
 <details>
     <summary>Linux</summary>
     
     1. Using Mono:
     
<code>mono "FastDL Generator.exe" "/home/myaccount/Downloads/an-addon"</code>
    
    2. Using Docker:
<code>docker run -it --rm -v /home/workstation/Downloads:/working ethorbit/fastdl-generator:latest /working/an-addon</code>
  
Make sure there is no forward slash after the path.
 </details>

