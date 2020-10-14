#define MyAppName "Playnite"
#define MyAppVersion "{version}"
#define MyAppPublisher "Josef Nemec"
#define MyAppURL "https://playnite.link"
#define MyAppExeName "Playnite.DesktopApp.exe"
#define SourcePath "{source_path}"
#define PlayniteAppMutex = "PlayniteInstaceMutex"

[Setup]
AppId={#MyAppName}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppMutex={#PlayniteAppMutex}
DefaultDirName={localappdata}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir="{out_dir}"
OutputBaseFilename="{out_file_name}"
SetupIconFile=..\source\Playnite.DesktopApp\Themes\Desktop\Default\Images\applogo.ico
Compression=lzma2/max
CompressionThreads=4
LZMANumBlockThreads=4
SolidCompression=yes
PrivilegesRequired=lowest
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppPublisher}
UsePreviousAppDir=yes
DisableDirPage=no
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
Uninstallable=GetCreateUninstaller()

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startmenu"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#SourcePath}\*"; DestDir: "{app}"; Flags: ignoreversion createallsubdirs recursesubdirs; Excludes: "*.log"

[Icons]
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Playnite\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startmenu; Check: GetCreateIcons
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Playnite\Safe Mode"; Filename: "{app}\Safe Mode.bat"; Tasks: startmenu; Check: GetCreateIcons
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Playnite\Uninstall"; Filename: "{uninstallexe}"; Tasks: startmenu; Check: GetCreateIcons
Name: "{commondesktop}\Playnite"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Check: GetCreateIcons

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Playnite"; Flags: nowait postinstall shellexec

[Code]
var
    CreateIcons: Boolean;
    CreateInstaller: Boolean;

function GetCreateUninstaller(): Boolean;
begin
    Result := CreateInstaller;
end;

function GetCreateIcons(): Boolean;
begin
    Result := CreateIcons;
end;

function CmdLineParamExists(const Value: string): Boolean;
var
    I: Integer;  
begin
    Result := False;
    for I := 1 to ParamCount do
        if CompareText(ParamStr(I), Value) = 0 then
        begin
            Result := True;
            Break;
        end;
end;

function InitializeSetup : Boolean;
begin
    Result := True;
    CreateIcons := True;
    CreateInstaller := True;

    { Don't create installer if running in portable mode }
    if (CmdLineParamExists('/PORTABLE') = true) then
    begin
        CreateInstaller := False;
        CreateIcons := False;
    end;

    { Don't re-create icons when updating, user might delete them }
    if (CmdLineParamExists('/UPDATE') = true) then
    begin
        CreateIcons := False;

        { Wait for Playnite process to shutdown first. }
        while CheckForMutexes('{#PlayniteAppMutex}') = true do
        begin
            Sleep(300);
        end;
    end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
    UninstallerPath, UninstallerLogPath, UninstallRegKey, WinmdReferencePath, SdkPath: string;
begin     
    if CurStep = ssPostInstall then
    begin
        { Cleanup old NSIS files and registry keys }
        UninstallerPath := ExpandConstant('{app}\uninstall.exe');
        UninstallerLogPath := ExpandConstant('{app}\uninstall.log');        
        UninstallRegKey := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Playnite';
        WinmdReferencePath := ExpandConstant('{app}\Windows.winmd');
        SdkPath := ExpandConstant('{app}\PlayniteSDK.dll');

        if FileExists(SdkPath) = true then
        begin
            DeleteFile(SdkPath);
        end;

        if FileExists(WinmdReferencePath) = true then
        begin
            DeleteFile(WinmdReferencePath);
        end;

        if FileExists(UninstallerPath) = true then
        begin
            DeleteFile(UninstallerPath);
        end;

        if FileExists(UninstallerLogPath) = true then
        begin
            DeleteFile(UninstallerLogPath);
        end;

        if RegKeyExists(HKEY_LOCAL_MACHINE, UninstallRegKey) = true then
        begin
            RegDeleteKeyIncludingSubkeys(HKEY_LOCAL_MACHINE, UninstallRegKey);
        end;
    end;
end;