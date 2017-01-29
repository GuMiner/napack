Consuming Nano APIs
-------------------
1. [Download](./Download.md) the Napack Framework and add it to your project.
 - Your project will now include a **Napack.json**, **Napack.attributions**, and **NapackSettings.json** file.
 - **Napack.json**: Defines the Napacks your project uses.
 - **Napack.attributions**: Lists attributions auto-generated from the Napacks you use.
 - **NapackSettings.json**: Declares settings customizing the behavior of the Napack Framework.
2. Add the Napack name and version of the Napack you want to use to the **Napack.json** file. 
 - For example, if you add the "PointInSphere" package, version 2.1.2, your **Napack.json** file will look like:
 -  ```{ "PointInSphere": "2.1.2" }```
4. Rebuild your project. 
 - In the example above, *PointInSphere* and dependent Napacks will be automatically downloaded and placed in the *{Project Root}/napacks/* folder.
5. For each source file you want to use the packages in, write ```using Package_Major``` in that file.
  - To use *PointInSphere*, you would write ```using PointSphere_2```

Attributing Consumed Napacks
----------------------------
The default Napack settings will prevent you from automatically including Napacks that force your code to be open-sourced or the purchase of a commercial license. Upon releasing your code, simply append the **Napack.attributions** file to your license file.

Default Settings
----------------
For more information on the default settings and how to change them, see the [Napack.settings](./NapackSettings.md) page.

The following paragraph details default functionality that is tracked in [this issue](https://github.com/GuMiner/napack/issues/2).

The default settings will periodically check for non-breaking MINOR or PATCH changes to your Napacks and update those Napacks automatically. For example, if PointInSphere released version 2.2.0, your **Napack.json** file would be updated to use that version instead of 2.1.2.