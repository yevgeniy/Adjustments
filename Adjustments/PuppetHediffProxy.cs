using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    public class PuppetHediffProxy
    {
        public Hediff Hediff;

        static Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        static Type Hediff_PuppetType= Assemblies.SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(v => v.Name == "Hediff_Puppet");
        static FieldInfo MasterFieldInfo = Hediff_PuppetType.GetField("master", BindingFlags.Public | BindingFlags.Instance);
        public PuppetHediffProxy(Hediff h)
        {
            Hediff = h;
        }

        public Pawn Master
        {
            get
            {
                return MasterFieldInfo.GetValue(Hediff) as Pawn;
            }
        }

    }
}
