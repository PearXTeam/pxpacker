!include MUI.nsh

!define appname "$pxpName"
!define size $pxpSize
!define icon "$pxpIcon"
!define fullicon "$pxpFullIcon"
!define company "$pxpCompany"
!define url "$pxpUrl"
!define version "$pxpVer"

!define MUI_ICON ${fullicon}

Name ${appname}
Icon ${fullicon}
InstallDir "$PROGRAMFILES64\${appname}"
OutFile "$pxpOut"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Russian"

Section "${appname}"
 SectionIn RO
 #SetOutPath $INSTDIR
 #File *
 $pxpFiles
 WriteUninstaller $INSTDIR\uninst.exe

 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "DisplayName" "${appname}"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "UninstallString" "$\"$INSTDIR\uninst.exe$\""
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "QuietUninstallString" "$\"$INSTDIR\uninst.exe$\" /S"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "InstallLocation" "$\"$INSTDIR$\""
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "DisplayIcon" "$INSTDIR\${icon}"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "Publisher" "${company}"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "HelpLink" "${url}"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "URLUpdateInfo" "${url}"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "URLInfoAbout" "${url}"
 WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "DisplayVersion" "${version}"
 WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "NoModify" 1
 WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "NoRepair" 1
 WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}" "EstimatedSize" ${size}

#CreateShortCut "$DESKTOP\${appname}.lnk" "$INSTDIR\${short}" "${args}" "$INSTDIR\${icon}" 0
$pxpShortcuts
SectionEnd

Section "Uninstall"
RMDir /r $INSTDIR
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${appname}"
SectionEnd