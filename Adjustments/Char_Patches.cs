using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

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
            if (Find.MainTabsRoot?.OpenTab?.TabWindow==null)
            {
                return;
            }

            MainTabWindow_Inspect tabWindow = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
            if (tabWindow == null)
            {
                return;
            }

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
            MainTabWindow_Inspect tabWindow = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
            if (tabWindow == null)
                return true;

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


    [HarmonyPatch]
    public class patch_pawn_expose_data
    {

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {

            yield return typeof(Pawn).GetMethod("ExposeData", BindingFlags.Public | BindingFlags.Instance);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {

            var newinstructions = new List<CodeInstruction>();
            foreach(var i in instructions)
            {
                if (i.ToString().Contains("ret"))
                {
                    newinstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newinstructions.Add(CodeInstruction.Call(typeof(Char_Manager), nameof(Char_Manager.ExposeDataSurgeryAndPreach)));
                }
                newinstructions.Add(i);
            }

            return newinstructions;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_Scanner), "HasJobOnThing")]
    class can_do_preaching
    {
        [HarmonyPostfix]
        public static void HasJobOnThingPatch(WorkGiver_Scanner __instance, ref bool __result, Pawn pawn, Thing t, bool forced = false)
        {
            if (__instance is WorkGiver_Warden_Enslave
                    || __instance is WorkGiver_Warden_Chat
                    || __instance is WorkGiver_Warden_Convert)
            {
                if (__result)
                {
                    var canDo = Char_Manager.CanDoPreach(pawn);
                    if (!canDo)
                    {
                        __result = false;
                        JobFailReason.Is("Not allowed to preach", null);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bill), "PawnAllowedToStartAnew")]
    class can_do_surgery
    {
        [HarmonyPrefix]
        public static bool PawnAllowedToStartAnewPatch(Bill __instance, ref bool __result, Pawn p)
        {
            if (__instance is Bill_Medical)
            {
                JobFailReason.Is("Not allowed to do surgery", __instance.Label);
                __result = Char_Manager.CanDoSurgery(p);
                return false;
            }

            return true;
        }
    }
}
