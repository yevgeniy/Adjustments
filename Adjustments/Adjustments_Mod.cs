using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Adjustments
{
    public class Adjustments_Mod : Mod
    {
        public Adjustments_Mod(ModContentPack content) : base(content)
        {
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

        public override string SettingsCategory() => "Nimm Adjustments";
    }
}
