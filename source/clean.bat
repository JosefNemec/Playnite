@echo off
echo --- Starting Nuclear Clean ---

:: Move to the directory where the script is located
cd /d "%~dp0"

:: Delete .vs folder (hidden)
if exist ".vs" (
    echo Deleting .vs folder...
    attrib -h -s -r ".vs" /s /d
    rd /s /q ".vs"
)

:: Recursively delete all bin and obj folders
for /d /r . %%d in (bin,obj) do (
    if exist "%%d" (
        echo Deleting: "%%d"
        rd /s /q "%%d"
    )
)

echo --- Clean Complete ---
pause