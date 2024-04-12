using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class Haul_Adjustments
    {
        static Haul_Adjustments()
        {
            var delivertoframe = DefDatabase<WorkGiverDef>.AllDefs.First(v => v.defName == "DeliverResourcesToFrames");
            delivertoframe.priorityInType = 17;

            var delivertoprints = DefDatabase<WorkGiverDef>.AllDefs.First(v => v.defName == "DeliverResourcesToBlueprints");
            delivertoprints.priorityInType = 16;

            var haulwork = DefDatabase<WorkTypeDef>.AllDefs.First(v => v.defName == "Hauling");
            haulwork.workGiversByPriority.Clear();
            haulwork.ResolveReferences();


        }
    }
}
