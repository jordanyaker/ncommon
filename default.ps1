properties {
  $base_dir = Resolve-Path .
  $build_dir = "$base_dir\build"
  $libs_dir = "$base_dir\Libs"
  $packages_dir = "$base_dir\packages"
  $output_dir = "$base_dir\output"
  $sln = "$base_dir\NCommon.build"
  $build_config = "release"
  $tools_dir = "$base_dir\Tools"
  $nuget_dir = "$base_dir\.nuget"
  $pacakges_dir = "$base_dir\packages"
  $version = "1.3.3"
}

$framework = "4.0x86"

Task default -depends debug

Task Clean {
    remove-item -force -recurse $build_dir -ErrorAction SilentlyContinue
    remove-item -force -recurse $output_dir -ErrorAction SilentlyContinue
}

Task Init -depends Clean {
    Generate-AssemblyInfo `
        -file "$base_dir\CommonAssemblyInfo.cs" `
        -product "Arcweb Flavored NCommon Framework $version" `
        -copyright "Arcweb Technologies, Inc. 2013" `
        -version $version `
        -clsCompliant "false"

    new-item $build_dir -itemType directory -ErrorAction SilentlyContinue
    
    foreach($file in Get-ChildItem -Include 'packages.config' -Recurse) {
        Write-Host "Installing packages from $file"
        exec { &"$nuget_dir\Nuget.exe" install -OutputDirectory $packages_dir $file.FullName}
    }
    Write-Host "Finished initializing build."
}

Task Compile -depends Init {
    
    Write-Host "Building $sln for Net40"
    exec {msbuild $sln /verbosity:minimal /p:OutDir="$build_dir\Net40\\" /p:Config="$build_config" /p:TargetFrameworkVersion=v4.0 /p:ToolsVersion=v4.0 /nologo}
}

Task debug {
    $build_config = "Debug"
    ExecuteTask Compile
}

Task release {
    $build_config = "Release"
    ExecuteTask Compile

    new-item $output_dir -itemType directory

    Create-PackageFolder -id "Arcweb.Ncommon" -projectPath "$base_dir\NCommon\src\"
    Copy-PackageLib -id "Arcweb.Ncommon" -lib "Arcweb.Ncommon.dll"

    Create-PackageFolder -id "Arcweb.Ncommon.ContainerAdapter.Autofac" -projectPath "$base_dir\NCommon.ContainerAdapters\NCommon.ContainerAdapter.Autofac\"
    Copy-PackageLib -id "Arcweb.Ncommon.ContainerAdapter.Autofac" -lib "Arcweb.Ncommon.ContainerAdapter.Autofac.dll"
    
    Create-PackageFolder -id "Arcweb.Ncommon.ContainerAdapter.CastleWindsor" -projectPath "$base_dir\NCommon.ContainerAdapters\NCommon.ContainerAdapter.CastleWindsor\"
    Copy-PackageLib -id "Arcweb.Ncommon.ContainerAdapter.CastleWindsor" -lib "Arcweb.Ncommon.ContainerAdapter.CastleWindsor.dll"

    Create-PackageFolder -id "Arcweb.Ncommon.ContainerAdapter.Ninject" -projectPath "$base_dir\NCommon.ContainerAdapters\NCommon.ContainerAdapter.Ninject\"
    Copy-PackageLib -id "Arcweb.Ncommon.ContainerAdapter.Ninject" -lib "Arcweb.Ncommon.ContainerAdapter.Ninject.dll"
    
    Create-PackageFolder -id "Arcweb.Ncommon.ContainerAdapter.StructureMap" -projectPath "$base_dir\NCommon.ContainerAdapters\NCommon.ContainerAdapter.StructureMap\"
    Copy-PackageLib -id "Arcweb.Ncommon.ContainerAdapter.StructureMap" -lib "Arcweb.Ncommon.ContainerAdapter.StructureMap.dll"

    Create-PackageFolder -id "Arcweb.Ncommon.ContainerAdapter.Unity" -projectPath "$base_dir\NCommon.ContainerAdapters\NCommon.ContainerAdapter.Unity\"
    Copy-PackageLib -id "Arcweb.Ncommon.ContainerAdapter.Unity" -lib "Arcweb.Ncommon.ContainerAdapter.Unity.dll"
    
    Create-PackageFolder -id "Arcweb.Ncommon.EntityFramework" -projectPath "$base_dir\NCommon.EntityFramework\src"
    Copy-PackageLib -id "Arcweb.Ncommon.EntityFramework" -lib "Arcweb.Ncommon.EntityFramework.dll"

    Create-PackageFolder -id "Arcweb.Ncommon.Mvc" -projectPath "$base_dir\NCommon.Mvc\src"
    Copy-PackageLib -id "Arcweb.Ncommon.Mvc" -lib "Arcweb.Ncommon.Mvc.dll"
}

Task publish -depends release {
    Generate-Package -id "Arcweb.Ncommon"
    Generate-Package -id "Arcweb.Ncommon.ContainerAdapter.Autofac"
    Generate-Package -id "Arcweb.Ncommon.ContainerAdapter.CastleWindsor"
    Generate-Package -id "Arcweb.Ncommon.ContainerAdapter.Ninject"
    Generate-Package -id "Arcweb.Ncommon.ContainerAdapter.StructureMap"
    Generate-Package -id "Arcweb.Ncommon.ContainerAdapter.Unity"
    Generate-Package -id "Arcweb.Ncommon.EntityFramework"
    Generate-Package -id "Arcweb.Ncommon.Mvc"

    Publish-Package -id "Arcweb.Ncommon"
    Publish-Package -id "Arcweb.Ncommon.ContainerAdapter.Autofac"
    Publish-Package -id "Arcweb.Ncommon.ContainerAdapter.CastleWindsor"
    Publish-Package -id "Arcweb.Ncommon.ContainerAdapter.Ninject"
    Publish-Package -id "Arcweb.Ncommon.ContainerAdapter.StructureMap"
    Publish-Package -id "Arcweb.Ncommon.ContainerAdapter.Unity"
    Publish-Package -id "Arcweb.Ncommon.EntityFramework"
    Publish-Package -id "Arcweb.Ncommon.Mvc"
}

function Generate-AssemblyInfo
{
    param(
        [string]$clsCompliant = "true",
        [string]$product,
        [string]$copyright,
        [string]$version,
        [string]$file = $(throw "file is a required parameter.")
    )
    $asmInfo = "using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [assembly: CLSCompliantAttribute($clsCompliant )]
    [assembly: ComVisibleAttribute(false)]
    [assembly: AssemblyProductAttribute(""$product"")]
    [assembly: AssemblyCopyrightAttribute(""$copyright"")]
    [assembly: AssemblyVersionAttribute(""$version"")]
    [assembly: AssemblyInformationalVersionAttribute(""$version"")]
    [assembly: AssemblyFileVersionAttribute(""$version"")]
    [assembly: AssemblyDelaySignAttribute(false)]
    "

        $dir = [System.IO.Path]::GetDirectoryName($file)
        if ([System.IO.Directory]::Exists($dir) -eq $false)
        {
            Write-Host "Creating directory $dir"
            [System.IO.Directory]::CreateDirectory($dir)
        }
        Write-Host "Generating assembly info file: $file"
        Write-Output $asmInfo > $file
}

function Create-PackageFolder {
    param([string] $id, [string] $projectPath)
    new-item $output_dir\$id -itemType directory
    new-item $output_dir\$id\lib -itemType directory
    new-item $output_dir\$id\lib\Net40 -itemType directory
    copy-item $projectPath\$id.nuspec -destination $output_dir\$id
}

function Copy-PackageLib {
    param([string] $id, [string] $lib)
    copy-item $build_dir\Net40\$lib -destination $output_dir\$id\lib\Net40
}

function Generate-Package {
    param([string] $id)
    Write-Host "Generating package $id version $version"
    exec { &"$nuget_dir\Nuget.exe" pack "$output_dir\$id\$id.nuspec" -Version "$version" -Verbose -OutputDirectory "$output_dir" }
}

function Publish-Package {
    param([string] $id)
    Write-Host "Pubishing package $id version $version to NuGet gallery"
    exec { &"$nuget_dir\Nuget.exe" push "$output_dir\$id.$version.nupkg"}
}