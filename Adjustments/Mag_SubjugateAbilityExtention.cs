using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments
{
    public class Mag_SubjugateAbilityExtention : AbilityExtension_Psycast
    {
        public override void PostCast(GlobalTargetInfo[] targets, Ability ability)
        {
            
            foreach (Pawn p in targets)
            {
                if (p == null)
                    continue;

                var hediff = p.health.hediffSet.GetFirstHediff<Mag_Hediff_Subjugation>();
                if (hediff == null)
                {
                    Log.Error("COULD NOT FIND SUBJUGATION HEDIFF");
                    continue;
                }
                hediff.MasterPawn = ability.pawn;
            }

            base.PostCast(targets, ability);
        }
    }

 
}
