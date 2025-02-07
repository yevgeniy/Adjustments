using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Adjustments.VehicleDelivery
{

    [HarmonyPatch]
    public class eval_what_we_need_to_send_caravan
    {
        [HarmonyTargetMethod]
        public static MethodInfo get_method()
        {

            var type = Adjustments.Adjustments_Mod.Assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "Dialog_FormVehicleCaravan");
            return type.GetMethod("CheckForErrors", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyPrefix]
        public static bool adjust(ref bool __result, List<Pawn> pawns)
        {
            if (pawns.Any(v=>v == VehicleDeliveryMap.Vehicle))
            {
                
                __result = true;
                return false;
            }
            return true;
        }
    }
    

    [HarmonyPatch]
    public class mass_deliver_gizmo
    {
        [HarmonyTargetMethod]
        public static MethodInfo get_method()
        {
            
            var type = Adjustments.Adjustments_Mod.Assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "VehiclePawn");
            return type.GetMethod("GetGizmos", BindingFlags.Public | BindingFlags.Instance); 
        }

        [HarmonyPostfix]
        public static void adjust(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            var gizs = __result.ToList();

            Command_Action toggleCaravaning = new Command_Action
            {
                defaultLabel = VehicleDeliveryMap.Vehicle == null ? "Activate Huling" : "Stop Hauling",
                icon = ContentFinder<Texture2D>.Get("haul", true),
                action = delegate ()
                {
                    VehicleDeliveryMap.ToggleVehicle(__instance);

                    Log.Message("CLICKED");
                }
            };
            gizs.Add(toggleCaravaning);


            __result = gizs;
        }
    }
}
