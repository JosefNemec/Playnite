!include "UninstallLog.nsh"
!include "MUI2.nsh"
!include "nsProcess.nsh"
!include "LogicLib.nsh"
!include "FileFunc.nsh"

Var /GLOBAL ProgressOnly
Var /GLOBAL Portable
OutFile "PlayniteInstaller.exe"
Unicode true
SetCompressor /SOLID lzma

!define UninstLog "uninstall.log"
Var UninstLog

!define REG_ROOT "HKLM"
!define REG_APP_PATH "SOFTWARE\Playnite"
!define REG_UNINSTALL_PATH "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Playnite"

!define AddItem "!insertmacro AddItem"
!define BackupFile "!insertmacro BackupFile"
!define BackupFiles "!insertmacro BackupFiles"
!define CopyFiles "!insertmacro CopyFiles"
!define CreateDirectory "!insertmacro CreateDirectory"
!define CreateShortcut "!insertmacro CreateShortcut"
!define CreateShortcutSimple "!insertmacro CreateShortcutSimple"
!define File "!insertmacro File"
!define FileOname "!insertmacro FileOname"
!define Rename "!insertmacro Rename"
!define RestoreFile "!insertmacro RestoreFile"
!define RestoreFiles "!insertmacro RestoreFiles"
!define SetOutPath "!insertmacro SetOutPath"
!define WriteRegDWORD "!insertmacro WriteRegDWORD"
!define WriteRegStr "!insertmacro WriteRegStr"
!define WriteUninstaller "!insertmacro WriteUninstaller"
 
Section -openlogfile
  CreateDirectory "$INSTDIR"
  IfFileExists "$INSTDIR\${UninstLog}" +3
    FileOpen $UninstLog "$INSTDIR\${UninstLog}" w
  Goto +4
    SetFileAttributes "$INSTDIR\${UninstLog}" NORMAL
    FileOpen $UninstLog "$INSTDIR\${UninstLog}" a
    FileSeek $UninstLog 0 END
SectionEnd

!define AppExecutable "PlayniteUI.exe"
Name "Playnite ${VERSION}"
Caption "Playnite ${VERSION} Setup"
InstallDir $LOCALAPPDATA\Playnite
RequestExecutionLevel admin
AutoCloseWindow true

VIProductVersion "${VERSION}.0.0"
VIAddVersionKey "FileVersion" "${VERSION}.0.0"
VIAddVersionKey "ProductVersion" "${VERSION}.0.0"
VIAddVersionKey "FileDescription" "Playnite Setup"
VIAddVersionKey "ProductName" "Playnite"
VIAddVersionKey "LegalCopyright" "Josef Nemec"

!define MUI_ICON "..\source\PlayniteUI\Images\applogo.ico"
!define MUI_PAGE_CUSTOMFUNCTION_PRE dirPagePre
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\${AppExecutable}"
!define MUI_PAGE_CUSTOMFUNCTION_PRE finishPagePre
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "English"

LangString UninstLogMissing ${LANG_ENGLISH} "${UninstLog} not found!$\r$\nUninstallation cannot proceed!"

Function .onInit
  ${GetOptions} $CMDLINE "/ProgressOnly" $ProgressOnly
  ${GetOptions} $CMDLINE "/Portable" $Portable
FunctionEnd

Function un.onInit
  MessageBox MB_YESNO "Are you sure you want to uninstall Playnite?" IDYES NoAbort
      Abort
  NoAbort:
FunctionEnd

Function dirPagePre
  ${If} $ProgressOnly == "1"
    Abort
  ${EndIf}
FunctionEnd

Function finishPagePre
  ${If} $ProgressOnly == "1"
    Exec "$INSTDIR\${AppExecutable}"
    Abort
  ${EndIf}
FunctionEnd

!macro ClosePlaynite un
  Function ${un}ClosePlaynite
    DetailPrint "Closing existing Playnite instances..."
    nsExec::Exec '"taskkill" /im ${AppExecutable}'    
    loop:
    ${nsProcess::FindProcess} "${AppExecutable}" $R0
    StrCmp $R0 0 0 +3
    Sleep 200 
    Goto loop
  FunctionEnd
!macroend

!insertmacro ClosePlaynite ""
!insertmacro ClosePlaynite "un."

Section "Installer Section"
  Call ClosePlaynite

  ${SetOutPath} "$INSTDIR"
  
  ;{files_here}

  ${If} $Portable != "1"
  ${AndIf} $ProgressOnly != "1"
    ${WriteUninstaller} "$INSTDIR\uninstall.exe"
    
    ${CreateDirectory} "$SMPROGRAMS\Playnite"
    ${CreateShortcutSimple} "$SMPROGRAMS\Playnite\Playnite.lnk" "$INSTDIR\${AppExecutable}"
    ${CreateShortcutSimple} "$DESKTOP\Playnite.lnk" "$INSTDIR\${AppExecutable}"
    ${CreateShortcutSimple} "$SMPROGRAMS\Playnite\Uninstall.lnk" "$INSTDIR\uninstall.exe"

    ;Write the installation path into the registry
    ${WriteRegStr} "${REG_ROOT}" "${REG_APP_PATH}" "Install Directory" "$INSTDIR"
    ;Write information for Programs and Features uninstall
    ${WriteRegStr} "${REG_ROOT}" "${REG_UNINSTALL_PATH}" "DisplayName" "Playnite"
    ${WriteRegStr} "${REG_ROOT}" "${REG_UNINSTALL_PATH}" "DisplayIcon" "$INSTDIR\${AppExecutable}"
    ${WriteRegStr} "${REG_ROOT}" "${REG_UNINSTALL_PATH}" "DisplayVersion" "${VERSION}"
    ${WriteRegStr} "${REG_ROOT}" "${REG_UNINSTALL_PATH}" "InstallLocation" "$INSTDIR"
    ${WriteRegStr} "${REG_ROOT}" "${REG_UNINSTALL_PATH}" "Publisher" "Josef Nemec"
    ${WriteRegStr} "${REG_ROOT}" "${REG_UNINSTALL_PATH}" "UninstallString" "$INSTDIR\uninstall.exe"
  ${EndIf}
  
  ; VC++ redist installation
  DetailPrint "Installing VC++ redist files..."
  ${FileOname} "vcredist_x86.exe" "redist\vcredist_x86.exe"
  ExecWait '"$INSTDIR\vcredist_x86.exe" /install /passive /norestart'
SectionEnd

Section "Uninstall"
  Call un.ClosePlaynite

  ;Can't uninstall if uninstall log is missing!
  IfFileExists "$INSTDIR\${UninstLog}" +3
    MessageBox MB_OK|MB_ICONSTOP "$(UninstLogMissing)"
      Abort
 
  Push $R0
  Push $R1
  Push $R2
  SetFileAttributes "$INSTDIR\${UninstLog}" NORMAL
  FileOpen $UninstLog "$INSTDIR\${UninstLog}" r
  StrCpy $R1 -1
 
  GetLineCount:
    ClearErrors
    FileRead $UninstLog $R0
    IntOp $R1 $R1 + 1
    StrCpy $R0 $R0 -2
    Push $R0   
    IfErrors 0 GetLineCount
 
  Pop $R0
 
  LoopRead:
    StrCmp $R1 0 LoopDone
    Pop $R0
 
    IfFileExists "$R0\*.*" 0 +3
      RMDir $R0  #is dir
    Goto +9
    IfFileExists $R0 0 +3
      Delete $R0 #is file
    Goto +6
    StrCmp $R0 "${REG_ROOT} ${REG_APP_PATH}" 0 +3
      DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}" #is Reg Element
    Goto +3
    StrCmp $R0 "${REG_ROOT} ${REG_UNINSTALL_PATH}" 0 +2
      DeleteRegKey ${REG_ROOT} "${REG_UNINSTALL_PATH}" #is Reg Element
 
    IntOp $R1 $R1 - 1
    Goto LoopRead
  LoopDone:
  FileClose $UninstLog
  Delete "$INSTDIR\${UninstLog}"
  RMDir "$INSTDIR"
  Pop $R2
  Pop $R1
  Pop $R0
 
  ;Remove registry keys
  DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}"
  DeleteRegKey ${REG_ROOT} "${REG_UNINSTALL_PATH}"
SectionEnd