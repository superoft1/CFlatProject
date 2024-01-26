set equ==
for /f "delims=" %%i in (
'git log -1 origin/master --pretty%%equ%%"%%ad" --date format:%%Y%%m%%d_%%H%%M'
) do (
set datetime=%%i
)
set exportDirName=%datetime%_VTP
rd /s /q "bin"
rd /s /q "%datetime%_VTP"
"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -quit -logFile .\build.log -projectPath ".\" -executeMethod BuildClass.Build
xcopy "bin" "%datetime%_VTP" /Y /E /Q /I
xcopy "ChiyodaLib\netcoreapp2.0" "%datetime%_VTP\ChiyodaLib\netcoreapp2.0" /Y /E /Q /I
rd /s /q "bin"
