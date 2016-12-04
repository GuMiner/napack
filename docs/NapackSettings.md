# Napack Settings

Changes to the **Napack.settings** file are applied automatically on the next rebuild of your project.

###Default Settings
```json
{
"napackFrameworkServer":"https://WhereYouDownloadedTheClientFrom:443",
"allowCommercial":"false",
"allowCopyLeft":"false",
"allowCustomLicenses":"false",
"autoUpdatePatch":"true",
"autoUpdateMinor":"true",
"warnMajorUpdates":"false",
"lastUpdateTime":"2016-09-20T06:09:48.6681779Z",
"autoUpdateInterval":"01:00:00"
}
```

###Detailed Informaiton
- **Allow Commercial:** If ```true```, allows commercially-licensed Napack packages to be added to your project. **You are responsible for ensuring you follow all posted license terms.**
- **Allow Copy-left:** If ```true```, allows copy-left-licensed Napack packages to be added to your project. **May be incompable with ```allowCommercial```. This usually makes your project fall under the copy-left license terms.**
- **Allow Custom Licenses**: If ```true```, allows custom-licensed Napack packages to be added to your project. **You are responsible for ensuring you follow all posted license terms.**
- **AutoUpdatePatch:** Automatically updates Napack packages that have the same MAJOR and MINOR versions, but newer PATCH versions. **Ignored if autoUpdateMinor is set to ```true```.**
- **AutoUpdateMinor:** Automatically updates Napack packages that have the same MAJOR version, but newer MINOR  versions. Implies ```autoUpdatePatch``` is true.
- **warnMajorUpdates:** If true, a build warning is generated for each consumed Napack package that has a newer MAJOR versions. Does not apply to dependent Napack packages consumed by the listed Napack packages in **Napack.json**.
- **lastUpdateTime:** The time that the auto-update process was last run.
- **autoUpdateInterval:** The interval that Napack will wait before checking for updates. As Napack will only check for updates as part of a build, Napack updates will usually be less frequent.
