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
