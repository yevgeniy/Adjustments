using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace Adjustments.VehicleDelivery
{
    public class VehicleDeliveryMap : MapComponent
    {
        public static SlotGroup To;
        public static SlotGroup From;
        public static Pawn Vehicle;
        public static List<Pawn> Pawns;

        public static Action OnVehicleStopped = delegate () { };


        public VehicleDeliveryMap(Map map) : base(map)
        {

        }

        public static void ToggleVehicle(Pawn vehicle)
        {
            if (Vehicle != null)
            {
                Log.Message($"turn off");
                Vehicle = null;
            }
            else
            {
                Log.Message($"turn on");
                if (!RecordPawns(vehicle, out var pawns))
                {
                    Log.Message("No pawns in a vehicle");
                    return;
                }
                if (!FindZones(out SlotGroup from, out SlotGroup to))
                {
                    Log.Message($"Can't find 'FROM' and/or 'TO' stockpiles.  Set the names directly");
                    return;
                }


                Pawns = pawns;
                Vehicle = vehicle;
                To = to;
                From = from;
                Run();
            }

        }

        private static bool FindZones(out SlotGroup from, out SlotGroup to)
        {
            Log.Message($"logging zones");
            to = Find.Maps.SelectMany(v => v.haulDestinationManager.AllGroupsListForReading).FirstOrDefault(v => v.GetName() == "To");
            from = Find.Maps.SelectMany(v => v.haulDestinationManager.AllGroupsListForReading).FirstOrDefault(v => v.GetName() == "From");

            if (to != null && from != null)
                return true;

            return false;
        }

        private static bool RecordPawns(Pawn instance, out List<Pawn> pawns)
        {
            Log.Message($"recording pawns");
            pawns = null;
            var vehicle = new VehiclePawnProxy(instance);
            var r = vehicle.AllPawnsAboard;
            if (r == null || r.Count == 0)
            {
                return false;
            }
            pawns = r;
            return true;
        }
        public static HaulState State;

        public static void Run()
        {
            Log.Message($"RUN");
            State = new HaulState(To, From, Vehicle, Pawns, () => Vehicle == null || Pawns.Any(v => v.Downed));
            State.Start();
        }




        public override void MapComponentTick()
        {
            if (State != null)
            {
                State.Tick(out bool terminated);
                if (terminated)
                {
                    State = null;
                    Vehicle = null;
                }
            }

        }
    }

    public class HaulState
    {
        private SlotGroup to;
        private SlotGroup from;
        private Pawn vehicle;
        private List<Pawn> pawns;
        private Func<bool> terminator;
        private List<Action> steps;
        private Action currentStep;
        private bool stepDone;
        private bool stepInit;
        private bool isSameZone;

        public HaulState(SlotGroup to, SlotGroup from, Pawn vehicle, List<Pawn> pawns, Func<bool> terminator)
        {
            this.to = to;
            this.from = from;
            this.vehicle = vehicle;
            this.pawns = pawns;
            this.terminator = terminator;
            Log.Message($"PAWNS: {string.Join(", ", this.pawns)}");
        }
        private int currentStepIndex = -1;
        internal void Start()
        {
            Log.Message($"START");
            steps = GetStateActions().ToList();

            this.currentStep = steps[0];
            this.currentStepIndex = 0;

        }
        internal void Tick(out bool terminated)
        {

            terminated = this.terminator();
            if (terminated)
            {
                Log.Message($"TERMINATING");
                return;
            }

            if (this.currentStep != null)
            {
                if (this.stepDone)
                {
                    NextStep();
                }

                this.currentStep();
            }
        }

        
        private void NextStep(Action nextStep = null)
        {
            if (nextStep != null)
            {
                this.currentStep = nextStep;
                this.currentStepIndex = this.steps.IndexOf(this.currentStep);
            }
            else
            {
                this.currentStepIndex++;
                if (this.currentStepIndex>= this.steps.Count)
                {
                    this.currentStep = delegate () { };
                } 
                else
                {
                    this.currentStep = this.steps[this.currentStepIndex];
                }
            }

            this.stepDone = false;
            this.stepInit = true;
        }

        public IEnumerable<Action> GetStateActions()
        {
            var findFromSource = FindFromSource();
            var boardHandlers1 = BoardHandlers();
            var boardHandlers2 = BoardHandlers();
            var disembark1 = Disembark();
            var disembark2 = Disembark();
            var driveToFromZone = DriveToFromZone();
            var caravanToFromZone = CaravanToFromZone();
            var loadItemsInFromZone = LoadItemsInFromZone();
            var dropExcessWeight = DropExcessWeight();
            var findToSource = FindToSource();
            var driveToToZone = DriveToToZone();
            var caravanToToZone = CaravanToToZone();
            var unloadItems = UnloadItems();


            yield return findFromSource;
            yield return () =>
            {
                NextStep(this.isSameZone ? boardHandlers1 : caravanToFromZone);
            };
            yield return caravanToFromZone;

            yield return boardHandlers1;
            yield return driveToFromZone;
            yield return disembark1;
            yield return loadItemsInFromZone;
            yield return dropExcessWeight;

            yield return findToSource;
            yield return () =>
            {
                NextStep(this.isSameZone ? boardHandlers2 : caravanToToZone);
            };
            yield return caravanToToZone;

            yield return boardHandlers2;
            yield return driveToToZone;
            yield return disembark2;
            yield return unloadItems;
            yield return () =>
            {
                NextStep(findFromSource);
            };


        }

        private Action UnloadItems()
        {
            return ()=>
            {
                Log.Message($"UNLOAD ALL ITEMS");

                
                var v = new VehiclePawnProxy(this.vehicle);

                var firstItem = this.vehicle.inventory.innerContainer.FirstOrDefault();
                if (firstItem!=null)
                {
                    this.vehicle.inventory.innerContainer.TryDrop(firstItem, ThingPlaceMode.Near, firstItem.stackCount, out var _);
                }
                else
                {
                    this.stepDone = true;
                }

            };
        }

        private Action DriveToToZone()
        {
            var targetPosition = this.to.CellsList[Convert.ToInt32(this.to.CellsList.Count / 2)];
            return () =>
            {
                Log.Message($"DRIVE TO TO ZONE ");

                if (this.stepInit)
                {
                    var v = new VehiclePawnProxy(this.vehicle);
                    v.GoTo(targetPosition);
                    VehicleDeliveryMap.OnVehicleStopped = () =>
                    {
                        this.stepDone = true;
                    };

                    this.stepInit = false;
                }
            };
        }

        private Action CaravanToToZone()
        {
            return () =>
            {
                Log.Message($"FORMING CARAVAN TO TO LOCATION");

                if (this.vehicle.Map != null)
                {
                    if (this.vehicle.Map.Tile == this.to.parent.Map.Tile)
                    {
                        this.stepDone = true;
                        return;
                    }
                }

                if (this.stepInit)
                {

                    var v = new VehiclePawnProxy(this.vehicle);
                    v.SendCaravan(this.pawns, this.to.parent.Map.Tile);
                    this.stepInit = false;
                }

            };
        }

        private Action FindToSource()
        {
            return () =>
            {
                Log.Message($"FIND TO SOURCE {this.to}");
                this.isSameZone = this.to.parent.Map == this.vehicle.Map;
                Log.Message($"--is same zone? -- {this.isSameZone}");

                this.stepDone = true;
            };
        }

        private Action DropExcessWeight()
        {
            return () =>
            {
                Log.Message($"DROP EXCESS WEIGHT");

                var v = new VehiclePawnProxy(this.vehicle);
                if (v.IsOverloaded)
                {
                    var firstItem = this.vehicle.inventory.innerContainer.First();
                    Log.Message($"--still overweight dropping {firstItem}");

                    this.vehicle.inventory.innerContainer.TryDrop(firstItem, ThingPlaceMode.Near, firstItem.stackCount, out var _);
                }
                else
                {
                    Log.Message($"--weight good.");
                    this.stepDone = true;
                }
            };
        }

        private Action LoadItemsInFromZone()
        {
            var c = 0;
            return () =>
            {
                Log.Message($"LOAD ITEMS IN FROM ZONE");

                if (this.stepInit)
                {
                    var v = new VehiclePawnProxy(this.vehicle);
                    v.ScheduleItemsToLoad(this.from.HeldThings.ToList());
                    this.stepInit = false;
                    return;
                }
                c++;

                if (c % 200 == 0)
                {
                    var v = new VehiclePawnProxy(this.vehicle);

                    if (this.from.HeldThings.Count() == 0)
                    {
                        Log.Message($"--no more items in from zone");
                        if (this.vehicle.inventory.innerContainer.Count > 0)
                        {
                            Log.Message($"--stuff loaded in vehicle.  Will deliver what is there currently.");
                            this.stepDone = true;
                        }
                        else
                        {
                            Log.Message($"--nothing in load.  Waiting for things to be loaded.");
                        }
                        return;
                    }

                    if (v.IsOverloaded)
                    {
                        Log.Message($"--enough weight loaded.  Time to depart.");
                        /*need to cancel all other things*/
                        v.ClearCargoSchedule();
                        this.stepDone = true;
                        return;
                    }

                    v.ScheduleItemsToLoad(this.from.HeldThings.ToList());
                    return;
                }


            };
        }

        private Action CaravanToFromZone()
        {
            return () =>
            {
                Log.Message($"FORMING CARAVAN TO FROM LOCATION");

                if (this.vehicle.Map != null)
                {
                    if (this.vehicle.Map.Tile == this.from.parent.Map.Tile)
                    {
                        this.stepDone = true;
                        return;
                    }
                }


                if (this.stepInit)
                {

                    var v = new VehiclePawnProxy(this.vehicle);
                    v.SendCaravan(this.pawns, this.from.parent.Map.Tile);
                    this.stepInit = false;
                }



            };
        }

        private Action DriveToFromZone()
        {
            var targetPosition = this.from.CellsList[Convert.ToInt32(this.to.CellsList.Count / 2)];
            return () =>
            {
                Log.Message($"DRIVE TO FROM ZONE ");



                if (this.stepInit)
                {
                    var v = new VehiclePawnProxy(this.vehicle);
                    v.GoTo(targetPosition);
                    VehicleDeliveryMap.OnVehicleStopped = () =>
                    {
                        this.stepDone = true;
                    };

                    this.stepInit = false;
                }


            };
        }

        private Action Disembark()
        {
            return () =>
            {
                Log.Message($"DISEMBARK");

                var v = new VehiclePawnProxy(this.vehicle);
                v.Drafted = false;

                var labels = this.pawns.Select(z => z.Label).ToList();

                v.DisembarkAll();

                /* At this point for some reason we lose our pawns.  Find the pawns again.*/
                this.pawns = Find.Maps.SelectMany(map=>map.mapPawns.AllPawns).Where(z => labels.Contains(z.Label)).ToList();

                this.stepDone = true;
            };
        }

        private Action BoardHandlers()
        {
            var vehicleProxy = new VehiclePawnProxy(this.vehicle);
            var waitOnJobs = new JobDef[] {
                JobDefOf.Ingest, JobDefOf.LayDown, JobDefOf.Dance, JobDefOf.Deathrest, JobDefOf.DeliverFood,
                JobDefOf.DeliverToAltar, JobDefOf.DeliverToBed, JobDefOf.DeliverToCell, JobDefOf.LayDownAwake, JobDefOf.LayDownResting, JobDefOf.Lovin
            };
            return () =>
            {
                if (Find.TickManager.TicksGame % 200 == 0)
                {
                    Log.Message($"BOARD HANDLERS");

                    var boarded = vehicleProxy.AllPawnsAboard;
                    if (boarded.Count > 0 && this.pawns.All(v => boarded.Contains(v) ))
                    {
                        Log.Message($"--All pawns boarded");
                        this.stepDone = true;
                        return;
                    }



                    if (this.stepInit)
                    {
                        if (this.pawns.Any(v => waitOnJobs.Any(vv => vv == v.CurJobDef)))
                        {
                            Log.Message($"--waiting for pawns to finish jobs");
                            return;
                        }

                        foreach (var pawn in this.pawns)
                        {
                            if (boarded.Contains(pawn))
                            {
                                Log.Message($"--pawn already boarded: {pawn}");
                                continue;
                            }
                                

                            Log.Message($"--telling pawn {pawn} to board");
                            var handler = vehicleProxy.NextAvailableHandler();
                            Log.Message($"----hander {handler}");
                            vehicleProxy.PromptToBoardVehicle(pawn, handler);
                        }

                        this.stepInit = false;
                    }

                }
            };
        }

        private Action FindFromSource()
        {
            return () =>
            {
                Log.Message($"FIND FROM SOURCE {this.from}");
                this.isSameZone = this.from.parent.Map == this.vehicle.Map;
                Log.Message($"--is same zone? -- {this.isSameZone}");

                this.stepDone = true;
            };

        }
    }
}
