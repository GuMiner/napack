# Consuming Nano APIs
---------------------

1. [Download] (./Download.md) the Napack Framework and add it to your project.
 - Your project will now include a **Napack.json**, **Napack.attributions**, and **Napack.settings** file.
 - **Napack.json**: Defines the Napacks your project uses.
 - **Napack.attributions**: Lists attributions auto-generated from the Napacks you use.
 - **Napack.settings**: Declares settings customizing the behavior of the Napack Framework.
2. [Search] (./Search.md) for a [Napack] (./Creation.md) that provides the functionality you want. For example, you can search for "Point within Sphere" to find an API that tests if a point is within a sphere.
3. Add the Napack name and version to the **Napack.json** file. 
 - For example, if you add the "PointSphere" package, version 2.1.2, your **Napack.json** file will look like:
 -  ```{ "PointSphere": "2.1.2" }```
4. Build your project. 
 - In the example above, *PointSphere* and dependent Napacks will be automatically downloaded and placed in the *{Project Root}/napacks/PointSphere.2.1.2/* folder.
5. For each source file you want to use the packages in, write ```using Package.Major``` in that file.
  - To use *PointSphere*, you would write ```using PointSphere.2```

The default Napack settings will prevent you from automatically including Napacks that force your code to be open-sourced or require a commercial license -- simply add the **Napack.attributions** file to your license file. The default settings will also periodically check for non-breaking MINOR or PATCH changes to your Napacks and update them automatically. 

For more information on how to change these settings, see the [Napack.settings] (./NapackSettings.md) page.