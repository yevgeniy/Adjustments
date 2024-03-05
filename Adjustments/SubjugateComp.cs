using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class SubjugateComp : ThingComp
    {
        private float CurrentRating;
        private float ResistanceCap;
        public static readonly TraitDef subjugatedTrait = DefDatabase<TraitDef>.GetNamed("Subjugated");
        private Pawn Pawn
        {
            get
            {
                if (parent is Pawn pawn)
                {
                    return pawn;
                }
                return null;
            }
        }
        static SubjugateComp()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(SubjugateComp) });
            }
        }
        public SubjugateComp()
        {
            CurrentRating = 0f;
            ResistanceCap = 0f;
        }

        public void ActivateSubjugation()
        {
            CurrentRating = 0f;
            if (Pawn==null)
            {
                Log.Error("Pawn not found on SubjugateComp.");
                return;
            }
            ResistanceCap = Pawn.guest.resistance * 10f;
            Log.Message("SUBJUCATION ACTIVATE");
        }

        public void RegisterSeverity(float suffering)
        {
            CurrentRating = Mathf.Min(ResistanceCap, CurrentRating + suffering);

            if (CurrentRating>=ResistanceCap)
            {
                ResistanceCapBreached();
            }
        }

        private void ResistanceCapBreached()
        {
            throw new NotImplementedException();
        }
    }
}
