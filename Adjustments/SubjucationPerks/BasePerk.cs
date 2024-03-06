using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.SubjucationPerks
{
    public class BasePerk : IPerk, INegSkillPerk, IExposable
    {

        private string explain;
        public virtual string Explain { get { return explain; } set { explain = value; } }


        public virtual SkillDef SkillDef => throw new NotImplementedException();

        private bool disabled;
        public virtual bool Disabled { get { return disabled; } set { disabled = value; } }

        public virtual void Activate(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanHandle(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual void Deactivate(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref explain, "subjugate-perk-explain");
            Scribe_Values.Look(ref disabled, "subjugate-perk-disabled");
        }

        public virtual bool IsDisabled(SkillRecord skill)
        {
            if (skill.def == SkillDef)
                return Disabled;

            return false;
        }

        public virtual string Describe(Pawn pawn)
        {
            return Explain.Replace("SKILL", SkillDef.skillLabel).Replace("PAWN", pawn.Name.ToStringShort);
        }
    }
}
