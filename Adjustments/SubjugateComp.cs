using Adjustments.SubjucationPerks;
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
        
        public static readonly TraitDef SubjugatedTrait = DefDatabase<TraitDef>.GetNamed("Subjugated");
        public static Dictionary<Pawn, SubjugateComp> Repo = new Dictionary<Pawn, SubjugateComp>();
        public static List<BasePerk> NegPerks = new List<BasePerk>
        {
            new PerkNegSkillShooting()
        };

        public int CurrentSubjugationLevel;
        private bool SubjugationActive=false;
        private float CurrentRating;
        private float ResistanceCap;
        public List<BasePerk> Perks = new List<BasePerk>();
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
        
        

        public void ActivateSubjugation()
        {
            
            if (SubjugationActive) {
                return;
            }

            if (!Repo.ContainsKey(Pawn))
                Repo.Add(Pawn, this);

            SubjugationActive =true;
            CurrentRating = 0f;

            ResistanceCap = GenResistance();


            Log.Message("SUBJUGATION ACTIVATED: CAP: " + ResistanceCap);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref Perks, "subjugate-perks");
            Scribe_Values.Look(ref CurrentSubjugationLevel, "subjugate-current-level" );
            Scribe_Values.Look(ref SubjugationActive, "subjugate-active" );
            Scribe_Values.Look(ref CurrentRating, "subjugate-current-rating" );
            Scribe_Values.Look(ref ResistanceCap, "subjugate-res" );


            if (!Repo.ContainsKey(Pawn))
                Repo.Add(Pawn, this);

            if (Perks == null)
                Perks = new List<BasePerk>();
        }
        

        private float GenResistance()
        {
            FloatRange value = Pawn.kindDef.initialResistanceRange.Value;
            float single = value.RandomInRange;
            if (Pawn.royalty != null)
            {
                RoyalTitle mostSeniorTitle = Pawn.royalty.MostSeniorTitle;
                if (mostSeniorTitle != null)
                {
                    single += mostSeniorTitle.def.recruitmentResistanceOffset;
                }
            }
            return (float)GenMath.RoundRandom(single);
        }

        public void RegisterSeverity(float suffering)
        {
            if (!SubjugationActive)
                return;

            CurrentRating = Mathf.Min(ResistanceCap, CurrentRating + suffering * .1f);

            /*lower resistance by the beating amount*/
            Pawn.guest.resistance = Mathf.Max(.1f, Pawn.guest.resistance - suffering * .1f);

            if (CurrentRating>=ResistanceCap)
            {
                UpgradeSubjugation();
            }
        }

        private void UpgradeSubjugation()
        {
            Log.Message("ADDING TRAIT");
            var trait = Pawn.story.traits.GetTrait(SubjugatedDefs.Subjugated);
            
            if (trait==null) {
                Pawn.story.traits.GainTrait(new Trait(SubjugatedDefs.Subjugated, 1, true));
            }

            CurrentSubjugationLevel++;

            AddRandNegative();
            //AddRandNegative();
            //AddRandPossitive();

            SubjugationActive=false;
        }

        private void AddRandPossitive()
        {
            throw new NotImplementedException();
        }

        private void AddRandNegative()
        {
            var negType = NegPerks.Where(v => v.CanHandle(Pawn)).RandomElement();

            var perk = (BasePerk)Activator.CreateInstance(negType.GetType());

            perk.Activate(parent as Pawn);
            Perks.Add(perk);
        }
    }
}
