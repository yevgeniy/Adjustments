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
    public static class Rel_Adjustments
    {
        public static bool HasCombatExtended;
        

        public static PropertyInfo SelectedAmmoPropInfo = null;
        
        public static StatDef ReloadSpeed = null;

        static Rel_Adjustments()
        {
            Log.Message("ADJUSTMENTS STARTED.");


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            /* find 'haul urgently' class */
            var compAmmoUserType = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "CompAmmoUser");
            if (compAmmoUserType != null)
            {
                HasCombatExtended = true;
                ReloadSpeed = StatDef.Named("ReloadSpeed");

            }

            Log.Message("HAS CE: " + HasCombatExtended);

            Harmony harmony = new Harmony("nimm.adjustments");

            if (HasCombatExtended)
                Pawn_CarryTracker_TryDropCarriedThing.Wire(harmony);

            ApplyOnPawn_CheckSurgeryFail.Wire(harmony);

            harmony.PatchAll();


            Log.Message("ADJUSTMENTS PATCHED.");

            AdjustDict();
        }

        private static void AdjustDict()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(Brand_Comp) });
            }

            /*replace all fat and hulking female bodytypes to normal*/
            var femaleBodyType = DefDatabase<BodyTypeDef>.AllDefs.First(v => v.defName == "Female");
            Log.Message("FEMALE BODY TYPE: " + femaleBodyType);
            foreach (var i in DefDatabase<BackstoryDef>.AllDefs)
            {
                if (i.bodyTypeFemale != null && (i.bodyTypeFemale.defName == "Fat" || i.bodyTypeFemale.defName == "Hulk"))
                    i.bodyTypeFemale = femaleBodyType;
            }


            var haulUrgent = DefDatabase<WorkTypeDef>.AllDefs.FirstOrDefault(v => v.defName == "HaulingUrgent");
            if (haulUrgent!=null)
            {
                var reloadDef= DefDatabase<WorkGiverDef>.AllDefs.FirstOrDefault(v => v.defName == "ReloadTurrets");
                if (reloadDef!=null)
                    reloadDef.workType = haulUrgent;
            }

        }
    }


}