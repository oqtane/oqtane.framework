# Oqtane Framework
Oqtane is a Modular Application Framework for Blazor

![Oqtane](https://github.com/oqtane/framework/blob/master/oqtane.png?raw=true "Oqtane")

Oqtane uses Blazor, a new web framework for .NET Core that lets you build interactive web UIs using C# instead of JavaScript. Blazor apps are composed of reusable web UI components implemented using C#, HTML, and CSS. Both client and server code is written in C#, allowing you to share code and libraries.

**To get started with Oqtane:**

   1.&nbsp;Oqtane is currently compatible with **[.NET Core 3.0 Preview 4 SDK (3.0.100-preview4-011223)](https://dotnet.microsoft.com/download/dotnet-core/3.0)**. Microsoft continues to release new versions of .NET Core 3.0 on a regular basis and we do our best to keep up; however, for the best results you should use the most compatible .NET Core 3.0 version.
   
   2.&nbsp;Install the latest **Preview** edition of [Visual Studio 2019](https://visualstudio.com/preview) with the **ASP.NET and web development** workload.

   3.&nbsp;Install the latest [Blazor extension](https://go.microsoft.com/fwlink/?linkid=870389) from the Visual Studio Marketplace. 

   4.&nbsp;Enable Visual Studio to use preview SDKs: Open **Tools** > **Options** in the menu bar. Open the **Projects and Solutions** node. Open the **.NET Core** tab. Check the box for **Use previews of the .NET Core SDK**. Select **OK**. Note that this option may no longer be applicable in newer versions of Visual Studio 2019 Preview edition.

   5.&nbsp;Download or Clone the Oqtane source code to your local system. Open the **Oqtane.sln** solution file. If you want to develop using **server-side** Blazor ( which includes a full debugging experience in Visual Studio ) you should choose to Build the solution using the default Debug configuration. If you want to develop using **client-side** Blazor ( WebAssembly ) you should first choose the "Wasm" configuration option in the Visual Studio toolbar and then Build.

# Background
Oqtane was created by [Shaun Walker](https://www.linkedin.com/in/shaunbrucewalker/) and is inspired by the DotNetNuke web application framework. Initially created as a proof of concept, Oqtane is a native Blazor application written from the ground up using modern .NET Core technology. It is a modular framework offering a fully dynamic page compositing model, multi-site support, designer friendly templates ( skins ), and extensibility via third party modules.

At this point Oqtane offers a minimum of desired functionality and is not recommended for production usage. The expectation is that Oqtane will rapidly evolve as a community driven open source project. At this point in time we do not promise any upgrade path from one version to the next, and developers should expect breaking changes as the framework stabilizes.

# Release Announcement

[Announcing Oqtane... a Modular Application Framework for Blazor!](https://www.oqtane.org/Resources/Blog/PostId/520/announcing-oqtane-a-modular-application-framework-for-blazor)

# Example Screenshots

A simplistic login flow ( note that a full authentication story has not been implemented at this point ):

![Login](https://github.com/oqtane/framework/blob/master/screenshot1.png?raw=true "Login")

Main view for authorized users, allowing full management of modules and content:

![Admin View](https://github.com/oqtane/framework/blob/master/screenshot2.png?raw=true "Admin View")

Content editing user experience using modal dialog:

![Edit Content](https://github.com/oqtane/framework/blob/master/screenshot3.png?raw=true "Edit Content")

Context menu for managing specific module on page:

![Manage Module](https://github.com/oqtane/framework/blob/master/screenshot4.png?raw=true "Manage Module")

Control panel for adding, editing, and deleting pages as well as adding new modules to a page:

![Manage Page](https://github.com/oqtane/framework/blob/master/screenshot5.png?raw=true "Manage Page")

