# Nano API Creation
-------------------

1. Create a folder containing the the .NET class source files. Set the namespace of all the classes to **PackageName**. [Note 1] (#PermittedFiles)
2. Add a package configuration JSON file inside that folder.
3. Fill in the package configuration JSON file as follows:
```json
{
"name":"PackageName",
"description":"Put a short / medium length description here. Public API docs are auto-generated from the source code and do not belong here.",
"moreInformation":"URL for additional information, source code repository, etc",
"tags":["tag1", "tag2", "tag3"],
"authorizedUserHashes":["hash1", "hash2", "hash3"],
"authors":["Note 3", "author 2"],
"license":
{
    "type":"See Note 4"
    "licenseText":"Only if the type specified is not a supported license"
},
"dependencies":
{
    "DependentPackageNameFollowedByMajorVersion":5,
    "OtherDependentPackage":2,
}
}
```

4. Upload the Napack by using one of the possible upload mechanisms:
 - Visit the Napack Framework Server to upload the Napack. 
 - Use the [Napack Client](./NapackClient.md) to upload the Napack.
 - **TODO integrate with Visual Studio**
5. The Napack Framework Server will automatically:
 - Add the user uploading the Napack to the publishers of the Napack.
 - (Update only) Verify the current user has permissions to update the specified Napack.
 - Verify the license is compatible with the dependent Napacks.
 - Scan the Napack to determine the publically-visible API and use the .NET documentation to generate visible API docs.
 - Verify the Napack contains no binary files. [Note 1] (#PermittedFiles)
 - Automatically assign a version based on the public API changes.
 - Verify the Napack name is valid [Note 5] (#ValidNames)
6. If successful, your Napack will be added to the Napacks on the Napack Framework Server.

## Notes
<a name="PermittedFiles">1</a> Binary files are not allowed within Napacks, with no exceptions. Binary files go against the spirit of Napacks, as they generally aren't small, isolated to a limited API, or come with a guarantee of backwards compatibility with MINOR or PATCH version updates. Generally speaking, if you're looking to distribute binaries along with your Napack, [NuGet](https://www.nuget.org/) is a better option than Napack.

2 Either the username or email address of the user uploading the Napack to the Napack Framework Server.  

3 The listing of authors who will be added to the license file. If a custom license is used, this field can be ignored.

4 One of the [Valid License Values](./SupportedLicenses.md),=. A word of warning: packages with copy-left, commercial, or custom licenses require explicit authorization by consumers to be used and are **not recommended.**

5 Valid Napack names match the following Regex ```[a-zA-Z0-9\.-]*```, don't start with a number, are within [10, 100] characters (inclusive), and don't contain any prohibited subphrases. Packages with non-professional names (obscene (not 'E' rated), derrogatory, racist, etc) are subject to removal.