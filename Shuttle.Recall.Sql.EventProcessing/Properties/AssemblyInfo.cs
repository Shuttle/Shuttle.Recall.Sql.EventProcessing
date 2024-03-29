using System.Reflection;
using System.Runtime.InteropServices;

#if NETFRAMEWORK
[assembly: AssemblyTitle(".NET Framework")]
#endif

#if NETCOREAPP
[assembly: AssemblyTitle(".NET Core")]
#endif

#if NETSTANDARD
[assembly: AssemblyTitle(".NET Standard")]
#endif

[assembly: AssemblyVersion("16.1.1.0")]
[assembly: AssemblyCopyright("Copyright (c) 2022, Eben Roux")]
[assembly: AssemblyProduct("Shuttle.Recall.Sql.EventProcessing")]
[assembly: AssemblyCompany("Eben Roux")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyInformationalVersion("16.1.1")]
[assembly: ComVisible(false)]