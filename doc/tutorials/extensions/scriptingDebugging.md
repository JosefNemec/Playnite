Debugging Scripts
=====================

Debugging PowerShell Scripts with PowerShell ISE
---------------------

1.  Open the 64-bit bit version of PowerShell ISE.
2.  Enter the PowerShell host process of Playnite:

    ```powershell
    Enter-PSHostProcess -Name Playnite.DesktopApp
    ```
3.  Identify the correct runspace name of your script. It will be the name of the module file. Use the commands `Get-Runspace` or `(Get-Runspace).Name` to list runspaces.
4.  Debug the script by attaching to the runspace:

    ```powershell
    Debug-Runspace -Name "LibraryExporter.psm1"
    ```
5.  The debugger will automatically break when code is executed. A tab named "[Remote File] LibraryExporter.psm1 [Read Only]" will appear in PowerShell ISE. To trigger the debugger to break, interact with the extension somehow. For example, click the Playnite menu button to break in `GetMainMenuItems`.
6.  Set breakpoints using Debug > Toggle Breakpoint.
7.  Continue execution by selecting Debug > Run/Continue.

When the breakpoint is reached, hover over any variable names to see their current value.

Typing into the PowerShell ISE console will evaluate statements in the current context. For example, you can use the `$PlayniteAPI` variable to interactively develop and test code. You may also interactively inspect local variables and their properties.

The Playnite interface will appear frozen while the debugger is paused on a breakpoint.

Debugging IronPython Scripts
---------------------

It is currently not possible to debug IronPython extensions without building Playnite from source and debugging with Visual Studio.
