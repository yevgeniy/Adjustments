using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Adjustments
{
    public class Driver_ReloadInStorage : JobDriver
    {
        private ThingComp _compReloader; /*CompAmmoUser*/

        private ThingWithComps Gun => this.job.targetB.Thing as ThingWithComps;
        private Thing Ammo => this.job.targetA.Thing; /*AmmoThing*/

        private ThingComp CompReloader { get
            {
                if (_compReloader==null && Gun!=null)
                {
                    var methinfo = typeof(ThingWithComps).GetMethod("GetComp");
                    var genMethod = methinfo.MakeGenericMethod(Adjustments.CompAmmoUserType);
                    var comp = genMethod.Invoke(Gun, null);
                    _compReloader = comp as ThingComp;
                }
                return _compReloader;
            } }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {

            var a = pawn.Reserve(Gun, job);
            var b= pawn.Reserve(Ammo, job, 1, Mathf.Min(Ammo.stackCount, job.count), null, errorOnFailed);
            return a && b;

        }
        public override string GetReport()
        {
            return "reloading guns";
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Message("MAKING TOILS");

            if (Gun == null)
            {
                Log.Error("No gun in driver");
                yield return null;
            }

            if (CompReloader == null)
            {
                Log.Error("no ammo comp on gun");
                yield return null;
            }

            AddEndCondition(delegate
            {
                return pawn.Downed || pawn.Dead || pawn.InMentalState || pawn.IsBurning()
                    ? JobCondition.Incompletable
                    : JobCondition.Ongoing;
            });




            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);


            yield return Toils_General.Wait(5, TargetIndex.None);

            var toilGoToCell = Toils_Goto.GotoCell(Ammo.Position, PathEndMode.Touch)
                .FailOnBurningImmobile(TargetIndex.A)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);

            var toilCarryThing = Toils_Haul.StartCarryThing(TargetIndex.A).FailOnBurningImmobile(TargetIndex.A);


            if (Adjustments.AmmoThingType.IsAssignableFrom(TargetThingA.GetType()))
            {

                toilGoToCell.AddEndCondition(() => IsCookingOff(Ammo) ? JobCondition.Incompletable : JobCondition.Ongoing);
                toilCarryThing.AddEndCondition(() => IsCookingOff(Ammo) ? JobCondition.Incompletable : JobCondition.Ongoing);

            }

            toilGoToCell.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            toilCarryThing.FailOnDestroyedNullOrForbidden(TargetIndex.A);

            yield return toilGoToCell;
            yield return toilCarryThing;

            yield return Toils_Goto.GotoCell(Gun.Position, PathEndMode.Touch);

            var reloadWait = new Toil {actor = pawn};
            reloadWait.initAction = () => Log.Message("RELOADING");
            reloadWait.defaultCompleteMode = ToilCompleteMode.Delay;
            reloadWait.defaultDuration = Mathf.CeilToInt(ReloadTime(CompReloader).SecondsToTicks() / pawn.GetStatValue(Adjustments.ReloadSpeed));

            yield return reloadWait.WithProgressBarToilDelay(TargetIndex.B);

            var reloadLogic = new Toil { actor = pawn };
            reloadLogic.initAction = ()=>
            {

                int carrying = pawn.carryTracker.CarriedThing.stackCount;
                int currentMag = CurrentMagCount(CompReloader);
                int total = TotalMagCount(CompReloader);
                var needed = total - currentMag;
                int toAdd = Mathf.Min(needed, carrying);
                CurrentMagCount(CompReloader, toAdd);
                if (CompReloader.parent.def.soundInteract!=null)
                    CompReloader.parent.def.soundInteract.PlayOneShot(new TargetInfo(Gun.Position, Find.CurrentMap, false));

                pawn.carryTracker.CarriedThing.stackCount -= toAdd;
                if (pawn.carryTracker.CarriedThing.stackCount <= 0)
                    pawn.carryTracker.DestroyCarriedThing();


            };

            reloadLogic.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return reloadLogic;




            var t = new Toil();
            t.defaultCompleteMode = ToilCompleteMode.Instant;
            t.initAction = () =>
            {
                Log.Message("COMPLETE");
            };
            t.defaultCompleteMode = ToilCompleteMode.Delay;
            t.defaultDuration = 5;
            yield return t;

        }

        private int TotalMagCount(ThingComp compReloader)
        {
            var props = Adjustments.PropsPropInfo.GetValue(CompReloader);
            return (int)Adjustments.MagazineSizeFieldInfo.GetValue(props);
        }

        private int CurrentMagCount(ThingComp compReloader, int? add=null)
        {
            var current= (int)Adjustments.CurMagCountPropInfo.GetValue(CompReloader);
            if (add==null)
                return current;


            Adjustments.CurMagCountPropInfo.SetValue(CompReloader, Mathf.Min(TotalMagCount(compReloader), current+add.Value) );

            return -1;
        }

        private float ReloadTime(ThingComp compReloader)
        {
            var props = Adjustments.PropsPropInfo.GetValue(CompReloader);
            return (float)Adjustments.ReloadTimeFieldInfo.GetValue(props);
        }

        private bool IsCookingOff(Thing ammo)
        {
            return (bool)Adjustments.IsCookingOffPropInfo.GetValue(ammo);
        }
    }
}
