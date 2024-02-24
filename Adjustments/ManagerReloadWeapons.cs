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
        

        public static void AddWeapon(ThingWithComps t)
        {
            weaponsInStorage.Add(t);
        }
        public static void RemoveWeapon(ThingWithComps t)
        {
            weaponsInStorage.Remove(t);
        }

        public static bool IsThingInConsideration(ThingWithComps thing)
        {
            return weaponsInStorage.Contains(thing);
        }
        public static IEnumerable<ThingWithComps>  ConsiderWeapons()
        {
            if (weaponsInStorage.Count() == 0)
                return null;

            var mapWeapons = weaponsInStorage.Where(v => v.Map == Find.CurrentMap).ToList();

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

                if (FullAmmo(wep))
                {
                    RemoveWeapon(wep);
                    continue;
                }
            }

            return weaponsInStorage.Where(v => v.Map == Find.CurrentMap);
        }

        private static bool WeaponUsesAmmo(ThingWithComps wep)
        {
            var methinfo = typeof(ThingWithComps).GetMethod("GetComp");
            var genMethod = methinfo.MakeGenericMethod(Adjustments.CompAmmoUserType);
            var compAmmoUser = genMethod.Invoke(wep, null);

            var r= (bool)Adjustments.HasMagazinePropInfo.GetValue(compAmmoUser);
            return r;
        }

        private static bool FullAmmo(ThingWithComps wep)
        {
            var i = 0;
            GetRequiredAmmoDef(wep, out i);

            return i == 0;
        }
        /*returns AmmoDef */
        public static object GetRequiredAmmoDef(ThingWithComps gun, out int howMuch)
        {

            var methinfo = typeof(ThingWithComps).GetMethod("GetComp");

            var genMethod = methinfo.MakeGenericMethod(Adjustments.CompAmmoUserType);

            var compAmmoUser = genMethod.Invoke(gun, null);


            var selectedAmmo = Adjustments.SelectedAmmoPropInfo.GetValue(compAmmoUser);
            var currentAmmo = Adjustments.CurrentAmmoPropInfo.GetValue(compAmmoUser);
            var curMag = (int)Adjustments.CurMagCountPropInfo.GetValue(compAmmoUser);


            var props = Adjustments.PropsPropInfo.GetValue(compAmmoUser);
            var magSize = (int)Adjustments.MagazineSizeFieldInfo.GetValue(props);



            if (selectedAmmo.Equals(currentAmmo))
                howMuch = magSize - curMag;
            else
                howMuch = magSize;

            return selectedAmmo;
        }

        static ManagerReloadWeapons()
        {

        }
        public ManagerReloadWeapons(Map map):base(map)
        { 
        
        }

        
    }
}
