using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.XR;
using Verse;

namespace Adjustments.Gamespeed_Adjustments
{

    [HarmonyPatch(typeof(ListerHaulables), "ListerHaulablesTick")]
    public static class HaulableListerTickStart
    {
        [HarmonyPrefix]
        public static bool prefix_adjust(ListerHaulables __instance)
        {
            var system = __instance.GetThrottleSystem();
            system.InMethod = true;
            system.TimesCalled = 0;

            return true;
        }
    }

    [HarmonyPatch(typeof(ListerHaulables), "ListerHaulablesTick")]
    public static class HaulableListerTickEnd
    {
        [HarmonyPostfix]
        public static void postfix_adjust(ListerHaulables __instance)
        {
            var system = __instance.GetThrottleSystem();
            system.InMethod = false;

            system.LastPosition += HaulableExtendables.SetSize;

            if (system.LastPosition >= system.TimesCalled)
            {
                system.LastPosition = 0;
            }

            //Log.Message(string.Join(", ", system.Handled));
            //system.Handled.Clear();
        }
    }

    [HarmonyPatch]
    public static class ConsiderOnlyXItemsAtATime
    {

        [HarmonyTargetMethod]
        public static MethodInfo getMethod()
        {
            var methinfo= typeof(ListerHaulables).GetMethod("ShouldBeHaulable", BindingFlags.NonPublic | BindingFlags.Instance);
            return methinfo;
        }

        [HarmonyPrefix]
        public static bool prefix_adjust(ListerHaulables __instance, ref bool __result, Thing t)
        {
            var system=__instance.GetThrottleSystem();

            if (!system.InMethod)
                return true;

            system.TimesCalled++;
            //Log.Message(__instance + " " + system.TimesCalled + " " + system.LastPosition);
            if (system.LastPosition < system.TimesCalled 
                && system.TimesCalled <= system.LastPosition + HaulableExtendables.SetSize)
            {
                //system.Handled.Add(t.def.defName);
                return true;
            }

            __result = false;
            return false;
        }
    }

    public class ListerThrottleSystem
    {
        public bool InMethod;
        public int TimesCalled;
        public int LastPosition;
        //public List<string> Handled;
    }
    public static class HaulableExtendables
    {
        public static int SetSize = 5;

        public static Dictionary<ListerHaulables, ListerThrottleSystem> System = new Dictionary<ListerHaulables, ListerThrottleSystem>();
        public static ListerThrottleSystem GetThrottleSystem(this ListerHaulables lister)
        {
            if (System.TryGetValue(lister, out var system))
            {
                return system;
            }

            system = new ListerThrottleSystem()
            {
                InMethod = false,
                TimesCalled=0,
                LastPosition=0,
                //Handled=new List<string>()
            };

            System.Add(lister, system);

            return system;
        }
    }
}
