[Setup]
AppName=Ani-Where
AppVersion=1.0.0
DefaultDirName={autopf}\Ani-Where
DefaultGroupName=Ani-Where
UninstallDisplayIcon={app}\Ani-Where.exe
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir=Output
OutputBaseFilename=AniWhereSetup
SetupIconFile=MALDiscordPresence\Assets\icon.ico
PrivilegesRequired=lowest

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "MALDiscordPresence\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Ani-Where"; Filename: "{app}\Ani-Where.exe"
Name: "{autodesktop}\Ani-Where"; Filename: "{app}\Ani-Where.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Ani-Where.exe"; Description: "{cm:LaunchProgram,Ani-Where}"; Flags: nowait postinstall skipifsilent
