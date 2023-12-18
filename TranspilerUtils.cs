using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using System.Reflection;
using System.Reflection.Emit;
using FMOD;
using FSPRO;
using Microsoft.Xna.Framework.Input;
using HarmonyLib;


namespace Eddie
{
    public static class TranspilerUtils
    {
        public static int? ExtractLocalIndex(CodeInstruction instruction)
        {
            if (instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Stloc_0)
                return 0;
            else if (instruction.opcode == OpCodes.Ldloc_1 || instruction.opcode == OpCodes.Stloc_1)
                return 1;
            else if (instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Stloc_2)
                return 2;
            else if (instruction.opcode == OpCodes.Ldloc_3 || instruction.opcode == OpCodes.Stloc_3)
                return 3;
            else if (instruction.opcode == OpCodes.Ldloc || instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloca || instruction.opcode == OpCodes.Ldloca_S)
                return ExtractLocalIndex(instruction.operand);
            else
                return null;
        }

        private static int? ExtractLocalIndex(object? operand)
        {
            if (operand is LocalBuilder local)
                return local.LocalIndex;
            else if (operand is int @int)
                return @int;
            else if (operand is sbyte @sbyte)
                return @sbyte;
            else
                return null;
        }

        public static bool IsLocalLoad(CodeInstruction instruction)
        {
            return (instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Ldloc_1 ||
                    instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Ldloc_3 ||
                    instruction.opcode == OpCodes.Ldloc || instruction.opcode == OpCodes.Ldloc_S ||
                    instruction.opcode == OpCodes.Ldloca || instruction.opcode == OpCodes.Ldloca_S);
        }
        public static bool IsLocalStore(CodeInstruction instruction)
        {
            return (instruction.opcode == OpCodes.Stloc_0 || instruction.opcode == OpCodes.Stloc_1 ||
                    instruction.opcode == OpCodes.Stloc_2 || instruction.opcode == OpCodes.Stloc_3 ||
                    instruction.opcode == OpCodes.Stloc || instruction.opcode == OpCodes.Stloc_S);
        }

        public static OpCode StoreToLoad(OpCode oc)
        {
            if (oc == OpCodes.Stloc_0)
                return OpCodes.Ldloc_0;
            if (oc == OpCodes.Stloc_1)
                return OpCodes.Ldloc_1;
            if (oc == OpCodes.Stloc_2)
                return OpCodes.Ldloc_2;
            if (oc == OpCodes.Stloc_3)
                return OpCodes.Ldloc_3;
            if (oc == OpCodes.Stloc)
                return OpCodes.Ldloc;
            if (oc == OpCodes.Stloc_S)
                return OpCodes.Ldloc_S;
            throw new Exception("Not a store");
        }

    }
}
