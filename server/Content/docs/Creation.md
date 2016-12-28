# Nano API Creation
-------------------

Recommended Napack Creation Process
-----------------------------------
The recommended way to create Napacks is from the Visual Studio Extension.

Alternative Napack Creation Process
-----------------------------------
1. Create a folder containing the the .NET class source files. Set the namespace of all the classes to **PackageName**. [Note 1](#PermittedFiles)
2. Add a **PackageName.json** file inside that folder.
3. Fill in **PackageName.json** as follows:
```json
{
"description":"Put a short / medium length description here. Public API docs are auto-generated from the source code and do not belong here.",
"moreInformation":"URL for additional information, source code repository, etc. See Note 2.",
"tags":["tag1", "tag2", "tag3"],
"authorizedUserIds":["See Note 3", "id2"],
"authors":["See Note 4", "author 2"],
"license": {
    "type":"See Note 5"
    "licenseText":"Only if the type specified is not a supported license"
},
"dependencies": {
    "DependentPackageNameFollowedByMajorVersion": 5,
    "OtherDependentPackage": 2,
}
}
```

4. Upload the Napack by:
 - Visiting the Napack Framework Server to upload the Napack. TODO write this functionality.
 - Using the [Napack Client](./NapackClient.md) to upload the Napack.
5. The Napack Framework Server will automatically:
 - Verify the license is compatible with the dependent Napacks.
 - Scan the Napack to determine the publically-visible API and generate [documentation](./Documentation.md)
 - Verify the Napack contains no binary files. [Note 1] (#PermittedFiles)
 - Automatically assign a version based on the public API changes.
 - Verify the Napack name is valid [Note 6] (#ValidNames)
6. If successful, your Napack will be added to the Napacks on the Napack Framework Server.

## Notes
<a name="PermittedFiles">1</a> Binary files are not allowed within Napacks, with no exceptions. Binary files go against the spirit of Napacks, as they generally aren't small, isolated to a limited API, readable for automatic documentation generation, or come with a guarantee of backwards compatibility with MINOR or PATCH version updates. Generally speaking, if you're looking to distribute binaries along with your Napack, [NuGet](https://www.nuget.org/) is a better option than Napack.  Packages with binaries that make it through this filter are subject to removal at administrative discretion.

2 This field is required -- a reasonable default you can provide is https://napack.net/api/{PackageName} 

3 The user uploading the Napack will be automatically added if not listed.

4 The listing of authors who will be added to the license file. If a custom license is used, this field can be ignored.

5 One of the [Valid License Values](./SupportedLicenses.md),=. A word of warning: packages with copy-left, commercial, or custom licenses require explicit authorization by consumers to be used and are **not recommended.**

6 Valid Napack names match the following Regex ```[a-zA-Z0-9\.-]*```, don't start with a number, are within 10 to 100 characters (inclusive), and don't contain any prohibited subphrases. Packages with non-professional names that make it through this filter are subject to removal at administrative discretion.