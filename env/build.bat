@echo off

del dici.exe
del ..\bin\* /q /f

call csc /out:..\bin\dici.exe ..\src\*.cs ..\src\engine\*.cs 

copy ..\src\api_style.css ..\bin
copy ..\bin\* .\*
