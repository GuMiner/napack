#Documentation
When packages are updated, the Napack framework will automatically scan the publically-facing API of the package code to perform [automatic versioning](./Versioning.md). This operation also generates documentation used to automatically form package documentation.

Operation
---------
Documentation is only generated for publically-facing classes, interfaces, constructors, methods, fields, and properties. Any [Roslyn trivia](https://github.com/dotnet/roslyn/wiki/Roslyn%20Overview) before these items will be included in the documentation, as well as method return types, parameters, etc. 

Results
-------
Each package's API can be found by visiting [http://napack.net/api/{PackageName}](http://napack.net/api/{PackageName})
