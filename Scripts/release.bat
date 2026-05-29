@echo off
setlocal enabledelayedexpansion

::============================================================================
:: NetPulse Release Pipeline (single-file)
::
:: One command:
::   - resolves/sets a new version in Directory.Build.props
::   - publishes BOTH win-x64 + win-arm64 self-contained single-file EXEs
::     to the (tracked) releases\ folder
::   - rewrites the README version line + direct-download table
::   - git add / commit / tag / push so the EXEs are immediately
::     downloadable from the GitHub repo page
::
:: Usage:
::   release.bat                 - rebuild current version
::   release.bat 1.2.3           - set explicit version, then release
::   release.bat patch           - bump 0.1.0 -> 0.1.1
::   release.bat minor           - bump 0.1.0 -> 0.2.0
::   release.bat major           - bump 0.1.0 -> 1.0.0
::   release.bat <ver> --no-push - skip "git push" (commit locally only)
::============================================================================

set "REPO_ROOT=%~dp0.."
for %%I in ("%REPO_ROOT%") do set "REPO_ROOT=%%~fI"

set "PROJECT=%REPO_ROOT%\src\NetPulse\NetPulse.csproj"
set "PROPS_FILE=%REPO_ROOT%\Directory.Build.props"
set "README=%REPO_ROOT%\README.md"
set "PUBLISH_ROOT=%REPO_ROOT%\publish"
set "RELEASES_DIR=%REPO_ROOT%\releases"
set "SELF=%~f0"

set "ARG=%~1"
set "DO_PUSH=1"
if /i "%~2"=="--no-push" set "DO_PUSH=0"
if /i "%~1"=="--no-push" ( set "DO_PUSH=0" & set "ARG=%~2" )
if /i "%ARG%"=="-h"     goto usage
if /i "%ARG%"=="--help" goto usage
if /i "%ARG%"=="help"   goto usage

call :require_tool dotnet || goto fail
call :require_tool git    || goto fail

pushd "%REPO_ROOT%" >nul
git rev-parse --git-dir >nul 2>&1
if errorlevel 1 (
    echo ERROR: %REPO_ROOT% is not a git repository.
    popd >nul
    goto fail
)
popd >nul

if not exist "%PROJECT%"    ( echo ERROR: Project not found: %PROJECT%    & goto fail )
if not exist "%PROPS_FILE%" ( echo ERROR: Props not found:   %PROPS_FILE% & goto fail )
if not exist "%README%"     ( echo ERROR: README not found:  %README%     & goto fail )

for /f "tokens=3 delims=<>" %%a in ('findstr "<Version>" "%PROPS_FILE%"') do set "CURRENT=%%a"
if not defined CURRENT (
    echo ERROR: Could not read ^<Version^> from %PROPS_FILE%.
    goto fail
)

call :resolve_version "%ARG%" "%CURRENT%" || goto fail

echo.
echo === NetPulse release: v!VERSION!  ^(previous: v%CURRENT%^) ===
echo.

if not "!CURRENT!"=="!VERSION!" (
    call :write_version "%CURRENT%" "!VERSION!" || goto fail
) else (
    echo Version unchanged ^(v!VERSION!^).
)

if not exist "%RELEASES_DIR%" mkdir "%RELEASES_DIR%"

call :publish_one win-x64   || goto fail
call :publish_one win-arm64 || goto fail

echo.
echo --- Updating README.md ---
call :update_readme || goto fail

echo.
echo --- Committing release artifacts ---
pushd "%REPO_ROOT%" >nul
git add -- "Directory.Build.props" "README.md" >nul
git add -f -- "releases/NetPulse-win-x64.exe"          "releases/NetPulse-win-x64.exe.sha256"   >nul 2>&1
git add -f -- "releases/NetPulse-win-arm64.exe"        "releases/NetPulse-win-arm64.exe.sha256" >nul 2>&1

git diff --cached --quiet
if errorlevel 1 (
    git commit -m "release: v!VERSION!"
    if errorlevel 1 (
        echo ERROR: git commit failed.
        popd >nul
        goto fail
    )
    git tag "v!VERSION!" >nul 2>&1
    if errorlevel 1 (
        echo NOTE: tag v!VERSION! already exists, not retagged.
    ) else (
        echo Created tag v!VERSION!.
    )
) else (
    echo Nothing to commit ^(working tree clean^).
)

if "%DO_PUSH%"=="1" (
    echo.
    echo --- Pushing to origin ---
    git push
    if errorlevel 1 (
        echo WARNING: git push failed. Resolve manually, then run:
        echo   git push ^&^& git push origin v!VERSION!
        popd >nul
        goto fail
    )
    git push origin "v!VERSION!" >nul 2>&1
    echo Pushed commit and tag v!VERSION!.
) else (
    echo.
    echo Skipping push ^(--no-push^). To publish manually:
    echo   git push ^&^& git push origin v!VERSION!
)
popd >nul

echo.
echo === Release v!VERSION! complete ===
echo Latest EXEs are now downloadable from the GitHub repo:
echo   releases\NetPulse-win-x64.exe
echo   releases\NetPulse-win-arm64.exe
goto :final

:fail
endlocal
exit /b 1

:final
endlocal
exit /b 0

::----------------------------------------------------------------------------
:require_tool
%~1 --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: %~1 not found on PATH.
    exit /b 1
)
exit /b 0

::----------------------------------------------------------------------------
:: Resolve version argument into !VERSION!.
::   %1 = user-supplied arg (may be empty, X.Y.Z, patch, minor, major)
::   %2 = current version
:resolve_version
set "_ARG=%~1"
set "_CUR=%~2"

if "%_ARG%"=="" (
    set "VERSION=%_CUR%"
    exit /b 0
)

for /f "tokens=1,2,3 delims=." %%a in ("%_CUR%") do (
    set "_M=%%a"
    set "_N=%%b"
    set "_P=%%c"
)

if /i "%_ARG%"=="major" ( set /a _M+=1 & set "_N=0" & set "_P=0" & set "VERSION=!_M!.!_N!.!_P!" & exit /b 0 )
if /i "%_ARG%"=="minor" ( set /a _N+=1 & set "_P=0" & set "VERSION=!_M!.!_N!.!_P!" & exit /b 0 )
if /i "%_ARG%"=="patch" ( set /a _P+=1 & set "VERSION=!_M!.!_N!.!_P!" & exit /b 0 )

echo %_ARG%| findstr /r /c:"^[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*$" >nul
if errorlevel 1 (
    echo ERROR: "%_ARG%" is not a valid version. Use X.Y.Z or patch/minor/major.
    exit /b 1
)
set "VERSION=%_ARG%"
exit /b 0

::----------------------------------------------------------------------------
:: Rewrite all 4 version fields in Directory.Build.props (UTF-8, no BOM).
:write_version
set "_OLD=%~1"
set "_NEW=%~2"
echo Bumping version: %_OLD% -^> %_NEW%
set "NP_PROPS=%PROPS_FILE%"
set "NP_OLD=%_OLD%"
set "NP_NEW=%_NEW%"

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$p=$env:NP_PROPS; $old=$env:NP_OLD; $new=$env:NP_NEW; " ^
  "$c=[System.IO.File]::ReadAllText($p,[System.Text.Encoding]::UTF8); " ^
  "$c=$c -replace [regex]::Escape(\"<InformationalVersion>$old</InformationalVersion>\"), \"<InformationalVersion>$new</InformationalVersion>\"; " ^
  "$c=$c -replace [regex]::Escape(\"<Version>$old</Version>\"),                         \"<Version>$new</Version>\"; " ^
  "$c=$c -replace [regex]::Escape(\"<FileVersion>$old.0</FileVersion>\"),               \"<FileVersion>$new.0</FileVersion>\"; " ^
  "$c=$c -replace [regex]::Escape(\"<AssemblyVersion>$old.0</AssemblyVersion>\"),       \"<AssemblyVersion>$new.0</AssemblyVersion>\"; " ^
  "[System.IO.File]::WriteAllText($p,$c,(New-Object System.Text.UTF8Encoding $false))"

if errorlevel 1 (
    echo ERROR: Failed to update %PROPS_FILE%
    exit /b 1
)
exit /b 0

::----------------------------------------------------------------------------
:: Publish one architecture and copy single-file EXE into releases\.
:publish_one
set "_RID=%~1"
set "_OUT=%PUBLISH_ROOT%\%_RID%"
set "_DEST=%RELEASES_DIR%\NetPulse-%_RID%.exe"

echo.
echo --- Publishing %_RID% ---
if exist "%_OUT%" rmdir /s /q "%_OUT%"
mkdir "%_OUT%"

dotnet publish "%PROJECT%" ^
    -c Release -r %_RID% ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishReadyToRun=true ^
    -p:DebugType=embedded ^
    -o "%_OUT%"
if errorlevel 1 (
    echo ERROR: dotnet publish failed for %_RID%.
    exit /b 1
)
if not exist "%_OUT%\NetPulse.exe" (
    echo ERROR: Build output missing: %_OUT%\NetPulse.exe
    exit /b 1
)

copy /y "%_OUT%\NetPulse.exe" "%_DEST%" >nul
if errorlevel 1 (
    echo ERROR: Failed to copy %_OUT%\NetPulse.exe to %_DEST%
    exit /b 1
)

for %%I in ("%_DEST%") do set "_SZ=%%~zI"
set /a _SZ_MB=!_SZ! / 1048576

set "NP_EXE=%_DEST%"
set "NP_RID=%_RID%"
powershell -NoProfile -Command ^
  "$h=(Get-FileHash -Algorithm SHA256 $env:NP_EXE).Hash.ToLower(); " ^
  "Set-Content -Path ($env:NP_EXE + '.sha256') -Value (\"$h  NetPulse-$($env:NP_RID).exe\") -Encoding ASCII"

echo   -^> NetPulse-%_RID%.exe ^(!_SZ_MB! MB^)
exit /b 0

::----------------------------------------------------------------------------
:: Extract the embedded PowerShell (lines starting with "::ps:") into a
:: temp .ps1 file and run it. Keeps everything in this single .bat while
:: avoiding CMD/PowerShell quoting pain.
:update_readme
set "NP_README=%README%"
set "NP_RELEASES=%RELEASES_DIR%"
set "NP_VERSION=!VERSION!"
set "NP_TMP=%TEMP%\netpulse-update-readme-%RANDOM%-%TIME:~6,2%%TIME:~9,2%.ps1"

set "NP_SELF=%SELF%"
set "NP_OUT=%NP_TMP%"
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$src = [System.IO.File]::ReadAllLines($env:NP_SELF, [System.Text.Encoding]::UTF8); " ^
  "$ps  = $src | Where-Object { $_ -like '::ps:*' } | ForEach-Object { if ($_.Length -gt 5) { $_.Substring(5) } else { '' } }; " ^
  "[System.IO.File]::WriteAllLines($env:NP_OUT, $ps, (New-Object System.Text.UTF8Encoding $false))"
if errorlevel 1 (
    echo ERROR: Failed to extract embedded PowerShell.
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%NP_TMP%"
set "_RC=%errorlevel%"
del "%NP_TMP%" >nul 2>&1
if not "%_RC%"=="0" (
    echo ERROR: README update failed.
    exit /b 1
)
exit /b 0

::----------------------------------------------------------------------------
:usage
echo.
echo NetPulse release pipeline ^(single-file^)
echo.
echo Usage: release.bat [version^|patch^|minor^|major] [--no-push]
echo.
echo Examples:
echo   release.bat                   Rebuild current version, commit, push
echo   release.bat 1.2.3             Set version to 1.2.3, build, commit, push
echo   release.bat patch             Bump patch (e.g. 0.1.0 -^> 0.1.1)
echo   release.bat minor             Bump minor (e.g. 0.1.0 -^> 0.2.0)
echo   release.bat major             Bump major (e.g. 0.1.0 -^> 1.0.0)
echo   release.bat patch --no-push   Bump and commit locally; do not push
echo.
echo Pipeline:
echo   1. Resolve / set version  ^(Directory.Build.props^)
echo   2. Publish self-contained single-file EXEs:
echo        publish\win-x64\NetPulse.exe   -^> releases\NetPulse-win-x64.exe
echo        publish\win-arm64\NetPulse.exe -^> releases\NetPulse-win-arm64.exe
echo      ^(SHA-256 checksums written as *.sha256^)
echo   3. Update README.md  ^(version line + direct-download table^)
echo   4. git add Directory.Build.props, README.md, releases\*.exe ^(+.sha256^)
echo   5. git commit -m "release: vX.Y.Z" and tag vX.Y.Z
echo   6. git push  ^(unless --no-push^)
echo.
endlocal
exit /b 0

::============================================================================
:: Embedded PowerShell script (extracted by :update_readme at runtime).
:: CMD never executes these "::ps:" lines because they sit after the final
:: "endlocal & exit /b 0" above. Edit freely as normal PowerShell.
::============================================================================
::ps:$ErrorActionPreference = 'Stop'
::ps:
::ps:$path = $env:NP_README
::ps:$rel  = $env:NP_RELEASES
::ps:$v    = $env:NP_VERSION
::ps:$date = (Get-Date).ToString('yyyy-MM-dd')
::ps:$nl   = [char]13 + [char]10
::ps:
::ps:function Get-AssetInfo([string]$fileName) {
::ps:    $f = Join-Path $rel $fileName
::ps:    if (Test-Path $f) {
::ps:        $sizeMb = [math]::Round(((Get-Item $f).Length) / 1MB, 1)
::ps:        $sha    = (Get-FileHash -Algorithm SHA256 $f).Hash.ToLower()
::ps:        return @{ Size = "$sizeMb MB"; Sha = $sha }
::ps:    }
::ps:    return @{ Size = 'pending build'; Sha = '' }
::ps:}
::ps:
::ps:$x64   = Get-AssetInfo 'NetPulse-win-x64.exe'
::ps:$arm64 = Get-AssetInfo 'NetPulse-win-arm64.exe'
::ps:
::ps:$versionBlock = @"
::ps:<!-- VERSION:START -->
::ps:**Current release: v$v** &nbsp;&middot;&nbsp; Released $date &nbsp;&middot;&nbsp; [Direct download](#direct-download-from-repository)
::ps:<!-- VERSION:END -->
::ps:"@
::ps:
::ps:$downloadBlock = @"
::ps:<!-- DIRECT_DOWNLOAD:START -->
::ps:### Direct download from repository
::ps:
::ps:Files in this repo's ``releases/`` folder are refreshed by ``Scripts\release.bat`` and always reflect the latest committed build (currently **v$v**).
::ps:
::ps:| Architecture | Direct download | Approx. size | SHA-256 |
::ps:| :---: | :---: | :---: | :--- |
::ps:| **Windows x64** | [NetPulse-win-x64.exe](releases/NetPulse-win-x64.exe) | $($x64.Size) | ``$($x64.Sha)`` |
::ps:| **Windows ARM64** | [NetPulse-win-arm64.exe](releases/NetPulse-win-arm64.exe) | $($arm64.Size) | ``$($arm64.Sha)`` |
::ps:
::ps:Checksum files are committed beside each EXE as ``*.sha256``. Older versions live on the [GitHub Releases page](https://github.com/dipak-katariya/NetPulse/releases).
::ps:<!-- DIRECT_DOWNLOAD:END -->
::ps:"@
::ps:
::ps:$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
::ps:
::ps:$versionPattern = '(?s)<!-- VERSION:START -->.*?<!-- VERSION:END -->'
::ps:if ([regex]::IsMatch($content, $versionPattern)) {
::ps:    $content = [regex]::Replace($content, $versionPattern, { param($m) $versionBlock })
::ps:} else {
::ps:    $i = $content.IndexOf('</div>')
::ps:    if ($i -ge 0) {
::ps:        $j = $content.IndexOf([char]10, $i) + 1
::ps:        $content = $content.Insert($j, $nl + $versionBlock + $nl)
::ps:    }
::ps:}
::ps:
::ps:$downloadPattern = '(?s)<!-- DIRECT_DOWNLOAD:START -->.*?<!-- DIRECT_DOWNLOAD:END -->'
::ps:if ([regex]::IsMatch($content, $downloadPattern)) {
::ps:    $content = [regex]::Replace($content, $downloadPattern, { param($m) $downloadBlock })
::ps:} else {
::ps:    $anchor = '### First run'
::ps:    $k = $content.IndexOf($anchor)
::ps:    if ($k -ge 0) {
::ps:        $content = $content.Insert($k, $downloadBlock + $nl + $nl)
::ps:    } else {
::ps:        $content += $nl + $nl + $downloadBlock
::ps:    }
::ps:}
::ps:
::ps:$utf8NoBom = New-Object System.Text.UTF8Encoding $false
::ps:[System.IO.File]::WriteAllText($path, $content, $utf8NoBom)
::ps:
::ps:Write-Host "README.md updated:"
::ps:Write-Host "  Version : v$v"
::ps:Write-Host "  Date    : $date"
::ps:Write-Host "  x64     : $($x64.Size)"
::ps:Write-Host "  arm64   : $($arm64.Size)"
