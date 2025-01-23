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
                return !pawn.health.hediffSet.HasHediff(Adjustments.BrainLeechHediff);
            }
        }

    }
}
