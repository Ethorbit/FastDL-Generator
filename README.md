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
  
  <details>
   <summary>Directly</summary>
     
   <code>mono "FastDL Generator.exe" "/home/myaccount/Downloads/an-addon"</code>
  </details>

  <details>
   <summary>With Docker</summary>

   <code>docker run -it --rm -v /home/workstation/Downloads:/working ethorbit/fastdl-generator:latest /working/an-addon</code>
  </details>

Make sure there is no forward slash after the path.
 </details>
