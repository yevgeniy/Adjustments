using Adjustments.Puppeteer_Adjustments;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
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
            GetPuppetHediffDef();
        }

        private static void GetPuppetHediffDef()
        {
            VPEP_Puppet = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppet");
        }



        private static void AttachSubjugationAbility()
        {
            var subjabil = DefDatabase<VFECore.Abilities.AbilityDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Subjugation");
            if (subjabil != null)
            {

                (subjabil.modExtensions[0] as AbilityExtension_Psycast).prerequisites.Clear();
                (subjabil.modExtensions[0] as AbilityExtension_Psycast).prerequisites.Add(Defs.ADJ_SoulLeech);


                /*attach master ref on haddif after cast */
                subjabil.modExtensions.Add(new Mag_SubjugateAbilityExtention());

                /*can cast on any pawn.*/
                subjabil.modExtensions.RemoveAll(v => v.GetType().Name == "AbilityExtension_TargetValidator");

                Log.Message("nimm subjugation modifield");
            }
        }

        private static void AttachSubjugation()
        {
            var subjhedd = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Subjugation");
            if (subjhedd != null)
            {
                subjhedd.hediffClass = typeof(Mag_Hediff_Subjugation);
                Log.Message("nimm subjugation hediff modified");
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
