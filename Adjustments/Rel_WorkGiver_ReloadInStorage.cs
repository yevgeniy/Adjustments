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
    public class Rel_WorkGiver_ReloadInStorage: WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Weapon);
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (!Rel_Adjustments.HasCombatExtended)
                return null;

            return Rel_ManagerReloadWeapons.ConsiderWeapons();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!Rel_Adjustments.HasCombatExtended)
                return null;

            if (!Rel_ManagerReloadWeapons.IsThingInConsideration(t as ThingWithComps))
                return null;

            if (pawn.CurJob != null && pawn.CurJob.def.driverClass == Rel_JobDefOf.ReloadInStorage.driverClass)
                return null;

            var gun = new Rel_GunProxy(t as ThingWithComps);

            if (!pawn.CanReserveAndReach(gun.Thing, PathEndMode.Touch, Danger.Deadly))
                return null;

            var ammoDef = gun.CurrentAmmo;
            int howMuchNeededForFullReload = gun.TotalMagCount - gun.CurrentMagCount;

            if (ammoDef==null)
            {
                Log.Error("Somehow got a gun with no ammoDef");
                return null;
            }
            if (howMuchNeededForFullReload == 0)
            {
                return null;
            }

            Thing ammoThing = FindClosestReachableAmmoThing(ammoDef, pawn, ThingRequestGroup.Pawn)
                ?? FindClosestReachableAmmoThing(ammoDef, pawn, ThingRequestGroup.HaulableEver);

            if (ammoThing == null)
                return null;

            var job = JobMaker.MakeJob(Rel_JobDefOf.ReloadInStorage, ammoThing, gun.Thing);
            job.count = howMuchNeededForFullReload;

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

    }
}
