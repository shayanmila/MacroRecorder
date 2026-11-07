#ifndef AppVersion
#define AppVersion "1.0.0"
#endif
[Setup]
AppId={{27DC59B0-87BC-465E-A266-05BCBE02D82A}
AppName=Masoom Macro Recorder
AppVersion={#AppVersion}
AppPublisher=MasoomDev
AppPublisherURL=https://github.com/shayanmila/MacroRecorder
AppSupportURL=https://github.com/shayanmila/MacroRecorder/issues
AppUpdatesURL=https://github.com/shayanmila/MacroRecorder/releases
VersionInfoVersion={#AppVersion}
LicenseFile=LICENSE.txt
InfoBeforeFile=PRIVACY.txt
DisableDirPage=no
DisableProgramGroupPage=no
UsePreviousAppDir=yes
DirExistsWarning=auto
ShowLanguageDialog=no
UninstallDisplayName=Masoom Macro Recorder
CloseApplications=yes
RestartApplications=no
DefaultDirName={localappdata}\Programs\Masoom Macro Recorder
DefaultGroupName=Masoom Macro Recorder
AllowNoIcons=yes
OutputDir=..\dist\installer
OutputBaseFilename=Masoom-Macro-Recorder-{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\MacroRecorder\Assets\MasoomMacro.ico
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\MacroRecorder.exe
[Files]
Source: "..\dist\portable\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\Masoom Macro Recorder"; Filename: "{app}\MacroRecorder.exe"
Name: "{autodesktop}\Masoom Macro Recorder"; Filename: "{app}\MacroRecorder.exe"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcut"; GroupDescription: "Additional shortcuts:"
[Run]
Filename: "{app}\MacroRecorder.exe"; Description: "Launch Masoom Macro Recorder"; Flags: nowait postinstall skipifsilent