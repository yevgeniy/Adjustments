using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [HarmonyPatch]
    public class pick_up_and_haul_considers_frames_and_blueprints
    {
        //[HarmonyTargetMethod]
        //static MethodBase get_method()
        //{
        //    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //    var type = assemblies.SelectMany(assembly => assembly.GetTypes())
        //        .FirstOrDefault(v => v.Name == "WorkGiver_HaulToInventory");

        //    if (type == null)
        //        return null;

        //    MethodBase meth = type.GetMethod("HasJobOnThing", BindingFlags.Public | BindingFlags.Instance);
        //    return meth;
        //}

        //[HarmonyTranspiler]
        //static IEnumerable<CodeInstruction> transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        //{
        //    var code = new List<CodeInstruction>();
        //    var inentry = false;
        //    foreach (var instruction in instructions)
        //    {
        //        if (inentry)
        //        {
        //            if (instruction.ToString().Contains("ret"))
        //            {
        //                code.Add(new CodeInstruction(OpCodes.Ldarg_1));
        //                code.Add(new CodeInstruction(OpCodes.Ldarg_2));
        //                code.Add(CodeInstruction.Call(typeof(Haul_Adjustments), nameof(Haul_Adjustments.HasBetterPlace)));
        //                code.Add(instruction);
        //                inentry = false;
        //            }
        //        }
        //        else
        //        {
        //            code.Add(instruction);

        //            if (instruction.ToString().Contains("brfalse.s Label2"))
        //            {
        //                inentry = true;
        //            }
                    
        //        }
                
        //    }

        //    foreach (var i in code)
        //    {
        //        Log.Message(i);
        //        yield return i;

        //    }
        //}
    }
}
