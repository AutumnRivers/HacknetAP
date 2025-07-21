using System;
using System.Collections.Generic;

using HarmonyLib;
using Hacknet;

using Mono.Cecil.Cil;
using MonoMod.Cil;

using System.Reflection;
using HacknetArchipelago.Managers;
using System.Runtime.InteropServices;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class PreventOutOfLogicMissions
    {
        public const string ENTROPY_ID = "entropy00";

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionListingServer),"draw")]
        /*
         * This patch prevents the player from being able to accept Entropy missions if they don't have
         * the required items to accept it.
         */
        public static void PreventAcceptingOutOfLogicEntropyMissions(ILContext il)
        {
            OS os = OS.currentInstance;
            if(os != null)
            {
                if(os.connectedComp != null)
                {
                    if (os.connectedComp.idName != ENTROPY_ID) return;
                }
            }

            ILCursor c = new(il);

            ILLabel missionUnavailableLabel = il.DefineLabel();
            var missionUnavailableLocal = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Import(typeof(string))));

            FieldInfo missionsField = typeof(MissionListingServer).GetField("missions", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo targetIndexField = typeof(MissionListingServer).GetField("targetIndex", BindingFlags.Public | BindingFlags.Instance);
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

                if(!hasRequiredItemsForMission)
                {
                    unavailableReason = "You are missing required Archipelago item(s).";
                }

                if(HacknetAPCore.SlotData.EnableFactionAccess && InventoryManager._factionAccess < FactionAccess.Entropy
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

        [HarmonyILManipulator]
        [HarmonyPatch(typeof(MissionHubServer),"doContractPreviewScreen")]
        /*
         * Same as above, but for CSEC.
         * Labyrinths doesn't need a patch - everything is locked behind
         * the Kaguya Trials.
         */
        public static void PreventAcceptingOutOfLogicCSECMissions(ILContext il)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                HacknetAPCore.Logger.LogWarning("!! PLUGIN DETECTED AS BEING RAN ON LINUX !!\n" +
                    "Due to a bug within Mono, the CSEC logic wall is disabled on Linux. This will be reversed when " +
                    "Pathfinder finishes its coreclr branch. Relevant issue:\n" +
                    "https://github.com/AutumnRivers/HacknetAP/issues/3");
                return;
            }

            OS os = OS.currentInstance;
            if (os != null)
            {
                if (os.connectedComp != null)
                {
                    if (os.connectedComp.idName != "mainHub") return;
                }
            }

            ILCursor c = new(il);

            FieldInfo daemonOSField = typeof(Daemon).GetField("os",
                BindingFlags.Public | BindingFlags.Instance);
            FieldInfo osCurrentMissionField = typeof(OS).GetField("currentMission",
                BindingFlags.Public | BindingFlags.Instance);

            ILLabel missionUnavailableLabel = il.DefineLabel();
            var missionUnavailableLocal = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Import(typeof(string))));

            c.Goto(0);
            c.Emit(OpCodes.Ldstr, "This mission is out of logic.");
            c.Emit(OpCodes.Stloc_S, (byte)missionUnavailableLocal);

            var unavCursor = c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(daemonOSField),
                x => x.MatchLdfld(osCurrentMissionField),
                x => x.MatchLdnull(),
                x => x.MatchCeq(),
                x => x.MatchLdcI4(0),
                x => x.MatchCeq(),
                x => x.MatchStloc(out int _),
                x => x.MatchLdloc(out int _),
                x => x.MatchBrtrue(out missionUnavailableLabel));

            c.Goto(0);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(2171618));

            c.Emit(OpCodes.Ldloc_1);

            c.EmitDelegate<Func<ActiveMission, (bool, string)>>((mission) =>
            {
                bool hasCSECAccess = (!HacknetAPCore.SlotData.EnableFactionAccess) ||
                (InventoryManager._factionAccess >= FactionAccess.LabyrinthsOrCSEC && !HacknetAPCore.SlotData.ShuffleLabyrinths) ||
                (InventoryManager._factionAccess >= FactionAccess.CSEC);

                bool hasKaguyaTrialsAccess = (!HacknetAPCore.SlotData.EnableFactionAccess) ||
                (InventoryManager._factionAccess >= FactionAccess.LabyrinthsOrCSEC && HacknetAPCore.SlotData.ShuffleLabyrinths);

                var subject = mission.email.subject;
                string reason = "UNAVAILABLE : ";

                if(subject == KAGUYA_TRIALS_SUBJECT)
                {
                    reason += "You don't have enough Faction Access.";
                    return (hasKaguyaTrialsAccess, reason);
                } else
                {
                    bool hasRequiredExecs = ArchipelagoLocations.HasItemsForLocation(subject);
                    if(!hasRequiredExecs)
                    {
                        reason += "You are missing required Archipelago item(s).";
                    } else if(!hasCSECAccess)
                    {
                        reason += "You don't have enough Faction Access.";
                    } else if(OS.currentInstance.currentMission != null)
                    {
                        reason += "Abort your current contract to accept a new one.";
                    }
                    return (hasRequiredExecs && hasCSECAccess, reason);
                }
            });

            // Unpack tuple (C#-style)
            c.Emit(OpCodes.Dup); // duplicate tuple
            c.Emit(OpCodes.Ldfld, typeof(ValueTuple<bool, string>).GetField("Item2"));
            c.Emit(OpCodes.Stloc_S, (byte)missionUnavailableLocal); // store the message
            c.Emit(OpCodes.Ldfld, typeof(ValueTuple<bool, string>).GetField("Item1")); // extract bool
            c.Emit(OpCodes.Brfalse_S, missionUnavailableLabel);

            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Stloc_0);

            c.GotoNext(
                x => x.MatchLdstr("Abort current contract to accept new ones.")
                );
            c.Next.OpCode = OpCodes.Ldloc_S;
            c.Next.Operand = (byte)missionUnavailableLocal;
        }
    }
}
