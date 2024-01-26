-------------------------------------------------------------------------------------------------
                                      Native File Browser
                                         Version 1.0.7
                                       PygmyMonkey Tools
                                     tools@pygmymonkey.com
                   	    http://pygmymonkey.com/tools/native-file-browser/
-------------------------------------------------------------------------------------------------
Thank you for buying Native File Browser!

If you have questions, suggestions, comments or feature requests, please send us an email
at tools@pygmymonkey.com



-------------------------------------------------------------------------------------------------
                            Support, Documentation, Examples and FAQ
-------------------------------------------------------------------------------------------------
You can find everything at http://pygmymonkey.com/tools/native-file-browser/



-------------------------------------------------------------------------------------------------
                                How to update Native File Browser
-------------------------------------------------------------------------------------------------
1. Delete everything under the 'FileBrowser' folder from the Project View
2. Import the latest version from the Asset Store



-------------------------------------------------------------------------------------------------
                                         Requirements
-------------------------------------------------------------------------------------------------
Please, make sure you have the following settings, otherwise Native File Browser won't work:
- Windows: There's a FileBrowser.exe file in StreamingAssets/PygmyMonkey/FileBrowser, it is
absolutely required for Native File Browser to work... don't move it from this exact location.



-------------------------------------------------------------------------------------------------
                                           Get Started
-------------------------------------------------------------------------------------------------
Native File Browser provides an easy way to use the native File Browser on Mac and Windows.
You just call a static methods from the FileBrowser class, and it will display the file browser
window for you, giving you a callback for when a file/folder is selected.

You can use the FileBrowser wrapper class if you're developing for both Mac and Windows,
this will automatically call the correct platform methods.
So, using FileBrowser.OpenFilePanel will automatically call:
- FileBrowserMac.OpenFilePanel on Mac
- FileBrowserWindows.OpenFilePanel on Windows
With that, you don't have to take care of checking the current platform with Application.platform.

But if you want, you can use the FileBrowserMac and FileBrowserWindows classes and not use the
FileBrowser wrapper class.
There's some small differences between each platform (for example, only Windows allow to define
a title), so when calling FileBrowser.OpenFilePanel(title), the title will only be used on Windows.
That's why the FileBrowserMac.OpenFilePanel does not have a title parameter.


---------------------------------------- Open File Panel -----------------------------------------
Opens the select file panel (Select a single file).
Will only allow to select files with extension defined in extensionArray.

Parameters:
- title: Title (Only available on Windows).
- startingDirectory: Starting directory (if null, will use the last opened folder).
- extensionArray: Extension array (specify only the extension, no symbols (,.*) - example "jpg",
"png"). If null, it will allow any file.
- buttonName: The name of the button. You can set this to null to use the defaut value (Only
available on Mac).
- onDone: Callback called when a file has been chosen (It has two parameters. First (bool) to 
check if the panel has been canceled. Second (string) is the file selected).


------------------------------------ Open Multiple Files Panel ------------------------------------
Opens the select multiple files panel (Select multiple files).
Will only show files with extension defined in extensionArray.

Parameters:
- title: Title (Only available on Windows).
- startingDirectory: Starting directory (if null, will use the last opened folder).
- extensionArray: Extension array (specify only the extension, no symbols (,.*) - example "jpg",
"png"). If null, it will allow any file.
- buttonName: The name of the button. You can set this to null to use the defaut value (Only
available on Mac).
- onDone: Callback called when files have been chosen (It has two parameters. First (bool) to
check if the panel has been canceled. Second (string) is the file selected array).


---------------------------------------- Open Folder Panel -----------------------------------------
Opens the select folder panel (Select a single folder).

Parameters:
- title: Title (Only available on Windows).
- startingDirectory: Starting directory (if null, will use the last opened folder).
- buttonName: The name of the button. You can set this to null to use the defaut value (Only
available on Mac).
- onDone: Callback called when a folder has been chosen (It has two parameters. First (bool) to
check if the panel has been canceled. Second (string) is the folder selected).


---------------------------------- Open Multiple Folders Panel -------------------------------------
Opens the select multiple folders panel (Select multiple folders).

Parameters:
- title: Title (Only available on Windows).
- startingDirectory: Starting directory (if null, will use the last opened folder).
- buttonName: The name of the button. You can set this to null to use the defaut value (Only
available on Mac).
- onDone: Callback called when folders have been chosen (It has two parameters. First (bool) to
check if the panel has been canceled. Second (string) is the folder selected array).


----------------------------------------- Save File Panel ------------------------------------------
Opens the save file panel (Save a file).
Will set the file types dropdown with the extensions defined in extensionArray, if not null.

Parameters:
- title: Title (Only available on Windows).
- message: A hint message on top of the panel, to display a hint to users (Only available on Mac).
- startingDirectory: Starting directory (if null, will use the last opened folder).
- defaultName: Default Name of the file to be saved. (If null, no name is pre-filled in
the inputField).
- extensionArray: Extension array (specify only the extension, no symbols (,.*) - example "jpg",
"png"). If null, it will allow any file.
- buttonName: The name of the button. You can set this to null to use the defaut value (Only
available on Mac).
- onDone: Callback called when a folder has been chosen (It has two parameters. First (bool) to
check if the panel has been canceled. Second (string) is the folder selected).



-------------------------------------------------------------------------------------------------
                                               Demo
-------------------------------------------------------------------------------------------------
You can find the demo scene in "PygmyMonkey/FileBrowser/Demo/".
This is just a simple demo scene showing you how to use the different methods of the File Browser.
Take a look at the FileBrowserDemo script to see how it's used!



-------------------------------------------------------------------------------------------------
                                          Release Notes
-------------------------------------------------------------------------------------------------
1.0.7
- NEW: Fixed issue when throwing an exception in the callback method on OS X,
- NEW: The filePath returned when cancel is clicked is now null (previously it was "cancel")

1.0.6
- NEW: Added #define NATIVE_FILE_BROWSER in case you need to use this with scripting define symbol,
- NEW: A dispatcher is now automatically included and use to "push" results to the main thread.

1.0.5
- NEW: Windows: Having multiple extensions is now displayed under a single dropdown (instead of
multiples).

1.0.4
- NEW: Added possibility to define the button name on Mac (not easily possible on Windows),

1.0.3
- FIX: Windows: Fixed issue happening when a LOT of files were selected.

1.0.2
- FIX: Windows: Fix startingDirectory not working if the path contained / instead of \. You can
now safely use paths with either / or \.
- FIX: Windows: Fixed possible issue because result return was not Trim(),
- NEW: Added FAQ "Why are you using a .exe file on Windows?".

1.0.1
- FIX: Fixed Native File Browser not working when building with architecture x86 on Mac. It now
works with any architecture (Universal, x86 and x64),
- NEW: The FileBrowser on Mac now always display the "Create folder" button.

1.0.0
- NEW: Initial release.



-------------------------------------------------------------------------------------------------
                                               FAQ
-------------------------------------------------------------------------------------------------
- Why are you using a .exe file on Windows?
At first I wanted to make a DLL that does all the work. But in order to use the File Browser UI
we're used to on Windows, you have to target at least .NET 4.5 when creating the Library (DLL).
Which is not compatible and won't load in Unity...
So you'll have to target .NET 3.5. But when you're doing that, C# will use an old, ugly UI for
the FileBrowser: http://i.imgur.com/n9wwJms.png
So I built a FileBrowser.exe file (with .NET 4.5), placed it in the StreamingAssets folder (to
be able to find/load it at runtime in a build), and communicate back to the main app by reading
the return value of the Process.Start method (see FileBrowserWindows.cs for that).

- What platforms are supported?
Only Mac and Windows standalone platforms are supported.

- How can I help?
Thank you! You can take a few seconds and rate the tool in the Asset Store and leave a nice
comment, that would help a lot ;)

- What's the minimum Unity version required?
Native File Browser will work starting with Unity 5.0.0.



-------------------------------------------------------------------------------------------------
                                           Other tools
-------------------------------------------------------------------------------------------------

--- Material UI (http://u3d.as/mQH) ---
It's now easier than ever to create beautiful Material Design layouts in your apps and games
with MaterialUI!
Almost all of the components featured in Google's Material Design specification can be created
with the click of a button, then tweaked and modified with powerful editor tools.

--- Advanced Builder (http://u3d.as/6ab) ---
Advanced Builder provides an easy way to manage multiple versions of your game on a lot of
platforms. For example, with one click, Advanced Builder will build a Demo and Paid version of
your game on 4 different platforms (that's 8 builds in one click).

--- Color Palette (http://u3d.as/cbR) ---
Color Palette will help you manage all your color palettes directly inside Unity!.
Instead of manually setting each color from the color picker, you can just pick the color you
want from the Color Palette Window. You can even apply an entire palette on all the objects in
your scene with just one click.

--- Gif Creator (http://u3d.as/icC) ---
Gif Creator allows you to record a gif from a camera, or the entire game view, directly inside Unity.



-------------------------------------------------------------------------------------------------
                                              Other
-------------------------------------------------------------------------------------------------
Native File Browser icon used is used under the LGPL license. It's avaialble here:
https://www.iconfinder.com/icons/17904/cabinet_drawer_file_filing_manager_icon