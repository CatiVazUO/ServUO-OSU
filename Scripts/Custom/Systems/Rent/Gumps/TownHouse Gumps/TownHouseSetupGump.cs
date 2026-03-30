using System;
using Server;
using Server.Targeting;
using System.Collections;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Rent
{
    public class TownHouseSetupGump : GumpPlusLight
    {
        public static Rectangle2D FixRect(Rectangle2D rect)
        {
            Point3D pointOne = Point3D.Zero;
            Point3D pointTwo = Point3D.Zero;

            if (rect.Start.X < rect.End.X)
            {
                pointOne.X = rect.Start.X;
                pointTwo.X = rect.End.X;
            }
            else
            {
                pointOne.X = rect.End.X;
                pointTwo.X = rect.Start.X;
            }

            if (rect.Start.Y < rect.End.Y)
            {
                pointOne.Y = rect.Start.Y;
                pointTwo.Y = rect.End.Y;
            }
            else
            {
                pointOne.Y = rect.End.Y;
                pointTwo.Y = rect.Start.Y;
            }

            return new Rectangle2D(pointOne, pointTwo);
        }

        public enum TargetType { SignLoc, MinZ, MaxZ, BlockOne, BlockTwo }

        private readonly TownHouseSign c_Sign;

        private bool RentOnly
        {
            get
            {
                return c_Sign != null &&
                       (c_Sign.PropertyType == OSUPropertyType.Commercial || c_Sign.PropertyType == OSUPropertyType.Tomb);
            }
        }

        public TownHouseSetupGump(Mobile m, TownHouseSign sign) : base(m, 0, 0)
        {
            m.CloseGump(typeof(TownHouseSetupGump));
            c_Sign = sign;

            while (RentOnly && c_Sign.PriceType == "Sale")
                c_Sign.NextPriceType();

            if (c_Sign != null && c_Sign.Map != null && c_Sign.Map != Map.Internal && c_Sign.RootParent == null)
            {
                m.Hidden = true;
                m.MoveToWorld(new Point3D(c_Sign.Location.X, c_Sign.Location.Y, c_Sign.Location.Z + 5), c_Sign.Map);
            }
        }

        protected override void BuildGump()
        {
            if (c_Sign == null)
                return;

            AddImageTiled(275, 131, 450, 640, 398);
            AddImageTiled(716, 138, 25, 607, 369);
            AddImageTiled(254, 140, 26, 618, 370);
            AddImageTiled(269, 115, 450, 25, 371);
            AddImageTiled(281, 764, 443, 30, 372);
            AddImage(246, 107, 415);
            AddImage(679, 105, 414);
            AddImage(249, 730, 412);
            AddImage(680, 728, 413);
            AddLabel(443, 147, 0, c_Sign.Owned ? "Editar Propriedade" : "Criar Propriedade");
            AddImage(293, 163, 443);

            AddButton(601, 199, 559, 559, "Nome", new GumpCallback(Name));
            AddTextField(415, 199, 186, 20, 0, 0, "Name", c_Sign.Name);
            AddLabel(305, 201, 0, GetNameLabel());

            AddLabel(306, 256, 0, "Adicionar Area");
            AddButton(411, 256, 535, 535, "Add Area", new GumpCallback(AddBlock));
            AddImage(289, 224, 443);
            AddLabel(555, 256, 0, "Limpar Area");
            AddButton(645, 256, 535, 535, "ClearAll", new GumpCallback(ClearAll));

            AddLabel(307, 317, 0, "Altura Térreo: " + (c_Sign.MinZ == short.MinValue ? "" : c_Sign.MinZ.ToString()));
            AddButton(414, 318, 535, 535, "Base Floor", new GumpCallback(MinZSelect));
            AddLabel(490, 313, 0, c_Sign.PropertyType == OSUPropertyType.Tomb ? "Sem Primeiro Andar" : "Altura Primeiro Andar: " + (c_Sign.MaxZ == short.MaxValue ? "" : (c_Sign.MaxZ - 19).ToString()));

            if (c_Sign.PropertyType != OSUPropertyType.Tomb)
                AddButton(644, 313, 535, 535, "Top Floor", new GumpCallback(MaxZSelect));

            AddImage(289, 284, 443);
            AddImage(289, 339, 443);

            AddLabel(304, 378, 0, c_Sign.PropertyType == OSUPropertyType.Tomb ? "Lugar da Lápide" : "Lugar da Placa");
            AddButton(464, 375, 535, 535, "Sign Loc", new GumpCallback(SignLocSelect));
            AddLabel(524, 378, 0, "Sul");
            AddButton(553, 368, c_Sign.Flip ? 440 : 442, c_Sign.Flip ? 440 : 442, "Facing South", new GumpCallback(FacingSouth));
            AddLabel(606, 378, 0, "Leste");
            AddButton(650, 368, !c_Sign.Flip ? 440 : 442, !c_Sign.Flip ? 440 : 442, "Facing East", new GumpCallback(FacingEast));

            AddLabel(307, 427, 0, "Número de Lockdowns:");
            AddTextField(456, 425, 125, 20, 0, 0, "Lockdowns", c_Sign.Locks.ToString());
            AddButton(596, 425, 559, 559, "Lockdowns", new GumpCallback(Lockdowns));

            AddImage(295, 482, 443);

            if (c_Sign.PropertyType == OSUPropertyType.House)
            {
                AddLabel(307, 457, 0, "Numero de Secures:");
                AddTextField(456, 455, 126, 20, 0, 0, "Secures", c_Sign.Secures.ToString());
                AddButton(596, 456, 559, 559, "Secures", new GumpCallback(Secures));
            }


            string shownPriceType = c_Sign.PriceType;

            if (RentOnly && shownPriceType == "Sale")
                shownPriceType = "Daily";

            AddLabel(480, 516, 0, shownPriceType == "Sale" ? "Venda" : (shownPriceType == "Daily" ? "Diária" : (shownPriceType == "Weekly" ? "Semanal" : "Mensal")));

            AddButton(410, 513, 453, 453, "LengthDown", new GumpCallback(PriceDown));
            AddButton(570, 513, 452, 452, "LengthUp", new GumpCallback(PriceUp));

            AddLabel(315, 555, 0, "Valor:");
            AddTextField(369, 556, 208, 20, 0, 0, "Price", c_Sign.Price.ToString());
            AddButton(602, 556, 559, 559, "Price", new GumpCallback(Price));

            AddImage(296, 583, 443);
            DrawCultureButton(379, 605, "Mataluns", "Mataluns");
            DrawCultureButton(379, 653, "Kamay", "Kamay");
            DrawCultureButton(521, 607, "Sarangs", "Sarangs");
            DrawCultureButton(521, 655, "Zorteros", "Zorteros");
            DrawCultureButton(642, 609, "Todos", "Todos");
            AddLabel(314, 615, 0, "Mataluns");
            AddLabel(316, 659, 0, "Kamay");
            AddLabel(455, 617, 0, "Sarangs");
            AddLabel(457, 661, 0, "Zorteros");
            AddLabel(596, 616, 0, "Todos");

            AddButton(496, 734, 559, 559, c_Sign.Owned ? "Salvar" : "Criar Casa", new GumpCallback(ClaimOrSave));
            AddLabel(419, 734, 0, c_Sign.Owned ? "Salvar Alterações" : (c_Sign.PropertyType == OSUPropertyType.Tomb ? "Criar Tumba" : (c_Sign.PropertyType == OSUPropertyType.Commercial ? "Criar Comercial" : "Criar Casa")));
        }

        private void DrawCultureButton(int x, int y, string cultureValue, string label)
        {
            bool selected = String.Equals(c_Sign.AllowedCulture, cultureValue, StringComparison.OrdinalIgnoreCase);
            AddButton(x, y, selected ? 440 : 442, selected ? 440 : 442, "Culture " + cultureValue, new GumpStateCallback(CultureSelect), cultureValue);
        }

        private string GetNameLabel()
        {
            if (c_Sign.PropertyType == OSUPropertyType.Commercial)
                return "Nome da Lojal:";
            if (c_Sign.PropertyType == OSUPropertyType.Tomb)
                return "Nome da Lápide:";
            return "Nome da Casa:";
        }

        private void Name()
        {
            c_Sign.Name = GetTextField("Name");
            Owner.SendMessage("Nome definido!");
            NewGump();
        }

        private void FacingSouth()
        {
            c_Sign.Flip = true;
            c_Sign.UpdateSignItem();
            c_Sign.ShowSignPreview();
            NewGump();
        }

        private void FacingEast()
        {
            c_Sign.Flip = false;
            c_Sign.UpdateSignItem();
            c_Sign.ShowSignPreview();
            NewGump();
        }

        private void CultureSelect(object obj)
        {
            if (obj is string)
            {
                c_Sign.AllowedCulture = (string)obj;
                NewGump();
            }
        }

        private void SignLocSelect()
        {
            Owner.SendMessage(c_Sign.PropertyType == OSUPropertyType.Tomb ? "Aponte o local da tumba." : "Aponte o local da placa da casa.");
            Owner.Target = new InternalTarget(this, c_Sign, TargetType.SignLoc);
        }

        private void MinZSelect()
        {
            Owner.SendMessage("Aponte o piso térreo.");
            c_Sign.ShowFloorsPreview(Owner);
            Owner.Target = new InternalTarget(this, c_Sign, TargetType.MinZ);
        }

        private void MaxZSelect()
        {
            Owner.SendMessage("Aponte o primeiro andar.");
            c_Sign.ShowFloorsPreview(Owner);
            Owner.Target = new InternalTarget(this, c_Sign, TargetType.MaxZ);
        }

        private void Secures()
        {
            c_Sign.Secures = GetTextFieldInt("Secures");
            Owner.SendMessage("Secures definidos!");
            NewGump();
        }

        private void Lockdowns()
        {
            c_Sign.Locks = GetTextFieldInt("Lockdowns");
            Owner.SendMessage("Lockdowns definidos!");
            NewGump();
        }

        private void Price()
        {
            c_Sign.Price = GetTextFieldInt("Price");
            Owner.SendMessage("Valor definido!");
            NewGump();
        }

        private void AddBlock()
        {
            Owner.SendMessage("Aponte o canto noroeste da área.");
            Owner.Target = new InternalTarget(this, c_Sign, TargetType.BlockOne);
        }

        private void ClearAll()
        {
            c_Sign.Blocks.Clear();
            c_Sign.ClearPreview();
            c_Sign.UpdateBlocks();
            Owner.SendMessage("Área limpa.");
            NewGump();
        }

        private void PriceUp()
        {
            c_Sign.NextPriceType();

            while (RentOnly && c_Sign.PriceType == "Sale")
                c_Sign.NextPriceType();

            NewGump();
        }

        private void PriceDown()
        {
            c_Sign.PrevPriceType();

            while (RentOnly && c_Sign.PriceType == "Sale")
                c_Sign.PrevPriceType();

            NewGump();
        }

        private void ClaimOrSave()
        {
            Name();
            Lockdowns();
            if (c_Sign.PropertyType == OSUPropertyType.House)
                Secures();

            Price();

            while (RentOnly && c_Sign.PriceType == "Sale")
                c_Sign.NextPriceType();

            if (c_Sign.PropertyType == OSUPropertyType.House)
            {
                c_Sign.ForcePrivate = true;
                c_Sign.ForcePublic = false;
            }
            else
            {
                c_Sign.ForcePrivate = false;
                c_Sign.ForcePublic = true;
            }

            if (!c_Sign.Owned)
            {
                new TownHouseConfirmGump(Owner, c_Sign);
                OnClose();
            }
            else
            {
                Owner.SendMessage("Alterações salvas.");
                OnClose();
            }
        }

        protected override void OnClose()
        {
            c_Sign.ClearPreview();
        }

        private class InternalTarget : Target
        {
            private readonly TownHouseSetupGump c_Gump;
            private readonly TownHouseSign c_Sign;
            private readonly TargetType c_Type;
            private readonly Point3D c_BoundOne;

            public InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type) : this(gump, sign, type, Point3D.Zero) { }

            public InternalTarget(TownHouseSetupGump gump, TownHouseSign sign, TargetType type, Point3D point) : base(20, true, TargetFlags.None)
            {
                c_Gump = gump;
                c_Sign = sign;
                c_Type = type;
                c_BoundOne = point;
            }

            protected override void OnTarget(Mobile m, object o)
            {
                IPoint3D point = (IPoint3D)o;
                switch (c_Type)
                {
                    case TargetType.SignLoc:
                        c_Sign.SignLoc = new Point3D(point.X, point.Y, point.Z);
                        c_Sign.MoveToWorld(c_Sign.SignLoc, c_Sign.Map);
                        c_Sign.UpdateSignItem();
                        c_Sign.ShowSignPreview();
                        c_Gump.NewGump();
                        break;

                    case TargetType.MinZ:
                        c_Sign.MinZ = point.Z;
                        if (c_Sign.MaxZ < c_Sign.MinZ + 19 || c_Sign.MaxZ == short.MaxValue)
                            c_Sign.MaxZ = point.Z + 19;
                        c_Sign.ShowFloorsPreview(m);
                        m.SendMessage("Altura do piso térreo definida em {0}.", c_Sign.MinZ);
                        c_Gump.NewGump();
                        break;

                    case TargetType.MaxZ:
                        c_Sign.MaxZ = point.Z + 19;
                        if (c_Sign.MinZ > c_Sign.MaxZ)
                            c_Sign.MinZ = point.Z;
                        c_Sign.ShowFloorsPreview(m);
                        m.SendMessage("Altura do primeiro andar definida em {0}.", point.Z);
                        c_Gump.NewGump();
                        break;

                    case TargetType.BlockOne:
                        m.SendMessage("Agora aponte o canto sudeste da área.");
                        m.Target = new InternalTarget(c_Gump, c_Sign, TargetType.BlockTwo, new Point3D(point.X, point.Y, point.Z));
                        break;

                    case TargetType.BlockTwo:
                        c_Sign.Blocks.Add(FixRect(new Rectangle2D(c_BoundOne, new Point3D(point.X + 1, point.Y + 1, point.Z))));
                        c_Sign.UpdateBlocks();
                        c_Sign.ShowAreaPreview(m);
                        c_Gump.NewGump();
                        break;
                }
            }

            protected override void OnTargetCancel(Mobile m, TargetCancelType cancelType)
            {
                c_Gump.NewGump();
            }
        }
    }
}
