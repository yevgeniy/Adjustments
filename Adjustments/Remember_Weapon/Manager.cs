using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.Remember_Weapon
{
    public class Manager
    {
        public static Dictionary<Pawn, string> WeaponMemory = new Dictionary<Pawn, string>();
        public static string GetWeaponName(Pawn pawn)
        {
            return WeaponMemory.ContainsKey(pawn) ? WeaponMemory[pawn] : null;       
        }
        public static void SetWeaponName(Pawn pawn, string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                if (WeaponMemory.ContainsKey(pawn))
                {
                    WeaponMemory.Remove(pawn);
                }
            }
            else
            {
                WeaponMemory[pawn] = name;
            }
        }

        public static void ExposeWeaponData(Pawn pawn)
        {
            string weaponMemory = GetWeaponName(pawn);
            Scribe_Values.Look(ref weaponMemory, "pawn-weap-mem");

            SetWeaponName(pawn, weaponMemory);
        }

    }
}
