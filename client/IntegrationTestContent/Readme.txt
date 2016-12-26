Test Instructions
- Run: "Napack.Client.exe -Operation Register -UserEmail email -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json"
- Update NapackSettings.json to have the default UserId (email, string) and AuthenticationKeys (secrets, string list) included.
- Run: "Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napack\PointInSphere.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json"

