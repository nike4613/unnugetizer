$baseDir = $PSScriptRoot
pushd $baseDir

# first, lets make sure we're behaving ourselves and wait for our ratelimit, if any

# get currently authenticated user rate limit info
$rate = gh api rate_limit | convertfrom-json | select -expandproperty rate
# if we don't have at least 100 requests left, wait until reset
if ($rate.remaining -lt 10) {
  $wait = ($rate.reset - (Get-Date (Get-Date).ToUniversalTime() -UFormat %s))
  echo "Rate limit remaining is $($rate.remaining), waiting for $($wait / 1000) seconds to reset"
  sleep $wait
  $rate = gh api rate_limit | convertfrom-json | select -expandproperty rate
  echo "Rate limit has reset to $($rate.remaining) requests"
}

$result = &{
  # now, lets grab our victim's list of versions
  $versions = gh api repos/devlooped/nugetizer/releases |
    ConvertFrom-Json |
    Sort-Object published_at |
    Select -ExpandProperty tag_name
  if (!$?) { return -1 }
  
  # we'll write that list out to a file which we use for change tracking
  $versions > versions.txt
	
  # check if the versions list has changed
  git diff -w --quiet --exit-code -- versions.txt
  if ($?) { return $false } # if it returns 0, the list hasn't changed

  # the list has changed, so we should build the most recent (last) element of the versions list
  [Array]::Reverse($versions)
  $ver = $versions[0]
	
  # execute the build
  &"$baseDir/do_build.ps1" -VersionFromTag -Commit $ver
  if (!$?) { return $LastExitCode }
	
  return $true
}

popd

return $result
