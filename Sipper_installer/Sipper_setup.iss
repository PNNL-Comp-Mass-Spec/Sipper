; This is an Inno Setup configuration file
; http://www.jrsoftware.org/isinfo.php

#define ApplicationVersion GetFileVersion('..\Sipper\bin\x86\Debug\Sipper.exe')

[CustomMessages]
AppName=Sipper

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.
; Example with multiple lines:
; WelcomeLabel2=Welcome message%n%nAdditional sentence

[Files]
Source: ..\Sipper\bin\x86\Debug\Sipper.exe                 ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\Sipper.exe.config          ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\alglibnet2.dll             ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\BrukerDataReader.dll       ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\DeconTools.Backend.dll     ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\DeconTools.Workflows.dll   ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\GWSGraphLibrary.dll        ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\IMSCOMP.dll                ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\log4net.dll                ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\Mapack.dll                 ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\MassLynxRaw.dll            ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\MathNet.Numerics.dll       ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\MSDBLibrary.dll            ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\MultiAlignEngine.dll       ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\OxyPlot.dll                ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\OxyPlot.Wpf.dll            ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\PNNLOmics.dll              ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\PRISM.dll                  ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\ProteowizardWrapper.dll    ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\pwiz_bindings_cli.dll      ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\Scinet.ChartControl.dll    ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\Scinet.dll                 ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\System.Data.SQLite.dll     ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\UIMFLibrary.dll            ; DestDir: {app}
Source: ..\Sipper\bin\x86\Debug\ZedGraph.dll               ; DestDir: {app}

Source: ..\README.md                                       ; DestDir: {app}
Source: ..\Library\PNNLOmicsElementData.xml                ; DestDir: {app}

Source: Images\delete_16x.ico                              ; DestDir: {app}

Source: ..\ExampleData\Yellow_C13_070_targets.txt                                   ; DestDir: {app}
Source: ..\ExampleData\Yellow_C13_070_targets_NoDataset.txt                         ; DestDir: {app}
Source: ..\ExampleData\Yellow_C13_070_targets_NoDatasetOrMassTagID.txt              ; DestDir: {app}
Source: ..\ExampleData\SipperTargetedWorkflowParameters_Example.xml                 ; DestDir: {app}
Source: ..\ExampleData\SipperTargetedWorkflowParameters_sum7.xml                    ; DestDir: {app}
Source: ..\ExampleData\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt  ; DestDir: {app}


[Dirs]
Name: {commonappdata}\Sipper; Flags: uninsalwaysuninstall

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
; Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Icons]
; Name: {commondesktop}\MASIC; Filename: {app}\Sipper.exe; Tasks: desktopicon; Comment: MASIC
Name: {group}\Sipper; Filename: {app}\Sipper.exe; Comment: Sipper

[Setup]
AppName=Sipper
AppVersion={#ApplicationVersion}
;AppVerName=Sipper
AppID=SipperId
AppPublisher=Pacific Northwest National Laboratory
AppPublisherURL=http://omics.pnl.gov/software
AppSupportURL=http://omics.pnl.gov/software
AppUpdatesURL=http://omics.pnl.gov/software
ArchitecturesInstallIn64BitMode=x64
DefaultDirName={pf}\Sipper
DefaultGroupName=PAST Toolkit
AppCopyright=� PNNL
;LicenseFile=.\License.rtf
PrivilegesRequired=poweruser
OutputBaseFilename=Sipper_Installer
VersionInfoVersion={#ApplicationVersion}
VersionInfoCompany=PNNL
VersionInfoDescription=Sipper
VersionInfoCopyright=PNNL
DisableFinishedPage=true
ShowLanguageDialog=no
ChangesAssociations=false
EnableDirDoesntExistWarning=false
AlwaysShowDirOnReadyPage=true
UninstallDisplayIcon={app}\delete_16x.ico
ShowTasksTreeLines=true
OutputDir=.\Output

[Registry]
;Root: HKCR; Subkey: MyAppFile; ValueType: string; ValueName: ; ValueDataMyApp File; Flags: uninsdeletekey
;Root: HKCR; Subkey: MyAppSetting\DefaultIcon; ValueType: string; ValueData: {app}\wand.ico,0; Flags: uninsdeletevalue

[UninstallDelete]
Name: {app}; Type: filesandordirs
