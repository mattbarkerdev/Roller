using System;
using System.Collections.Generic;
using System.Text;

namespace Roller.Classes
{
    public interface IRollable
    {
      //  double[] CalculateDemProbabilities(Loadout setup);
        DamageOutput RollDemDice(Loadout setup);
    }
}
