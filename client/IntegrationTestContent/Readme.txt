Test Instructions
- Run: "Napack.Client.exe -Operation Register -UserEmail email -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json"
- Update NapackSettings.json to have the default UserId (email, string) and AuthenticationKeys (secrets, string list) included.

In order:
Vector
Vector.Core
PointInSphere

Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector\BasicVector.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector.Core\BasicVector.Core.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\PointInSphere\PointInSphere.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json

