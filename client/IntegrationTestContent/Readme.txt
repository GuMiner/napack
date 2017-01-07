Test Instructions
1. Register:
  Napack.Client.exe -Operation Register -UserEmail gus.gran@gmail.com -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json -SaveAsDefault true

2. Upload in order:

Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector\BasicVector.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector.Core\BasicVector_Core.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\PointInSphere\PointInSphere.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageFile D:\napack\client\IntegrationTestContent\Napacks\PointInCylinder\PointInCylinder.json -NapackSettingsFile D:\napack\client\IntegrationTestContent\NapackSettings.json

