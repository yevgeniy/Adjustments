using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class SubjectComp : ThingComp
    {
        static SubjectComp()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(SubjectComp) });
            }
        }
    }
}
