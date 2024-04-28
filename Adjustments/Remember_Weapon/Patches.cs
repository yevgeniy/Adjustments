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
        [HarmonyPostfix]
        public static void postfix(JobGiver_PickupDroppedWeapon __instance, Pawn pawn, ref Job __result)
        {
            if (__result==null && pawn.mindState?.droppedWeapon!=null)
            {
                var weaponName = pawn.mindState.droppedWeapon.def.defName;

                var thingRequestGroup = ThingRequestGroup.Weapon;

                var anotherWeapon = GenClosest.ClosestThingReachable(
                    pawn.Position,
                    pawn.Map,
                    ThingRequest.ForGroup(thingRequestGroup),
                    PathEndMode.Touch,
                    TraverseParms.For(pawn),
                    validator: (Thing thing) =>
                        thing.def.defName == weaponName
                        &&!thing.IsForbidden(pawn.Faction)
                        && pawn.CanReserve(thing)
                );

                if (anotherWeapon!=null)
                {
                    __result = JobGiver_PickupDroppedWeapon.PickupWeaponJob(pawn, anotherWeapon, false);
                }
            }
        }
    }

}
