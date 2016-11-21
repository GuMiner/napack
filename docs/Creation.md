# Nano API Creation
-------------------

1. Copy the .NET class source files containing the public classes and methods part of your API to a **PackageName** folder. [Note 1] (#PermittedFiles)
2. Add a **PackageName.json** file inside that folder.
3. Fill in the **PackageName.json** file as follows:
```json
{
"name":"PackageName",
"version":"1.2.3",
"authors":["Note 3", "author 2"],
"license":"Note 4",
"description":"Put a short description here. API docs are auto-generated and do not belong here.",
"moreinfo":"URL for additional information, source code repository, etc"
"tags":["tag1", "tag2", "tag3"]
}
```

4. Visit the Napack Framework Server to upload the Napack. TODO write a cmd and GUI version.
5. The Napack Framework Server will automatically:
 - Add the user uploading the Napack to the publishers of the Napack.
 - (Update only) Verify the current user has permissions to update the specified Napack.
 - Add the list of Napacks this Napack depends upon to the package information.
 - Verify the license is compatible with the dependent Napacks.
 - Scan the Napack to determine the publically-visible API and use the .NET documentation to generate visible API docs.
 - Verify the Napack contains no binary files. [Note 1] (#PermittedFiles)
 - Verify that API-breaking changes have updated the MAJOR package version.
 - Verify the Napack name is valid [Note 5] (#ValidNames)
6. If successful, your Napack will be added to the Napacks on the Napack Framework Server.

## Notes
<a name="PermittedFiles">1</a> Binary files are not allowed within Napacks, with no exceptions. Binary files go against the spirit of Napacks, as they generally aren't small, isolated to a limited API, or come with a guarantee of backwards compatibility with MINOR or PATCH version updates. Generally speaking, if you're looking to distribute binaries along with your Napack, [NuGet](https://www.nuget.org/) is a better option than Napack.

2 Either the username or email address of the user uploading the Napack to the Napack Framework Server.  

3 The listing of authors who will be added to the license file. If a custom license is used, this field can be ignored.

4 One of the [Supported Licenses](./SupportedLicenses.md), or the filename of a file containing custom license information. A word of warning: packages with custom licenses require explicit authorization by consumers to be used and are **not recommended.**

5 Valid Napack names match the following Regex ```[a-zA-Z0-9\.-]*```. Packages with non-professional names (obscene (not 'E' rated), derrogatory, racist, etc) are subject to removal.