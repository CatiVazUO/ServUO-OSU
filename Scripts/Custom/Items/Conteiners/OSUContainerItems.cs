using Server;
using Server.Items;

namespace Server.Items
{
    public class OSUBag : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 10; } }
        public override int OSUDefaultMaxWeight { get { return 60; } }
        public override string OSUContainerName { get { return "bolsa"; } }

        [Constructable]
        public OSUBag()
            : base(0xE76, OSUContainerResource.RegularLeather, OSUContainerWearKind.Leather)
        {
            Name = "bolsa";
            Weight = 2.0;
        }

        public OSUBag(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUPouch : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 6; } }
        public override int OSUDefaultMaxWeight { get { return 25; } }
        public override string OSUContainerName { get { return "algibeira"; } }

        [Constructable]
        public OSUPouch()
            : base(0xE79, OSUContainerResource.RegularLeather, OSUContainerWearKind.Leather)
        {
            Name = "algibeira";
            Weight = 1.0;
        }

        public OSUPouch(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUBackpack : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 30; } }
        public override int OSUDefaultMaxWeight { get { return 120; } }
        public override string OSUContainerName { get { return "mochila"; } }

        [Constructable]
        public OSUBackpack()
            : base(0xE75, OSUContainerResource.RegularLeather, OSUContainerWearKind.Leather)
        {
            Name = "mochila";
            Weight = 3.0;
        }

        public OSUBackpack(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSmallBagBall : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 8; } }
        public override int OSUDefaultMaxWeight { get { return 30; } }
        public override string OSUContainerName { get { return "sacola pequena redonda"; } }

        [Constructable]
        public OSUSmallBagBall()
            : base(0x2256, OSUContainerResource.None, OSUContainerWearKind.Cloth)
        {
            Name = "sacola pequena redonda";
            Weight = 1.0;
        }

        public OSUSmallBagBall(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSULargeBagBall : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 18; } }
        public override int OSUDefaultMaxWeight { get { return 80; } }
        public override string OSUContainerName { get { return "sacola grande redonda"; } }

        [Constructable]
        public OSULargeBagBall()
            : base(0x2257, OSUContainerResource.None, OSUContainerWearKind.Cloth)
        {
            Name = "sacola grande redonda";
            Weight = 3.0;
        }

        public OSULargeBagBall(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 20; } }
        public override int OSUDefaultMaxWeight { get { return 40; } }
        public override string OSUContainerName { get { return "cesta"; } }

        [Constructable]
        public OSUBasket()
            : base(0x990, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta";
            Weight = 1.0;
        }

        public OSUBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUPicnicBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 25; } }
        public override int OSUDefaultMaxWeight { get { return 50; } }
        public override string OSUContainerName { get { return "cesta de piquenique"; } }

        [Constructable]
        public OSUPicnicBasket()
            : base(0xE7A, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta de piquenique";
            Weight = 2.0;
        }

        public OSUPicnicBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUBasketCraftable : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 20; } }
        public override int OSUDefaultMaxWeight { get { return 45; } }
        public override string OSUContainerName { get { return "cesta artesanal"; } }

        [Constructable]
        public OSUBasketCraftable()
            : base(9431, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta artesanal";
            Weight = 1.0;
        }

        public OSUBasketCraftable(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSURoundBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 20; } }
        public override int OSUDefaultMaxWeight { get { return 40; } }
        public override string OSUContainerName { get { return "cesta redonda"; } }

        [Constructable]
        public OSURoundBasket()
            : base(0x990, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta redonda";
            Weight = 1.0;
        }

        public OSURoundBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSURoundBasketHandles : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 20; } }
        public override int OSUDefaultMaxWeight { get { return 45; } }
        public override string OSUContainerName { get { return "cesta redonda com alças"; } }

        [Constructable]
        public OSURoundBasketHandles()
            : base(0x9AC, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta redonda com alças";
            Weight = 1.0;
        }

        public OSURoundBasketHandles(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSmallBushel : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 18; } }
        public override int OSUDefaultMaxWeight { get { return 45; } }
        public override string OSUContainerName { get { return "cesto pequeno"; } }

        [Constructable]
        public OSUSmallBushel()
            : base(0x09B1, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesto pequeno";
            Weight = 1.0;
        }

        public OSUSmallBushel(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSmallRoundBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 12; } }
        public override int OSUDefaultMaxWeight { get { return 30; } }
        public override string OSUContainerName { get { return "cesta redonda pequena"; } }

        [Constructable]
        public OSUSmallRoundBasket()
            : base(0x24DD, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta redonda pequena";
            Weight = 1.0;
        }

        public OSUSmallRoundBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSmallSquareBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 12; } }
        public override int OSUDefaultMaxWeight { get { return 30; } }
        public override string OSUContainerName { get { return "cesta quadrada pequena"; } }

        [Constructable]
        public OSUSmallSquareBasket()
            : base(0x24D9, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta quadrada pequena";
            Weight = 1.0;
        }

        public OSUSmallSquareBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSquareBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 20; } }
        public override int OSUDefaultMaxWeight { get { return 40; } }
        public override string OSUContainerName { get { return "cesta quadrada"; } }

        [Constructable]
        public OSUSquareBasket()
            : base(0x24D5, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta quadrada";
            Weight = 1.0;
        }

        public OSUSquareBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUTallBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 24; } }
        public override int OSUDefaultMaxWeight { get { return 55; } }
        public override string OSUContainerName { get { return "cesta alta"; } }

        [Constructable]
        public OSUTallBasket()
            : base(0x24DB, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta alta";
            Weight = 1.0;
        }

        public OSUTallBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUTallRoundBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 24; } }
        public override int OSUDefaultMaxWeight { get { return 55; } }
        public override string OSUContainerName { get { return "cesta redonda alta"; } }

        [Constructable]
        public OSUTallRoundBasket()
            : base(0x24D8, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "cesta redonda alta";
            Weight = 1.0;
        }

        public OSUTallRoundBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUWinnowingBasket : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 14; } }
        public override int OSUDefaultMaxWeight { get { return 25; } }
        public override string OSUContainerName { get { return "peneira de vime"; } }

        [Constructable]
        public OSUWinnowingBasket()
            : base(0x1882, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "peneira de vime";
            Weight = 1.0;
        }

        public OSUWinnowingBasket(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUBarrel : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 80; } }
        public override int OSUDefaultMaxWeight { get { return 450; } }
        public override string OSUContainerName { get { return "barril"; } }

        [Constructable]
        public OSUBarrel()
            : base(0xE77, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "barril";
            Weight = 1e+01;
        }

        public OSUBarrel(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUClosedBarrel : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 80; } }
        public override int OSUDefaultMaxWeight { get { return 450; } }
        public override string OSUContainerName { get { return "barril fechado"; } }
        public override int DefaultGumpID { get { return 0x3E; } }

        [Constructable]
        public OSUClosedBarrel()
            : base(0x0FAE, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "barril fechado";
            Weight = 1e+01;
        }

        public OSUClosedBarrel(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUKeg : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 20; } }
        public override int OSUDefaultMaxWeight { get { return 200; } }
        public override string OSUContainerName { get { return "barrilete"; } }

        [Constructable]
        public OSUKeg()
            : base(0xE7F, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "barrilete";
            Weight = 8.0;
        }

        public OSUKeg(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUWoodenBox : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 35; } }
        public override int OSUDefaultMaxWeight { get { return 180; } }
        public override string OSUContainerName { get { return "caixa de madeira"; } }

        [Constructable]
        public OSUWoodenBox()
            : base(0xE7D, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "caixa de madeira";
            Weight = 4.0;
        }

        public OSUWoodenBox(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUMetalBox : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 45; } }
        public override int OSUDefaultMaxWeight { get { return 260; } }
        public override string OSUContainerName { get { return "caixa de metal"; } }

        [Constructable]
        public OSUMetalBox()
            : base(0x9A8, OSUContainerResource.Iron, OSUContainerWearKind.Metal)
        {
            Name = "caixa de metal";
            Weight = 6.0;
        }

        public OSUMetalBox(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUDecorativeBox : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 45; } }
        public override int OSUDefaultMaxWeight { get { return 220; } }
        public override string OSUContainerName { get { return "caixa decorativa"; } }
        public override int DefaultGumpID { get { return 0x43; } }
                public override int DefaultDropSound { get { return 0x42; } }
                public override Rectangle2D Bounds { get { return new Rectangle2D(16, 51, 168, 73); } }

        [Constructable]
        public OSUDecorativeBox()
            : base(0x2DF3, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "caixa decorativa";
            Weight = 3.0;
        }

        public OSUDecorativeBox(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSmallCrate : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 50; } }
        public override int OSUDefaultMaxWeight { get { return 250; } }
        public override string OSUContainerName { get { return "caixote pequeno"; } }

        [Constructable]
        public OSUSmallCrate()
            : base(0x9A9, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "caixote pequeno";
            Weight = 6.0;
        }

        public OSUSmallCrate(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUMediumCrate : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 70; } }
        public override int OSUDefaultMaxWeight { get { return 350; } }
        public override string OSUContainerName { get { return "caixote medio"; } }

        [Constructable]
        public OSUMediumCrate()
            : base(0xE3F, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "caixote medio";
            Weight = 8.0;
        }

        public OSUMediumCrate(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSULargeCrate : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 500; } }
        public override string OSUContainerName { get { return "caixote grande"; } }

        [Constructable]
        public OSULargeCrate()
            : base(0xE3D, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "caixote grande";
            Weight = 1e+01;
        }

        public OSULargeCrate(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUWoodenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 650; } }
        public override string OSUContainerName { get { return "bau de madeira"; } }

        [Constructable]
        public OSUWoodenChest()
            : base(0xE43, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "bau de madeira";
            Weight = 2e+01;
        }

        public OSUWoodenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUPlainWoodenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 650; } }
        public override string OSUContainerName { get { return "bau de madeira simples"; } }

        [Constructable]
        public OSUPlainWoodenChest()
            : base(0x280B, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "bau de madeira simples";
            Weight = 2e+01;
        }

        public OSUPlainWoodenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUOrnateWoodenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 700; } }
        public override string OSUContainerName { get { return "bau de madeira ornamentado"; } }

        [Constructable]
        public OSUOrnateWoodenChest()
            : base(0x280D, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "bau de madeira ornamentado";
            Weight = 2e+01;
        }

        public OSUOrnateWoodenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUGildedWoodenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 720; } }
        public override string OSUContainerName { get { return "bau de madeira dourado"; } }

        [Constructable]
        public OSUGildedWoodenChest()
            : base(0x280F, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "bau de madeira dourado";
            Weight = 2e+01;
        }

        public OSUGildedWoodenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUFinishedWoodenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 700; } }
        public override string OSUContainerName { get { return "bau de madeira polida"; } }

        [Constructable]
        public OSUFinishedWoodenChest()
            : base(0x2813, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "bau de madeira polida";
            Weight = 2e+01;
        }

        public OSUFinishedWoodenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUWoodenFootLocker : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 600; } }
        public override string OSUContainerName { get { return "bau baixo de madeira"; } }
        public override int DefaultGumpID { get { return 0x10B; } }

        [Constructable]
        public OSUWoodenFootLocker()
            : base(0x2811, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "bau baixo de madeira";
            Weight = 2e+01;
        }

        public OSUWoodenFootLocker(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUHeartwoodChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 800; } }
        public override string OSUContainerName { get { return "bau de heartwood"; } }
        public override int DefaultGumpID { get { return 0x10C; } }
                public override int DefaultDropSound { get { return 0x42; } }
                public override Rectangle2D Bounds { get { return new Rectangle2D(80, 5, 140, 70); } }

        [Constructable]
        public OSUHeartwoodChest()
            : base(0x2DF1, OSUContainerResource.Heartwood, OSUContainerWearKind.Wood)
        {
            Name = "bau de heartwood";
            Weight = 2e+01;
        }

        public OSUHeartwoodChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUGargishChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 850; } }
        public override string OSUContainerName { get { return "bau gargula"; } }

        [Constructable]
        public OSUGargishChest()
            : base(0x4026, OSUContainerResource.None, OSUContainerWearKind.Stone)
        {
            Name = "bau gargula";
            Weight = 2e+01;
        }

        public OSUGargishChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUGargoyleWoodenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 720; } }
        public override string OSUContainerName { get { return "bau gargula de madeira"; } }
        public override int DefaultGumpID { get { return 0x42; } }

        [Constructable]
        public OSUGargoyleWoodenChest()
            : base(0x4025, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "bau gargula de madeira";
            Weight = 2e+01;
        }

        public OSUGargoyleWoodenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUMetalChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 850; } }
        public override string OSUContainerName { get { return "bau de ferro"; } }

        [Constructable]
        public OSUMetalChest()
            : base(0x9AB, OSUContainerResource.Iron, OSUContainerWearKind.Metal)
        {
            Name = "bau de ferro";
            Weight = 2e+01;
        }

        public OSUMetalChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUMetalGoldenChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 850; } }
        public override string OSUContainerName { get { return "bau metalico dourado"; } }

        [Constructable]
        public OSUMetalGoldenChest()
            : base(0xE41, OSUContainerResource.Gold, OSUContainerWearKind.Metal)
        {
            Name = "bau metalico dourado";
            Weight = 2e+01;
        }

        public OSUMetalGoldenChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUDullCopperChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 950; } }
        public override string OSUContainerName { get { return "bau de dull copper"; } }

        [Constructable]
        public OSUDullCopperChest()
            : base(0x9AB, OSUContainerResource.DullCopper, OSUContainerWearKind.Metal)
        {
            Name = "bau de dull copper";
            Weight = 2e+01;
        }

        public OSUDullCopperChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUValoriteChest : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 125; } }
        public override int OSUDefaultMaxWeight { get { return 1100; } }
        public override string OSUContainerName { get { return "bau de valorite"; } }

        [Constructable]
        public OSUValoriteChest()
            : base(0x9AB, OSUContainerResource.Valorite, OSUContainerWearKind.Metal)
        {
            Name = "bau de valorite";
            Weight = 2e+01;
        }

        public OSUValoriteChest(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUTallCabinet : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 90; } }
        public override int OSUDefaultMaxWeight { get { return 420; } }
        public override string OSUContainerName { get { return "armario alto"; } }

        [Constructable]
        public OSUTallCabinet()
            : base(0x2815, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "armario alto";
            Weight = 2e+01;
        }

        public OSUTallCabinet(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUShortCabinet : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 70; } }
        public override int OSUDefaultMaxWeight { get { return 320; } }
        public override string OSUContainerName { get { return "armario baixo"; } }

        [Constructable]
        public OSUShortCabinet()
            : base(0x2817, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "armario baixo";
            Weight = 2e+01;
        }

        public OSUShortCabinet(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUDrawer : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 50; } }
        public override int OSUDefaultMaxWeight { get { return 220; } }
        public override string OSUContainerName { get { return "gaveteiro"; } }
        public override int DefaultGumpID { get { return 0x51; } }

        [Constructable]
        public OSUDrawer()
            : base(0xA2C, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "gaveteiro";
            Weight = 2e+01;
        }

        public OSUDrawer(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUFancyDrawer : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 60; } }
        public override int OSUDefaultMaxWeight { get { return 260; } }
        public override string OSUContainerName { get { return "gaveteiro ornamentado"; } }
        public override int DefaultGumpID { get { return 0x48; } }

        [Constructable]
        public OSUFancyDrawer()
            : base(0xA30, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "gaveteiro ornamentado";
            Weight = 2e+01;
        }

        public OSUFancyDrawer(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUChestOfDrawers : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 70; } }
        public override int OSUDefaultMaxWeight { get { return 300; } }
        public override string OSUContainerName { get { return "comoda"; } }
        public override int DefaultGumpID { get { return 0x51; } }
                public override int DefaultDropSound { get { return 0x42; } }

        [Constructable]
        public OSUChestOfDrawers()
            : base(0x0A2C, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "comoda";
            Weight = 2e+01;
        }

        public OSUChestOfDrawers(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUFootedChestOfDrawers : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 80; } }
        public override int OSUDefaultMaxWeight { get { return 340; } }
        public override string OSUContainerName { get { return "comoda com pes"; } }
        public override int DefaultGumpID { get { return 0x48; } }
                public override int DefaultDropSound { get { return 0x42; } }

        [Constructable]
        public OSUFootedChestOfDrawers()
            : base(0x0A30, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "comoda com pes";
            Weight = 2e+01;
        }

        public OSUFootedChestOfDrawers(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUChinaCabinet : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 80; } }
        public override int OSUDefaultMaxWeight { get { return 300; } }
        public override string OSUContainerName { get { return "cristaleira"; } }
        public override int DefaultGumpID { get { return 0x4F; } }

        [Constructable]
        public OSUChinaCabinet()
            : base(0xA29F, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "cristaleira";
            Weight = 2e+01;
        }

        public OSUChinaCabinet(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUPieSafe : OSUContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 60; } }
        public override int OSUDefaultMaxWeight { get { return 250; } }
        public override string OSUContainerName { get { return "armario de tortas"; } }
        public override int DefaultGumpID { get { return 0x4F; } }

        [Constructable]
        public OSUPieSafe()
            : base(0xA29B, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "armario de tortas";
            Weight = 2e+01;
        }

        public OSUPieSafe(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSURedArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 90; } }
        public override int OSUDefaultMaxWeight { get { return 320; } }
        public override string OSUContainerName { get { return "guarda-roupas vermelho"; } }

        [Constructable]
        public OSURedArmoire()
            : base(0x2857, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas vermelho";
            Weight = 2e+01;
        }

        public OSURedArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUCherryArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 90; } }
        public override int OSUDefaultMaxWeight { get { return 320; } }
        public override string OSUContainerName { get { return "guarda-roupas de cerejeira"; } }

        [Constructable]
        public OSUCherryArmoire()
            : base(0x285D, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas de cerejeira";
            Weight = 2e+01;
        }

        public OSUCherryArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUMapleArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 90; } }
        public override int OSUDefaultMaxWeight { get { return 320; } }
        public override string OSUContainerName { get { return "guarda-roupas de bordo"; } }

        [Constructable]
        public OSUMapleArmoire()
            : base(0x285B, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas de bordo";
            Weight = 2e+01;
        }

        public OSUMapleArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUElegantArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 380; } }
        public override string OSUContainerName { get { return "guarda-roupas elegante"; } }

        [Constructable]
        public OSUElegantArmoire()
            : base(0x2859, OSUContainerResource.Heartwood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas elegante";
            Weight = 2e+01;
        }

        public OSUElegantArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 80; } }
        public override int OSUDefaultMaxWeight { get { return 300; } }
        public override string OSUContainerName { get { return "guarda-roupas"; } }

        [Constructable]
        public OSUArmoire()
            : base(0xA4D, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas";
            Weight = 2e+01;
        }

        public OSUArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUFancyArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 380; } }
        public override string OSUContainerName { get { return "guarda-roupas ornamentado"; } }

        [Constructable]
        public OSUFancyArmoire()
            : base(0xA4F, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas ornamentado";
            Weight = 2e+01;
        }

        public OSUFancyArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUSimpleElvenArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 90; } }
        public override int OSUDefaultMaxWeight { get { return 330; } }
        public override string OSUContainerName { get { return "guarda-roupas elfico simples"; } }
        public override int DefaultGumpID { get { return 0x4F; } }
                public override int DefaultDropSound { get { return 0x42; } }
                public override Rectangle2D Bounds { get { return new Rectangle2D(30, 30, 90, 150); } }

        [Constructable]
        public OSUSimpleElvenArmoire()
            : base(0x2D05, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas elfico simples";
            Weight = 2e+01;
        }

        public OSUSimpleElvenArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUFancyElvenArmoire : OSUClothingContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 110; } }
        public override int OSUDefaultMaxWeight { get { return 420; } }
        public override string OSUContainerName { get { return "guarda-roupas elfico ornamentado"; } }
        public override int DefaultGumpID { get { return 0x4E; } }
                public override int DefaultDropSound { get { return 0x42; } }
                public override Rectangle2D Bounds { get { return new Rectangle2D(30, 30, 90, 150); } }

        [Constructable]
        public OSUFancyElvenArmoire()
            : base(0x2D07, OSUContainerResource.Heartwood, OSUContainerWearKind.Wood)
        {
            Name = "guarda-roupas elfico ornamentado";
            Weight = 2e+01;
        }

        public OSUFancyElvenArmoire(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUFullBookcase : OSUBookContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 450; } }
        public override string OSUContainerName { get { return "estante de livros cheia"; } }
        public override int DefaultGumpID { get { return 0x4D; } }

        [Constructable]
        public OSUFullBookcase()
            : base(0xA97, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "estante de livros cheia";
            Weight = 2e+01;
        }

        public OSUFullBookcase(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUEmptyBookcase : OSUBookContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 450; } }
        public override string OSUContainerName { get { return "estante de livros vazia"; } }
        public override int DefaultGumpID { get { return 0x4D; } }

        [Constructable]
        public OSUEmptyBookcase()
            : base(0xA9D, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "estante de livros vazia";
            Weight = 2e+01;
        }

        public OSUEmptyBookcase(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUAcademicBookCase : OSUBookContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 450; } }
        public override string OSUContainerName { get { return "estante academica"; } }
        public override int DefaultGumpID { get { return 0x4D; } }

        [Constructable]
        public OSUAcademicBookCase()
            : base(0xA99, OSUContainerResource.OakWood, OSUContainerWearKind.Wood)
        {
            Name = "estante academica";
            Weight = 2e+01;
        }

        public OSUAcademicBookCase(Serial serial)
            : base(serial)
        {
        }
    }

    public class OSUWoodenBookcase : OSUBookContainerBase
    {
        public override int OSUDefaultMaxItems { get { return 100; } }
        public override int OSUDefaultMaxWeight { get { return 450; } }
        public override string OSUContainerName { get { return "estante de madeira"; } }
        public override int DefaultGumpID { get { return 0x4D; } }

        [Constructable]
        public OSUWoodenBookcase()
            : base(0x0A9D, OSUContainerResource.RegularWood, OSUContainerWearKind.Wood)
        {
            Name = "estante de madeira";
            Weight = 2e+01;
        }

        public OSUWoodenBookcase(Serial serial)
            : base(serial)
        {
        }
    }

}