[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SourcePath,

    [Parameter(Mandatory = $true)]
    [string] $DestinationPath
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$source = Get-Item -LiteralPath $SourcePath
if (-not $source.PSIsContainer) {
    throw 'The archive source must be a directory.'
}
$sourceRoot = $source.FullName.TrimEnd([char[]]@('\', '/')) + [IO.Path]::DirectorySeparatorChar
$destination = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($DestinationPath)
$comparison = if ([IO.Path]::DirectorySeparatorChar -eq '\') { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
if ($destination.StartsWith($sourceRoot, $comparison)) {
    throw 'The archive destination must be outside the source directory.'
}

$stream = [IO.File]::Open($destination, [IO.FileMode]::Create)
try {
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
    try {
        # Explicit entry names avoid the platform-specific separators used by older Compress-Archive versions.
        foreach ($file in [IO.Directory]::EnumerateFiles($sourceRoot, '*', [IO.SearchOption]::AllDirectories)) {
            $entryName = $file.Substring($sourceRoot.Length).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file, $entryName, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
        foreach ($directory in [IO.Directory]::EnumerateDirectories($sourceRoot, '*', [IO.SearchOption]::AllDirectories)) {
            if ([IO.Directory]::GetFileSystemEntries($directory).Length -eq 0) {
                $entryName = $directory.Substring($sourceRoot.Length).Replace('\', '/') + '/'
                $archive.CreateEntry($entryName) | Out-Null
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $stream.Dispose()
}
