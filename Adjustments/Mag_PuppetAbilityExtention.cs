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


    }
}
