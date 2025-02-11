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
    public enum SoulLeechType
    {
        Master,
        Subject
    }

    public class Hediff_SoulLeech : HediffWithComps, IMinHeatGiver
    {

        public SoulLeechType Type;
        public override bool ShouldRemove =>
            Type == SoulLeechType.Master && Subjects.Count == 0 && TotalLeachedReserve == 0f
            || Type == SoulLeechType.Subject && LeechActive == false && AmmountLeachedAway == 0f;

        public Pawn Master = null;
        public List<Pawn> Subjects = new List<Pawn>();
        public Pawn Subject = null;

        private float ammountLeachedAway;
        public float AmmountLeachedAway { get { return ammountLeachedAway; } set { ammountLeachedAway = value; } }
        private float totalLeachedReserve;
        public float TotalLeachedReserve { get { return totalLeachedReserve; } set { totalLeachedReserve = value; } }


        private bool leechActive = true;
        public bool LeechActive
        {
            get { return leechActive; }
            set
            {
                leechActive = value;
            }
        }

        public override string Label => Type == SoulLeechType.Master ? MasterLabel : SubjectLabel;
        public string MasterLabel
        {
            get
            {
                if (Subjects.Count > 0)
                {
                    return $"Soul leeching {string.Join(", ", Subjects.Select(v => v.LabelShort))} ({TotalLeachedReserve * 100f})";
                }
                return $"Leeched away soul reserve ({TotalLeachedReserve * 100f})";

            }
        }
        public float MaxLeechStore
        {
            get
            {
                return .5f + 50f * GetStatPoints();
            }
        }
        public string SubjectLabel
        {
            get
            {
                if (LeechActive)
                {
                    return $"{Master.LabelShort} leeching my soul! ({AmmountLeachedAway * 100})";
                }
                else
                {
                    return $"My soul was leached away ({AmmountLeachedAway * 100})";
                }

            }
        }

        public int MinHeat => 5;
        public bool IsActive => LeechActive;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);


            if (Type == SoulLeechType.Subject)
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

        void StopLeeching()
        {
            foreach (var i in Subjects)
            {
                if (i.health.hediffSet.TryGetHediff(Adjustments.BrainLeechHediff, out var h))
                {
                    var soulLeechHediff = h as Hediff_SoulLeech;
                    soulLeechHediff.LeechActive = false;
                }
                else
                {
                    Log.Error($"could no find soul leech hediff on subject {i}!");
                    return;
                }
            }
            Subjects.Clear();
        }

        void StopLeech()
        {

            LeechActive = false;
            if (Master.health.hediffSet.TryGetHediff(Adjustments.BrainLeechingHediff, out var h))
            {
                var soulLeechingHediff = h as Hediff_SoulLeech;
                soulLeechingHediff.Subjects.Remove(Subject);
            }
            else
            {
                Log.Error($"count not find soul leeching hediff on master {Master}!");
            }
        }

        public override void PostRemoved()
        {

            base.PostRemoved();
        }

        public int GetStatPoints()
        {
            return Utils.GetPsyStatPoints(Master);
        }

        private int totalTicks = 0;
        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                /*if master is not on the same map, stop leech */
                if (Type == SoulLeechType.Subject && LeechActive)
                {
                    if (Subject.Map != Master.Map)
                    {
                        StopLeech();
                    }
                }

            }

            if (!pawn.Dead)
            {
                totalTicks++;

                if (totalTicks % GenDate.TicksPerHour == 0)
                {
                    if (Type == SoulLeechType.Subject && LeechActive)
                    {

                        if (Master.health.hediffSet.TryGetHediff(Adjustments.BrainLeechingHediff, out var h)
                            && h is Hediff_SoulLeech masterSoulLeechHediff)
                        {
                            var psyupgrades = masterSoulLeechHediff.GetStatPoints();
                            var potency= .01f + psyupgrades * .01f * 1f / 10f;

                            Log.Message($"POTENCY LEECH: {potency}");

                            AmmountLeachedAway += potency;
                            masterSoulLeechHediff.AddLeechedReserve(potency);
                            Subject.health.capacities.Notify_CapacityLevelsDirty();
                        }
                        else
                        {
                            Log.Error($"Could not find master leech hediff {Master}");
                        }
                    }
                }
            }
        }

        private void AddLeechedReserve(float amt)
        {
            TotalLeachedReserve = Mathf.Min(MaxLeechStore, TotalLeachedReserve + amt);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Type == SoulLeechType.Master && Subjects.Count>0)
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
            else if (Type == SoulLeechType.Subject && LeechActive)
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
        public bool TryDrawReservePoint(out float value)
        {
            

            value = 0f;
            if (TotalLeachedReserve > 0f)
            {
                var psyupgrades = GetStatPoints();
                var potency = .01f + psyupgrades * .01f * 1f / 10f;
                Log.Message($"POTENCY GROWTH: {potency}");

                value = potency;
                TotalLeachedReserve -= value;
                return true;
            }

            return false;

        }
        public bool TryDrawReserve(float amt, out float value)
        {
            value = 0f;

            if (amt >= TotalLeachedReserve)
            {
                TotalLeachedReserve -= amt;
                value = amt;

                return true;
            }

            return false;
        }



        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref shouldRemove, "hed-bl-shouldRemove");
            Scribe_Values.Look(ref leechActive, "hed-bl-active");
            Scribe_Values.Look(ref ammountLeachedAway, "hed-bl-leached-away");
            Scribe_Values.Look(ref totalLeachedReserve, "hed-bl-leeched-res");

            Scribe_Values.Look(ref Type, "hed-bl-type");
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


    }

}
