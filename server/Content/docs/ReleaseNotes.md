#ReleaseNotes
-------------

Status
------
**Incomplete** -- The system version will remain at 0.1.0 until core functionality and bug fixes required for full usage of the Napack Framework are completed / fixed.

The Napack Database is subject to arbitrary deletion until there are no more remaining tasks in this list.

Core Functionality
------------------
* Visual Studio Extension to integrate package consumption / creation with the Visual Studio workflow. Update [Download.md](./Download.md), [Creation.md](Creation.md), and [Search.md](./Search.md) when done.

Final Release Steps
-------------------
* Testing framework needs overhaul to include many more FTs, UTs, and setup to test the website locally with a test data store.
* Documentation and CSS need a complete overhaul and scrub.
* Create several Napacks around key searching, sorting, vectors, and algorithmic functionality.

Known Issues
------------
These issues will be fixed post-release, if possible.
* The Napack Client doesn't support auto-update workflow for the Napack Client.
* Have option to add to project file directly, making it appear in solution explorer and giving Intellisense support for free.
* NuGet package still auto-includes the exe incorrectly.
* Intellisense support for added Napacks requires a project reload.
* Search doesn't include a code search or documentation search.
* The Napack Server Framework should have a webpage for Napack upload/update and user registration. Update [Creation.md](./Creation.md) and [Registration.md](Registration.md) when done.
* Graph view actually doesn't do anything yet.
* Two clients updating a Napack at the exact same time may overwrite each other.