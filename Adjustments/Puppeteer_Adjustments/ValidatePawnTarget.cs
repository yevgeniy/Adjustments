using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore.Abilities;

namespace Adjustments.Puppeteer_Adjustments
{
    public class ValidatePawnTarget: AbilityExtension_AbilityMod
    {
        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            if (target.HasThing && target.Pawn!=null)
            {
                var pawn = target.Pawn;
                return true;
            }
            return false;
        }
    }
}
