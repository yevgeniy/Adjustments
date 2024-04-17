using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_Augment:Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            
            var target = targets[0].Thing as Pawn;
            
            var augmenting = pawn.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_Augmenting) as Hediff_Augment;
            
            if (augmenting != null)
            {
                
                pawn.health.RemoveHediff(augmenting);
            }
            
            var augmented = target.health.hediffSet.GetFirstHediffOfDef(Defs.AJD_Augmented) as Hediff_Augment;
            
            if (augmented != null)
            {
                
                target.health.RemoveHediff(augmented);
            }
            
            augmenting = HediffMaker.MakeHediff(Defs.ADJ_Augmenting, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_Augment;
            
            augmenting.Master = pawn;
            augmenting.Subject = target;

            augmented = HediffMaker.MakeHediff(Defs.AJD_Augmented, target, target.health.hediffSet.GetBrain()) as Hediff_Augment;
            
            augmented.Master = pawn;
            augmented.Subject = target;

            pawn.health.AddHediff(augmenting, pawn.health.hediffSet.GetBrain());

            target.health.AddHediff(augmented, target.health.hediffSet.GetBrain());
            

            var effectdef = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_PsycastSkipFlashPurple");
            if (effectdef!=null)
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
