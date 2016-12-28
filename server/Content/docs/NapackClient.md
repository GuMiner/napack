#Napack Client
About
---------
The Napack Client ("Napack.Client.exe") is a command-line executable that registers users, uploads new Napacks, and downloads consumed Napacks. This client is used by the Visual Studio extension and by the Napack build integration, but can also be called manually.


Operations
----------
TODO fill in more than just examples.

###Registration
Napack.Client.exe -Operation Register -UserEmail email -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json

###Uploading Napacks
Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector\BasicVector.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\BasicVector.Core\BasicVector.Core.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\PointInSphere\PointInSphere.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json
Napack.Client.exe -Operation Upload -PackageJsonFile D:\napack\client\IntegrationTestContent\Napacks\PointInCylinder\PointInCylinder.json -NapackSettings D:\napack\client\IntegrationTestContent\NapackSettings.json

###Downloading Napacks
Napack.Client.exe -Operation Update -NapackDirectory D:\napack\Demo\napacks -NapackSettingsFile D:\napack\Demo\NapackSettings.json -NapacksFile D:\napack\Demo\Napacks.json -ProjectDirectory D:\napack\Demo
