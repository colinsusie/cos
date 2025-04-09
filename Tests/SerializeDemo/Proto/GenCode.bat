set PROTOC_DIR=..\..\..\Tools\protoc\protoc-win64
set INC_DIR=%PROTOC_DIR%\include
set SRC_DIR=.
set CS_DIR=..\ProtoCs
set DESC_DIR=..\ProtoDesc

%PROTOC_DIR%\bin\protoc.exe -I%INC_DIR% -I%SRC_DIR% --csharp_out=%CS_DIR% %SRC_DIR%\*.proto
%PROTOC_DIR%\bin\protoc.exe -I%INC_DIR% -I%SRC_DIR% --descriptor_set_out=%DESC_DIR%\desc.pb %SRC_DIR%\*.proto

pause

