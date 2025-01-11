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
    [HarmonyPatch]
    public class ApplyOnPawn_CheckSurgeryFail
    {
        public static void Wire(Harmony harmony)
        {
            var methInfo = typeof(Recipe_Surgery).GetMethod("CheckSurgeryFail", BindingFlags.NonPublic | BindingFlags.Instance);
            harmony.Patch(methInfo, postfix: new HarmonyMethod(typeof(ApplyOnPawn_CheckSurgeryFail), nameof(Postfix)));
        }

        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            var methInfo = typeof(Recipe_Surgery).GetMethod("CheckSurgeryFail", BindingFlags.NonPublic | BindingFlags.Instance);
            yield return methInfo;
        }

        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
        {
            if (!Adjustments_Settings.SurgeryActive)
            {
                return;
            }

            if (__result)
            {
                /* This gets called twice for some reason.  Do not spawn again if something was already spawn for instance of this ingredient */
                var ingOfBodyPart = ingredients.FirstOrDefault(v => v.def.thingCategories.Any(vv => vv.defName.Contains("BodyParts")));
                if (ingOfBodyPart==null || AlreadySpawned(ingOfBodyPart))
                {
                    return;
                }

                SpawnIngredient(ingOfBodyPart, surgeon);
            }
        }

        static HashSet<Thing> AlreadySpawnedThings = new HashSet<Thing>();
        public static bool AlreadySpawned(Thing ingOfBodyPart)
        {
            return AlreadySpawnedThings.Any(v => v == ingOfBodyPart);
        }

        public static void SpawnIngredient(Thing ingOfBodyPart, Pawn surgeon)
        {
            var thing = ThingMaker.MakeThing(ingOfBodyPart.def, ingOfBodyPart.Stuff);
            GenSpawn.Spawn(thing, surgeon.Position, surgeon.Map);

            AlreadySpawnedThings.Add(ingOfBodyPart);
            if (AlreadySpawnedThings.Count()>50)
            {
                AlreadySpawnedThings = AlreadySpawnedThings.Skip(1).ToHashSet<Thing>();
            }
        }
    }
}
