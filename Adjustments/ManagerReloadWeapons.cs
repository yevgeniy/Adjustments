using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class ManagerReloadWeapons: MapComponent
    {
        private static HashSet<ThingWithComps> weaponsInStorage=new HashSet<ThingWithComps>();
        private static HashSet<ThingWithComps> changeAmmoRequest=new HashSet<ThingWithComps>();
        

        public static void AddWeapon(ThingWithComps t)
        {
            weaponsInStorage.Add(t);
        }
        public static void RemoveWeapon(ThingWithComps t)
        {
            weaponsInStorage.Remove(t);
        }

        public static IEnumerable<ThingWithComps>  ConsiderWeapons()
        {
            if (weaponsInStorage.Count() == 0)
                return null;

            /*Get weapons on map (hauled and placed) */
            var mapWeapons = weaponsInStorage.Where(v => v.Map == Find.CurrentMap).ToList();
            Log.Message("map weapons: " + mapWeapons.Count);

            if (mapWeapons.Count == 0)
                return null;

            var areaManager = new AreaManager(Find.CurrentMap);
            
            foreach (var wep in mapWeapons)
            {
                /* clean up despawned weapons */
                if (!wep.Spawned)
                {
                    RemoveWeapon(wep);
                    continue;
                }


                /* Removed weapons not in home zone */
                var homeArea = wep.Map.areaManager.Home;
                if (homeArea == null)
                {
                    Log.Error("NO HOME AREA?");
                    RemoveWeapon(wep);
                    continue;
                }
                if (!homeArea.ActiveCells.Any(v => v == wep.Position))
                {
                    RemoveWeapon(wep);
                    continue;
                }

                if (!WeaponUsesAmmo(wep))
                {
                    RemoveWeapon(wep);
                    continue;
                }

                if (!changeAmmoRequest.Contains(wep) && FullAmmo(wep))
                {
                    RemoveWeapon(wep);
                    continue;
                }
                
                /* Remove weapons fully loaded w/ assigned ammunition */

            }

            return weaponsInStorage.Where(v => v.Map == Find.CurrentMap);
        }

        private static bool WeaponUsesAmmo(ThingWithComps wep)
        {
            var methinfo = typeof(ThingWithComps).GetMethod("GetComp");
            var genMethod = methinfo.MakeGenericMethod(Adjustments.CompAmmoUserType);
            var compAmmoUser = genMethod.Invoke(wep, null);

            var r= (bool)Adjustments.HasMagazinePropInfo.GetValue(compAmmoUser);
            Log.Message("USES AMMO: " + r);
            return r;
        }

        private static bool FullAmmo(ThingWithComps wep)
        {
            var methinfo = typeof(ThingWithComps).GetMethod("GetComp");
            var genMethod = methinfo.MakeGenericMethod(Adjustments.CompAmmoUserType);
            var compAmmoUser = genMethod.Invoke(wep, null);

            var props = Adjustments.PropsPropInfo.GetValue(compAmmoUser);
            var magsize = (int)Adjustments.MagazineSizeFieldInfo.GetValue(props);

            var curmagsize = (int)Adjustments.CurMagCountPropInfo.GetValue(compAmmoUser);

            Log.Message("MAG: " + curmagsize + "/" + magsize);

            return magsize == curmagsize;
        }

        static ManagerReloadWeapons()
        {

        }
        public ManagerReloadWeapons(Map map):base(map)
        { 
        
        }

        
    }
}
