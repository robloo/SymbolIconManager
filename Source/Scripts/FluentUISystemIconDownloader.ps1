$repoOwner = "microsoft"
$repoName = "fluentui-system-icons"
$targetDirectory = "assets"
$repoDirectory = "$($env:USERPROFILE)\Desktop\$repoName"
$outputDirectory = "$($env:USERPROFILE)\Desktop\$repoName-svg-output"
$maxFileCount = 1000000

# Clone the repository if it doesn't already exist
if (!(Test-Path $repoDirectory))
{
    git clone "https://github.com/$repoOwner/$repoName.git" $repoDirectory
}

# Create a dictionary to store the latest version of each file
$files = [System.Collections.Generic.Dictionary[string, string]]::new()

# Get all commits for the repository
$commitHashes = git -C $repoDirectory log --pretty=format:"%h"

# Loop through each commit
foreach ($commitHash in $commitHashes)
{
    # Get all files in the commit
	$commitFiles = git -C $repoDirectory show --pretty="" --name-only $commitHash

    # Loop through each file in the commit
    foreach ($commitFile in $commitFiles)
    {
		# Check if the file is in the target directory and is a .SVG file
        if ($commitFile.StartsWith($targetDirectory) -and $commitFile -like "*.svg")
        {
            if ($files.ContainsKey($commitFile))
            {
                # Get the Unix timestamp for both versions of the file (seconds since January 1, 1970)
                $existingTimestamp = git -C $repoDirectory log -1 --pretty=format:"%ct" $files[$commitFile]
                $newTimestamp = git -C $repoDirectory log -1 --pretty=format:"%ct" $commitHash -- $commitFile

                # Check if the file in the dictionary is older than the current file
                if ($existingTimestamp -lt $newTimestamp)
                {
                    # Replace the file in the dictionary with the newer version
                    $files[$commitFile] = $commitHash
                }
            }
            else
            {
                $files.Add($commitFile, $commitHash)
            }
        }

        if ($files.Count -ge $maxFileCount)
        {
            break;
        }
    }

    if ($files.Count -ge $maxFileCount)
    {
        break;
    }
}

# Create the final output SVG files
foreach ($file in $files.Keys)
{
    # Get the subdirectories in the file path
    $filePath = $file
    $subDirs = $filePath.Split("/")[0..($filePath.Split("/").Length - 2)]

    # Create the subdirectories in the output destination
    $dir = $outputDirectory
    foreach ($subDir in $subDirs)
    {
        $dir = Join-Path $dir $subDir
        if (!(Test-Path $dir))
        {
            New-Item $dir -ItemType Directory
        }
    }

    $sourcePath = Join-Path $repoDirectory $file
    $destPath = Join-Path $outputDirectory $file

    # Checkout the file
    git -C $repoDirectory checkout $files[$file] -- $file

    # Copy the file to the output destination (or replaces an existing file)
    Copy-Item -Path $sourcePath -Destination $destPath
}
