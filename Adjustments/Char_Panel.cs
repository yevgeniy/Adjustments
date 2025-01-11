using Adjustments.Remember_Weapon;
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
    public class Char_Panel
    {
        //static MainTabWindow_Work WorkWindow;
        //static MainTabWindow_Assign AssignWindow;
        //static MainTabWindow_Schedule ScheduleWindow;
        //public static void Reset()
        //{
        //    WorkWindow = new MainTabWindow_Work
        //    {
        //        def = DefDatabase<MainButtonDef>.AllDefs.First(v => v.defName == "Work")
        //    };
        //    WorkWindow.PostOpen();


        //    AssignWindow = new MainTabWindow_Assign
        //    {
        //        def = DefDatabase<MainButtonDef>.AllDefs.First(v => v.defName == "Assign")
        //    };
        //    AssignWindow.PostOpen();

        //    ScheduleWindow = new MainTabWindow_Schedule
        //    {
        //        def = DefDatabase<MainButtonDef>.AllDefs.First(v => v.defName == "Schedule")
        //    };
        //    ScheduleWindow.PostOpen();


        //}
        private static Pawn SelectedPawn = null;
        public static void Fill(Rect inRect, Pawn pawn)
        {
            if (SelectedPawn==null || !Find.Selector.SingleSelectedThing.Equals(SelectedPawn))
            {
                //Reset();
                SelectedPawn = Find.Selector.SingleSelectedThing as Pawn;
            }
            var anchor = Text.Anchor;
            var font = Text.Font;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;


            var r = inRect.ContractedBy(7f);


            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(new Rect(r.x, r.y, r.width/4, r.height));

            /*surgery*/
            var surgery=Char_Manager.CanDoSurgery(pawn);
            listingStandard.CheckboxLabeled("Can Do Surgery:", ref surgery, "Allow pawn to do surgery.");
            
            Char_Manager.CanDoSurgery(pawn, surgery);

            
            listingStandard.Gap();

            /*preaching*/
            var preaching = Char_Manager.CanDoPreach(pawn);
            listingStandard.CheckboxLabeled("Can Preach:", ref preaching, "Allow pawn to preach to prisoners.");
            Char_Manager.CanDoPreach(pawn, preaching);

            listingStandard.Gap();

            /*weapon memory*/
            var weaponName= Manager.GetWeaponName(pawn);
            if (!string.IsNullOrEmpty(weaponName))
            {
                listingStandard.Label(weaponName);
            }
            

            listingStandard.End();


            //r = new Rect(r.x, r.y, r.width, 80f);
            //Widgets.BeginGroup(r);
            //RenderMisc(new Rect(0, 0, r.width, 80f), pawn);
            //Widgets.EndGroup();


            Text.Anchor = anchor;
            Text.Font = font;



            //var r1 = new Rect(r.x, r.y, r.width, 150f);
            //Widgets.BeginGroup(r1);
            //RenderSchedule(new Rect(0,0, r.width, 150f));
            //Widgets.EndGroup();


            //var r2 = new Rect(r1.x, r1.y + 150f, r.width, 150f);
            //Widgets.BeginGroup(r2);
            //RenderWork(new Rect(0, 0, r.width, 150f));
            //Widgets.EndGroup();

            //var r3 = new Rect(r2.x, r2.y + 150f, r.width, 150f);
            //Widgets.BeginGroup(r3);
            //RenderAssign(new Rect(0,0,r.width, 150f));
            //Widgets.EndGroup();

            

        }

        private static void RenderMisc(Rect rect, Pawn pawn)
        {
            var r = default(Rect);
            var text = "";
            var size = default(Vector2);

            /* can do surgery label */
            text = "Can Do Surgery:";
            size = Text.CalcSize(text);
            r = new Rect(0, 0, size.x + 50f, size.y);
            var canDoSurgery = Char_Manager.CanDoSurgery(pawn);
            Widgets.CheckboxLabeled(r, text, ref canDoSurgery);
            Char_Manager.CanDoSurgery(pawn, canDoSurgery);

            /* can preach */
            text = "Can Preach:";
            size = Text.CalcSize(text);
            r = new Rect(size.x + 200f, 0, size.x + 50f, size.y);
            var canDoPreach = Char_Manager.CanDoPreach(pawn);
            Widgets.CheckboxLabeled(r, text, ref canDoPreach);
            Char_Manager.CanDoPreach(pawn, canDoPreach);

            /* weapon memory */
            text = Manager.GetWeaponName(pawn);
            text = string.IsNullOrEmpty(text) ? string.Empty : text;
            size = Text.CalcSize(text);
            r = new Rect(r.x + r.width + 50f, 0, size.x, size.y);
            Widgets.Label(r, text);

        }

        //private static void RenderSchedule(Rect r)
        //{
        //    ScheduleWindow.DoWindowContents(r);
        //}

        //private static void RenderAssign(Rect r)
        //{
        //    AssignWindow.DoWindowContents(r);
        //}

        //private static void RenderWork(Rect r)
        //{
        //    WorkWindow.DoWindowContents(r);
        //}

    }
}
