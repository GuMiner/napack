#ReleaseNotes
-------------

Status
------
**Incomplete** -- The system version will remain at 0.1.0 until core functionality and bug fixes required for full usage of the Napack Framework are completed / fixed.

The Napack Database is subject to arbitrary deletion until there are no more remaining tasks in this list.

Bugs
----
* NapackAnalyst doesn't have support for interfaces yet.
* Napack NuGet package automatically imports the client binaries into the package, which is not intended.
* Case sensitivity / insensitivity issues accross the board, esp. related to JSON serialization of package names.
* Default user ID / authentication shouldn't be in the NapackSettings.json file, but somewhere else reasonably common and accessible.
* Significantly increase unit testing and functional testing, so that the website can be tested locally with the in-memory store.
* The napack icon needs professional attention.
* Search is missing the 'click for details' function on retrieved items linking to API, versions, authors, etc.

Core Functionality
------------------
* Add a persistent Napack database store instead of an in-memory store.
* Validate the workflow (admin) for recalling and deleting packages.
* Add a user deregistration pathway.
* Visual Studio Extension to integrate package consumption / creation with the Visual Studio workflow. Update [Download.md](./Download.md), [Creation.md](Creation.md), and [Search.md](./Search.md) when done.
* The Napack NuGet package doesn't automatially create or integrate with MSBuild without manual intervention. Update [Download.md](./Download.md) when done.
* The Napack Server Framework needs a webpage for Napack upload/update and user registration. Update [Creation.md](./Creation.md) and [Registration.md](Registration.md) when done.
* Email validation for user registration, and appropriate backend state for that.
* Create a site statistics page detailing user activity, Napacks downloaded, uptime, etc.
* Create a tree-view page to navigate through Napacks and their dependencies.

Final Release Steps
-------------------
* CSS needs a complete overhaul and scrub.
* Validate database backup is working
* Create several Napacks around key searching, sorting, vectors, and algorithmic functionality.
* Scrub through the documentation one more time.