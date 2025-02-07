using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class Adjustments_Mod : Mod
    {
        public Adjustments_Mod(ModContentPack content) : base(content)
        {
        }

        public static Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        public static bool HasCombatExtended { get; } = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Combat Extended");
        public static bool VehicleIsActive { get; } = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Vehicle Framework");

        public static bool AllowToolIsActive { get; } = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Allow Tool");

        public override string SettingsCategory() => "Nimm Adjustments";



        static Adjustments_Mod()
        {
            
            Harmony harmony = new Harmony("nimm.adjustments");
#if DEBUG
            Harmony.DEBUG = true;
#endif

            harmony.PatchAll();

            Log.Message("ADJUSTMENTS PATCHED.");

            AdjustDict();
        }

        private static void AdjustDict()
        {
            /*replace all fat and hulking female bodytypes to normal*/
            var femaleBodyType = DefDatabase<BodyTypeDef>.AllDefs.FirstOrDefault(v => v.defName == "Female");
            Log.Message("FEMALE BODY TYPE: " + femaleBodyType);
            if (femaleBodyType != null)
            {

                foreach (var i in DefDatabase<BackstoryDef>.AllDefs)
                {
                    if (i.bodyTypeFemale != null && (i.bodyTypeFemale.defName == "Fat" || i.bodyTypeFemale.defName == "Hulk"))
                        i.bodyTypeFemale = femaleBodyType;
                }
            }


            var haulUrgent = DefDatabase<WorkTypeDef>.AllDefs.FirstOrDefault(v => v.defName == "HaulingUrgent");
            if (haulUrgent != null)
            {
                var reloadDef = DefDatabase<WorkGiverDef>.AllDefs.FirstOrDefault(v => v.defName == "ReloadTurrets");
                if (reloadDef != null)
                    reloadDef.workType = haulUrgent;
            }

        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            // Surgery Toggle
            listingStandard.CheckboxLabeled("Surgery", ref Adjustments_Settings.SurgeryActive, "Toggle Surgery Adjustment");

            // Explanation area for Surgery
            Rect surgeryExplanationRect = listingStandard.GetRect(
                Text.CalcHeight(
                    "Don't waste surgery component on failed surgery attempt",
                    inRect.width - 30
                )
            );
            Widgets.Label(surgeryExplanationRect, "Don't waste surgery component on failed surgery attempt");

            // Add space between features
            listingStandard.Gap();

            /////////////////////////////////////////

            listingStandard.CheckboxLabeled("Branding", ref Adjustments_Settings.BrandingActive, "Toggle Branding Adjustment");
            var text = "Brand a character with an icon (from skill panel).  Once you pause the game that icon will appear over that character.  On paused screen you will also see how many branded characters you have on screen.";
            Rect brandingExplanationRect = listingStandard.GetRect(
                Text.CalcHeight(
                    text,
                    inRect.width - 30
                )
            );
            Widgets.Label(brandingExplanationRect, text);

            listingStandard.Gap();



            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }


    }
}
