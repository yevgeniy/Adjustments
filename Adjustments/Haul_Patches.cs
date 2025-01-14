using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static System.Collections.Specialized.BitVector32;
using Verse.Noise;
using System.IO;
using UnityEngine;
using System.Diagnostics.PerformanceData;
using Unity.Jobs;
using VFECore;

namespace Adjustments
{

    [HarmonyPatch]
    public class consider_blueprint_containers
    {
        [HarmonyTargetMethod]
        public static MethodBase get_method()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "JobDriver_UnloadYourHauledInventory");

            if (type == null)
                return null;

            MethodBase meth = type.GetMethod("MakeNewToils", BindingFlags.Public | BindingFlags.Instance);

            return meth;
        }
        [HarmonyPostfix]
        public static void adjust(ref IEnumerable<Toil> __result, JobDriver __instance)
        {

            var res = __result.ToList();
            res.Insert(5, Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B, TargetIndex.None));

            __result = res;

        }
    }


    /* for some reason part of gathering stuff to inventory, the pawn goes to a
     * closest supply pile as the last instruction.  Going to remove this instruction.
     * (going to unload splot should be part of unload job driver) */
    [HarmonyPatch]
    public class dont_goto_supply
    {
        static Type JobDriver_HaulToInventoryType;
        static Type CompHauledToInventoryType;
        [HarmonyTargetMethod]
        public static MethodBase get_method()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            JobDriver_HaulToInventoryType = assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "JobDriver_HaulToInventory");

            CompHauledToInventoryType = assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "CompHauledToInventory");


            if (JobDriver_HaulToInventoryType == null)
                return null;

            MethodBase meth = JobDriver_HaulToInventoryType.GetMethod("MakeNewToils", BindingFlags.Public | BindingFlags.Instance);

            return meth;
        }
        [HarmonyPostfix]
        public static void adjust(ref IEnumerable<Toil> __result, JobDriver __instance)
        {
            var res = __result.ToList();


            var gotoIndex = 6;
            var takeThingIndex = 3;
            var unloadJobIndex = 7;

            var unloadToil = res[unloadJobIndex];

            /*get rid of goto things toil.  I don't know what it does*/
            res[gotoIndex] = new Toil { };


            /*take things index should not be spawning a HaulToStorageJob.
             * Instead it should jump to unloadJob toil (to create new unload inventory job */
            var pawn = __instance.pawn;
            var job = __instance.job;
            var takenToInventory = InventoryComp(pawn);
            var adjustedTakeThingsToil = new Toil
            {
                initAction = () =>
                {
                    var actor = pawn;
                    var thing = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                    Toils_Haul.ErrorCheckForCarry(actor, thing);

                    //get max we can pick up
                    var countToPickUp = Mathf.Min(job.count, MassUtility.CountToPickUpUntilOverEncumbered(actor, thing));
                    Log.Message($"{actor} is hauling to inventory {thing}:{countToPickUp}");


                    if (countToPickUp > 0)
                    {
                        var splitThing = thing.SplitOff(countToPickUp);
                        var shouldMerge = GetHashSet(takenToInventory).Any(x => x.def == thing.def);
                        actor.inventory.GetDirectlyHeldThings().TryAdd(splitThing, shouldMerge);
                        RegisterHauledItem(takenToInventory, splitThing);

                    }

                    //thing still remains, so queue up hauling if we can + end the current job (smooth/instant transition)
                    //This will technically release the reservations in the queue, but what can you do
                    if (thing.Spawned)
                    {
                        actor.jobs.curDriver.JumpToToil(unloadToil);
                    }
                }
            };
            res[takeThingIndex] = adjustedTakeThingsToil;


            __result = res;

        }

        public static object InventoryComp(Pawn pawn)
        {

            MethodInfo tryGetCompMethod = typeof(ThingWithComps).GetMethod("GetComp", BindingFlags.Public | BindingFlags.Instance);


            MethodInfo genericMethod = tryGetCompMethod.MakeGenericMethod(CompHauledToInventoryType);

            // Invoke the method
            var inv = genericMethod.Invoke(pawn, null);


            return inv;
        }
        public static HashSet<Thing> GetHashSet(object invComp)
        {
            var r = (HashSet<Thing>)CompHauledToInventoryType.GetMethod("GetHashSet", BindingFlags.Instance | BindingFlags.Public)
                   .Invoke(invComp, new object[] { });


            return r;
        }
        public static void RegisterHauledItem(object invComp, Thing splitThing)
        {
            CompHauledToInventoryType.GetMethod("RegisterHauledItem", BindingFlags.Instance | BindingFlags.Public)
                    .Invoke(invComp, new object[] { splitThing });
        }


    }


    [HarmonyPatch]
    public class when_hauling_check_constructables_first
    {
        static MethodInfo FirstUnloadableThingMethod;
        static FieldInfo CountToDropField;

        [HarmonyTargetMethod]
        public static MethodBase get_method()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var JobDriver_UnloadYourHauledInventoryType = assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "JobDriver_UnloadYourHauledInventory");

            FirstUnloadableThingMethod = JobDriver_UnloadYourHauledInventoryType
                .GetMethod("FirstUnloadableThing", BindingFlags.NonPublic | BindingFlags.Static);
            Log.Message("METHOD FirstUnloadableThing " + FirstUnloadableThingMethod);

            CountToDropField = JobDriver_UnloadYourHauledInventoryType
                   .GetField("_countToDrop", BindingFlags.NonPublic | BindingFlags.Instance);
            Log.Message("FIELD _countToDrop " + CountToDropField);


            var type = assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "JobDriver_UnloadYourHauledInventory");

            if (type == null)
                return null;

            var meth = type.GetMethod("FindTargetOrDrop", BindingFlags.NonPublic | BindingFlags.Instance);
            Log.Message("ADJUSTING METHOD: " + meth);


            return meth;


            

        }
        [HarmonyPrefix]
        public static bool adjust(ref Toil __result, JobDriver __instance, HashSet<Thing> carriedThings)
        {
            


            var pawn = __instance.pawn;
            var job = __instance.job;

            Func<ThingCount> getFirstUnloadableThing = () =>
            {
                return (ThingCount)FirstUnloadableThingMethod.Invoke(__instance, new object[] { pawn, carriedThings });
            };

            Action<int> set_countToDropField = (int n) =>
            {
                CountToDropField.SetValue(__instance, n);
            };

            __result = new Toil()
            {
                initAction = () =>
                {
                    var unloadableThing = getFirstUnloadableThing();

                    if (unloadableThing.Count == 0)
                    {
                        if (carriedThings.Count == 0)
                        {
                            __instance.EndJobWith(JobCondition.Succeeded);
                        }
                        return;
                    }

                    var currentPriority = StoragePriority.Unstored; // Currently in pawns inventory, so it's unstored
                    if (BestPlace(unloadableThing.Thing, pawn, pawn.Map, currentPriority, pawn.Faction))
                    {
                        job.SetTarget(TargetIndex.A, unloadableThing.Thing);

                        if (!pawn.Map.reservationManager.Reserve(pawn, job, job.targetB))
                        {
                            pawn.inventory.innerContainer.TryDrop(unloadableThing.Thing, ThingPlaceMode.Near,
                                unloadableThing.Thing.stackCount, out _);
                            __instance.EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                        set_countToDropField(unloadableThing.Thing.stackCount);
                    }
                    else
                    {
                        pawn.inventory.innerContainer.TryDrop(unloadableThing.Thing, ThingPlaceMode.Near,
                            unloadableThing.Thing.stackCount, out _);
                        __instance.EndJobWith(JobCondition.Succeeded);
                    }
                }
            };

            return false;
        }
        public static bool BestPlace(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction)
        {
            Thing haulTo = null;
            IntVec3 cel = IntVec3.Invalid;

            haulTo = Haul_Map.GetLocationOfMatchingConstructable(carrier, t);

            if (haulTo != null)
            {
                carrier.CurJob.SetTarget(TargetIndex.B, haulTo);
                return true;
            }

            if (StoreUtility.TryFindBestBetterStorageFor(t, carrier, map, currentPriority, faction, out cel, out var haulDestination))
            {
                if (cel==IntVec3.Invalid)
                {
                    carrier.CurJob.SetTarget(TargetIndex.B, haulDestination as Thing);
                }
                else
                {
                    carrier.CurJob.SetTarget(TargetIndex.B, cel);
                }

                return true;
            }

            return false;
        }


    }

    /* when a constructable things spawns we need to cache it (will clean the cashe every so often ticks in 'map') */
    [HarmonyPatch(typeof(GenSpawn))]
    [HarmonyPatch(nameof(GenSpawn.Spawn), new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool) })]
    public class record_things_to_build
    {

        [HarmonyPostfix]
        public static void Postfix(ref Thing __result, Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = 0, bool respawningAfterLoad = false, bool forbidLeavings = false)
        {
            if (__result is IConstructible)
            {
                Log.Message("RECORDING CONSTRUCTABLE");
                Haul_Map.Constructibles.Add(__result);
            }

        }
    }

}


//[HarmonyTranspiler]
//public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//{
//    MethodInfo BestPlaceMethod = typeof(when_hauling_check_constructables_and_deliverables_first)
//             .GetMethod("BestPlace", BindingFlags.Static | BindingFlags.Public);

//    foreach (var i in instructions)
//    {
//        Log.Message(i);
//        if (i.ToString().Contains("TryFindBestBetterStorageFor"))
//        {
//            var inst = new CodeInstruction(OpCodes.Call, BestPlaceMethod);
//            yield return inst;
//        }
//        else if (i.ToString()== "ldloca.s 3 (RimWorld.IHaulDestination)")
//        {
//            yield return new CodeInstruction(OpCodes.Ldloc_S, new Thing());
//        }
//        else
//            yield return i;
//    }
//}


//////////////////////////////////
///
/////Type displayClassType = type.GetNestedType("<>c__DisplayClass8_0", BindingFlags.NonPublic);

//if (displayClassType != null)
//{
//    // Get the method. The exact name might differ slightly, so be precise with what you see in your IL or decompiled code.
//    MethodInfo method = displayClassType.GetMethod("<FindTargetOrDrop>b__0", BindingFlags.NonPublic | BindingFlags.Instance);

//    if (method != null)
//    {
//        return method;

//        // Here you could invoke the method if you had an instance of the display class
//        // object result = method.Invoke(instanceOfDisplayClass, null); // Assuming no parameters
//    }
//    else
//    {
//        Log.Message("Method not found in the display class.");
//    }
//}
//else
//{
//    Log.Message("Display class not found.");
//}
