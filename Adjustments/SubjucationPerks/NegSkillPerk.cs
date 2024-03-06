using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.SubjucationPerks
{
    public class NegSkillPerk : BasePerk
    {
        public override SkillDef SkillDef => null;

        public override bool CanHandle(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDef);
            if (SubjugateComp.Repo.ContainsKey(pawn))
            {
                var existing = SubjugateComp.Repo[pawn].Perks.FirstOrDefault(v => v.SkillDef == SkillDef);
                if (existing == null)
                    return true;
                return existing.Disabled == false;
            }
            return false;
        }
        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDefOf.Shooting);
            var currPassion = skill.passion;

            /* All passions above minors will be set to minor passion */
            byte x = (byte)currPassion;
            if (x > 1)
            {
                Log.Message("SKILL MINOR PASSION");
                currPassion = Passion.Minor;
                Explain = "SKILL degraded to Minor passion.";
                skill.passion = currPassion;
            }
            else if (x==1)
            {
                Log.Message("NO PASSION");
                currPassion = Passion.None;
                Explain = "No longer passionate about SKILL.";
                skill.passion = currPassion;
            }
            else
            {
                Log.Message("SKILL DISABLED");
                Disabled = true;
                Explain = "PAWN will no longer do SKILL.";
                skill.passion = Passion.None;
                skill.Notify_SkillDisablesChanged();
            }
            
        }

    }
}
