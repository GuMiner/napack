The Napack Client &mdash; Napack.Client.exe &mdash; is a command-line executable that registers users, creates Napacks, deletes Napacks, and downloads consumed Napacks. This client is used by the Napack build integration, but can also be called manually.

Operations
----------
**For all operations**: The operation takes in a **-NapackSettingsFile** argument, which configures how the client runs.

###User Registration
Registers a new user with the Napack Framework Server
<br />
<br />
```Napack.Client.exe -Operation Register -UserEmail <Email> -NapackSettingsFile <Path> -SaveAsDefault <Boolean>```
<br />
* **UserEmail**: The email of the user to be registered. <br />
* **SaveAsDefault**: If true, creates or replaces the user's default ID and authentication keys. Defaults to false.

###User Verification
Verifies a newly-created user with the Napack Framework Server
<br />
<br />
```Napack.Client.exe -Operation VerifyEmail -UserEmail <Email> -VerificationCode <GUID> -NapackSettingsFile <Path>```
<br />
* **UserEmail**: The email of the user to be registered. <br />
* **VerificationCode**: The GUID verification code sent to the user's email.

###Create or Update a Napack
Uploads a napack definition, creating a new Napack or updates an existing one.
<br />
<br />
```Napack.Client.exe -Operation Upload -PackageFile <Path> -NapackSettingsFile <Path> -ForceMajorUpversioning <Boolean> -ForceMinorUpversioning <Boolean> -UserId <Email> -AuthorizationKeys <Keys>```
<br />
* **PackageFile**: The path to the [PackageName.json file](./Creation.md) for Napack to create / update. <br/>
* **ForceMajorUpversioning**: If this is a package update, forces the update to increment the major version. Ignored for new packages, defaults to false. <br />
* **ForceMinorUpversioning**: If this is a package update, forces the update to increment the minor version. Ignored for new packges, overridden if **ForceMajorUpversioning** is set to true, defaults to false. <br />
* **UserId**: The email of the user authorized to perform the creation or Napack update. If not present, the default user ID will be used. <br />
* **AuthorizationKeys**: The semicolon-deliminated keys authorizing the user to create or update this Napack. If not present, the default user authentication keys will be used.

###Update Napack Metadata
Updates the metadata of an existing Napack.
<br />
<br />
```Napack.Client.exe -Operation UpdateMetadata -PackageFile <Path> -NapackSettingsFile <Path> -UserId <Email> -AuthorizationKeys <Keys>```
<br />
* **PackageFile**: The path to the [PackageName.json file](./Creation.md) for Napack to create / update. <br/>
* **UserId**: The email of the user authorized to perform the creation or Napack update. If not present, the default user ID will be used. <br />
* **AuthorizationKeys**: The semicolon-deliminated keys authorizing the user to create or update this Napack. If not present, the default user authentication keys will be used.

###Download Napacks
Updates Napacks consumed within the current project.
<br />
<br />
```Napack.Client.exe -Operation Update -NapacksFile <Path> -NapackSettingsFile <Path> -NapackDirectory <Path> -ProjectDirectory <Path>```
<br />
* **NapacksFile**: The path to the **Napack.json** file listing the napacks being used. <br />
* **NapackDirectory**: The directory where downloaded Napacks should be placed. <br />
* **ProjectDirectory**: The directory where the project is and the **Napack.attributions** file should be placed.