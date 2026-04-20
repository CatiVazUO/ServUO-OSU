
using System;

namespace Server.Custom.Systems.Health
{
    public enum OSUInjuryType
    {
        None = 0,
        Winded = 1,
        Bruised = 2,
        MinorCut = 3,
        MinorConcussion = 4,
        Bloodied = 5,
        Exhausted = 6,
        MajorConcussion = 7,
        FracturedLeftArm = 8,
        FracturedRightArm = 9,
        FracturedLeftLeg = 10,
        FracturedRightLeg = 11,
        FracturedRibs = 12,
        FracturedSkull = 13,
        DeepCut = 14,
        InternalBleeding = 15,
        LaceratedTorso = 16,
        BrokenLeftArm = 17,
        BrokenRightArm = 18,
        BrokenLeftLeg = 19,
        BrokenRightLeg = 20,
        BrokenJaw = 21,
        ChestTrauma = 22,
        RupturedSpleen = 23,
        BrokenSkull = 24,
        MassiveBleeding = 25
    }

    public enum OSUDiseaseType
    {
        None = 0,
        Influenza = 1,
        HundredDaysCough = 2,
        Diptheria = 3,
        Dysentery = 4,
        Consumption = 5,
        WesternFever = 6,
        Bile = 7,
        Leprosy = 8,
        LoveDisease = 9
    }

    public enum OSUInjurySeverity
    {
        Light = 0,
        Moderate = 1,
        Severe = 2,
        Critical = 3,
        Deadly = 4
    }

    public enum OSUBodyZone
    {
        Unknown = 0,
        Head,
        Back,
        Arms,
        Legs,
        Torso
    }

    public enum OSUMedicatedBandageType
    {
        None = 0,
        HealingBonus = 1,
        SpeedBonus = 2,
        Antiseptic = 3
    }

    public enum OSUSurgeryToolType
    {
        None = 0,
        Anestesico = 1,
        FacaDisseccao = 2,
        Tesoura = 3,
        AguaEsteril = 4,
        Gazes = 5,
        VelaCauterizadora = 6,
        BrasaCauterizadora = 7,
        Sanguessuga = 8,
        LinhaSutura = 9,
        CuteloCirurgico = 10,
        AdagaSangria = 11,
        TochaCauterizadora = 12,
        AlcoolCirurgico = 13
    }
}
