using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using Verse.AI;

namespace Adjustments
{
    public class VehiclePawnProxy
    {
        private readonly Pawn _vehicle;


        public VehiclePawnProxy(Pawn vehicle)
        {
            _vehicle = vehicle;
        }

        public Thing Thing { get { return _vehicle; } }

        public Faction Faction
        {
            get
            {
                return _vehicle.Faction;
            }
        }

        private static Assembly[] assemblies => AppDomain.CurrentDomain.GetAssemblies();
        private static Type VehicleHandlerType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "VehicleHandler");
        private static Type VehiclePawnType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "VehiclePawn");
        private static Type HandlingTypeFlagsType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "HandlingTypeFlags");
        private static Type JobGiver_GotoTravelDestinationVehicleType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "JobGiver_GotoTravelDestinationVehicle");
        private static Type Dialog_FormVehicleCaravanType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "Dialog_FormVehicleCaravan");
        private static Type VehicleDefType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "VehicleDef");
        private static Type VehicleReservationManagerType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "VehicleReservationManager");


        public List<TransferableOneWay> CargoToLoad
        {
            get
            {
                return ClassMaster.GetValueOnInstance<List<TransferableOneWay>>(Thing, "cargoToLoad");
            }
        }

        private PropertyInfo allPawnsAboardPropInfo = VehiclePawnType.GetProperty("AllPawnsAboard", BindingFlags.Public | BindingFlags.Instance);
        public List<Pawn> AllPawnsAboard
        {
            get
            {
                return allPawnsAboardPropInfo.GetValue(Thing) as List<Pawn>;
                //return ClassMaster.GetValueOnInstance<List<Pawn>>(Thing, "AllPawnsAboard");
            }
        }

        public object NextAvailableHandler()
        {
            Type nullableType = typeof(Nullable<>).MakeGenericType(HandlingTypeFlagsType);

            return ClassMaster.Call<object>(Thing, "NextAvailableHandler",
                new object[] {null, false}, new Type[] { nullableType, typeof(bool) });
        }

        //public List<VehicleHandlerProxy> OccupiedHandlers
        //{
        //    get
        //    {
        //        var r= ClassMaster.GetValueOnInstance<List<object>>(Thing, "OccupiedHandlers");
        //        if (r == null)
        //            return null;
        //        return r.Select(v => new VehicleHandlerProxy(v)).ToList();
        //    }
        //}




        public bool CanAccept(Thing thing, out int? count)
        {
            count = null;

            var transferable = GetTransferable(CargoToLoad, thing);

            if (transferable != null && transferable.CountToTransfer > 0)
            {
                count = transferable.CountToTransfer;
                return true;
            }
            return false;

        }

        public int AddOrTransfer(Thing thing, int count)
        {
            return ClassMaster.Call<int>(
                Thing,
                "AddOrTransfer",
                new object[] { thing, count, null },
                new Type[] { typeof(Thing), typeof(int), typeof(Pawn) }
            );
        }

        public static TransferableOneWay GetTransferable(List<TransferableOneWay> transferables, Thing thing)
        {
            foreach (TransferableOneWay transferable in transferables)
            {
                foreach (Thing transferableThing in transferable.things)
                {
                    if (transferableThing == thing)
                    {
                        return transferable;
                    }
                }
            }
            //Unable to find thing instance, match on def
            foreach (TransferableOneWay transferable in transferables)
            {
                foreach (Thing transferableThing in transferable.things)
                {
                    if (transferableThing.def == thing.def)
                    {
                        return transferable;
                    }
                }
            }
            return null;
        }

        public void PromptToBoardVehicle(Pawn pawn, object handler)
        {
            ClassMaster.Call(
               Thing,
               "PromptToBoardVehicle",
               new object[] { pawn, handler }
            );
        }

        public void DisembarkAll()
        {
            ClassMaster.Call(
               Thing,
               "DisembarkAll",
               new object[] { }
            );
        }

        public bool Drafted
        {
            set
            {
                var ignition = ClassMaster.GetValue(Thing, "ignition");
                ClassMaster.SetValue(ignition, "Drafted", true);
            }
        }

        public bool IsOverloaded
        {
            get
            {
                var p = Thing as Pawn;
                return MassUtility.IsOverEncumbered(p);
            }
        }

        public void GoTo(IntVec3 targetPosition)
        {
            Drafted = true;

            Job job = new Job(JobDefOf.Goto, targetPosition)
            {
                locomotionUrgency = LocomotionUrgency.Jog,
                expiryInterval = 999999999
            };

            var p = (Thing as Pawn);
            if (p.jobs.curDriver != null )
            {
                p.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
            }
            p.jobs.StartJob(job);

        }

        private static void AddToTransferables(List<TransferableOneWay> transferables, Thing t, bool setToTransferMax = false)
        {
            TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                transferables.Add(transferableOneWay);
            }
            if (transferableOneWay.things.Contains(t))
            {
                Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t);
                return;
            }
            transferableOneWay.things.Add(t);
            if (setToTransferMax)
            {
                transferableOneWay.AdjustTo(transferableOneWay.CountToTransfer + t.stackCount);
            }
        }

        public void ScheduleItemsToLoad(List<Thing> items)
        {
            List<TransferableOneWay> transferables = new List<TransferableOneWay>();
            foreach (var i in items)
            {
                AddToTransferables(transferables, i, true);
            }

            ClassMaster.SetValue(Thing, "cargoToLoad", transferables);


            var comp = Thing.Map.GetComponent(VehicleReservationManagerType);
            ClassMaster.Call(comp, "RegisterLister", new object[] { Thing, "LoadVehicle" });


        }


        public void SendCaravan(List<Pawn> occupants, int destinationTile)
        {
            var d = Activator.CreateInstance(Dialog_FormVehicleCaravanType, new object[] {
                Thing.Map, false, null, false
            });


            var transferables = new List<TransferableOneWay>();

            var vehTransf = new TransferableOneWay();
            vehTransf.things.Add(Thing);
            vehTransf.AdjustTo(1);

            transferables.Add(vehTransf);

         
            foreach (var p in occupants)
            {
                var pTransf = new TransferableOneWay();
                pTransf.things.Add(p);
                pTransf.AdjustTo(1);

                transferables.Add(pTransf);
            }
            

            Type listType = typeof(List<>).MakeGenericType(VehicleDefType);
            IList vehicleDefs = (IList)Activator.CreateInstance(listType);
            vehicleDefs.Add(Convert.ChangeType(Thing.def, VehicleDefType));


            var startingTile = ClassMaster.CallStatic<int>("CaravanHelper", "BestExitTileToGoTo",
                new object[] { vehicleDefs, destinationTile, Thing.Map }
            );

            ClassMaster.SetValue(d, "transferables", transferables);

            ClassMaster.SetValue(d, "startingTile", startingTile);

            ClassMaster.SetValue(d, "destinationTile", destinationTile);

            ClassMaster.Call(d, "TryFormAndSendCaravan");

        }

        public void ClearCargoSchedule()
        {
            var comp = Thing.Map.GetComponent(VehicleReservationManagerType);
            ClassMaster.Call(comp, "RemoveLister", new object[] { Thing, "LoadVehicle" });
            ClassMaster.SetValue(Thing, "cargoToLoad", new List<TransferableOneWay>());
        }
    }
    //public class VehicleHandlerProxy
    //{
    //    private object _vehicleHandler;

    //    public VehicleHandlerProxy(object t) {
    //        _vehicleHandler = t;
    //    }


    //}
}
