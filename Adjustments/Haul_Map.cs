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
    public class Haul_Map : MapComponent
    {

        //IConstructible c
        public static HashSet<Thing> Constructibles = new HashSet<Thing>();

        static Haul_Map()
        {

        }

        public Haul_Map(Map map) : base(map)
        {
        }

        

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (Find.TickManager.TicksGame % 250==0)
            {
                Constructibles.RemoveWhere(v => !v.Spawned);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref Constructibles, "nimm-constructables", LookMode.Reference);

            if (Constructibles == null)
                Constructibles = new HashSet<Thing>();
        }


        public static Thing GetLocationOfMatchingConstructable(Pawn pawn, Thing thing)
        {

            foreach (var i in Constructibles)
            {
                if (!i.Spawned)
                {
                    continue;
                }

                if (i.Faction != pawn.Faction)
                {
                    continue;
                }

                var project = i as IConstructible;

                if (project == null)
                    continue;

                var numberOfThisThingNeeded = project.ThingCountNeeded(thing.def);
                if (numberOfThisThingNeeded > 0)
                {
                    Log.Message(i + ", " + thing + " " + numberOfThisThingNeeded);
                    return i;
                }
            }

            return null;
        }

    }
}

public class ThingHaulDestination : IHaulDestination
{
    private Thing _thing;



    public ThingHaulDestination(Thing t)
    {
        _thing = t;
    }

    public Thing Thing => _thing;

    public static explicit operator Thing(ThingHaulDestination haulableDestination)
    {
        return haulableDestination.Thing;
    }

    public IntVec3 Position => Thing.Position;

    public Map Map => Thing.Map;

    public bool StorageTabVisible => true;

    public bool Accepts(Thing t)
    {
        var project = Thing as IConstructible;

        if (project == null)
            return false;

        var numberOfThisThingNeeded = project.ThingCountNeeded(t.def);
        if (numberOfThisThingNeeded > 0)
        {
            return true;
        }

        return false;
    }

    public StorageSettings GetParentStoreSettings()
    {
        return null;
    }

    public StorageSettings GetStoreSettings()
    {
        return null;
    }

    public void Notify_SettingsChanged()
    {

    }
}

/* Find all frames */
//pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
//bool flag = pawn.carryTracker?.CarriedThing != null && scanner.PotentialWorkThingRequest.Accepts(pawn.carryTracker.CarriedThing)
//  && Validator(pawn.carryTracker.CarriedThing);
