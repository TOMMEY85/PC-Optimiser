#define MyAppName "PC Optimizer"
#define MyAppVersion "7.0.0"
#define MyAppExeName "PCOptimizer.exe"
#define PublishDir "..\PCOptimizer.App\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{8FB27C74-89A2-4CDD-BC68-64B829BC2E15}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\PC Optimizer
DefaultGroupName=PC Optimizer
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=PCOptimizer-Setup-v7.0
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName=PC Optimizer v7.0
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
RestartApplications=no
SetupLogging=yes

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le Bureau"; GroupDescription: "Raccourcis :"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\PC Optimizer"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\PC Optimizer"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\PCOptimizer-v6.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Lancer PC Optimizer v7.0"; Flags: nowait postinstall skipifsilent runascurrentuser
