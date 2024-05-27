# SpellCrafter

## Introduction
SpellCrafter is a mod manager for The Elder Scrolls Online (ESO) designed to enhance your gaming experience by simplifying the installation of mods. It supports automatic dependency management, allowing you to effortlessly install and manage game add-ons with their required dependencies.

## Features
- **Automatic Dependency Management**: Automatically handles dependencies for mods, ensuring all necessary components are installed.
- **Cross-platform Compatibility**: Runs on various platforms, supporting Windows, macOS, and Linux.

## Prerequisites
Before you begin, ensure you have the following installed:
- [.NET Core 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git (for cloning the repository)

## Getting Started

### Cloning the Repository
To get started with SpellCrafter, clone the repository to your local machine:

```git clone https://github.com/yourusername/SpellCrafter.git```
```cd SpellCrafter```

## Building the Application
SpellCrafter uses Native AOT (Ahead-of-Time) compilation to improve performance. To build the application, use the following command:

```dotnet publish -c Release -r <RID> --self-contained```
Replace <RID> with the appropriate Runtime Identifier for your platform, such as win-x64, linux-x64, or osx-x64.

Running the Application
After building, you can run the application directly from the publish directory:

```./bin/Release/net8/<RID>/publish/SpellCrafter```