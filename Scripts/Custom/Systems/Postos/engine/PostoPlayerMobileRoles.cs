using System;
using Server;

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
            if (!OSUAmbassador)
                return false;

            return String.Equals(
                NormalizeOSUCityId(OSUCitizenCityId),
                NormalizeOSUCityId(cityId),
                StringComparison.OrdinalIgnoreCase);
        }

        public bool IsOSUDispatcherFor(string cityId)
        {
            if (!OSUDispatcher)
                return false;

            return String.Equals(
                NormalizeOSUCityId(OSUCitizenCityId),
                NormalizeOSUCityId(cityId),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
