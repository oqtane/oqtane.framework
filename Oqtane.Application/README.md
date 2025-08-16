# Oqtane Application Template

This folder contains content files for a Visual Studio Project Template designed for Oqtane development projects. The template relies on the native templating capabilities of the .NET Command Line Interface (CLI):

```
dotnet new install Oqtane.Application.Template
dotnet new oqtane-app -o MyCompany.MyProject
```

When using this approach you do not need to have a local copy of the oqtane.framework source code - you simply utilize Oqtane as a standard application dependency.

The solution contains an AppHost project which must be identified as the Startup project. It is responsible for loading the development environment and launching the Oqtane framework.

The solution also contains Build, Client, Server, and Shared folders which is where you you would implement your custom functionality. An example module and theme are included for reference, and you can add additional modules and themes within the same projects by following the standard Oqtane folder/namespace conventions. 

*Known Issues*

- do not use the term "Oqtane" in your output name or else you will experience namespace conflicts
- the application's *.nuspec file should be included in the *.nupkg package however Nuget is excluding it
- when calling "dotnet new" the PostBuild section in the Oqtane.Application.Build.csproj is being modified - not sure why
