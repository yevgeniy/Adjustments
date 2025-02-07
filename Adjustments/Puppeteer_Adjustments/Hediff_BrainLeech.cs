using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using static UnityEngine.GraphicsBuffer;

namespace Adjustments.Puppeteer_Adjustments
{

    public class Hediff_BrainLeech : HediffWithComps, IMinHeatGiver
    {
        public bool shouldRemove = false;
        public override bool ShouldRemove => shouldRemove;
        public Pawn Master = null;
        public List<Pawn> Subjects = new List<Pawn>();
        public Pawn Subject = null;

        private bool active = true;
        public bool Active { get { return active; } set {  active = value; } }

        public override string Label => Subject == null
            ? Master.LabelShort + " is brian leeching " + string.Join(", ", Subjects.Select(v => v.LabelShort))
            : Subject.LabelShort + "  is being leeched by " + Master.LabelShort + (!this.Active ? " (inactive)" : "");

        public int TotalToRedistribute => Subjects.Count * 50;

        Hediff _puppeteerhediff;
        Hediff PuppeteerHeriff
        {
            get
            {
                if (_puppeteerhediff == null)
                {
                    this.Master.health.hediffSet.TryGetHediff(Adjustments.VPEP_PuppeteerHediff, out _puppeteerhediff);
                }

                return _puppeteerhediff;

            }
        }



        public int? NumberOfPuppets
        {
            get
            {
                if (Subject != null)
                    return null;

                if (PuppeteerHeriff != null)
                {
                    var p = new PuppetHedifProxy(PuppeteerHeriff);
                    var c = p.Puppets.Count;
                    return c;
                }

                return 0;
            }
        }

        public int ActiveSubjectsCount
        {
            get
            {
                var c = 0;
                foreach(var i in this.Subjects)
                {
                    if (i.health.hediffSet.GetFirstHediff<Hediff_BrainLeech>().Active)
                        c++;
                }
                return c;
            }
        }

        public float ConsciousnessAdjustment
        {
            get
            {
                if (NumberOfPuppets != null)
                {
                    return (this.ActiveSubjectsCount * 50) / (NumberOfPuppets.Value + 1) * .01f;
                }
                return 0f;
            }
        }

        public bool IsActive => !shouldRemove;

        public int MinHeat => 5;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);


            if (Subject != null)
            {
                Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(Defs.ADJ_GetOutOfMyHead);
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);

                if (Master.health.hediffSet.TryGetHediff(VPE_DefOf.VPE_PsycastAbilityImplant, out var h))
                {
                    var psyhediff = h as Hediff_PsycastAbilities;
                    psyhediff.AddMinHeatGiver(this);
                }
            }

        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shouldRemove, "hed-bl-shouldRemove");
            Scribe_Values.Look(ref active, "hed-bl-active");

            Scribe_References.Look(ref Master, "hed-bl-m");
            Scribe_References.Look(ref Subject, "hed-bl-subj");
            Scribe_Collections.Look(ref Subjects, "nim-bl-subjs", LookMode.Reference);

            if (Subjects == null)
                Subjects = new List<Pawn>();

        }

        public void ThrowFleck(Vector3 loc, Map map, float throwAngle, Vector3 inheritVelocity)
        {
            if (loc.ToIntVec3().ShouldSpawnMotesAt(map))
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc + new Vector3(Rand.Range(-0.005f, 0.005f),
                    0f, Rand.Range(-0.005f, 0.005f)), map, FleckDefOf.AirPuff, Rand.Range(0.6f, 0.7f));
                dataStatic.rotationRate = Rand.RangeInclusive(-240, 240);
                dataStatic.velocityAngle = throwAngle + (float)Rand.Range(-10, 10);
                dataStatic.velocitySpeed = Rand.Range(0.1f, 0.8f);
                dataStatic.velocity = inheritVelocity * 0.5f;
                dataStatic.instanceColor = Color.magenta;
                dataStatic.scale = 0.9f;
                map.flecks.CreateFleck(dataStatic);
            }
        }

        void StopLeeching()
        {
            this.shouldRemove = true;
        }

        void StopLeech()
        {

            this.shouldRemove = true;
        }

        public override void PostRemoved()
        {
            if (Subject != null)
            {
                if (!Master.health.hediffSet.TryGetHediff(Adjustments.BrainLeechingHediff, out var h))
                {
                    return;
                }

                var mastershediff = h as Hediff_BrainLeech;

                mastershediff.Subjects.Remove(Subject);
                if (mastershediff.Subjects.Count == 0)
                {
                    mastershediff.shouldRemove = true;
                }
            }
            else
            {
                foreach (var subject in Subjects)
                {
                    if (!subject.health.hediffSet.TryGetHediff(Adjustments.BrainLeechHediff, out var h))
                        continue;

                    var brainLeechHediff = h as Hediff_BrainLeech;
                    brainLeechHediff.shouldRemove = true;
                }
            }


            base.PostRemoved();
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 2000==0)
            {
                if (pawn.IsColonist || pawn.IsPrisoner || pawn.IsSlaveOfColony)
                {

                }
                else
                {
                    shouldRemove = true;
                }
            }
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Subject == null)
                yield return new Command_Action
                {
                    defaultLabel = "Stop leeching",
                    defaultDesc = "Stop brainleedhing all subjects",
                    icon = ContentFinder<Texture2D>.Get("Effects/Technomancer/BreakLink/BreakLinkRockConstruct"),
                    action = delegate
                    {
                        StopLeeching();
                    }
                };
            else if (Subject != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Stop brain leech",
                    defaultDesc = "Make " + Master.LabelShort + " stop brainleeching this subject",
                    icon = ContentFinder<Texture2D>.Get("Effects/Technomancer/BreakLink/BreakLinkRockConstruct"),
                    action = delegate
                    {
                        StopLeech();
                    }
                };
            }

            var basegiz = base.GetGizmos();
            foreach (var i in basegiz)
                yield return i;
        }

    }


    class PuppetHedifProxy
    {
        private Hediff _hediff;

        public static List<Assembly> Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();


        public PuppetHedifProxy(Hediff hediff)
        {
            _hediff = hediff;
        }

        public List<Pawn> Puppets
        {
            get
            {
                return PuppetsField.GetValue(_hediff) as List<Pawn>;
            }
        }


        private static FieldInfo _puppetsField;
        public static FieldInfo PuppetsField
        {
            get
            {
                return _puppetsField ?? (
                    _puppetsField = Assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "Hediff_Puppeteer")
                    .GetField("puppets", BindingFlags.Instance | BindingFlags.Public)
                 );
            }
        }

        private static FieldInfo _masterField;
        public static FieldInfo MasterField
        {
            get
            {
                return _masterField ?? (
                    _masterField = Assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "Hediff_Puppet")
                    .GetField("master", BindingFlags.Instance | BindingFlags.Public)
                );
            }
        }
        public Pawn Master
        {
            get
            {
                return MasterField.GetValue(_hediff) as Pawn;
            }
        }
    }



}
