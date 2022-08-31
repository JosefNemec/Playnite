Use VSCode to develop Playnite extensions in Windows
====================================================

This is a step by step tutorial to build the Playnite extensions using VSCode.

Download and setup dependencies
-------------------------------

1. Install [VSCode](https://code.visualstudio.com/Download)
2. Install [.NET SDK for VSCode](https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code)
3. Install [.NET Framework 4.6.2 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462)
4. Install [Visual Build Tools for Visual Studio 2015 with Update 3](https://my.visualstudio.com/Downloads?q=%22Visual%20Build%20Tools%20for%20Visual%20Studio%202015%20with%20Update%203%22) (Requires registration)
5. Install [C# extension in VSCode](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
6. Install [nuget](https://www.nuget.org/downloads) (If in doubt, follow the instruction in [Microsoft Docs](https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools#nugetexe-cli), and be sure to add it to the path!)

Prepare your project folder
---------------------------
1. [Create the project using toolbox.exe](https://playnite.link/docs/master/tutorials/toolbox.html#plugins)
2. Open the project folder in VSCode
3. In VSCode, press `ctrl+shit+P`, type `Terminal: Create New Terminal` and press `Enter`
4. Create a new `AssemblyInfo.cs` file:

```powershell
Get-Content .\Properties\AssemblyInfo.cs | Where-Object {$_ -notmatch '^\[assembly: Assembly.+'} | Set-Content .\AssemblyInfo.cs
Remove-Item -path .\Properties\ -recurse -force
```
5. Install PlayniteSDK package using `nuget`:

```shell
nuget restore -SolutionDirectory . -Verbosity normal
```
6. Replace the csproj file content in your project folder for this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net4.6.2</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="PlayniteSDK" Version="6.2.0" />
  </ItemGroup>
  <ItemGroup>
    <None Include="extension.yaml">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="packages.config" />
  </ItemGroup>
  <ItemGroup>
    <None Include="icon.png">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```
7. In VSCode, press `ctrl+shift+P`, type `.NET: Generate Assets for Build and Debug` and press `Enter`
8. In VSCode, press `ctrl+shift+P`, type `Tasks: Run Task > publish` and press `Enter`
9. Output folder should be in `.\bin\Debug\net4.6.2\publish`. Add the full path (including drive letter) to the external extensions list in Playnite, and restart it.
10. In Playnite, you should see your extension running in the Add-Ons window.

You can use something like [XAML Studio](https://aka.ms/xamlstudio) to edit single XAML files graphically.
