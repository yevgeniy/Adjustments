using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [HarmonyPatch(typeof(Trait), "TipString")]
    public class trait_should_include_perk_descriptions
    {
        [HarmonyPostfix]
        public static void postfix(Trait __instance, ref string __result, Pawn pawn)
        {
            if (__instance.def != SubjugatedDefs.Subjugated)
                return;

            if (!SubjugateComp.Repo.ContainsKey(pawn))
            {
                Log.Error("Subjugate: GOT A PAWN W/ NO REPO COMP");
            }

            var comp = SubjugateComp.Repo[pawn];

            var explanations = comp.Perks.Select(v => v.Describe(pawn)).ToList();
            __result = __result + "\n\n" + string.Join("\n", explanations);
        }
    }

    [HarmonyPatch(typeof(SkillRecord), "CalculatePermanentlyDisabled")]
    public class disabling_skills_based_on_perks
    {
        [HarmonyPrefix]
        public static bool prefixer(SkillRecord __instance, ref bool __result)
        {
            if (!SubjugateComp.Repo.ContainsKey(__instance.Pawn))
                return true;

            var comp = SubjugateComp.Repo[__instance.Pawn];
            if (comp != null)
            {
                __result = comp.Perks.Any(v =>
                {
                    return v.IsDisabled(__instance);
                });
                if (__result) /*dont evaluate default.  We know it's disabled */
                    return false;

                return true;
            }

            return true;
        }
    }


    //public void AddDirect(Hediff hediff, DamageInfo? dinfo = null, DamageWorker.DamageResult damageResult = null)
    [HarmonyPatch(typeof(HediffSet), "AddDirect")]
    public class register_severity_for_beating
    {
        [HarmonyPrefix]
        public static bool Patch(Hediff hediff, DamageInfo dinfo, DamageWorker.DamageResult damageResult, HediffSet __instance)
        {
            if (hediff!=null)
            {
                var pawn = __instance.pawn;
                if (pawn.gender == Gender.Female)
                {
                    var comp = pawn.GetComp<SubjugateComp>();
                    if (comp != null)
                    {
                        comp.RegisterSeverity(hediff.Severity);
                    }
                }

            }

            return true;
        }

    }

    //public void SetGuestStatus(Faction newHost, GuestStatus guestStatus = 0)
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    public class register_prisoner_start
    {
        [HarmonyPrefix]
        public static bool Patch(Faction newHost, GuestStatus guestStatus, Pawn_GuestTracker __instance)
        {
            if (!__instance.IsPrisoner && guestStatus == GuestStatus.Prisoner)
            {
                var pawn = GetPawn(__instance);
                if (pawn.gender==Gender.Female /*&& pawn.guilt.IsGuilty*/)
                {
                    var comp = pawn.GetComp<SubjugateComp>();
                    if (comp!=null)
                    {
                        comp.ActivateSubjugation();
                    }
                }

            }

            return true;
        }

        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            Type type = typeof(Pawn_GuestTracker);

            // Get the private field info
            FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

            return (Pawn)fieldInfo.GetValue(instance);

        }
    }
}
