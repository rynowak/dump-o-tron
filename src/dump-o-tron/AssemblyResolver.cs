using Mono.Cecil;

namespace DumpOTron
{
    internal class AssemblyResolver : DefaultAssemblyResolver
    {
        public void Add(AssemblyDefinition assembly)
        {
            RegisterAssembly(assembly);
        }
    }
}
