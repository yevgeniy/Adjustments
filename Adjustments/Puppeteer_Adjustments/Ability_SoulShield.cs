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
using VFECore;
using static UnityEngine.GraphicsBuffer;
using Ability = VFECore.Abilities.Ability;

namespace Adjustments.Puppeteer_Adjustments
{
    public class Ability_SoulShield : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var subject = target.Thing as Pawn;

            if (subject.Dead)
            {
                Messages.Message($"Target is dead.", MessageTypeDefOf.NeutralEvent);
                return false;

            }

            if (subject.IsColonist || subject.IsPrisoner || subject.IsSlaveOfColony)
            {
                return true;
            }

            Messages.Message($"Not a valid colonist.", MessageTypeDefOf.NeutralEvent);
            return false;

        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            /*TODO: if already has, remove and add new */

            var subject = targets[0].Thing as Pawn;


            var hediff = subject.health.hediffSet.GetFirstHediffOfDef(Defs.ADJ_SoulShield_Hediff) as Hediff_SoulShield;
            if (hediff != null)
            {
                subject.health.RemoveHediff(hediff);
            }
            hediff = HediffMaker.MakeHediff(Defs.ADJ_SoulShield_Hediff, subject, subject.health.hediffSet.GetBrain()) as Hediff_SoulShield;
            hediff.Subject = subject;
            hediff.Master = pawn;
            subject.health.AddHediff(hediff, pawn.health.hediffSet.GetBrain());



            var effectdef = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_PsycastSkipFlashPurple");
            if (effectdef != null)
            {
                this.AddEffecterToMaintain(SpawnEffecter(effectdef, subject, this.pawn.Map, new Vector3(), 0.3f), subject.Position, 60);
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
