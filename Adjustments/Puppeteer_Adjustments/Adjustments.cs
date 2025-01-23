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
        public static HediffDef BrainLeechHediff;
        public static HediffDef BrainLeechingHediff;
        public static HediffDef VPEP_PuppetHediff;
        public static HediffDef VPEP_PuppeteerHediff;
        public static FieldInfo Master;


        static Adjustments()
        {
            Puppeteer_change();
            BrainLeech_change();
            GetMasterField();
            GetPuppetHediff();

            Remove();



            var pupPath = DefDatabase<PsycasterPathDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
            if (pupPath != null)
                pupPath.ResolveReferences();

        }

        private static void GetPuppetHediff()
        {
            VPEP_PuppetHediff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppet");
            VPEP_PuppeteerHediff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
        }

        private static void GetMasterField()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "Hediff_Puppet");

            if (type == null)
                return;

            Master = type.GetField("master", BindingFlags.Public | BindingFlags.Instance);
        }

        private static void Remove()
        {

            var remove = new string[] { "VPEP_PuppetSwarm", "VPEP_SummonPuppet", "VPEP_BrainCut", "VPEP_Ascension", "VPEP_BrainLeech" };
                var l = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.ToList();
            l.RemoveAll(v => remove.Contains(v.defName));

            DefDatabase<VFECore.Abilities.AbilityDef>.Clear();
            foreach ( var v in l )
            {
                DefDatabase<VFECore.Abilities.AbilityDef>.Add(v);
            }
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
            BrainLeechHediff.hediffClass = typeof(Hediff_BrainLeech);
            

            BrainLeechingHediff = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Leeching");
            BrainLeechingHediff.stages.ForEach(v => v.capMods.First().offset = 0);
            BrainLeechingHediff.hediffClass = typeof(Hediff_BrainLeech);

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
