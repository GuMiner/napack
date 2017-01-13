#ReleaseNotes
-------------

Status
------
**Incomplete** -- The system version will remain at 0.1.0 until core functionality and bug fixes required for full usage of the Napack Framework are completed / fixed.

The Napack Database is subject to arbitrary deletion until there are no more remaining tasks in this list.

Bugs
----
* NuGet package still auto-includes the exe incorrectly.
* Case sensitivity / insensitivity issues accross the board, esp. related to JSON serialization of package names.
* Significantly increase unit testing and functional testing, so that the website can be tested locally with the in-memory store.
* Search is missing the 'click for details' function on retrieved items linking to API, versions, authors, etc.
* Request IP throttling internals and country geolocation database reading work.
* Data storage layer needs another layer of abstraction to properly avoid duplicating business logic.

Core Functionality
------------------
* Backend email setup on Napack to support email validation; fix the default email and settings at that time.
* Validate all the workflows, admin included.
* Visual Studio Extension to integrate package consumption / creation with the Visual Studio workflow. Update [Download.md](./Download.md), [Creation.md](Creation.md), and [Search.md](./Search.md) when done.
* The Napack Server Framework needs a webpage for Napack upload/update and user registration. Update [Creation.md](./Creation.md) and [Registration.md](Registration.md) when done.
* Create a tree-view page to navigate through Napacks and their dependencies.
* Create a system stats page listing requests per country for the current day.
* Search should include code search, doc search, and main search.

Misc
----
* Have option to add to project file directly, making it appear in solution explorer and giving Intellisense support for free.
* Auto-update workflow for the Napack Client.

Final Release Steps
-------------------
* CSS needs a complete overhaul and scrub.
* Validate database backup is working
* Create several Napacks around key searching, sorting, vectors, and algorithmic functionality.
* Scrub through the documentation one more time.

Known Issues
------------
These issues will be fixed post-release, if possible.
* Intellisense support for added Napacks requires a project reload.