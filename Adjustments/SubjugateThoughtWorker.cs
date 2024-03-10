using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    public class SubjugateThoughtWorker : ThoughtWorker_Precept
    {
        public override float MoodMultiplier(Pawn p)
        {
            return Find.CurrentMap.mapPawns.AllPawns.Where(v => v.IsColonist && v.gender == Gender.Female).Count();

        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.IsColonist && p.gender==Gender.Male)
                return Find.CurrentMap.mapPawns.AllPawns.Any(v => v.IsColonist && v.gender == Gender.Female);
            return false;
        }
    }

    public class SubjugateAllWomenSlaves:ThoughtWorker_Precept
    {
        
        public override float MoodMultiplier(Pawn p)
        {
            return Find.CurrentMap.mapPawns.AllPawns.Where(v => v.IsSlave && v.gender == Gender.Female).Count();
        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            var hasSlavewomen = false;
            foreach(var pawn in Find.CurrentMap.mapPawns.AllPawns.Where(v=>v.gender==Gender.Female))
            {
                if (pawn.IsColonist)
                    return false;

                else if (pawn.IsSlave)
                {
                    hasSlavewomen = true;
                } 
            }

            return hasSlavewomen;
        }
    }
}
