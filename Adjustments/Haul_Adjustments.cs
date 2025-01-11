using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class Haul_Adjustments : MapComponent
    {

        //IConstructible c
        public static Dictionary<Faction, Dictionary<Map, List<Thing>>> Constructibles;
        
        static Haul_Adjustments()
        {
            
        }

        public Haul_Adjustments(Map map) : base(map)
        {
        }

        long ticks = 0;

        public override void MapComponentTick()
        {
            ticks++;
            if (ticks % 200 == 0) {
                Constructibles = null;
            }
            base.MapComponentTick();
        }

        
        public static Thing GetSomeConstructable(Pawn pawn, Thing thing)
        {
            Constructibles = Constructibles ?? new Dictionary<Faction, Dictionary<Map, List<Thing>>>();

            if (!Constructibles.ContainsKey(pawn.Faction))
            {
                Constructibles.Add(
                    pawn.Faction,
                    new Dictionary<Map, List<Thing>>()
                );
            }
            if (!Constructibles[pawn.Faction].ContainsKey(pawn.Map))
            {
                Constructibles[pawn.Faction].Add(
                    pawn.Map,
                    pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame))
                        .Concat(pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Blueprint))).ToList()
                );
            }

            var constructs = Constructibles[pawn.Faction][pawn.Map];
            
            foreach (var i in constructs)
            {
                var project = i as IConstructible;
                if (project == null)
                    continue;

                var numberOfThisThingNeeded = project.ThingCountNeeded(thing.def);
                if (numberOfThisThingNeeded > 0)
                {
                    return i;
                }
            }

            return null;
        }

        //public static Job TryIssueConstructableJob(Pawn p, Thing t)
        //{
        //    var constructable = GetSomeConstructable(p, t);

        //    if (constructable == null)
        //        return null;


        //    return null;
        //}

        //public static bool HasConstructable(Thing thing, Pawn pawn)
        //{
        //    return TryIssueConstructableJob(pawn,thing) != null;
        //}

        //public static bool HasBetterPlace(Pawn pawn, Thing thing)
        //{
        //    return HasConstructable(thing, pawn) ||
        //        StoreUtility.TryFindBestBetterStorageFor(thing, pawn, pawn.Map, StoreUtility.CurrentStoragePriorityOf(thing), pawn.Faction, out _, out _, false);
        //}
    }
}

/* Find all frames */
//pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
//bool flag = pawn.carryTracker?.CarriedThing != null && scanner.PotentialWorkThingRequest.Accepts(pawn.carryTracker.CarriedThing) && Validator(pawn.carryTracker.CarriedThing);
