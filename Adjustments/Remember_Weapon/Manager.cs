using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.Remember_Weapon
{
    public class Manager
    {
        public static void SetWeapon(Pawn pawn, string name)
        {

        }

        public static void ExposeWeaponData(Pawn pawn)
        {
            Log.Message("SAVE WEAPON FOR PAWN: " + pawn);
        }
    }
}
