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
using VFECore.Abilities;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;
using static Verse.PawnCapacityUtility;

namespace Adjustments.Puppeteer_Adjustments
{




 

    [HarmonyPatch]
    public class adjust_puppet_validation
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "Ability_Puppet");
            yield return type.GetMethod("ValidateTarget", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyPrefix]
        public static bool no_validation(ref bool __result, LocalTargetInfo target, bool showMessages)
        {
            if (target.Pawn is null)
            {
                __result = false;
            }
            if (target.Pawn.health.hediffSet.hediffs.FirstOrDefault(v => v.def.defName == "VPEP_Puppet") != null)
            {
                Log.Message("ALREADY A PUPPET");
                __result = false;
            }

            if (new bool[] { target.Pawn.IsColonist, target.Pawn.IsSlave, target.Pawn.IsPrisonerOfColony }.ToList().All(v => v == false))
            {
                Log.Message("PAWN SHOULD BE A COLONIST, A SLAVE, OR PRISONER");
                __result = false;
            }

            __result = true;
            return false;
        }
    }



    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
    public class calc_mindmerge_capacity
    {
        [HarmonyPostfix]
        public static void postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity, List<PawnCapacityUtility.CapacityImpactor> impactors, bool forTradePrice)
        {
            var original = __result;
            if (capacity.defName == "Consciousness")
            {

                Hediff hediff;
                if (diffSet.TryGetHediff(Adjustments.BrainLeechHediff, out hediff) && hediff is Hediff_SoulLeech soulLeech)
                {
                    if (__result>.01f)
                    {
                        __result = Mathf.Max(__result - soulLeech.AmmountLeachedAway, .01f);
                    }
                        
                }
                if (diffSet.TryGetHediff(Adjustments.VPEP_PuppetHediff_HediffDef, out var _))
                {
                    __result = CalcPuppetConsc(diffSet, __result, original);

                }

                //var shhediff = diffSet.GetFirstHediffOfDef(Defs.ADJ_SoulShield_Hediff) as Hediff_SoulShield;
                //if (shhediff != null)
                //{
                //    __result = shhediff.Act(original);
                //}


                //if (diffSet.TryGetHediff(Adjustments.BrainLeechingHediff, out var hediff))
                //{
                //    var brainleechinghediff = hediff as Hediff_SoulLeech;
                //    var adjustBy = brainleechinghediff.ConsciousnessAdjustment;

                //    __result += adjustBy;
                //}
                //if (diffSet.TryGetHediff(Adjustments.VPEP_PuppetHediff_HediffDef, out hediff))
                //{
                //    var puppetHediffProxy = new PuppetHediffProxy(hediff);
                //    var master = puppetHediffProxy.Master;
                //    if (master.health.hediffSet.TryGetHediff(Adjustments.BrainLeechingHediff, out var blinghedif))
                //    {
                //        var adjustBy = (blinghedif as Hediff_SoulLeech).ConsciousnessAdjustment;
                //        __result += adjustBy;
                //    }
                //}
                //if (diffSet.HasHediff(Defs.ADJ_Augmented))
                //{
                //    __result += .5f;
                //}
                //__result = Mathf.Clamp(__result, 0f, 1.2f);

                //if (diffSet.HasHediff(Defs.ADJ_MindMerged))
                //{
                //    __result *= 2;
                //    __result = Math.Max(__result, capacity.minValue);
                //}
                __result = GenMath.RoundedHundredth(__result);
            }
            else if (capacity.defName == "Moving")
            {

            }
       
        }


        private static float CalcPuppetConsc(HediffSet diffSet, float current, float original)
        {
            var offsetforpuppet = Mathf.Max(original - .7f, 0f);
            var res = 0f;

            if (current >= .3f)
            {
                res = .3f + offsetforpuppet;
            }
            else
            {
                res = current;
            }


            var hediff = diffSet.GetFirstHediffOfDef(Defs.ADJ_SoulGrowth_Hediff) as Hediff_SoulGrowth;
            if (hediff != null)
            {
                res += hediff.CurrentRework;
            }

            return res;

        }
    }


    [HarmonyPatch(typeof(PawnCapacityWorker_Consciousness), "CalculateCapacityLevel")]
    public class calc_capacity
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors)
        {
            

            

            //if (diffSet.TryGetHediff(Adjustments.BrainLeechHediff, out var h))
            //{


            //}

            //if (diffSet.HasHediff(Defs.ADJ_Augmenting))
            //{
            //    __result -= .5f;
            //}

            //__result = Mathf.Max(0f, __result);


            //if (diffSet.HasHediff(Defs.ADJ_PsySurged))
            //{
            //    __result += .6f;
            //}

            //__result = GenMath.RoundedHundredth(__result);
        }

    }

    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.TickRare))]
    public class puppet_has_skill_of_master
    {

        [HarmonyPrefix]
        public static bool Prefix(ThingWithComps __instance)
        {
            if (__instance is Pawn pawn)
            {
                
                if (pawn.health.hediffSet.TryGetHediff(Adjustments.VPEP_PuppetHediff_HediffDef, out var h))
                {
                    var pupetHediffProxy = new PuppetHediffProxy(h);
                    var master = pupetHediffProxy.Master;

                    foreach (var skill in master.skills.skills)
                    {
                        var targetSkill = pawn.skills.GetSkill(skill.def);
                        targetSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                        targetSkill.xpSinceMidnight = skill.xpSinceMidnight;
                        targetSkill.Level = skill.Level;
                        targetSkill.passion = skill.passion;
                    }
                }
            }

            return true;
        }

    }

}
