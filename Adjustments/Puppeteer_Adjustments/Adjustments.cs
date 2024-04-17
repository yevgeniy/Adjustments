using Adjustments.Puppeteer_Adjustments;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{

    [StaticConstructorOnStartup]
    public class Adjustments
    {
        public static HediffDef BrainLeechHediff;
        public static HediffDef BrainLeechingHdeiff;

        static Adjustments()
        {
            Puppeteer_change();
            BrainLeech_change();
            BrainCut_change();
        }

        private static void BrainCut_change()
        {
            var l = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.ToList();
            l.RemoveAll(v => v.defName == "VPEP_BrainCut");

            DefDatabase<VFECore.Abilities.AbilityDef>.Clear();
            foreach (var v in l)
                DefDatabase<VFECore.Abilities.AbilityDef>.Add(v);

            var pupPath = DefDatabase<PsycasterPathDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
            if (pupPath != null)
                pupPath.ResolveReferences();
        }

        private static void Puppeteer_change()
        {
            
            var pupptree = DefDatabase<VanillaPsycastsExpanded.PsycasterPathDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
            if (pupptree != null)
            {
                pupptree.requiredBackstoriesAny.Clear();
                //var requiredBackstoriesAny = pupptree.GetType().GetField("requiredBackstoriesAny", BindingFlags.Public).GetValue(pupptree);
                //requiredBackstoriesAny.GetType().GetMethod("Clear", BindingFlags.Public).Invoke(requiredBackstoriesAny, new object[] { });
            }
        }

        private static void BrainLeech_change()
        {
            BrainLeechHediff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_BrainLeech");
            BrainLeechHediff.stages.ForEach(v => v.capMods.First().offset = 0);

            BrainLeechingHdeiff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Leeching");
            BrainLeechingHdeiff.stages.ForEach(v => v.capMods.First().offset = 0);

            var brainLeechAbility = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_BrainLeech");
            if (brainLeechAbility == null)
                return;
            brainLeechAbility.castTime = 50;
        }
    }

}
