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
    internal class Char_Patches
    {
    }

    [HarmonyPatch(typeof(PawnTable))]
    [HarmonyPatch("Columns", MethodType.Getter)]
    public static class exclude_some_rows
    {
        [HarmonyPostfix]
        public static void Postfix(ref List<PawnColumnDef> __result)
        {
            MainTabWindow_Inspect tabWindow = (MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.TabWindow;

            if (tabWindow.CurTabs == null)
            {
                return;
            }

            var charTab = tabWindow.CurTabs.FirstOrDefault(v => v is ITab_Pawn_Character);
            if (charTab == null)
            {
                return;
            }

            if (tabWindow.OpenTabType == typeof(ITab_Pawn_Character)
                    && Find.Selector.SingleSelectedThing != null
                    && Find.Selector.SingleSelectedThing is Pawn pawn)
            {

                var remove = new string[]
                {
                    "Label", "LabelWithIcon","LabelShortWithIcon","Age","Master",
                    "Guest", "Ideo", "Xenotype", 
                    "CopyPasteWorkPriorities","CopyPasteTimetable",
                    "Gender","LifeStage","Pregnant","Bond","Info","MentalState","Faction",
                    "MedicalCare", "HostilityResponse"
                };
                __result.RemoveAll(v => remove.Contains(v.defName));
            }
        }
    }

    /* Ctrl+Right clicking on ground should bring up bring menu */
    [HarmonyPatch(typeof(MainButtonsRoot), "MainButtonsOnGUI")]
    public class MainButtonRoot_MainButtonOnGUI
    {
        public static bool Prefix()
        {

            switch (Event.current.type)
            {
                case EventType.KeyUp:

                    if (Event.current.keyCode == KeyCode.C)
                    {
                        Char_Adjustments.EvalPressed();
                    }
                    break;

            }

            return true;

        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Character), "FillTab")]
    public class show_more_stuf {
        [HarmonyPostfix]
        public static void Postfix(ITab_Pawn_Character __instance)
        {
            if (Current.ProgramState == ProgramState.Playing && Char_Adjustments.IsShowMore)
            {
                var tabRect = (Rect)__instance.GetType().GetProperty("TabRect", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                
                Find.WindowStack.ImmediateWindow(8931795,
                    new Rect(new Vector2(tabRect.width, tabRect.y), new Vector2(tabRect.width*2, tabRect.height)),
                    WindowLayer.GameUI,
                    () =>
                    {
                        if (Find.Selector.SingleSelectedThing is Pawn || Find.Selector.SingleSelectedThing is Corpse) {
                            Char_Panel.Fill(
                                new Rect(new Vector2(), new Vector2(tabRect.width*2, tabRect.height)), 
                                Find.Selector.SingleSelectedThing as Pawn
                            );
                            
                        }
                    });
            }
                
        }
    }

    [HarmonyPatch(typeof(PawnTable), "RecachePawns")]
    public class render_only_selected_pawn
    {
        [HarmonyPrefix]
        public static bool prefixer(PawnTable __instance)
        {
            MainTabWindow_Inspect tabWindow = (MainTabWindow_Inspect)Find.MainTabsRoot.OpenTab.TabWindow;

            if (tabWindow.CurTabs == null)
            {
                return true;
            }
            
            var charTab = tabWindow.CurTabs.FirstOrDefault(v => v is ITab_Pawn_Character);
            if (charTab == null)
            {
                return true;
            }
            
            if (    tabWindow.OpenTabType == typeof(ITab_Pawn_Character) 
                    && Find.Selector.SingleSelectedThing!=null 
                    && Find.Selector.SingleSelectedThing is Pawn pawn)
            {
                
                var f = typeof(PawnTable).GetField("cachedPawns", BindingFlags.NonPublic | BindingFlags.Instance);
                if (f == null)
                    Log.Error("NOT CORRECT FIELD");
                var cachedPawns = (List<Pawn>)f.GetValue(__instance);

                cachedPawns.Clear();
                cachedPawns.Add(pawn);

                return false;
            }

            return true;

        }
    }

}
