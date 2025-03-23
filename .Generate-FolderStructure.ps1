# Set UTF-8 encoding
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Define the output file
$outputFile = "tree_structure.txt"

# Define directories and files to exclude
$excludeDirs = @('node_modules', '.next', '.git', 'dist', 'build', '.cache', '.vscode', '.idea', 'coverage', 'public', '.vercel', '.npm', '.yarn', '.pnpm-store', 'out')
$excludeFiles = @('*.log', '*.tmp', '*.swp', '*.map', '*.md', 'LICENSE', 'README.md', '*.ico', '*.png', '*.jpg', '*.svg', '*.zip', '*.tar.gz', 'package-lock.json', 'yarn.lock', '.DS_Store', 'Thumbs.db')

# Function to generate folder structure
function Get-FolderStructure {
    param (
        [string]$Path,
        [string]$Indent = ""
    )

    # Get the current directory name
    $directoryName = Split-Path $Path -Leaf
    "$Indent+---$directoryName"

    # Get all child items (files and directories) in the current path
    $items = Get-ChildItem -Path $Path -Force

    foreach ($item in $items) {
        # Skip excluded directories
        if ($item.PSIsContainer -and $excludeDirs -contains $item.Name) {
            continue
        }

        # Skip excluded files
        if (-not $item.PSIsContainer -and $excludeFiles -contains $item.Name) {
            continue
        }

        # If it's a directory, recurse
        if ($item.PSIsContainer) {
            Get-FolderStructure -Path $item.FullName -Indent "$Indent|   "
        }
        # If it's a file, display it
        else {
            "$Indent|   $($item.Name)"
        }
    }
}

# Clear the console
Clear-Host

# Generate the folder structure
Write-Host "Generating folder structure..."
$structure = Get-FolderStructure -Path (Get-Location).Path

# Save the structure to a file
$structure | Out-File -FilePath $outputFile -Encoding UTF8

# Display the structure in the console
Get-Content -Path $outputFile

Write-Host "`nFolder structure saved in $outputFile"