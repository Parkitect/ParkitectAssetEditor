using System;

namespace ParkitectAssetEditor.Utility
{
    public static class ProjectDocument
    {
        public static String DefaultCsProjNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static String MainCS
        {
            get
            {
                return @"namespace ParkitectMod
{

    public class Main : IMod
    {

        public void onEnabled()
        {
        }

        public void onDisabled()
        {
        }

        public string Name => "" "";

            public string Description => "" "";

        string IMod.Identifier => "" "";

    }
}";
            }
        }

        public static string CSProj
        {
            get
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Project ToolsVersion=""12.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
                <PropertyGroup>
                    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
                    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
                    <ProjectGuid>{62921F50-6388-43FE-9DB2-B7F8556A7931}</ProjectGuid>
                    <OutputType>Library</OutputType>
                    <AppDesignerFolder>Properties</AppDesignerFolder>
                    <RootNamespace>${RootNamespace}</RootNamespace>
                    <AssemblyName>${AssemblyName}</AssemblyName>
                    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
                    <FileAlignment>512</FileAlignment>
                </PropertyGroup>
                <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
                    <PlatformTarget>AnyCPU</PlatformTarget>
                    <DebugSymbols>true</DebugSymbols>
                    <DebugType>full</DebugType>
                    <Optimize>false</Optimize>
                    <OutputPath>${OutputPath}</OutputPath>
                    <DefineConstants>DEBUG;TRACE</DefineConstants>
                    <ErrorReport>prompt</ErrorReport>
                    <WarningLevel>4</WarningLevel>
                    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                </PropertyGroup>
                <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
                    <PlatformTarget>AnyCPU</PlatformTarget>
                    <DebugType>pdbonly</DebugType>
                    <Optimize>true</Optimize>
                    <OutputPath>${OutputPath}</OutputPath>
                    <DefineConstants>TRACE</DefineConstants>
                    <ErrorReport>prompt</ErrorReport>
                    <WarningLevel>4</WarningLevel>
                    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                </PropertyGroup>
                <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
            </Project>";
            }
        }
    }
}
