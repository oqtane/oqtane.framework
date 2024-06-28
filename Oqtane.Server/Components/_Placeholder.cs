// This is just a placeholder file
// It is necessary for the documentation to successfully build this project.
// Reason is that docfx will run the .net compiler and find references
// to this class in the project.
// But since the real class is just a .razor file, ATM docfx will fail.
//
// Note added 2024-06-27 by @iJungleboy.
// We hope that as .net and docfx improve, the razor-compiler will work in that scenario
// as well, and this file can be removed.

using Oqtane.Documentation;

namespace Oqtane.Components;

[PrivateApi]
public class _Placeholder;
