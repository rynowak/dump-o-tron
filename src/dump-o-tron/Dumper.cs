using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DumpOTron
{
    internal static class Dumper
    {
        public static Task DumpAsync(IConsole console, DirectoryInfo directory)
        {
            var assemblyFiles = directory.EnumerateFiles("*.dll", new EnumerationOptions() { RecurseSubdirectories = true, });
            
            var assemblyResolver = new AssemblyResolver();
            var metadataResolver = new MetadataResolver(assemblyResolver);

            var parameters = new ReaderParameters()
            {
                AssemblyResolver = assemblyResolver,
                MetadataResolver = metadataResolver,
                ReadSymbols = false,
            };

            var assemblies = new List<AssemblyDefinition>();
            assemblies.AddRange(assemblyFiles.Select(f => AssemblyDefinition.ReadAssembly(f.FullName, parameters)));
            
            foreach (var assembly in assemblies)
            {
                assemblyResolver.Add(assembly);
            }

            var pnse = FindType(assemblies, "System.PlatformNotSupportedException");
            if (pnse == null)
            {
                throw new CommandException("Cannot find PlatformNotSupportedException.");
            }

            var visitor = new PNSEUsageVisitor();
            foreach (var assembly in assemblies)
            {
                visitor.Visit(assembly);
            }

            foreach (var kvp in visitor.Visited)
            {
                if (kvp.Value)
                {
                    console.Out.WriteLine(kvp.Key.FullName);
                }
            }

            return Task.CompletedTask;
        }

        private static TypeDefinition FindType(List<AssemblyDefinition> assemblies, string fullName)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var module in assembly.Modules)
                {
                    foreach (var type in module.Types)
                    {
                        if (string.Equals(type.FullName, fullName, StringComparison.Ordinal))
                        {
                            return type.Resolve();
                        }
                    }
                }
            }

            return null;
        }
    }
}
