#Napacks
Synopsis
-----------
The Napack Framework Server provides C# algorithms, source code files, and APIs in a nano-scale package format.
The goal of the Napack design is to avoid code duplication for small pieces of functionality while avoiding monolithic packages traditionally present in package managers.

Getting Started
---------------
[Finding Napacks](./Search.md)

[Consume Napacks](./Consumption.md)

[Create Napacks](./Creation.md) (Prerequisite: [User Registration](./Registration.md))

Features
----------------

###Nano-scale
Napacks are small, easily consumable APIs that provide simple functionality without a monolithic package containing extraneous functionality.

###Automatic Versioning
Napacks strictly follow [Semantic Versioning](http://semver.org/) with the basic MAJOR.MINOR.PATCH format. This enables you to add ```using PackageName.2;``` to the top of each source file consuming the Napack, with the guarantee that package updates won't break your code. For more information, see the [Versioning](./Versioning.md) page.

###Automatic Documentation Generation
Napacks are automatically scanned for their publically-facing API, which is partially used to automatically generate documentation. For more information, see the [Documentation](./Documentation.md) page.

###Licensing
Napack uses a unique licensing strategy to ensure that you don't accidentally consume copy-left code or commercial code -- simply append the generated **Napack.attributions** to the attributions section in your current license file. For more information, see the [Supported Licenses](./SupportedLicenses.md) page.

###Build Integration
Napack integrates within MSBuild to automatically update Napacks with non-breaking changes, download requested Napacks and dependencies, and auto-generate a **Napack.attributions** file.

Release Notes
------------------
Current Version: 0.1.0 ("Prerelease")

[Release Notes](./ReleaseNotes.md)