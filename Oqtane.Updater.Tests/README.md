# Updater and release archive regression tests

Run from the repository root:

```text
dotnet test Oqtane.Updater.slnx -c Release
```

The tests invoke the actual updater entry point against temporary application folders. They cover both ZIP separators, explicit directory entries, trailing separators on the application root, backup and rollback after an extraction failure, and rejecting escaping or duplicate target paths before changing existing files or backups.

Packaging tests run the real install and upgrade scripts against a small publish fixture. They check portable entry names, file contents, hidden files, empty directories, replacement of an existing archive, and execution from a different working directory.

Windows uses Windows PowerShell by default, matching `release.cmd`. Linux requires `pwsh` on PATH. Set `OQTANE_TEST_POWERSHELL` to test another executable, for example:

```powershell
$env:OQTANE_TEST_POWERSHELL = (Get-Command pwsh).Source
dotnet test Oqtane.Updater.slnx -c Release --filter FullyQualifiedName~PackagingTests
```

These tests do not start a web server, connect to a database, or modify an installed Oqtane site.
