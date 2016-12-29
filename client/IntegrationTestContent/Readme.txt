Test Instructions
- Run: Napack.Client.exe -Operation Register -UserEmail email -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
- Update NapackSettings.json to have the default UserId (email, string) and AuthenticationKeys (secrets, string list) included.

In order:
Vector
Vector.Core
PointInSphere

Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector\BasicVector.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector.Core\BasicVector_Core.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\PointInSphere\PointInSphere.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\PointInCylinder\PointInCylinder.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json

