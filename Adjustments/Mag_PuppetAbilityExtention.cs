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
using Ability = VFECore.Abilities.Ability;

namespace Adjustments
{
    public class Mag_PuppetAbilityExtention: AbilityExtension_Psycast
    {

        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            return base.ValidateTarget(target, ability, throwMessages);
        }

        public override void PostCast(GlobalTargetInfo[] targets, Ability ability)
        {

            foreach(Pawn p in targets)
            {
                if (p == null)
                    continue;

                var hediff = p.health.hediffSet.hediffs.FirstOrDefault(v => v.def.defName == Mag_Adjustments.VPEP_Puppet.defName);
                if (hediff == null)
                {
                    Log.Error("COULD NOT FIND PUPPET HEDIFF");
                    continue;
                }

                RecruitUtility.Recruit(p, ability.pawn.Faction);

                p.Notify_DisabledWorkTypesChanged();
                ClearSkillsModCache();
            }

            base.PostCast(targets, ability);

        }
        private MethodInfo clearCachemeth;

        private void ClearSkillsModCache()
        {
            clearCachemeth = clearCachemeth
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .First(v => v.Name == "LearnRateFactorCache")
                    .GetMethod("ClearCache", BindingFlags.Public | BindingFlags.Static);

            clearCachemeth.Invoke(null, null);
        }
    }
}
