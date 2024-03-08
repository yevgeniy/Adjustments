using Adjustments.SubjucationPerks;
using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            new PerkNegSkillConstruction(),
            new PerkNegSkillMelee(),
            new PerkNegSkillMining(),
            new PerkNegSkillShooting(),
            new PerkNegHateArmor()
        };
        public static List<BasePerk> OtherPerks = new List<BasePerk>
        {
            new PerkPlant(),
            new PerkCooking(),
            new PerkArtistic(),
            new PerkNudistTrait(),
            new PerkTailoring()
        };


        public int CurrentSubjugationLevel;
        private bool SubjugationActive=false;
        private float CurrentRating;
        private float ResistanceCap;
        private double CurrentContentScore;
        private double ContentScoreLimit;
        static double gainPerTickPerPerk = Convert.ToDouble(20) / Convert.ToDouble(GenDate.TicksPerHour);

        public List<BasePerk> Perks = new List<BasePerk>();
        private bool IsContent;
        private Need_Suppression SupNeed;

        public float ContentPercent { get
            {
                if (IsContent)
                    return 100f;
                return Mathf.Floor( (float)( CurrentContentScore / ContentScoreLimit * 100f) ) ;
            } }

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

        public object ContentStr => IsContent
            ? Pawn.Name.ToStringShort + " is happy being a slave"
            : "Content: " + ContentPercent + "%";

        static SubjugateComp()
        {
            /*add subjugate comp to all defs having a race */
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
        long ticksbuffer = 0;
        public override void CompTick()
        {
            ticksbuffer++;
            if (IsContent)
            {
                SupNeed = SupNeed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                SupNeed.CurLevel = 1f;
            }

            base.CompTick();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();


            if (!IsContent && Pawn.gender == Gender.Female && Pawn.IsSlave)
            {
                double newval = CurrentContentScore + Convert.ToDouble(CurrentSubjugationLevel) * gainPerTickPerPerk * ticksbuffer;
                CurrentContentScore = newval > ContentScoreLimit ? ContentScoreLimit : newval;

                if (CurrentContentScore == ContentScoreLimit)
                    IsContent = true;
            }

            ticksbuffer = 0;
        }

        public override void PostDeSpawn(Map map)
        {
            Repo.Remove(Pawn);
            base.PostDeSpawn(map);
        }
     


        public void ActivateSubjugation()
        {
            
            if (SubjugationActive) {
                return;
            }

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
            Scribe_Values.Look(ref CurrentContentScore, "subjugate-cur-cont-scor");
            Scribe_Values.Look(ref ContentScoreLimit, "subjugate-cont-scor-lim");
            Scribe_Values.Look(ref IsContent, "subjugate-is-cont");
            


            if (!Repo.ContainsKey(Pawn))
                Repo.Add(Pawn, this);

            if (Perks == null)
                Perks = new List<BasePerk>();

            Log.Message("POST INIT " + Pawn);
            if (Pawn.gender!=Gender.Female || Pawn.RaceProps.Animal)
            {
                Log.Message("REMOVING: " + Pawn);
                Pawn.AllComps.Remove(this);
            }
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

        public void RegisterSeverity(float severity)
        {
            if (!SubjugationActive)
                return;

            ContentScoreLimit += severity;
            IsContent = false;

            CurrentRating = Mathf.Min(ResistanceCap, CurrentRating + severity * .1f);
            


            /*lower resistance by the beating amount*/
            Pawn.guest.will = Mathf.Max(.1f, Pawn.guest.will - severity * .01f);

            if (CurrentRating>=ResistanceCap)
            {
                UpgradeSubjugation();
            }
        }

        private void UpgradeSubjugation()
        {
            Log.Message("ADDING TRAIT");

            var t = Pawn.story.traits.GetTrait(SubjugatedDefs.Subjugated);
            if (t==null) {
                Pawn.story.traits.GainTrait(new Trait(SubjugatedDefs.Subjugated, 0, true));
            } else
            {
                Pawn.story.traits.RemoveTrait(t);
                Pawn.story.traits.GainTrait(new Trait(SubjugatedDefs.Subjugated));
            }
            

            CurrentSubjugationLevel++;

            AddPerks();

            Messages.Message("Subjugated: " + Pawn.Name.ToStringShort, MessageTypeDefOf.NeutralEvent, true);

            SubjugationActive=false;
        }

        private void AddPerks()
        {
            /* Negative perk */
            var perkType = NegPerks.Where(v => v.CanHandle(Pawn)).RandomElement();
            if (perkType!=null)
            {
                var perk = Perks.FirstOrDefault(v => v.GetType().Name == perkType.GetType().Name);
                if (perk == null)
                {
                    perk = (BasePerk)Activator.CreateInstance(perkType.GetType());
                    Perks.Add(perk);
                }
                perk.Activate(parent as Pawn);
            }
            

            /*Other perk*/
            perkType = OtherPerks.Where(v => v.CanHandle(Pawn)).RandomElement();
            if (perkType!=null)
            {
                var perk = Perks.FirstOrDefault(v => v.GetType().Name == perkType.GetType().Name);
                if (perk == null)
                {
                    perk = (BasePerk)Activator.CreateInstance(perkType.GetType());
                    Perks.Add(perk);
                }

                perk.Activate(parent as Pawn);
            }
            
            
        }

        public static SubjugateComp GetComp(Pawn pawn)
        {
            if (!Repo.ContainsKey(pawn))
            {
                var comp = pawn.GetComp<SubjugateComp>();
                if (comp == null)
                    return null;

                Repo.Add(pawn, comp);

            }
            return Repo[pawn];

        }
    }
}
