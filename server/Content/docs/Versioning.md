# Versioning
Napacks follow a subset of [Semantic Versioning](http://semver.org/) with the basic MAJOR.MINOR.PATCH format.
When packages are updated, the Napack framework will automatically detect API changes and perform the appropriate version increment.

Therefore:
* Breaking API changes will increment the MAJOR version.
* Capability additions will increment the MINOR version.
* Changes that don't affect the API (ususally bug fixes) will increment the PATCH version.

This "auto-upversioning" can't prevent all breaking changes, but provides the following benefits:
* A common understanding of what version changes represent.
* An automated way of performing the version changes.
* Automatic public API documentation

## But what about prerelease/alpha/beta/rc packages?
Napacks are intended to be small enough to not *need* a 'not for public consumption' tag. If you want to include such a tag in your Napack version, that usually indicates:
* There are not sufficient unit tests associated with your Napack.
* Your Napack is too large to be easily testable and verifiable.
