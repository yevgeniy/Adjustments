using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace Adjustments
{
    internal class Driver_ReloadInStorage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
