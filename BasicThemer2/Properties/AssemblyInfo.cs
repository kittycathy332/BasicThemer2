using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Win32 VERSIONINFO mapping (Explorer "Details" tab):
//   AssemblyTitle                -> File description
//   AssemblyDescription          -> Comments
//   AssemblyCompany              -> Company
//   AssemblyProduct              -> Product name
//   AssemblyCopyright            -> Copyright
//   AssemblyTrademark            -> Legal trademarks
//   AssemblyFileVersion          -> File version
//   AssemblyInformationalVersion -> Product version
// Attributes left empty are hidden from the Details tab entirely.
[assembly: AssemblyTitle("BasicThemer 2")]
[assembly: AssemblyDescription("Apply the Windows Vista/7 basic theme to Windows Vista-11, without disabling the DWM composition.")]
[assembly: AssemblyConfiguration("")]
// NOTE: Explorer always labels this row "Company" (公司) -- the label itself comes from
// the shell and cannot be renamed from here, only the value can. Indie programs commonly
// use it for the maker's name, so the value below reads as a credit rather than a company.
// Set it to "" if you would rather hide the row entirely (empty attributes are hidden).
[assembly: AssemblyCompany("Ingan121 (original), kittycathy332 (fork)")]
[assembly: AssemblyProduct("BasicThemer 2")]
[assembly: AssemblyCopyright("Copyright © 2026 kittycathy332 (MIT)")]
[assembly: AssemblyTrademark("Released under the MIT License")]
[assembly: AssemblyCulture("")]

// Shows 0.6.1 as the product version as well (same value as the file version).
[assembly: AssemblyInformationalVersion("0.6.1")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("78ad378d-446e-461d-a415-a22a3feae33e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
// NOTE: keep AssemblyVersion, AssemblyFileVersion and AssemblyInformationalVersion
// in sync, and update the version marker in the repo root "latest.txt" as well --
// that file is what the in-app update check compares against.
[assembly: AssemblyVersion("0.6.1")]
[assembly: AssemblyFileVersion("0.6.1")]
[assembly: NeutralResourcesLanguage("en")]
