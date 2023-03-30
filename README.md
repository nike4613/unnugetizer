# UnNuGetizer [![update](https://github.com/nike4613/unnugetizer/actions/workflows/update.yml/badge.svg)](https://github.com/nike4613/unnugetizer/actions/workflows/update.yml) [![Version](https://img.shields.io/nuget/vpre/UnNuGetizer.svg?color=royalblue)](https://www.nuget.org/packages/UnNuGetizer)

A "fork" of [NuGetizer](https://github.com/devlooped/nugetizer) which removes SponsorLink.

SponsorLink is a closed-source, obfuscated, Roslyn analyzer which, during the compiler invocation, performs
a network request to check if the user is sponsoring the developer. If the user is not (or does not have
everything set up so that request is possible) then it issues a warning, and *pauses the build for up to 4
seconds*. It is supposed to only do this in IDE builds (which is bad enough) but experimentally, it also
appears to do this for command line builds. In one case ([MonoMod](https://github.com/MonoMod/MonoMod))
this brings the build time from around 2:30 to 7:30.

The scripts in this repository automatically check the NuGetizer repository for new releases, and when one
exists, it fetches that release, applies the patch, builds it, and publishes it to NuGet as
[UnNuGetizer](https://www.nuget.org/packages/UnNuGetizeri) and 
[dotnet-unnugetize](https://www.nuget.org/packages/dotnet-unnugetize).


