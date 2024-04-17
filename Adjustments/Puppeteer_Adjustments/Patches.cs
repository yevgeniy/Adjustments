using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Verse.PawnCapacityUtility;

namespace Adjustments.Puppeteer_Adjustments
{
    
    [HarmonyPatch(typeof(PawnCapacityWorker_Consciousness), "CalculateCapacityLevel")]
    public class calc_capacity
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors)
        {
            if (diffSet.HasHediff(Adjustments.BrainLeechHediff))
            {
                __result -= .5f;                    
            }

            if (diffSet.HasHediff(Adjustments.BrainLeechingHdeiff))
            {
                __result += .5f;
            }

            if (diffSet.HasHediff(Defs.ADJ_Augmenting))
            {
                __result -= .5f;
            }

            if (diffSet.HasHediff(Defs.AJD_Augmented))
            {
                __result += .5f;
            }

            __result = Mathf.Clamp(__result, 0f, 1.2f);
            __result = GenMath.RoundedHundredth(__result);
        }
    }

}
