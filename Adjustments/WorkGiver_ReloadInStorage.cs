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
            if (!Adjustments.HasCombatExtended)
                return null;

            return ManagerReloadWeapons.ConsiderWeapons();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is ThingWithComps gun))
                return null;

            if (!pawn.CanReach(new LocalTargetInfo(gun.Position), PathEndMode.Touch, Danger.Deadly))
                return null;

            if (!ManagerReloadWeapons.IsThingInConsideration(gun))
                return null;

            int howMuch = 0;

            /*returns AmmoDef*/
            var ammoDef = GetRequiredAmmoDef(gun, out howMuch);


            if (ammoDef==null)
            {
                Log.Error("Somehow got a gun with no ammoDef");
                return null;
            }
            if (howMuch == 0)
            {
                Log.Error("Somehow considering a gun already full on ammo.");
                return null;
            }

            Thing ammoThing = FindClosestReachableAmmoThing(ammoDef, pawn, ThingRequestGroup.Pawn)
                ?? FindClosestReachableAmmoThing(ammoDef, pawn, ThingRequestGroup.HaulableEver);
            
            if (ammoThing == null)
                return null;


            var job = JobMaker.MakeJob(JobDefOf.ReloadInStorage, ammoThing, gun);

            return job;
        }

        private Thing FindClosestReachableAmmoThing(object ammoDef, Pawn pawn, ThingRequestGroup group)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(group),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                validator: (Thing thing) => thing.def==ammoDef
                    && pawn.CanReserve(thing)
                    && !thing.IsForbidden(pawn.Faction)
            );
            
        }

        private object GetRequiredAmmoDef(ThingWithComps gun, out int howMuch)
        {
            
            var selectedAmmo = ManagerReloadWeapons.GetRequiredAmmoDef(gun, out howMuch);


            return selectedAmmo;
            

        }
    }
}
