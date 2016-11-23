for /d %%i in ("%~dp0*") do (
	rmdir "%%i\obj" /S /Q
	rmdir "%%i\bin" /S /Q
)
for %%i in ("%~dp0Debug\*.pdb","%~dp0Debug\*.vshost.*","%~dp0Release\*.pdb","$~dp0Release\*.vshost.*") do (
	del "%%i" /Q
)
for %%i in ("%~dp0*.suo") do del "%%i" /Q