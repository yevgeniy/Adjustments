using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_PsychicSurge : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var targetPawn = target.Pawn as Pawn;
            if (targetPawn != null)
            {
                var exclusives = new string[] { "ADJ_Augmented", "ADJ_MindMerged", "ADJ_PsySurged" };
                if (targetPawn.health.hediffSet.hediffs.Any(v => exclusives.Contains(v.def.defName)))
                {
                    return false;
                }

                var hediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(Adjustments.VPEP_PuppetHediff);
                if (hediff == null)
                {
                    Log.Message("NOT A PUPPET");
                    return false;
                }

                var master = Adjustments.Master.GetValue(hediff);
                if (master == null)
                {
                    Log.Message("NO MASTER");
                    return false;
                }


                if (master == pawn)
                    return true;
                else
                {
                    Log.Message("WRONG MASTER");
                    return false;
                }


            }

            return base.ValidateTarget(target, showMessages);
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {

            var target = targets[0].Thing as Pawn;

            Hediff_PsySurging masthed = pawn.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_PsySurging) as Hediff_PsySurging;
            if (masthed == null)
            {
                masthed = HediffMaker.MakeHediff(Defs.ADJ_PsySurging, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_PsySurging;
                masthed.Master = pawn;
                masthed.AddSubject(target);
                pawn.health.AddHediff(masthed, pawn.health.hediffSet.GetBrain());
            }
            else
                masthed.AddSubject(target);

            var hed = HediffMaker.MakeHediff(Defs.ADJ_PsySurged, target, target.health.hediffSet.GetBrain()) as Hediff_PsySurge;
            hed.Master = pawn;
            hed.Subject = target;

            target.health.AddHediff(hed, target.health.hediffSet.GetBrain());

            var effectdef = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_PsycastSkipFlashPurple");
            if (effectdef != null)
            {
                this.AddEffecterToMaintain(SpawnEffecter(effectdef, target, this.pawn.Map, new Vector3(), 0.3f), target.Position, 60);
            }


            base.Cast(targets);
        }

        public override void PostCast(params GlobalTargetInfo[] targets)
        {
            base.PostCast(targets);

            
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