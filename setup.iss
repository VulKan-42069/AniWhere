[Setup]
AppName=Anime
AppVersion=1.0.0
DefaultDirName={autopf}\Anime
DefaultGroupName=Anime
UninstallDisplayIcon={app}\MALStatus.exe
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir=Output
OutputBaseFilename=AnimeSetup
SetupIconFile=MALDiscordPresence\Assets\icon.ico
PrivilegesRequired=lowest

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "MALDiscordPresence\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Anime"; Filename: "{app}\MALStatus.exe"
Name: "{autodesktop}\Anime"; Filename: "{app}\MALStatus.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\MALStatus.exe"; Description: "{cm:LaunchProgram,Anime}"; Flags: nowait postinstall skipifsilent
