param ([string]$configuration = "Release")
$ErrorActionPreference = "Stop"

# Set location to the Solution directory
(Get-Item $PSScriptRoot).Parent.FullName | Push-Location
[xml] $versionFile = Get-Content "./EPiServer.Azure.StorageExplorer/EPiServer.Azure.StorageExplorer.csproj"

$node = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.CMS.UI.Core']")
$uiVersion = $node.Attributes["Version"].Value
$parts = $uiVersion.Split(".")
$major = [int]::Parse($parts[0]) + 1
$uiNextMajorVersion = ($major.ToString() + ".0.0") 

$azureNode = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='EPiServer.Azure']")
$azureVersion = $azureNode.Attributes["Version"].Value
$azureParts = $azureVersion.Split(".")
$azureMajor = [int]::Parse($azureParts[0]) + 1
$azureNextMajorVersion = ($azureMajor.ToString() + ".0.0") 

$nonFactorsNode = $versionFile.SelectSingleNode("Project/ItemGroup/PackageReference[@Include='NonFactors.Grid.Core.Mvc6']")
$nonFactorsVersion = $nonFactorsNode.Attributes["Version"].Value

[xml] $versionFile = Get-Content "./build/version.props"
$version = $versionFile.SelectSingleNode("Project/PropertyGroup/VersionPrefix").InnerText + $Env:versionSuffix 


Remove-Item -Path ./zipoutput -Recurse -Force -Confirm:$false -ErrorAction Ignore

Copy-Item "./EPiServer.Azure.StorageExplorer/ClientResources" -Destination "./zipoutput/EPiServer.Azure.StorageExplorer/clientResources" -Recurse

[xml] $moduleFile = Get-Content "./EPiServer.Azure.StorageExplorer/module.config"
$module = $moduleFile.SelectSingleNode("module")
$module.Attributes["clientResourceRelativePath"].Value = $version
$moduleFile.Save("./zipoutput/EPiServer.Azure.StorageExplorer/module.config")

New-Item -Path "./zipoutput/EPiServer.Azure.StorageExplorer" -Name "$version" -ItemType "directory"
Move-Item -Path "./zipoutput/EPiServer.Azure.StorageExplorer/clientResources" -Destination "./zipoutput/EPiServer.Azure.StorageExplorer/$version/clientResources"

$compress = @{
  Path = "./zipoutput/EPiServer.Azure.StorageExplorer/*"
  CompressionLevel = "Optimal"
  DestinationPath = "./zipoutput/EPiServer.Azure.StorageExplorer.zip"
}

Compress-Archive @compress
dotnet pack --no-restore --no-build -c $configuration /p:PackageVersion=$version /p:UiVersion=$uiVersion /p:UiNextMajorVersion=$uiNextMajorVersion /p:AzureVersion=$azureVersion /p:AzureNextMajorVersion=$azureNextMajorVersion /p:NonFactorsVersion=$nonFactorsVersion EPiServer.Azure.StorageExplorer.sln

Pop-Location