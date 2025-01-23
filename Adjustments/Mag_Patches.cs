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
