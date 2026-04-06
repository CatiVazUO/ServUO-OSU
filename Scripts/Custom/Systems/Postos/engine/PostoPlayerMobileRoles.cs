using System;
using Server;
using Server.Custom.Reinos;

namespace Server.Mobiles
{
    public partial class PlayerMobile
    {
        public static string NormalizeOSUCityId(string cityId)
        {
            if (String.IsNullOrWhiteSpace(cityId))
                return String.Empty;

            string lower = cityId.Trim().ToLowerInvariant();

            switch (lower)
            {
                case "aurora":
                    return "Aurora";
                case "xeta":
                case "xetá":
                    return "Xetá";
                case "lurone":
                    return "Lurone";
                case "willran":
                    return "Willran";
                default:
                    return cityId.Trim();
            }
        }

        public bool IsOSUAmbassadorFor(string cityId)
        {
            return ReinoEmploymentSystem.IsRoleAmbassadorFor(this, cityId);
        }

        public bool IsOSUDispatcherFor(string cityId)
        {
            return ReinoEmploymentSystem.IsRoleDispatcherFor(this, cityId);
        }
    }
}
