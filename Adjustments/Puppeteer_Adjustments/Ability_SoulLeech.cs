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
    public class Ability_SoulLeech : Ability 
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            
            base.Cast(targets);

            var target = targets[0].Thing as Pawn;
            Log.Message($"Casting soul leech on target {target}");

            var targetSoulLeech = target.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechHediff) as Hediff_SoulLeech;
            if (targetSoulLeech!=null)
            {
                Log.Message($"--target {target} already has hediff");
                if (targetSoulLeech.LeechActive)
                {
                    Log.Message($"--hediff already active.  Ability fail");
                    return;
                }

                Log.Message($"--activating soul leech on target with master {pawn}");
                targetSoulLeech.Master = pawn;
                targetSoulLeech.LeechActive = true;

            }
            else
            {
                Log.Message($"--creating new leech hediff on target");
                targetSoulLeech = HediffMaker.MakeHediff(Adjustments.BrainLeechHediff, target, target.health.hediffSet.GetBrain()) as Hediff_SoulLeech;
                targetSoulLeech.Type = SoulLeechType.Subject;
                targetSoulLeech.Subject = target;
                targetSoulLeech.Master = pawn;
                targetSoulLeech.LeechActive = true;

                target.health.AddHediff(targetSoulLeech, target.health.hediffSet.GetBrain());
            }


            var masterSoulLeeching = pawn.health.hediffSet.GetFirstHediffOfDef(Adjustments.BrainLeechingHediff) as Hediff_SoulLeech;
            if (masterSoulLeeching!=null)
            {
                Log.Message($"--master {pawn} already has hediff");
                if (masterSoulLeeching.Subjects.Contains(target))
                {
                    Log.Message($"--master already leaching from target {target}.  fail ability.");
                    return;
                }
                masterSoulLeeching.Subjects.Add(target);
            }
            else
            {
                Log.Message($"--master does not have hediff.  Make new one.");
                masterSoulLeeching = HediffMaker.MakeHediff(Adjustments.BrainLeechingHediff, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_SoulLeech;
                masterSoulLeeching.Type = SoulLeechType.Master;
                masterSoulLeeching.Master = pawn;
                masterSoulLeeching.Subjects = new List<Pawn>() { target };

                pawn.health.AddHediff(masterSoulLeeching, pawn.health.hediffSet.GetBrain());
            }


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
