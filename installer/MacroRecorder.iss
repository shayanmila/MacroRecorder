#ifndef AppVersion
#define AppVersion "1.0.0"
#endif
[Setup]
AppId={{27DC59B0-87BC-465E-A266-05BCBE02D82A}
AppName=Masoom Macro Recorder
AppVersion={#AppVersion}
AppPublisher=MasoomDev
DefaultDirName={autopf}\Masoom Macro Recorder
DefaultGroupName=Masoom Macro Recorder
OutputDir=..\dist\installer
OutputBaseFilename=Masoom-Macro-Recorder-{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
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