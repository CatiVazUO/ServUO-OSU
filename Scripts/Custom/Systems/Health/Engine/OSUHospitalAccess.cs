
using System;
using Server.Mobiles;

namespace Server.Custom.Reinos
{
    public static class OSUHospitalAccess
    {
        public static bool CanAccessHospital(PlayerMobile pm, int cityId, string constructionKey)
        {
            if (pm == null || pm.Deleted)
                return false;

            if (ReinoAccessHelper.HasGovernmentAccess(pm, cityId))
                return true;

            ReinoCargoEntry role = ReinoEmploymentSystem.GetOccupiedRole(pm, cityId);
            if (role == null || !role.IsOccupied)
                return false;

            if (String.IsNullOrWhiteSpace(role.LinkedConstructionKey))
                return false;

            return String.Equals(role.LinkedConstructionKey, constructionKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
