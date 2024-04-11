using HarmonyLib;
using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Adjustments
{
    


    /* Register guns placed in an inventory */
    public class Pawn_CarryTracker_TryDropCarriedThing
    {
        public static void Wire(Harmony harmony)
        {
            
            var methInfo = typeof(Pawn_CarryTracker).GetMethod("TryDropCarriedThing"
                , new Type[] { typeof(IntVec3), typeof(ThingPlaceMode), typeof(Thing).MakeByRefType(), typeof(Action<Thing, int>) });
            
            harmony.Patch(methInfo, postfix: new HarmonyMethod(typeof(Pawn_CarryTracker_TryDropCarriedThing), nameof(Postfix)));
            
        }
        //HaulAIUtility
        //public static void UpdateJobWithPlacedThings(Job curJob, Thing th, int added)

        public static void Postfix(IntVec3 dropLoc, ThingPlaceMode mode, Thing resultingThing, Pawn_CarryTracker __instance, ref bool __result)
        {
            if (!__result)
                return;
            var thing = resultingThing;

            if (thing != null && thing is ThingWithComps compsThing )
            { 

                var gun = new Rel_GunProxy(compsThing);
                var comp = gun.CompAmmoUser;

                if (comp!=null)
                {
                    Rel_ManagerReloadWeapons.AddWeapon(compsThing);
                }

            }

        }
    }

    
}