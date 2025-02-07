using Adjustments.Puppeteer_Adjustments;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;

namespace Adjustments.Puppeteer_Adjustments
{

    [StaticConstructorOnStartup]
    public class Adjustments
    {
        public static Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();

        public static HediffDef BrainLeechHediff;
        public static HediffDef BrainLeechingHediff;
        public static HediffDef VPEP_PuppetHediff_HediffDef;
        public static HediffDef VPEP_PuppeteerHediff_HediffDef;
        public static FieldInfo Master;


        static Adjustments()
        {
            
            BrainLeech_ability_changes();
            Puppet_ability_changes();
            GetPuppetHediff();
            Puppeteer_tree_changes();

            var pupPath = DefDatabase<PsycasterPathDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
            if (pupPath != null)
                pupPath.ResolveReferences();

        }

        private static void Puppet_ability_changes()
        {
            var puppetAbility = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppet");
            if (puppetAbility != null)
            {

                (puppetAbility.modExtensions[0] as AbilityExtension_Psycast).prerequisites.Clear();
                (puppetAbility.modExtensions[0] as AbilityExtension_Psycast).prerequisites.Add(Defs.ADJ_SoulLeech);

                puppetAbility.modExtensions.Add(new AbilityExtension_Puppet());

                /*can cast on any pawn.*/
                puppetAbility.modExtensions.RemoveAll(v => v.GetType().Name == "AbilityExtension_TargetValidator");

                Log.Message("nimm subjugation modifield");
            }
        }

        private static void GetPuppetHediff()
        {
            VPEP_PuppetHediff_HediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppet");
            VPEP_PuppeteerHediff_HediffDef = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
        }

        private static void RemoveAbilities()
        {

            var remove = new string[] { "VPEP_PuppetSwarm", "VPEP_SummonPuppet", "VPEP_BrainCut", "VPEP_Ascension", "VPEP_BrainLeech",
                "VPEP_Subjugation", "VPEP_Degrade"
            };
            
            var l = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.ToList();
            l.RemoveAll(v => remove.Contains(v.defName));

            DefDatabase<VFECore.Abilities.AbilityDef>.Clear();
            foreach (var v in l)
            {
                DefDatabase<VFECore.Abilities.AbilityDef>.Add(v);
            }
        }


        private static void Puppeteer_tree_changes()
        {
            var pupptree = DefDatabase<VanillaPsycastsExpanded.PsycasterPathDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
            if (pupptree != null)
            {
                pupptree.requiredBackstoriesAny.Clear();
                //var requiredBackstoriesAny = pupptree.GetType().GetField("requiredBackstoriesAny", BindingFlags.Public).GetValue(pupptree);
                //requiredBackstoriesAny.GetType().GetMethod("Clear", BindingFlags.Public).Invoke(requiredBackstoriesAny, new object[] { });
            }

            RemoveAbilities();
        }

        private static void BrainLeech_ability_changes()
        {
            BrainLeechHediff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_BrainLeech");
            BrainLeechHediff.stages.ForEach(v => v.capMods.First().offset = 0);
            BrainLeechHediff.hediffClass = typeof(Hediff_SoulLeech);


            BrainLeechingHediff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Leeching");
            BrainLeechingHediff.stages.ForEach(v => v.capMods.First().offset = 0);
            BrainLeechingHediff.hediffClass = typeof(Hediff_SoulLeech);

            //var brainLeechAbility = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_BrainLeech");
            //if (brainLeechAbility == null)
            //    return;
            //brainLeechAbility.castTime = 50;
            //brainLeechAbility.abilityClass = typeof(Ability_BrainLeech);

            ///*can cast on any pawn.*/
            //brainLeechAbility.modExtensions.RemoveAll(v => v.GetType().Name == "AbilityExtension_TargetValidator");
            //Log.Message("nimm brainleach modified");

        }



    }

}
