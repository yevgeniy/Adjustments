using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_SoulGrowth: VFECore.Abilities.Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var t = target.Thing as Pawn;
            if (!t.health.hediffSet.TryGetHediff(Adjustments.VPEP_PuppetHediff_HediffDef, out var hediff))
            {
                Messages.Message($"Target is not a puppet.", MessageTypeDefOf.NeutralEvent);
                return false;
            }

            PuppetHediffProxy h = new PuppetHediffProxy(hediff);
            var master = h.Master;

            if (this.pawn != master)
            {
                Messages.Message($"Target must be puppet of the caster.", MessageTypeDefOf.NeutralEvent);
                return false;
            }

            if (t.health.hediffSet.TryGetHediff(Defs.ADJ_SoulGrowth_Hediff, out var h2) 
                && h2 is Hediff_SoulGrowth soulGrowth 
                && soulGrowth.IsGrowthActive)
            {
                Messages.Message($"Target is already under effect of ming growth", MessageTypeDefOf.NeutralEvent);
                return false;
            }


            return true;
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            Log.Message($"Casting mind growth.");
            base.Cast(targets);

            var target = targets[0].Thing as Pawn;

            var mindGrowth = HediffMaker.MakeHediff(Defs.ADJ_SoulGrowth_Hediff, target, target.health.hediffSet.GetBrain()) as Hediff_SoulGrowth;
            mindGrowth.Type = MindGrowthType.Subject;
            mindGrowth.Subject = target;
            mindGrowth.Master = pawn;
            mindGrowth.IsGrowthActive = true;

            target.health.AddHediff(mindGrowth, target.health.hediffSet.GetBrain());


            var masterMindGrowth = pawn.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulGrowth_Hediff) as Hediff_SoulGrowth;
            if (masterMindGrowth==null)
            {
                Log.Message($"--master does not have hediff");
                masterMindGrowth= HediffMaker.MakeHediff(Defs.ADJ_SoulGrowth_Hediff, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_SoulGrowth;
                masterMindGrowth.Type = MindGrowthType.Master;
                masterMindGrowth.Subjects = new List<Pawn>() { };
                masterMindGrowth.Master = pawn;
                pawn.health.AddHediff(masterMindGrowth, pawn.health.hediffSet.GetBrain());
            }
            masterMindGrowth.Subjects.Add(target);



            var effectdef = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_PsycastSkipFlashPurple");
            if (effectdef != null)
            {
                this.AddEffecterToMaintain(SpawnEffecter(effectdef, target, this.pawn.Map, new Vector3(), 0.3f), target.Position, 60);
            }

        }

        public Effecter SpawnEffecter(EffecterDef effecterDef, Thing target, Map map, Vector3 offset, float scale)
        {
            Effecter effecter = new Effecter(effecterDef);
            effecter.offset = offset;
            effecter.scale = scale;
            TargetInfo targetInfo = new TargetInfo(target.Position, map);
            effecter.Trigger(targetInfo, targetInfo);
            return effecter;
        }
    }
}
