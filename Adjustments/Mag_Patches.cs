using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace Adjustments
{
    public class subjugated_ppl_dont_rebell
    {
        [HarmonyPrefix]
        public static bool patch(Pawn pawn, ref bool __result)
        {


            if (pawn.health.hediffSet.HasHediff(Mag_Adjustments.VPEP_Puppet))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GuestUtility), "GetDisabledWorkTypes")]
    public class subjugated_ppl_can_do_art_and_research
    {
        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            Type type = typeof(Pawn_GuestTracker);

            // Get the private field info
            FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

            return (Pawn)fieldInfo.GetValue(instance);

        }
        [HarmonyPostfix]
        public static void postfix(Pawn_GuestTracker guest, ref List<WorkTypeDef> __result)
        {
            var pawn = GetPawn(guest);

            if (pawn.health.hediffSet.HasHediff(Mag_Adjustments.VPEP_Puppet))
            {
                __result.RemoveAll(v => v==WorkTypeDefOf.Research /*|| v==WorkTypeDefOf.Art*/);
            }

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
                var hediff_Puppet = pawn.health.hediffSet.GetFirstHediffOfDef(Mag_Adjustments.VPEP_Puppet);
                if (hediff_Puppet != null)
                {
                    var master = Mag_Adjustments.Master.GetValue(hediff_Puppet) as Pawn;

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
