# <img src="/src/icon.png" height="30px"> ObjectApproval

[![Build status](https://ci.appveyor.com/api/projects/status/qt5bqw30vp7ywgh3/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/ObjectApproval)
[![NuGet Status](https://img.shields.io/nuget/v/ObjectApproval.svg?cacheSeconds=86400)](https://www.nuget.org/packages/ObjectApproval/)

Extends [ApprovalTests](https://github.com/approvals/ApprovalTests.Net) to allow simple approval of complex models using [Json.net](https://www.newtonsoft.com/json).

toc


## Release Notes

See [closed milestones](../../milestones?state=closed).


## Diff Tool

 * `VerifyDiffProcess`: The process name. Short name if the tool exists in the current path, otherwise the full path.
 * `VerifyDiffArguments`: The argument syntax to pass to the process. Must contain the strings `{receivedPath}` and `{verifiedPath}`.


### Visual Studio

```
setx VerifyDiffProcess "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"
setx VerifyDiffArguments "/diff {receivedPath} {verifiedPath}"
```


## Icon

[Helmet](https://thenounproject.com/term/helmet/9554/) designed by [Leonidas Ikonomou](https://thenounproject.com/alterego) from [The Noun Project](https://thenounproject.com).