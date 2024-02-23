using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Adjustments
{
    public class WorkGiver_ReloadInStorage: WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return ManagerReloadWeapons.ConsiderWeapons();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return null;
        }
    }
}
