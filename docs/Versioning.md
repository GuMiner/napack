# Versioning
Napacks strictly follow [Semantic Versioning](http://semver.org/) with the basic MAJOR.MINOR.PATCH format. Therefore:
* API changes require an increment to the MAJOR version.
* Capability changes require an increment to the MINOR version.
* Bug fixes require an increment to the PATCH version.

The Napack framework will automatically detect API changes and reject package changes that do not increment the MAJOR version. The framework will also reject package changes that use the same version as a previous package version.
