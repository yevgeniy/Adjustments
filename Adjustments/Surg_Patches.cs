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

    /* body part items on failed surgeries are not destroyed */
    public class ApplyOnPawn_CheckSurgeryFail
    {
        public static void Wire(Harmony harmony)
        {
            var methInfo = typeof(Recipe_Surgery).GetMethod("CheckSurgeryFail", BindingFlags.NonPublic | BindingFlags.Instance);
            harmony.Patch(methInfo, postfix: new HarmonyMethod(typeof(ApplyOnPawn_CheckSurgeryFail), nameof(Postfix)));
        }

        public static void Postfix(ref bool __result, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
        {
            if (__result)
            {
                var ingBodyPart = ingredients.FirstOrDefault(v => v.def.thingCategories.Any(vv => vv.defName.Contains("BodyParts")));
                if (ingBodyPart != null)
                {
                    var thing = ThingMaker.MakeThing(ingBodyPart.def, ingBodyPart.Stuff);
                    GenSpawn.Spawn(thing, surgeon.Position, surgeon.Map);
                }
            }
        }
    }
}
