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
        public static HashSet<Pawn, SubjugateComp> Repo = new HashSet<Pawn, SubjugateComp();
        private Trait CahedTrait=null;
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

        public override void PostDestroy(DestroyMode mode, Map map) {
            Repo.Remove(Pawn);
            
            base.PostDestroy(mode,map);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            Repo.Add(Pawn, this);
            CacheTrait();

            base.PostSpawnSetup(respawningAfterLoad);
        }
        private void CacheTrait() {
            CahedTrait = Pawn.story.traits.GetTrait(SubjugatedDefs.Subjugated);
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
            var trait = Pawn.story.traits.GetTrait(SubjugatedDefs.Subjugated);
            if (trait==null) {
                trait = new Trait(SubjugatedDefs.Subjugated, 1, true);
                Pawn.story.traits.GainTrait(trait);
            }
        }
    }
}
