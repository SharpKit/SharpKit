SharpKit
========

C# to JavaScript Compiler

This repository contains the complete, fully functional source code for SharpKit cross-compiler.

Website: http://sharpkit.net

Library definitions, JsClr framework and other useful libraries is at:
https://github.com/SharpKit/SharpKit-SDK

##### License
SharpKit is licensed under GPL. It has all features and has no restrictions. For commercial licenses please refer to http://sharpkit.net 


##### Building
An installer is available here: http://sharpkit.net, to build from source code follow this:

###### Windows
```
git clone https://github.com/SharpKit/SharpKit.git
cd SharpKit
configure
make
```

###### Linux
```
git clone https://github.com/SharpKit/SharpKit.git
cd SharpKit
chmod +x configure
./configure
make
```

###### Creating the installer (without publishing a new version)
Verify the VERSION-file in the root directory . Change the Version number if needed. Than:
```
configure # When you called this already, you do not need to call it again.
cd contrib/installer/
create
```
The installer will be created in the contrib directory and can be used for windows and linux. Note: If you create the installer on linux, it cannot be used for windows (because some windows-only libaries are missing). You should generate it on windows for better compatibility.

##### Changes from previous versions
[Path-changes](../../wiki/Path-changes) wiki page contains changes made to the file system installation of SharpKit from previous versions.
