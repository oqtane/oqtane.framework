# Oqtane Framework
Oqtane is .NET Core Web Application Framework for Blazor

![Oqtane](https://github.com/oqtane/framework/blob/master/oqtane.png?raw=true "Oqtane")

Oqtane uses Blazor, a new web framework for .NET Core that lets you build interactive web UIs using C# instead of JavaScript. Blazor apps are composed of reusable web UI components implemented using C#, HTML, and CSS. Both client and server code is written in C#, allowing you to share code and libraries.

To get started with Oqtane:

   1.&nbsp;Install the latest preview of [Visual Studio 2019](https://visualstudio.com/preview) with the **ASP.NET and web development** workload.

   2.&nbsp;Install the latest [Blazor extension](https://go.microsoft.com/fwlink/?linkid=870389) from the Visual Studio Marketplace. This step makes Blazor templates available to Visual Studio.

   3.&nbsp;Enable Visual Studio to use preview SDKs: Open **Tools** > **Options** in the menu bar. Open the **Projects and Solutions** node. Open the **.NET Core** tab. Check the box for **Use previews of the .NET Core SDK**. Select **OK**.

   4.&nbsp;Open the **Oqtane.sln** solution file. If you want to develop using **server-side** Blazor ( which includes a full debugging experience in Visual Studio ) you should choose to Build the solution using the default Debug configuration. If you want to develop using **client-side** Blazor ( WebAssembly ) you should first choose the "Wasm" configuration option in the Visual Studio toolbar and then Build.
