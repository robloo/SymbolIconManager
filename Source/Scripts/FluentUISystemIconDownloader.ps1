# Define the repository owner and name
$repoOwner = "microsoft"
$repoName = "fluentui-system-icons"
$targetDirectory = "assets"
$downloadDirectory = "$($env:USERPROFILE)\Desktop"

# Get all commit hashes for the repository
$commits = Invoke-WebRequest -Uri "https://api.github.com/repos/$repoOwner/$repoName/commits" -UseBasicParsing |
           ConvertFrom-Json

# Create a dictionary to store the latest version of each file
$files = [System.Collections.Generic.Dictionary[string, object]]::new()

# Loop through each commit
foreach ($commit in $commits)
{
    # Get the SHA hash of the commit
    $commitSHA = $commit.sha

    # Get all files in the commit
	$commitFiles = Invoke-WebRequest -Uri "https://api.github.com/repos/$repoOwner/$repoName/commits/$commitSHA" -UseBasicParsing | 
                   ConvertFrom-Json | 
                   Select-Object -ExpandProperty files

    # Loop through each file in the commit
    foreach ($commitFile in $commitFiles)
    {
		# Check if the file is in the target directory and is a .SVG file
		if ($commitFile.filename.StartsWith($targetDirectory) -and $commitFile.filename -like "*.svg")
		{
            if ($files.ContainsKey($commitFile.filename))
            {
                # Check if the file in the dictionary is older than the current file
                if ($files[$commitFile.filename].commit_date -lt $commitFile.commit_date)
                {
                    # Replace the file in the dictionary with the newer version
                    $files[$commitFile.filename] = $commitFile
                }
            }
            else
            {
                # Add the file to the dictionary
                $files.Add($commitFile.filename, $commitFile)
            }
        }
    }
}

# Download each file
foreach ($file in $files.Values)
{
    # Get the subdirectories in the file path
    $filePath = $file.filename
    $subDirs = $filePath.Split("/")[0..($filePath.Split("/").Length - 2)]

    # Create the subdirectories in the local file system
    $dir = $downloadDirectory
    foreach ($subDir in $subDirs)
    {
        $dir = Join-Path $dir $subDir
        if (!(Test-Path $dir))
        {
            New-Item $dir -ItemType Directory
        }
    }

    # Download the file
    Invoke-WebRequest -Uri $file.download_url -OutFile (Join-Path $dir $file.filename.Split("/")[-1])
}
