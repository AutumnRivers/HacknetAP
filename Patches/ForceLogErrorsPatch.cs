using System;
using System.Reflection;

using Hacknet;
using HarmonyLib;

using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class ForceLogErrorsPatch
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionListingServer), "loadInit")]
        public static void ForceLogEntropyErrors(ILContext il)
        {
            var consoleMethod = typeof(Console).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public,
                null, [typeof(string)], []);
            var toStringMethod = typeof(object).GetMethod("ToString");

            ILCursor c = new(il);
            Func<Instruction, bool>[] getToCatch = [
                x => x.MatchPop(),
                x => x.MatchNop(),
                x => x.MatchLdcI4(0)
                ];

            c.GotoNext(getToCatch);
            c.GotoNext(getToCatch);
            c.Index++;

            Console.WriteLine(c.Next.OpCode);
            Console.WriteLine(c.Prev.OpCode);

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, toStringMethod);
            c.Emit(OpCodes.Call, consoleMethod);
            c.Emit(OpCodes.Ldstr, "Test Test Test");
            c.Emit(OpCodes.Call, consoleMethod);
        }
    }
}
