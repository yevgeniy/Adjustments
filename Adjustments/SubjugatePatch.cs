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
    //public float LearnRateFactor(bool direct = false)
    [HarmonyPatch(typeof(SkillRecord), "LearnRateFactor")]
    public class learn_factor
    {

        private static FieldInfo _pawnFieldInfo
        private static FieldInfo PawnFieldInfo {
            get{
                if (_pawnFieldInfo==null) {
                    Type type = typeof(SkillRecord);

                    _pawnFieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                
                return _pawnFieldInfo;
            }
        }

        [HarmonyPostfix]
        public static void Patch(SkillRecord __instance, ref float __result )
        {
            var pawn = GetPawn(__instance);
            if (pawn.gender==Gender.Female) {
                
                var trait = pawn.story.traits.GetTrait(SubjugatedDefs.Subjugated);
                if (trait!=null) {

                }
            }

        }
        private static Pawn GetPawn(SkillRecord instance)
        {
            return (Pawn)PawnFieldInfo.GetValue(instance);
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
