Napacks follow a subset of [Semantic Versioning](http://semver.org/) with the basic MAJOR.MINOR.PATCH format.
When packages are updated, the Napack framework will automatically scan the publically-facing API of the package code, detect API changes, and perform the appropriate version increment.

When the Napack Framewok detects API changes:

* Publically-facing API edits or deletions will increment the MAJOR version.
* API additions will increment the MINOR version.
* Napack updates that don't impact the publically-facing API will increment the PATCH version.

While the Napack Framework's automatic versioning cannot prevent all breaking changes, this standardization provides:

* A common understanding of what version changes represent.
* An automated way of performing the version changes.
* Automatic public API documentation

##What about prerelease/alpha/beta/rc packages?
Napacks are intended to be small enough to not *need* a 'not for public consumption' tag. If you want to include such a tag in your Napack version, that usually indicates:

* Your Napack includes multiple pieces of functionality that should be broken apart into separate smaller Napacks.
* Your Napack is too large to create a ready-for-release version in a short period of time.
* Your Napack is too large to sufficiently test to validate you have created a ready-for-release version.

As such, the Napack Framework does not support version tags.
