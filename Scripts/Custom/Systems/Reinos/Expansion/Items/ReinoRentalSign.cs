using System;
using Server;
using Server.Custom.Systems.Rent;
using Server.Network;
using Server.Mobiles;

namespace Server.Custom.Systems.Reinos
{
    public class ReinoRentalSign : TownHouseSign
    {
        private int m_ReinoCityId;
        private int m_ParentLotId;
        private string m_ConstructionId;
        private string m_TemplateId;
        private string m_GroupTag;
        private bool m_GovernorManaged;
        private bool m_GovernorConfigured;
        private string m_AllowedCulturesCsv;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReinoCityId { get { return m_ReinoCityId; } set { m_ReinoCityId = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ParentLotId { get { return m_ParentLotId; } set { m_ParentLotId = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ConstructionId { get { return m_ConstructionId; } set { m_ConstructionId = value ?? String.Empty; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string TemplateId { get { return m_TemplateId; } set { m_TemplateId = value ?? String.Empty; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string GroupTag { get { return m_GroupTag; } set { m_GroupTag = value ?? String.Empty; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GovernorManaged { get { return m_GovernorManaged; } set { m_GovernorManaged = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GovernorConfigured { get { return m_GovernorConfigured; } set { m_GovernorConfigured = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string AllowedCulturesCsv { get { return m_AllowedCulturesCsv; } set { m_AllowedCulturesCsv = ReinoRentalCultureHelper.NormalizeCsv(value); InvalidateProperties(); } }

        [Constructable]
        public ReinoRentalSign() : base()
        {
            m_ConstructionId = String.Empty;
            m_TemplateId = String.Empty;
            m_GroupTag = "Residential";
            m_GovernorManaged = true;
            m_GovernorConfigured = true;
            m_AllowedCulturesCsv = "Todos";
            Name = "Imóvel do reino";
            AllowedCulture = "Todos";
        }

        public ReinoRentalSign(Serial serial) : base(serial)
        {
        }

        public bool CanGovernorConfigure(PlayerMobile pm)
        {
            if (pm == null || pm.Deleted || !m_GovernorManaged)
                return false;

            if (pm.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (Owned)
                return false;

            return ReinoAccessHelper.HasGovernmentAccess(pm, m_ReinoCityId);
        }

        public void OpenRentalOffer(Mobile m)
        {
            if (m == null)
                return;

            if (!Visible)
                return;

            if (!IsCultureAllowed(m))
                new TownHouseConfirmGump(m, this);
            else if (CanBuyHouse(m) && CanOwnThisProperty(m))
                new TownHouseConfirmGump(m, this);
            else if (!CanOwnThisProperty(m))
                m.SendMessage(CannotOwnMessage(m));
            else
                m.SendMessage("You cannot purchase this house.");
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m == null)
                return;

            PlayerMobile pm = m as PlayerMobile;

            if (pm == null)
            {
                base.OnDoubleClick(m);
                return;
            }

            // GM sempre abre o setup normal do sistema de rental
            if (pm.AccessLevel >= AccessLevel.GameMaster)
            {
                base.OnDoubleClick(m);
                return;
            }

            // Líder / chave do governo abre o setup reduzido do reino
            if (CanGovernorConfigure(pm))
            {
                pm.CloseGump(typeof(ReinoRentalSetupGump));
                pm.SendGump(new ReinoRentalSetupGump(pm, this));
                return;
            }

            if (!GovernorConfigured)
            {
                pm.SendMessage("Este imóvel ainda não foi liberado pelo governo.");
                return;
            }

            if (!ReinoRentalCultureHelper.IsAllowedFor(pm, m_AllowedCulturesCsv))
            {
                pm.SendMessage("Seu povo não pode alugar este imóvel no momento.");
                return;
            }

            string oldAllowed = AllowedCulture;
            AllowedCulture = "Todos";
            base.OnDoubleClick(m);
            AllowedCulture = oldAllowed;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!GovernorConfigured)
                list.Add("Aguardando liberação do governo.");

            list.Add("Povos permitidos: " + ReinoRentalCultureHelper.BuildDisplayLabel(m_AllowedCulturesCsv));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
            writer.Write(m_ReinoCityId);
            writer.Write(m_ParentLotId);
            writer.Write(m_ConstructionId ?? String.Empty);
            writer.Write(m_TemplateId ?? String.Empty);
            writer.Write(m_GroupTag ?? String.Empty);
            writer.Write(m_GovernorManaged);
            writer.Write(m_GovernorConfigured);
            writer.Write(m_AllowedCulturesCsv ?? "Todos");
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_ReinoCityId = reader.ReadInt();
            m_ParentLotId = reader.ReadInt();
            m_ConstructionId = reader.ReadString();
            m_TemplateId = reader.ReadString();
            m_GroupTag = reader.ReadString();
            m_GovernorManaged = reader.ReadBool();
            m_GovernorConfigured = reader.ReadBool();
            m_AllowedCulturesCsv = reader.ReadString();
            if (String.IsNullOrWhiteSpace(m_AllowedCulturesCsv))
                m_AllowedCulturesCsv = "Todos";
            AllowedCulture = "Todos";
        }
    }
}
