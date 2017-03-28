;AddItem macro
  !macro AddItem Path
    FileWrite $UninstLog "${Path}$\r$\n"
  !macroend
 
;File macro
  !macro File FileName
     IfFileExists "$OUTDIR\${FileName}" +2
     FileWrite $UninstLog "$OUTDIR\${FileName}$\r$\n"
     File "${FileName}"
  !macroend

  !macro FileOname DestinationName FileName
     IfFileExists "$OUTDIR\${DestinationName}" +2
     FileWrite $UninstLog "$OUTDIR\${DestinationName}$\r$\n"
     File "/oname=${DestinationName}" "${FileName}"
  !macroend
 
;CreateShortcut macro
  !macro CreateShortcut FilePath FilePointer Pamameters Icon IconIndex
    FileWrite $UninstLog "${FilePath}$\r$\n"
    CreateShortcut "${FilePath}" "${FilePointer}" "${Pamameters}" "${Icon}" "${IconIndex}"
  !macroend
  !macro CreateShortcutSimple FilePath FilePointer
    FileWrite $UninstLog "${FilePath}$\r$\n"
    CreateShortcut "${FilePath}" "${FilePointer}"
  !macroend
 
;Copy files macro
  !macro CopyFiles SourcePath DestPath
    IfFileExists "${DestPath}" +2
    FileWrite $UninstLog "${DestPath}$\r$\n"
    CopyFiles "${SourcePath}" "${DestPath}"
  !macroend
 
;Rename macro
  !macro Rename SourcePath DestPath
    IfFileExists "${DestPath}" +2
    FileWrite $UninstLog "${DestPath}$\r$\n"
    Rename "${SourcePath}" "${DestPath}"
  !macroend
 
;CreateDirectory macro
  !macro CreateDirectory Path
    CreateDirectory "${Path}"
    FileWrite $UninstLog "${Path}$\r$\n"
  !macroend
 
;SetOutPath macro
  !macro SetOutPath Path
    SetOutPath "${Path}"
    FileWrite $UninstLog "${Path}$\r$\n"
  !macroend
 
;WriteUninstaller macro
  !macro WriteUninstaller Path
    WriteUninstaller "${Path}"
    FileWrite $UninstLog "${Path}$\r$\n"
  !macroend
 
;WriteIniStr macro
  !macro WriteIniStr IniFile SectionName EntryName NewValue
     IfFileExists "${IniFile}" +2
     FileWrite $UninstLog "${IniFile}$\r$\n"
     WriteIniStr "${IniFile}" "${SectionName}" "${EntryName}" "${NewValue}"
  !macroend
 
;WriteRegStr macro
  !macro WriteRegStr RegRoot UnInstallPath Key Value
     FileWrite $UninstLog "${RegRoot} ${UnInstallPath}$\r$\n"
     WriteRegStr "${RegRoot}" "${UnInstallPath}" "${Key}" "${Value}"
  !macroend
 
 
;WriteRegDWORD macro
  !macro WriteRegDWORD RegRoot UnInstallPath Key Value
     FileWrite $UninstLog "${RegRoot} ${UnInstallPath}$\r$\n"
     WriteRegDWORD "${RegRoot}" "${UnInstallPath}" "${Key}" "${Value}"
  !macroend
 
;BackupFile macro
  !macro BackupFile FILE_DIR FILE BACKUP_TO
   IfFileExists "${BACKUP_TO}\*.*" +2
    CreateDirectory "${BACKUP_TO}"
   IfFileExists "${FILE_DIR}\${FILE}" 0 +2
    Rename "${FILE_DIR}\${FILE}" "${BACKUP_TO}\${FILE}"
  !macroend
 
;RestoreFile macro
  !macro RestoreFile BUP_DIR FILE RESTORE_TO
   IfFileExists "${BUP_DIR}\${FILE}" 0 +2
    Rename "${BUP_DIR}\${FILE}" "${RESTORE_TO}\${FILE}"
  !macroend
 
;BackupFiles macro
  !macro BackupFiles FILE_DIR FILE BACKUP_TO
   IfFileExists "${BACKUP_TO}\*.*" +2
    CreateDirectory "22222"
   IfFileExists "${FILE_DIR}\${FILE}" 0 +7
    FileWrite $UninstLog "${FILE_DIR}\${FILE}$\r$\n"
    FileWrite $UninstLog "${BACKUP_TO}\${FILE}$\r$\n"
    FileWrite $UninstLog "FileBackup$\r$\n"
    Rename "${FILE_DIR}\${FILE}" "${BACKUP_TO}\${FILE}"
    SetOutPath "${FILE_DIR}"
    File "${FILE}" #After the Original file is backed up write the new file.
  !macroend
 
;RestoreFiles macro
  !macro RestoreFiles BUP_FILE RESTORE_FILE
   IfFileExists "${BUP_FILE}" 0 +2
    CopyFiles "${BUP_FILE}" "${RESTORE_FILE}"
  !macroend