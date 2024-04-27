using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_MindMerge : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var exclusives = new string[] { "ADJ_Augmented", "ADJ_MindMerged", "ADJ_PsySurged", "VPEP_Puppet" };
            var pawn = target.Thing as Pawn;
            if (pawn.health.hediffSet.hediffs.Any(v => exclusives.Contains(v.def.defName)))
            {
                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            var target = targets[0].Thing as Pawn;


            var mergingWith = HediffMaker.MakeHediff(Defs.ADJ_MindMerging, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_MindMerge;

            mergingWith.Master = pawn;
            mergingWith.Subject = target;

            var mindMerged = HediffMaker.MakeHediff(Defs.ADJ_MindMerged, target, target.health.hediffSet.GetBrain()) as Hediff_MindMerge;

            mindMerged.Master = pawn;
            mindMerged.Subject = target;

            pawn.health.AddHediff(mergingWith, pawn.health.hediffSet.GetBrain());

            target.health.AddHediff(mindMerged, target.health.hediffSet.GetBrain());


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
