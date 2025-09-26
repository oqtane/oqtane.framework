# Oqtane Application Template

This is a Visual Studio Project Template designed for Oqtane development projects. This template relies on the native templating capabilities of the .NET Command Line Interface (CLI):

```
dotnet new install Oqtane.Application.Template
dotnet new oqtane-app -o MyCompany.MyProject
cd MyCompany.MyProject
dotnet build
cd Server
dotnet run
browse to Url
```

When using this approach you do not need to have a local copy of the oqtane.framework source code - you simply utilize Oqtane as a standard application dependency.

The solution also contains Client, Server, and Shared folders which is where you you would implement your custom functionality. An example module and theme are included for reference, and you can add additional modules and themes within the same projects by following the standard Oqtane folder/namespace conventions. 

*Known Issues*

- do not use the term "Oqtane" or "Module" in your output name or else you will experience namespace conflicts

