SharpKit
========

C# to JavaScript Compiler

Website: http://sharpkit.net

SDK with all type definitions, JsClr framework and other useful libraries is at:
https://github.com/SharpKit/SharpKit-SDK

##### License
SharpKit is licensed under GPL, for commercial licenses please refer to http://sharpkit.net. The GPL license model has all features and has no restrictions. If you choose a commercial license, you can get exclusive support and training.

##### Building

    git clone https://github.com/SharpKit/SharpKit.git
    cd SharpKit
    ./configure     (on windows, skip "./" and type only "configure")
    make
    
##### Folder Structure
```
Compiler/
  Compiler.sln
  skc5/
  MSBuild/
Installer/
    Packager/
    Installer/
lib/
*** From here TBD
Defs/
  Defs.sln
  JavaScript/
  Html/
  jQuery/
JsClr/
  JsClr.sln
Tests/
  Tests.sln
Samples/
  Samples.sln
```
