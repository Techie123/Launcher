!include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Hearthstone Mod"
  OutFile "HearthstoneMod.exe"

  !define MUI_ICON "HearthstoneMod\Launcher\Logo.ico"
  ; !define MUI_UNICON "HearthstoneMod\Launcher\Logo.ico"

  ;Default installation folder
  InstallDir "$LOCALAPPDATA\HearthstoneMod"

  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\HearthstoneMod" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Welcome Page

  !define MUI_WELCOMEPAGE_TEXT "\
  This setup will guide you through the installation of Hearthstone Mod. $\r$\n\
  $\r$\n\
  Click Next to continue."

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY

  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Game Data" S_GameData
  SectionIn RO

  SetOutPath "$INSTDIR"

  ;ADD YOUR OWN FILES HERE...
  File /nonfatal /a /r "HearthstoneMod\"

  ;Store installation folder
  WriteRegStr HKCU "Software\HearthstoneMod" "" $INSTDIR

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
 
  ;write uninstall information to the registry
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\HearthstoneMod" "DisplayName" "Hearthstone Mod"
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\HearthstoneMod' 'DisplayVersion' '1.0.0.0'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\HearthstoneMod' 'DisplayIcon' '$INSTDIR\Launcher\Logo.ico'
  WriteRegStr HKLM 'Software\Microsoft\Windows\CurrentVersion\Uninstall\HearthstoneMod' 'Publisher' 'Hearthstone Mod Team'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\HearthstoneMod" "UninstallString" "$INSTDIR\Uninstall.exe"

SectionEnd

Section "Start Menu Shortcuts" S_StartMenu
  CreateDirectory "$SMPROGRAMS\Hearthstone Mod"
  CreateShortcut "$SMPROGRAMS\Hearthstone Mod\Hearthstone Mod.lnk" "$INSTDIR\Launcher\Launcher.exe"
  CreateShortcut "$SMPROGRAMS\Hearthstone Mod\Uninstall Hearthstone Mod.lnk" "$INSTDIR\Uninstall.exe"
SectionEnd

Section "Desktop Shortcut" S_Desktop
  CreateShortcut "$DESKTOP\Hearthstone Mod.lnk" "$INSTDIR\Launcher\Launcher.exe"
SectionEnd

;--------------------------------
;Descriptions

  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${S_GameData} "The main game data."
    !insertmacro MUI_DESCRIPTION_TEXT ${S_StartMenu} "Start menu shortcuts for the game and the uninstaller."
    !insertmacro MUI_DESCRIPTION_TEXT ${S_Desktop} "Desktop shortcut for the game."
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  RMDir /r "$INSTDIR"
  RMDir /r "$SMPROGRAMS\Hearthstone Mod"
  DeleteRegKey /ifempty HKCU "Software\HearthstoneMod"
  DeleteRegKey HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\HearthstoneMod"  

SectionEnd