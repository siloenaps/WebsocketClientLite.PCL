param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\IWebsocketClientLite.Netstandard\bin\Release\netstandard1.3\IWebsocketClientLite.PCL.dll')).Version.ToString(3)
	}
else {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\IWebsocketClientLite.Netstandard\bin\Release\netstandard1.3\IWebsocketClientLite.PCL.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\nuget.exe push -Source "1iveowlNuGetRepo" -ApiKey key .\Nuget\WebsocketClientLite.PCL.$version.nupkg
