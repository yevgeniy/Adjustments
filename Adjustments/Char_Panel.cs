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
        static MainTabWindow_Work WorkWindow;
        static MainTabWindow_Assign AssignWindow;
        static MainTabWindow_Schedule ScheduleWindow;
        public static void Reset()
        {
            WorkWindow = new MainTabWindow_Work
            {
                def = DefDatabase<MainButtonDef>.AllDefs.First(v => v.defName == "Work")
            };
            WorkWindow.PostOpen();


            AssignWindow = new MainTabWindow_Assign
            {
                def = DefDatabase<MainButtonDef>.AllDefs.First(v => v.defName == "Assign")
            };
            AssignWindow.PostOpen();

            ScheduleWindow = new MainTabWindow_Schedule
            {
                def = DefDatabase<MainButtonDef>.AllDefs.First(v => v.defName == "Schedule")
            };
            ScheduleWindow.PostOpen();


        }
        private static Pawn SelectedPawn = null;
        public static void Fill(Rect inRect, Pawn pawn)
        {
            if (SelectedPawn==null || !Find.Selector.SingleSelectedThing.Equals(SelectedPawn))
            {
                Reset();
                SelectedPawn = Find.Selector.SingleSelectedThing as Pawn;
            }
            var r = inRect.ContractedBy(7f);

            var r1 = new Rect(r.x, r.y, r.width, 150f);
            Widgets.BeginGroup(r1);
            RenderSchedule(new Rect(0,0, r.width, 150f));
            Widgets.EndGroup();


            var r2 = new Rect(r1.x, r1.y + 150f, r.width, 150f);
            Widgets.BeginGroup(r2);
            RenderWork(new Rect(0, 0, r.width, 150f));
            Widgets.EndGroup();

            var r3 = new Rect(r2.x, r2.y + 150f, r.width, 150f);
            Widgets.BeginGroup(r3);
            RenderAssign(new Rect(0,0,r.width, 150f));
            Widgets.EndGroup();

            var r4 = new Rect(r3.x, r3.y + 150f, r.width, 80f);
            Widgets.BeginGroup(r4);
            RenderMisc(new Rect(0, 0, r.width, 80f), pawn);
            Widgets.EndGroup();

        }

        private static void RenderMisc(Rect rect, Pawn pawn)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;

            /* can do surgery label */
            var text = "Can Do Surgery:";
            var size = Text.CalcSize(text);
            var r1 = new Rect(0, 0, size.x + 50f, size.y);
            var canDoSurgery = Char_Manager.CanDoSurgery(pawn);
            Widgets.CheckboxLabeled(r1, text, ref canDoSurgery);
            Char_Manager.CanDoSurgery(pawn, canDoSurgery);

            /* can preach */
            var text2 = "Can Preach:";
            var size2 = Text.CalcSize(text2);
            var r2 = new Rect(size.x + 200f, 0, size2.x + 50f, size2.y);
            var canDoPreach = Char_Manager.CanDoPreach(pawn);
            Widgets.CheckboxLabeled(r2, text2, ref canDoPreach);
            Char_Manager.CanDoPreach(pawn, canDoPreach);

            /* weapon memory */
            var text3 = Manager.GetWeaponName(pawn);
            text3 = string.IsNullOrEmpty(text3) ? string.Empty : text3;
            var size3 = Text.CalcSize(text3);
            var r3 = new Rect(r2.x + r2.width + 50f, 0, size3.x, size3.y);
            Widgets.Label(r3, text3);

            Text.Anchor = anchor;
            Text.Font = font;
        }

        private static void RenderSchedule(Rect r)
        {
            ScheduleWindow.DoWindowContents(r);
        }

        private static void RenderAssign(Rect r)
        {
            AssignWindow.DoWindowContents(r);
        }

        private static void RenderWork(Rect r)
        {
            WorkWindow.DoWindowContents(r);
        }

    }
}
