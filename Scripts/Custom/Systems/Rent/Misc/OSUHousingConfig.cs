using Server;

namespace Server.Custom.Systems.Rent
{
    public static class OSUHousingConfig
    {
        public static bool EnableCommercialProperties = true;
        public static bool EnableTombs = true;
    }

    public enum OSUPropertyType
    {
        House,
        Commercial,
        Tomb
    }
}
