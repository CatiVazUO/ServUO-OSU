using System;

namespace OSU.Cemeteries
{
    public static class CemeteryHelpers
    {
        public static Func<int> CurrentShardYearProvider = delegate
        {
            return Server.Custom.Systems.WorldTime.OSUWorldTime.WorldNow.Year;
        };
    }
}
