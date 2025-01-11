using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    public class Adjustments_Settings : ModSettings
    {
        public static bool SurgeryActive = true;
        public static bool BrandingActive = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SurgeryActive, "SurgeryActive", true);
            Scribe_Values.Look(ref BrandingActive, "BrandingActive", true);
        }
    }
}
