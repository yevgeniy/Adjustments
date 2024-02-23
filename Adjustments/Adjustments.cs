using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public static class Adjustments
    {
        public static bool HasCombatExtended;
        public static Type CompAmmoUserType;
        public static Type CompProperties_AmmoUserType;
        public static PropertyInfo HasMagazinePropInfo = null;
        public static PropertyInfo CurMagCountPropInfo = null;
        public static PropertyInfo CurrentAmmoPropInfo = null;
        public static PropertyInfo SelectedAmmoPropInfo = null;
        public static PropertyInfo PropsPropInfo = null;
        public static FieldInfo MagazineSizeFieldInfo = null;

        static Adjustments()
        {
            Log.Message("ADJUSTMENTS STARTED.");

            Harmony.DEBUG = true;  // Enable Harmony Debug
            Harmony harmony = new Harmony("nimm.admustments");

            Pawn_CarryTracker_TryDropCarriedThing.Wire(harmony);

            harmony.PatchAll();

            Log.Message("ADJUSTMENTS PATCHED.");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            /* find 'haul urgently' class */
            CompAmmoUserType = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "CompAmmoUser");
            if (CompAmmoUserType != null)
            {
                HasCombatExtended = true;
                HasMagazinePropInfo = CompAmmoUserType.GetProperty("HasMagazine");
                CurMagCountPropInfo = CompAmmoUserType.GetProperty("CurMagCount");
                CurrentAmmoPropInfo = CompAmmoUserType.GetProperty("CurrentAmmo");
                SelectedAmmoPropInfo = CompAmmoUserType.GetProperty("SelectedAmmo");
                PropsPropInfo = CompAmmoUserType.GetProperty("Props");

                CompProperties_AmmoUserType = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "CompProperties_AmmoUser");
                MagazineSizeFieldInfo = CompProperties_AmmoUserType.GetField("magazineSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            
            Log.Message("HAS CE: " + HasCombatExtended);
                
        }
    }


}