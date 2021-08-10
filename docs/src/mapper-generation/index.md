AgileMapper.Buildable is an AgileMapper extension which uses a custom MSBuild target to generate
mapper source code at build time. This provides the best mapping performance, and enables you to step
into and debug your mappers at runtime.

It targets .NET 4.6.1+ and [.NET Standard 2.0+](https://docs.microsoft.com/en-us/dotnet/articles/standard/library),
and supports [dotnet build](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build) and 
[MSBuild](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild), 
[SDK](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview) and non-SDK projects.

[Available via NuGet](https://www.nuget.org/packages/AgileObjects.AgileMapper.Buildable) and licensed
with the [MIT licence](https://github.com/agileobjects/AgileMapper/blob/master/LICENCE.md), you can
install it via the [package manager console](https://docs.nuget.org/consume/package-manager-console)
with:

```shell
PM> Install-Package AgileObjects.AgileMapper.Buildable -Pre
```

The `-Pre` flag is required as AgileMapper.Buildable is currently in preview.