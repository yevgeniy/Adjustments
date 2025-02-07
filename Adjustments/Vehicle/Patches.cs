using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Adjustments.Vehicle
{
    [HarmonyPatch]
    public static class pause_on_vehicle_packed
    {
        static Type VehiclePawnType;


        [HarmonyTargetMethod]
        public static MethodInfo getmethod()
        {
            VehiclePawnType = Adjustments_Mod.Assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "VehiclePawn");

            return VehiclePawnType.GetMethod("AddOrTransfer", new Type[] { typeof(Thing), typeof(int), typeof(Pawn) });
        }

        [HarmonyPostfix]
        public static void adjust(ref int __result, Pawn __instance, Thing thing, int count, Pawn holder)
        {
            var vehicleProxy = new VehiclePawnProxy(__instance);
            var trans = vehicleProxy.CargoToLoad;
            var lastadded=trans.Select(v=>v.CountToTransfer).Sum(v=>v);
            if (lastadded==__result)
            {
                Messages.Message("Vehicle packed", MessageTypeDefOf.NeutralEvent);
                Find.TickManager.Pause();
            }
        }
    }

    [HarmonyPatch]
    public static class pause_on_vehicle_stop
    {
        static Type VehiclePawnType;


        [HarmonyTargetMethod]
        public static MethodInfo getmethod()
        {
            VehiclePawnType = Adjustments_Mod.Assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "VehiclePawn");

            return AccessTools.Method(typeof(JobDriver_Goto), "MakeNewToils");
        }

        [HarmonyPostfix]
        private static IEnumerable<Toil> postfix_adjust(IEnumerable<Toil> __result, Job ___job, Pawn ___pawn)
        {

            
            bool first = true;
            foreach (Toil toil in __result)
            {
                if (first )
                {
                    if (Adjustments_Mod.VehicleIsActive && VehiclePawnType.IsAssignableFrom(___pawn.GetType()))
                    {
                        toil.AddFinishAction(delegate ()
                        {
                            if (___pawn==VehicleDelivery.VehicleDeliveryMap.Vehicle)
                            {
                                VehicleDelivery.VehicleDeliveryMap.OnVehicleStopped();
                            }
                            else
                            {
                                Messages.Message("Vehicle stopped", MessageTypeDefOf.NeutralEvent);
                                Find.TickManager.Pause();
                            }
                            
                        });
                    }
                }
                    
                yield return toil;
            }


        }
    }
}
