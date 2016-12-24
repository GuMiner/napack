# Napacks
## Synopsis
-----------
The Napack Framework Server provides algorithms, source files, and APIs in a consumable, package format.
The goal of the Napack System is to avoid code duplication while avoiding monolithic packages traditionally present in library package managers.

## Getting Started
---------------
[Finding Napacks] (./Search.md)

[Consume Napacks] (./Consumption.md)

[Create Napacks] (./Creation.md)

## Features
----------------

### Nano scale
Napacks are small, easily consumable APIs that provide key programmatic functionality without a monolithic binary containing extraneous functionality.

### Versioning
Napacks strictly follow [Semantic Versioning](http://semver.org/) with the basic MAJOR.MINOR.PATCH format. This enables you to add ```using PackageName.2;``` to the top of each source file consuming the Napack, with the guarantee that package updates won't break your code. For more information, see the [Versioning](./Versioning.md) page.

### Licensing
Napack uses a unique licensing strategy to ensure that you don't accidentally consume copy-left code or commercial code -- simply append the generated **Napack.attributions** to the attributions section in your current license file. For more information, see the [Supported Licenses](./SupportedLicenses.md) page.

### Build Integration
Napack integrates within MSBuild to automatically update Napacks with non-breaking changes, download requested Napacks and dependencies, and auto-generate the **Napack.attributions** file.

## Future Features
------------------
* Add Napack searching. Update Search.md when done.
* Add a Napack extension installer to simplify integration with Visual Studio. Update Download.md when done.
* Add to the Napack binary functionality to create Napacks, instead of visiting the Napack Framework Server site. Update Creation.md when done.
* Create a GUI system to simplify Napack creation. Update Creation.md when done.
* Add auto-suggestion searching feature extension for Visual Studio. Update this page when done.
* Add an extension that can create a Napack from a selection of code from Visual Studio. Update Creation.md when done.