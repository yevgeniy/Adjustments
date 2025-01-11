using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;

namespace Adjustments.Remember_Weapon
{
    internal class Patches
    {
    }


    [HarmonyPatch]
    public class dropped_weapon_should_not_unset_on_despawn
    {

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(Pawn_MindState).GetMethod("MindStateTick", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            var newinstructions = new List<CodeInstruction>();
            foreach (var i in instructions)
            {
                if (i.opcode==OpCodes.Stfld && i.ToString().Contains("droppedWeapon"))
                {
                    /* skip this instruction */

                    /* pop last null instruction */
                    newinstructions.Pop();

                    /* pop last 'this' instruction */
                    newinstructions.Pop();
                } else
                {
                    newinstructions.Add(i);
                }
            }

            return newinstructions;
        }
    }

    [HarmonyPatch(typeof(JobGiver_PickupDroppedWeapon), "TryGiveJob")]
    public class pickup_similar_weapons
    {
        public static HashSet<Pawn> AlreadyChecked = new HashSet<Pawn>();
        public static float CheckInterval = GenDate.TicksPerHour;
        public static float? LastClear = default(float?);

        [HarmonyPostfix]
        public static void postfix(JobGiver_PickupDroppedWeapon __instance, Pawn pawn, ref Job __result)
        {
            if (__result != null)
                return;

            var weaponName = Manager.GetWeaponName(pawn);
            if (weaponName == null)
                return;

            if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.defName == weaponName)
                return;

            if (!LastClear.HasValue || Find.TickManager.TicksGame >= LastClear.Value + CheckInterval)
            {
                AlreadyChecked.Clear();
                LastClear = Find.TickManager.TicksGame;
            }

            if (AlreadyChecked.Contains(pawn))
                return;

            var thingRequestGroup = ThingRequestGroup.Weapon;

            var closestWeapon = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(thingRequestGroup),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                validator: (Thing thing) =>
                    thing.def.defName == weaponName
                    && !thing.IsForbidden(pawn.Faction)
                    && pawn.CanReserve(thing)
            );

            if (closestWeapon != null)
            {
                __result = JobGiver_PickupDroppedWeapon.PickupWeaponJob(pawn, closestWeapon, false);
            }
            else
                AlreadyChecked.Add(pawn);
        }
    }


    [HarmonyPatch]
    public class patch_pawn_expose_data_wep_mem
    {

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {

            yield return typeof(Pawn).GetMethod("ExposeData", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            var newinstructions = new List<CodeInstruction>();
            foreach (var i in instructions)
            {
                if (i.ToString().Contains("ret"))
                {
                    newinstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newinstructions.Add(CodeInstruction.Call(typeof(Manager), nameof(Manager.ExposeWeaponData)));
                }
                newinstructions.Add(i);
            }

            return newinstructions;
        }
    }

    [HarmonyPatch]
    public class add_equipment_memory
    {

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {

            yield return typeof(Pawn_EquipmentTracker).GetMethod("AddEquipment", BindingFlags.Public | BindingFlags.Instance);
        }
        public static void SetWeapon(Pawn_EquipmentTracker eqtrack, ThingWithComps newEq)
        {
            var pawn = eqtrack.pawn;
            if (pawn == null || newEq == null || !pawn.IsColonist)
                return;

            Log.Message("SETTING EQ: " + pawn + ": " + newEq.def.defName);
            Manager.SetWeaponName(pawn, newEq.def.defName);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            var newinstructions = new List<CodeInstruction>();
            foreach (var i in instructions)
            {
                if (i.opcode==OpCodes.Stfld && i.ToString().Contains("droppedWeapon"))
                {
                    newinstructions.Add(i); /* add original droppedWeapon=null */

                    newinstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newinstructions.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newinstructions.Add(CodeInstruction.Call(typeof(add_equipment_memory), nameof(add_equipment_memory.SetWeapon)));
                }
                else
                {
                    newinstructions.Add(i);
                }
                
            }

            return newinstructions;
        }
    }


    [HarmonyPatch]
    public class remove_equipment_memory
    {

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {

            yield return typeof(ITab_Pawn_Gear).GetMethod("InterfaceDrop", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static void DropWeapon(ITab_Pawn_Gear tab)
        {
            var pawn = typeof(ITab_Pawn_Gear).GetProperty("SelPawnForGear", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(tab) as Pawn;

            Manager.SetWeaponName(pawn, null);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            var newinstructions = new List<CodeInstruction>();
            var found_where_eq_drop = false;
            foreach (var i in instructions)
            {
                if (i.ToString().Contains("DropEquipment"))
                {
                    found_where_eq_drop = true;
                }
                else if (found_where_eq_drop && i.opcode==OpCodes.Ret)
                {
                    newinstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newinstructions.Add(CodeInstruction.Call(typeof(remove_equipment_memory), nameof(remove_equipment_memory.DropWeapon)));
                }

                newinstructions.Add(i);
            }

            return newinstructions;
        }
    }

}
