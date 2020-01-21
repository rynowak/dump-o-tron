using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DumpOTron
{
    internal abstract class MetadataVisitor
    {
        public void Visit(AssemblyDefinition assembly)
        {
            if (VisitCore(assembly))
            {
                foreach (var module in assembly.Modules)
                {
                    if (VisitCore(module))
                    {
                        foreach (var type in module.Types)
                        {
                            if (VisitCore(type))
                            {
                                foreach (var method in type.Methods)
                                {
                                    if (VisitCore(method) && method.Body != null)
                                    {
                                        VisitMethodBody(method.Body);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool VisitCore(AssemblyDefinition assembly)
        {
            return true;
        }

        protected virtual bool VisitCore(ModuleDefinition module)
        {
            return true;
        }

        protected virtual bool VisitCore(TypeDefinition type)
        {
            return true;
        }

        protected virtual bool VisitCore(MethodDefinition method)
        {
            return true;
        }

        protected virtual void VisitMethodBody(MethodBody body)
        {
        }
    }
}
