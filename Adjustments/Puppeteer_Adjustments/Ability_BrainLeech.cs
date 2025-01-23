using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_BrainLeech : Ability 
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {

            base.Cast(targets);

            var target = targets[0].Thing as Pawn;

            var brainLeechHediff = target.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechHediff) as Hediff_BrainLeech;
            if (brainLeechHediff != null)
            {
                Log.Message("TARGET ALREADY HAS BRAINLEECH HEDIFF");
                return;
            }

            var leechingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechingHediff) as Hediff_BrainLeech;
            if (leechingHediff==null)
            {
                leechingHediff = HediffMaker.MakeHediff(Adjustments.BrainLeechingHediff, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_BrainLeech;
                leechingHediff.Master = pawn;
                leechingHediff.Subjects = new List<Pawn>();
                pawn.health.AddHediff(leechingHediff, pawn.health.hediffSet.GetBrain());
            }
            leechingHediff.Subjects.Add(target);

            brainLeechHediff = HediffMaker.MakeHediff(Adjustments.BrainLeechHediff, target, target.health.hediffSet.GetBrain()) as Hediff_BrainLeech;
            brainLeechHediff.Master = pawn;
            brainLeechHediff.Subject = target;
            target.health.AddHediff(brainLeechHediff, target.health.hediffSet.GetBrain());

            Rot4 rotation = ((target.GetPosture() != 0) ? pawn.Drawer.renderer.LayingFacing() : Rot4.North);
            var offset = pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).RotatedBy(pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None));

            
            var effecterDef = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_PsycastSkipFlashPurple");
            this.AddEffecterToMaintain(SpawnEffecter(effecterDef, target, this.pawn.Map, offset, 0.3f), target.Position, 60);
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
