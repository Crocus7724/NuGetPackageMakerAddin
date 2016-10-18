using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
               "NuGetPackageMakerAddin",
               Namespace = "NuGetPackageMakerAddin",
               Version = "1.0"
           )]
[assembly: AddinName("NuGetPackageMakerAddin")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("NuGetPackageMakerAddin")]
[assembly: AddinAuthor("Yamamoto")]