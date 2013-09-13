@pushd "%~dp0"

xcopy /y .\SlideshowScreenSaver.scr %WINDIR%\System32\*.*

@popd

pause
