using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DumpOTron
{
    internal class PNSEUsageVisitor : MetadataVisitor
    {
        public PNSEUsageVisitor()
        {
            Visited = new Dictionary<MethodDefinition, bool>();
        }

        public Dictionary<MethodDefinition, bool> Visited { get; }

        protected override void VisitMethodBody(MethodBody body)
        {
            for (var i = 0; i < body.Instructions.Count; i++)
            {
                var instruction = body.Instructions[i];
                if (instruction.OpCode == OpCodes.Newobj && 
                    instruction.Operand is MethodReference ctor)
                {
                    var type = ctor.DeclaringType;
                    if (type.FullName == "System.PlatformNotSupportedException")
                    {
                        Visited.TryAdd(body.Method, true);
                        break;
                    }
                }
            }

            Visited.TryAdd(body.Method, false);
        }
    }
}
