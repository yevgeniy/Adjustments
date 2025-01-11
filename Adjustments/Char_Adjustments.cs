using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    public class Char_Adjustments
    {
        public static bool IsShowMore;
        public static void EvalPressed()
        {

            if (Find.Selector.SingleSelectedThing != null && Find.Selector.SingleSelectedThing is Pawn pawn)
            {
                MainTabWindow_Inspect tabWindow = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
                if (tabWindow == null)
                    return;

                if (tabWindow.CurTabs == null)
                {
                    return;
                }
                var charTab = tabWindow.CurTabs.FirstOrDefault(v => v is ITab_Pawn_Character);
                if (charTab == null)
                {
                    return;
                }
                if (tabWindow.OpenTabType == typeof(ITab_Pawn_Character))
                {
                    ShowMore();
                }
                else
                {
                    ShowChar();
                }
            }
        }

        private static void ShowChar()
        {
            InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Character));
        }

        private static void ShowMore()
        {
            IsShowMore = !IsShowMore;
            //if (IsShowMore)
            //    Char_Panel.Reset();
        }
    }
    
}
