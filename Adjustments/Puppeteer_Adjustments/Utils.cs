using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{
    public static class Utils
    {
        static FieldInfo StatPointsFieldInfo = typeof(Hediff_PsycastAbilities).GetField("statPoints", BindingFlags.NonPublic | BindingFlags.Instance);
        public static int GetPsyStatPoints(Pawn pawn)
        {
            Log.Message($"getting points {pawn}");
            if (pawn.health.hediffSet.TryGetHediff(VPE_DefOf.VPE_PsycastAbilityImplant, out var h)
                && h is Hediff_PsycastAbilities psyhediff)
            {
                Log.Message($"method {StatPointsFieldInfo}");
                return (int)StatPointsFieldInfo.GetValue(psyhediff);
            }

            Log.Error($"Master {pawn} does not seem to have psyhediff");
            return 0;
        }

        public static float MutateCost(SkillRecord skill)
        {
            if (skill == null)
                return -1f;

            var currentlevel = (byte)skill.passion;
            if (currentlevel < 2)
            {
                var cost = .5f + .5f * currentlevel;
                return cost;
            }
            return -1f;
        }
    }

}
