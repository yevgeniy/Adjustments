using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{

    public class Thought_GetOutOfMyHead : Thought_Memory
    {
        public override int CurStageIndex => 0;


        public override bool ShouldDiscard
        {
            get
            {
                if (pawn.health.hediffSet.TryGetHediff(Adjustments.BrainLeechHediff, out var h) && h is Hediff_SoulLeech soulLeechHediff )
                {
                    return soulLeechHediff.LeechActive;
                }
                return false;
            }
        }

    }
}
