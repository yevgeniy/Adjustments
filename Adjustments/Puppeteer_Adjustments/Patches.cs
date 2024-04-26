using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using static HarmonyLib.Code;
using static Verse.PawnCapacityUtility;

namespace Adjustments.Puppeteer_Adjustments
{
    [HarmonyPatch]
    public class Pawn_PsychicEntropyTracker_should_use_prop_not_field
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            var ignore = new string[] { "get_EntropyValue", "GetType" };
            foreach (var meth in typeof(Pawn_PsychicEntropyTracker).GetMethods())
            {
                if (ignore.Contains(meth.Name))
                    continue;
                
                yield return meth;
            }
                

        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Use_EntropyValue_insteadof_currentEntropy(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            //call instance float32 dog::get_Ent()
            //stloc.0

            foreach (var i in instructions)
            {
                
                if (i.ToString()== "ldfld System.Single RimWorld.Pawn_PsychicEntropyTracker::currentEntropy")
                {
                    var r = CodeInstruction.Call(typeof(Pawn_PsychicEntropyTracker), "get_EntropyValue");

                    yield return r;
                    continue;
                }

                yield return i;
            }
                
        }
    }


   [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "EntropyToRelativeValue")]
   public class entropy_relative_val
    {
        [HarmonyPrefix]
        public static void prefix(Pawn_PsychicEntropyTracker __instance, ref float val)
        {
            var pawn = __instance.Psylink.pawn;
            var h = pawn.health.hediffSet.hediffs.FirstOrDefault(v => v.def == Defs.ADJ_PsySurging);
            if (h!=null)
            {
                val += (h as Hediff_PsySurging).Subjects.Count * 20;
            }            
        }
    }

   [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "EntropyValue", MethodType.Getter)]
    public class min_heat_fix
    {
        public static Type Hediff_PsycastAbilitiesType;
        [HarmonyPostfix]
        public static void prefix(Pawn_PsychicEntropyTracker __instance, ref float __result)
        {
            var pawn = __instance.Psylink?.pawn;
            if (pawn == null)
                return;

            var c = pawn.health.hediffSet.hediffs.Where(v => v.def == Defs.ADJ_PsySurging).Count();
            __result += c * 20;
                
        }
    }

    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
    public class calc_mindmerge_capacity
    {
        [HarmonyPostfix]
        public static void postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity, List<PawnCapacityUtility.CapacityImpactor> impactors, bool forTradePrice)
        {
            if (capacity.defName== "Consciousness")
            {
                if (diffSet.HasHediff(Defs.ADJ_MindMerged))
                {
                    __result *= 2;
                    __result = Math.Max(__result, capacity.minValue);
                }
            }
        }
    }


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

            if (diffSet.HasHediff(Defs.ADJ_Augmented))
            {
                __result += .5f;
            }

            __result = Mathf.Clamp(__result, 0f, 1.2f);
            __result = GenMath.RoundedHundredth(__result);
        }
    }

}
