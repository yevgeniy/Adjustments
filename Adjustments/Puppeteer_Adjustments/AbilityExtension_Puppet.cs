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
using VFECore.Abilities;

namespace Adjustments.Puppeteer_Adjustments
{
    public class AbilityExtension_Puppet : AbilityExtension_Psycast
    {
        public override bool IsEnabledForPawn(VFECore.Abilities.Ability ability, out string reason)
        {
            reason = "";
            return true;
        }
        public override bool ValidateTarget(LocalTargetInfo target, VFECore.Abilities.Ability ability, bool throwMessages = false)
        {

            bool r = false;
            if (target.Pawn.IsColonist || target.Pawn.IsSlaveOfColony || target.Pawn.IsPrisoner)
            {
                return true;
            }
            else
            {
                Messages.Message($"Target must be colonist, slave, or a prisoner", MessageTypeDefOf.NeutralEvent);
                return false;
            }

        }

        public override void PostCast(GlobalTargetInfo[] targets, VFECore.Abilities.Ability ability)
        {

            foreach (Pawn p in targets)
            {
                if (p == null)
                    continue;


                //if (!p.health.hediffSet.TryGetHediff(Adjustments.VPEP_PuppetHediff_HediffDef, out var hediff))
                //{
                //    Log.Error("COULD NOT FIND PUPPET HEDIFF");
                //    continue;
                //}
                if (p.IsSlaveOfColony || p.IsPrisoner)
                {
                    RecruitUtility.Recruit(p, ability.pawn.Faction);

                    p.Notify_DisabledWorkTypesChanged();
                    ClearSkillsModCache();
                }


            }

            base.PostCast(targets, ability);

        }
        private static Type LearnRateFactorCacheType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "LearnRateFactorCache");
        private static MethodInfo ClearCachemeth = LearnRateFactorCacheType != null
            ? LearnRateFactorCacheType.GetMethod("ClearCache", BindingFlags.Public | BindingFlags.Static)
            : null;

        private void ClearSkillsModCache()
        {
            if (ClearCachemeth!=null)
            {

                ClearCachemeth.Invoke(null, null);
            }
            else
            {
                Log.Message($"Learn rate factor module is not there.");
            }

        }
    }
}
