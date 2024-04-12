using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class Mag_Adjustments
    {
        public static FieldInfo Master;
        public static HediffDef VPEP_Puppet;
        static Mag_Adjustments()
        {
            GetMasterField();
            AttachSubjugation();
            AttachSubjugationAbility();
            AdjustBrainLeech();
            AdjustPuppeteer();
            GetPuppetDef();
        }

        private static void GetPuppetDef()
        {
            VPEP_Puppet = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppet");
        }

        private static void AdjustPuppeteer()
        {
            var pupptree = DefDatabase<Def>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppeteer");
            if (pupptree!=null)
            {
                var requiredBackstoriesAny = pupptree.GetType().GetField("requiredBackstoriesAny", BindingFlags.Public).GetValue(pupptree);
                requiredBackstoriesAny.GetType().GetMethod("Clear", BindingFlags.Public).Invoke(requiredBackstoriesAny, new object[] { });
            }
            
        }

        private static void AdjustBrainLeech()
        {
            var brainleechabil = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_BrainLeech");
            if (brainleechabil != null)
            {
                /*can cast on any pawn.*/
                brainleechabil.modExtensions.RemoveAll(v => v.GetType().Name == "AbilityExtension_TargetValidator");
                Log.Message("ATTACHED3");
            }
        }

        private static void AttachSubjugationAbility()
        {
            var subjabil = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Subjugation");
            if (subjabil != null)
            {
                /*attach master ref on haddif after cast */
                subjabil.modExtensions.Add(new Mag_SubjugateAbilityExtention());

                /*can cast on any pawn.*/
                subjabil.modExtensions.RemoveAll(v => v.GetType().Name == "AbilityExtension_TargetValidator");

                Log.Message("ATTACHED2");
            }
        }

        private static void AttachSubjugation()
        {
            var subjhedd = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Subjugation");
            if (subjhedd != null)
            {
                subjhedd.hediffClass = typeof(Mag_Hediff_Subjugation);
                Log.Message("ATTACHED");
            }
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
    }
}
