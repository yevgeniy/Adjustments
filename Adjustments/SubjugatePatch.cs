using HarmonyLib;
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
    //public void SetGuestStatus(Faction newHost, GuestStatus guestStatus = 0)
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    public class PawnGuestTracker_SetGuestStatus
    {
        [HarmonyPrefix]
        public static bool Patch(Faction newHost, GuestStatus guestStatus, Pawn_GuestTracker __instance)
        {
            if (!__instance.IsPrisoner && guestStatus == GuestStatus.Prisoner)
            {
                var pawn = GetPawn(__instance);
                if (pawn.gender==Gender.Female)
                {
                    var comp = pawn.GetComp<SubjugateComp>();
                    if (comp!=null)
                    {
                        comp.ActivateSubjugation();
                    }
                }

            }

            return true;
        }

        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            Type type = typeof(Pawn_GuestTracker);

            // Get the private field info
            FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

            return (Pawn)fieldInfo.GetValue(instance);

        }
    }
}
