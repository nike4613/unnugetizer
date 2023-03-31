param (
  $Commit = $null,
  $Version = $null,
  [Switch] $VersionAsTag = $false,
  [Switch] $VersionFromTag = $false
)

$devloopUrl = "https://github.com/devlooped/nugetizer.git"
$baseDir = $PSScriptRoot

if ($VersionAsTag) {
  if ($Version -eq $null) {
    Write-Error "Version must be specified if VersionAsTag is used"
    return 2
  }
  $Commit = "v$Version"
} elseif ($VersionFromTag) {
  if ($Commit -eq $null) {
    Write-Error "Commit must be specified if VersionFromTag is used"
    return 4
  }
  if ($Commit -match "v(\d\.\d\.\d.*)") {
    $Version = $Matches[1]
  } else {
    Write-Error "Commit does not appear to be a typical version tag"
    return 4
  }
}

if ($Commit -ne $null -and $Version -ne $null) {
  Write-Host "Pulling commit $Commit and building as version $Version"
} elseif ($Commit -ne $null) {
  Write-Host "Pulling commit $Commit and building with default version"
} elseif ($Versuib -ne $null) {
  Write-Host "Pulling main branch and building as version $Version"
} else {
  Write-Host "Pulling main branch and building with default version"
}

pushd $baseDir

function Fetch-Commit {
  param (
    $Commit = $null
  )
	
  $x = git status -s
  #write-host "status" $?
  if (!$?) { return $false }
  git remote set-url origin $devloopUrl | Out-Host
  #write-host "remote set-url" $?
  if (!$?) { return $false }
	
  $fetchArgs = @("-ptf", "--depth=1")
  if ($Commit -ne $null) {
    git fetch origin $Commit $fetchArgs | Out-Host
  } else {
    git pull origin main $fetchArgs | Out-Host
  }
  #write-host "fetch" $?
  if (!$?) { return $false }
  return $true
}

$shouldClone = $true;
if (Test-Path -PathType Container .repo) {
  pushd .repo
  $didFetch = Fetch-Commit -Commit $Commit
  $shouldClone = !$didFetch
  popd
  #write-host "shouldClone" $shouldClone
  if ($shouldClone) {
    rm -r -force .repo
  }
}

if ($shouldClone) {
  Write-Host initing repo
  git init -q .repo
  pushd .repo
  git remote add origin $devloopUrl | Out-Host
  $didFetch = Fetch-Commit -Commit $Commit
  popd
  if (!$didFetch) {
    Write-Error "Could not fetch requested commit"
    return 1
  }
}

pushd .repo
$result = &{
  if ($Commit -ne $null) {
    git checkout -f --detach $Commit | Out-Host
    if (!$?) { return -4 } # exit early
  }
	
  git apply ../unsponsor.patch --ignore-whitespace --recount | Out-Host
  if (!$?) { return -3 } # exit early

  $props = @("-c","Release","-p:BuildPackageBaseName=UnNuGetizer","-p:BuildPackageBaseName2=unnugetize")
  $props += @("-p:Repository=https://github.com/nike4613/unnugetizer")
  if ($Version -ne $null) {
    $props += @("-p:Version=$Version")
  }
	
  dotnet build $props | Out-Host
  if (!$?) { return -1 } # exit early
  dotnet pack $props | Out-Host
  if (!$?) { return -2 } # exit early
}
popd

popd

return $result
