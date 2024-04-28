using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    public class Char_Manager
    {
        public static HashSet<Pawn> DoSurgery = new HashSet<Pawn>();
        public static HashSet<Pawn> DoPreach = new HashSet<Pawn>();


        public static void ExposeDataSurgeryAndPreach(Pawn pawn)
        {
            var canDoSurgery = CanDoSurgery(pawn);
            var canDoPreach = CanDoPreach(pawn);

            Scribe_Values.Look(ref canDoSurgery, "char-man-surg");
            Scribe_Values.Look(ref canDoPreach, "char-man-prech");

            CanDoSurgery(pawn, canDoSurgery);
            CanDoPreach(pawn, canDoPreach);
        }

        public static bool CanDoSurgery(Pawn subject, bool? val=null)
        {
            if (val==null)
                return DoSurgery.Contains(subject);

            if (val.Value==true)
            {
                if (!DoSurgery.Contains(subject))
                    DoSurgery.Add(subject);
            }
            else if (val.Value==false)
            {
                if (DoSurgery.Contains(subject))
                    DoSurgery.Remove(subject);
            }

            return true;
        }
        public static bool CanDoPreach(Pawn subject, bool? val=null)
        {
            if (val == null)
                return DoPreach.Contains(subject);

            if (val.Value == true)
            {
                if (!DoPreach.Contains(subject))
                    DoPreach.Add(subject);
            }
            else if (val.Value == false)
            {
                if (DoPreach.Contains(subject))
                    DoPreach.Remove(subject);
            }

            return true;
        }

    }
}
