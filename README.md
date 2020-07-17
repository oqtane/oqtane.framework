# Oqtane Framework
Oqtane is a Modular Application Framework for Blazor

![Oqtane](https://github.com/oqtane/framework/blob/master/oqtane.png?raw=true "Oqtane")

Oqtane uses Blazor, an open source and cross-platform web UI framework for building single-page apps using .NET and C# instead of JavaScript. Blazor apps are composed of reusable web UI components implemented using C#, HTML, and CSS. Both client and server code is written in C#, allowing you to share code and libraries.

Oqtane is being developed based on some fundamental principles which are outlined in the [Oqtane Philosophy](https://www.oqtane.org/Resources/Blog/PostId/538/oqtane-philosophy).

Please note that this project is owned by the .NET Foundation and is governed by the **[.NET Foundation Contributor Covenant Code of Conduct](https://dotnetfoundation.org/code-of-conduct)**

# Getting Started

**Using the latest code:**

- Install **[.NET Core 3.1 SDK (v3.1.300)](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.300-windows-x64-installer)**.
   
- Install the Preview edition of [Visual Studio 2019](https://visualstudio.microsoft.com/vs/preview/) (version 16.6 or higher) with the **ASP.NET and web development** workload enabled. Oqtane works with all editions of Visual Studio from Community to Enterprise. If you do not have a SQL Server installation available already and you wish to use LocalDB for development, you must also install the **.NET desktop development workload**.  

- Download or Clone the Oqtane source code to your local system. Open the **Oqtane.sln** solution file and Build the solution. 

**Using an official release:**

- A detailed set of instructions for installing Oqtane on IIS is located here: [Installing Oqtane on IIS](https://www.oqtane.org/Resources/Blog/PostId/542/installing-oqtane-on-iis)

**Additional Instructions**

- If you have already installed a previous version of Oqtane and you wish to do a clean database install, simply reset the DefaultConnection value in the Oqtane.Server\appsettings.json file to "". This will trigger a re-install when you run the application which will execute the database installation scripts.
   
- If you want to submit pull requests make sure you install the [Github Extension For Visual Studio](https://visualstudio.github.com/). It is recommended you ignore any local changes you have made to the appsettings.json file before you submit a pull request. To automate this activity, open a command prompt and navigate to the /Oqtane.Server/ folder and enter the command "git update-index --skip-worktree appsettings.json" 

# Roadmap
This project is a work in progress and the schedule for implementing enhancements is dependent upon the availability of community members who are willing/able to assist.

V.Next ( still in the process of being prioritized )
- [ ] Admin UI markup optimization
- [ ] DB Migrations for framework installation/upgrade
- [ ] Support for SQLite
- [ ] Static Localization ( ie. labels, help text, etc.. )
- [ ] Migrate to Code-Behind Pattern ( *.razor.cs )
- [ ] Generic Repository Pattern
- [ ] JwT token authentication ( possibly using IdentityServer )
- [ ] Optional Encryption for Settings Values 

V1.0.0 (MVP) 
- [x] Multi-Tenant ( Shared Database & Isolated Database ) 
- [x] Modular Architecture / Headless API
- [x] Dynamic Page Compositing Model / Site & Page Management
- [x] Authentication / User Management / Profile Management
- [x] Authorization / Roles Management / Granular Permissions
- [x] Dynamic Routing
- [x] Extensibility via Custom Modules
- [x] Extensibility via Custom Themes
- [x] Event Logging
- [x] Folder / File Management
- [x] Recycle Bin
- [x] Scheduled Jobs ( Background Processing )
- [x] Notifications / Email Delivery
- [x] Auto-Upgrade Framework

# Background
Oqtane was created by [Shaun Walker](https://www.linkedin.com/in/shaunbrucewalker/) and is inspired by the DotNetNuke web application framework. Initially created as a proof of concept, Oqtane is a native Blazor application written from the ground up using modern .NET Core technology. It is a modular application framework offering a fully dynamic page compositing model, multi-site support, designer friendly templates (skins), and extensibility via third party modules.

# Release Announcements

[Oqtane 1.0.1](https://www.oqtane.org/Resources/Blog/PostId/541/oqtane-builds-momentum-with-101-release)

[Oqtane 1.0](https://www.oqtane.org/Resources/Blog/PostId/540/announcing-oqtane-10-a-modular-application-framework-for-blazor)

[Oqtane POC](https://www.oqtane.org/Resources/Blog/PostId/520/announcing-oqtane-a-modular-application-framework-for-blazor)

# Example Screenshots

Install Wizard:

![Installer](https://github.com/oqtane/framework/blob/master/screenshots/Installer.png?raw=true "Installer")

Default view after installation:

![Home](https://github.com/oqtane/framework/blob/master/screenshots/screenshot0.png?raw=true "Home")

A seamless login flow utilizing .NET Core Identity services:

![Login](https://github.com/oqtane/framework/blob/master/screenshots/screenshot1.png?raw=true "Login")

Main view for authorized users, allowing full management of modules and content:

![Admin View](https://github.com/oqtane/framework/blob/master/screenshots/screenshot2.png?raw=true "Admin View")

Content editing user experience using modal dialog:

![Edit Content](https://github.com/oqtane/framework/blob/master/screenshots/screenshot3.png?raw=true "Edit Content")

Context menu for managing specific module on page:

![Manage Module](https://github.com/oqtane/framework/blob/master/screenshots/screenshot4.png?raw=true "Manage Module")

Control panel for adding, editing, and deleting pages as well as adding new modules to a page:

![Manage Page](https://github.com/oqtane/framework/blob/master/screenshots/screenshot5.png?raw=true "Manage Page")

Admin dashboard for accessing the various administrative features of the framework:

![Admin Dashboard](https://github.com/oqtane/framework/blob/master/screenshots/screenshot6.png?raw=true "Admin Dashboard")

Responsive design mobile view:

![Mobile View](https://github.com/oqtane/framework/blob/master/screenshots/screenshot7.png?raw=true "Mobile View")

