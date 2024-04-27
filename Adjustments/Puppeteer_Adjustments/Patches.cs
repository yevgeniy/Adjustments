using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using VanillaPsycastsExpanded;
using Verse;
using static HarmonyLib.Code;
using static Verse.PawnCapacityUtility;

namespace Adjustments.Puppeteer_Adjustments
{



    [HarmonyPatch]
    public class adjust_for_heat
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "Hediff_PsycastAbilities");
            yield return type.GetMethod("RecacheCurStage", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        static void Adjust(object psyHediff)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "Hediff_PsycastAbilities");
            var field = type.GetField("pawn", BindingFlags.Public | BindingFlags.Instance);

            var pawn = field.GetValue(psyHediff) as Pawn;
            if (pawn == null)
            {
                Log.Message("NO PAWN");
                return;
            }

            var surgingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_PsySurging) as Hediff_PsySurging;
            if (surgingHediff==null)
            {
                Log.Message("NOT SURGING");
                return;
            }


            var c = surgingHediff.Subjects.Count;
            var curStage = type.GetField("curStage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(psyHediff) as HediffStage;
            if (curStage==null)
            {
                Log.Message("NO CUR STAGE");
                return;
            }

            curStage.statOffsets.FirstOrDefault(v => v.stat.defName == "VPE_PsychicEntropyMinimum").value += c * 20f;

        }
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Use_EntropyValue_insteadof_currentEntropy(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            var newinst = new List<CodeInstruction>();
            for (var x = 0; x < instructions.ToList().Count; x++)
            {
                var i = instructions.ToArray()[x];
                if (i.ToString().Contains("Notify_HediffChanged") )
                {
                    newinst.Add(CodeInstruction.Call(typeof(adjust_for_heat), nameof(Adjust)));
                    newinst.Add(new CodeInstruction(OpCodes.Ldarg_0)); /*load this*/
                }

                newinst.Add(i);
            }

            foreach (var i in newinst)
            {
                yield return i;
            }
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
            else if (capacity.defName == "Moving")
            {
                if (diffSet.HasHediff(Defs.ADJ_MindMerged))
                {
                    __result += .3f;
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


            if (diffSet.HasHediff(Defs.ADJ_PsySurged))
            {
                __result += .6f;
            }

            __result = GenMath.RoundedHundredth(__result);
        }
    }

}
