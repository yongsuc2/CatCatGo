using System;
using System.Collections.Generic;

namespace CatCatGo.Shared.Responses
{
    [Serializable]
    public class ResourceBalanceResponse
    {
        public Dictionary<string, double> Balances;
    }
}
