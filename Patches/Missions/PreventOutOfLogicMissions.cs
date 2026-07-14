using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hacknet;

using Mono.Cecil.Cil;
using MonoMod.Cil;

using System.Reflection;
using HacknetArchipelago.Managers;
using System.Runtime.InteropServices;
using HacknetArchipelago.Daemons;
using HacknetArchipelago.Extensions;
using Pathfinder.Util;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class PreventOutOfLogicMissions
    {
        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionListingServer), "draw")]
        /*
         * This patch prevents the player from being able to accept Entropy missions if they don't have
         * the required items to accept it.
         */
        public static void PreventAcceptingOutOfLogicEntropyMissions(ILContext il)
        {
            ILCursor c = new(il);

            ILLabel missionUnavailableLabel = il.DefineLabel();
            var missionUnavailableLocal = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Import(typeof(string))));

            FieldInfo missionsField =
                typeof(MissionListingServer).GetField("missions", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo targetIndexField =
                typeof(MissionListingServer).GetField("targetIndex", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo getFromListMethod = typeof(List<ActiveMission>).GetMethod("get_Item",
                BindingFlags.Instance | BindingFlags.Public, null, [typeof(int)], null);

            c.Goto(0);
            c.Emit(OpCodes.Ldstr, "This mission is out of logic.");
            c.Emit(OpCodes.Stloc_S, (byte)missionUnavailableLocal);

            bool startExists = c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt(out var _),
                x => x.MatchNop(),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(out var _),
                x => x.MatchLdfld(out var _),
                x => x.MatchBrfalse(out var _));

            if (!startExists) return;

            c.GotoNext(); // ldarg.2 -> callvirt
            c.GotoNext(); // callvirt -> nop

            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit(OpCodes.Ldfld, missionsField); // this.missions
            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit(OpCodes.Ldfld, targetIndexField); // this.targetIndex
            c.Emit(OpCodes.Callvirt, getFromListMethod); // this.missions[this.targetIndex] (will be ActiveMission)

            c.EmitDelegate<Func<ActiveMission, (bool, string)>>((mission) =>
            {
                var hasRequiredItemsForMission = ArchipelagoLocations.HasItemsForLocation(mission.email.subject);
                string unavailableReason = "This mission is out of logic.";

                if (!hasRequiredItemsForMission)
                {
                    unavailableReason = "You are missing required Archipelago item(s).";
                }

                if (HacknetAPCore.SlotData.EnableFactionAccess && InventoryManager.FactionAccess < FactionAccess.Entropy
                                                               && hasRequiredItemsForMission)
                {
                    hasRequiredItemsForMission = false;
                    unavailableReason = "You don't have enough Progressive Faction Access.";
                }

                return (hasRequiredItemsForMission, unavailableReason);
            });

            // Unpack tuple (C#-style)
            c.Emit(OpCodes.Dup); // duplicate tuple
            c.Emit(OpCodes.Ldfld, typeof(ValueTuple<bool, string>).GetField("Item2"));
            c.Emit(OpCodes.Stloc_S, (byte)missionUnavailableLocal); // store the message
            c.Emit(OpCodes.Ldfld, typeof(ValueTuple<bool, string>).GetField("Item1")); // extract bool

            ILLabel skipLabel = il.DefineLabel();

            c.Emit(OpCodes.Brfalse_S, skipLabel); // If false, show Mission Unavailable screen

            bool middleExists = c.TryGotoNext(MoveType.After,
                x => x.MatchNop(),
                x => x.MatchNop(),
                x => x.MatchBr(out var _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(out var _),
                x => x.MatchBrfalse(out var _),
                x => x.MatchLdloc(out int _),
                x => x.MatchBr(out var _),
                x => x.MatchLdcI4(1),
                x => x.MatchNop(),
                x => x.MatchStloc(out int _),
                x => x.MatchLdloc(out int _),
                x => x.MatchBrtrue(out var _),
                x => x.MatchNop());
            if (!middleExists) return;
            c.Index -= 1;

            c.MarkLabel(skipLabel);

            bool finalExists = c.TryGotoNext(x => x.MatchLdstr("User ID Assigned to Different Faction"));
            if (!finalExists) return;
            c.Next.OpCode = OpCodes.Ldloc_S;
            c.Next.Operand = (byte)missionUnavailableLocal;
        }

        public const string KAGUYA_TRIALS_SUBJECT = "The Kaguya Trials";
        public const string CSEC_COMP_ID = "mainHub";

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.initDaemons))]
        public static void ReplaceCsecDaemon(Computer __instance)
        {
            if(__instance.idName != CSEC_COMP_ID) return;
            
            ArchipelagoMissionListingDaemon newDaemon = new(__instance, "AP Mission Listing",
                __instance.os);
            __instance.daemons.Add(newDaemon);

            // removes duplicates
            __instance.daemons = __instance.daemons.DistinctBy(d => d.name).ToList();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionHubServer), "navigatedTo")]
        public static bool PreventViewingOldCsecDaemon(MissionHubServer __instance)
        {
            var sysFolder = __instance.comp.getFolderFromPath("sys");
            var bootModuleList = sysFolder.searchForFile("DefaultBootModule.txt");
            bootModuleList.data = "Archipelago Mission Listing";
            
            Programs.disconnect([], OS.currentInstance);
            OS.currentInstance.terminal.writeLine("Do not connect to the old daemon!\n" +
                                                  "(If you loaded a save, reconnect to the node.)");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionHubServer), "addMission")]
        public static bool ReplaceCsecHubAddMission(MissionHubServer __instance,
            ActiveMission mission,
            bool insertAtTop,
            int desiredInsertionIndex)
        {
            var csecComp = __instance.comp;
            var newDaemon = csecComp.getDaemon(typeof(ArchipelagoMissionListingDaemon));

            if (newDaemon == null) return true;

            var archiDaemon = (ArchipelagoMissionListingDaemon)newDaemon;
            
            archiDaemon.AddMissionToListing(mission, desiredInsertionIndex);
            return false;
        }
    }
}
