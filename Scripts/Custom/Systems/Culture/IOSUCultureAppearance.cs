namespace Server.Custom.Systems.Creation.Cultures
{
    public interface IOSUCultureAppearance
    {
        int[] AllowedSkinHues { get; }
        int[] AllowedHairHues { get; }

        int[] HairGumpIdsFemale { get; }
        int[] HairGumpIdsMale { get; }

        int[] HairItemIdsFemale { get; }
        int[] HairItemIdsMale { get; }

       // opcional (se quiser barbas por cultura)
        int[] BeardGumpIds { get; }
        int[] BeardItemIds { get; }
    }
}
