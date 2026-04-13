using Server;
using Server.Commands;
using Server.Items;
using Server.Multis;
using Server.Targeting;
using Server.Custom.Systems.Rent;

namespace Server.Commands
{
    public static class EvictCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("Evict", AccessLevel.GameMaster, OnCommand);
        }

        private static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Selecione a casa de aluguel ou a placa para simular o despejo dos itens.");
            e.Mobile.Target = new EvictTarget();
        }

        private class EvictTarget : Target
        {
            public EvictTarget() : base(18, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                TownHouseSign sign = targeted as TownHouseSign;

                if (sign == null)
                {
                    TownHouse house = targeted as TownHouse;
                    if (house != null)
                        sign = house.ForSaleSign;
                }

                if (sign == null)
                {
                    HouseSign hs = targeted as HouseSign;
                    if (hs != null)
                    {
                        TownHouse house = BaseHouse.FindHouseAt(hs) as TownHouse;
                        if (house != null)
                            sign = house.ForSaleSign;
                    }
                }

                if (sign == null || sign.Deleted || !sign.Owned)
                {
                    from.SendMessage("Isso não é uma casa de aluguel válida para o teste.");
                    return;
                }

                sign.DebugEvictContents();
                from.SendMessage("Despejo simulado. Confira o depositário e a mochila do proprietário.");
            }
        }
    }
}
