
using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Custom.Reinos;
using System.Collections;

namespace Server.Custom.Systems.Rent
{
    public class ReinoEvictionVault : Bag
    {
        [Constructable]
        public ReinoEvictionVault()
        {
            Movable = false;
            Visible = false;
            Name = "vault de despejo";
            Map = Map.Internal;
            Location = Point3D.Zero;
        }

        public ReinoEvictionVault(Serial serial) : base(serial)
        {
        }

        public override int DefaultMaxWeight { get { return 0; } }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            Movable = false;
            Visible = false;
            Map = Map.Internal;
            Location = Point3D.Zero;
        }
    }

    public class ReinoRentEvictionClaim
    {
        public int OwnerSerial;
        public int CityId;
        public int VaultSerial;
        public DateTime StoredUtc;
        public string Reason;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(OwnerSerial);
            bw.Write(CityId);
            bw.Write(VaultSerial);
            bw.Write(StoredUtc.ToBinary());
            bw.Write(Reason ?? String.Empty);
        }

        public static ReinoRentEvictionClaim Deserialize(BinaryReader br)
        {
            ReinoRentEvictionClaim c = new ReinoRentEvictionClaim();
            c.OwnerSerial = br.ReadInt32();
            c.CityId = br.ReadInt32();
            c.VaultSerial = br.ReadInt32();
            c.StoredUtc = DateTime.FromBinary(br.ReadInt64());
            c.Reason = br.ReadString();
            return c;
        }
    }

    public static class ReinoRentEvictionStorageSystem
    {
        private static readonly string FilePath = Path.Combine(Core.BaseDirectory, "Data", "OSU_ReinoRentEvictions_v1.bin");
        private static readonly List<ReinoRentEvictionClaim> m_Claims = new List<ReinoRentEvictionClaim>();
        private static Timer m_Timer;

        public static int FinePerDayGold = 200;
        public static TimeSpan ExpireAfter = TimeSpan.FromDays(10.0);

        public static void Initialize()
        {
            Load();
            EventSink.WorldSave += delegate { Save(); };
            EventSink.Login += OnLogin;
            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(30.0), CleanupExpiredClaims);
        }

        public static void StoreClaim(Mobile owner, int cityId, IList items, string reason)
        {
            if (owner == null || owner.Deleted || items == null || items.Count == 0)
                return;

            ReinoEvictionVault vault = new ReinoEvictionVault();
            vault.MoveToWorld(Point3D.Zero, Map.Internal);

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i] as Item;
                if (item == null || item.Deleted)
                    continue;

                try
                {
                    item.Movable = true;
                    item.IsLockedDown = false;
                    item.IsSecure = false;
                    vault.DropItem(item);
                }
                catch
                {
                    try { item.Delete(); } catch { }
                }
            }

            m_Claims.Add(new ReinoRentEvictionClaim
            {
                OwnerSerial = owner.Serial.Value,
                CityId = cityId,
                VaultSerial = vault.Serial.Value,
                StoredUtc = DateTime.UtcNow,
                Reason = reason ?? String.Empty
            });
        }

        public static List<ReinoRentEvictionClaim> GetClaims(Mobile owner)
        {
            return GetClaims(owner, -1);
        }

        public static List<ReinoRentEvictionClaim> GetClaims(Mobile owner, int cityId)
        {
            CleanupExpiredClaims();

            List<ReinoRentEvictionClaim> list = new List<ReinoRentEvictionClaim>();
            if (owner == null)
                return list;

            for (int i = 0; i < m_Claims.Count; i++)
            {
                ReinoRentEvictionClaim c = m_Claims[i];
                if (c == null || c.OwnerSerial != owner.Serial.Value)
                    continue;

                if (cityId > 0 && c.CityId != cityId)
                    continue;

                list.Add(c);
            }

            return list;
        }

        public static int GetClaimFine(ReinoRentEvictionClaim claim)
        {
            if (claim == null)
                return 0;

            TimeSpan elapsed = DateTime.UtcNow - claim.StoredUtc;
            int days = Math.Max(0, (int)Math.Floor(elapsed.TotalDays));
            return FinePerDayGold * (1 + days);
        }

        public static int GetTotalFine(Mobile owner)
        {
            return GetTotalFine(owner, -1);
        }

        public static int GetTotalFine(Mobile owner, int cityId)
        {
            int total = 0;
            List<ReinoRentEvictionClaim> claims = GetClaims(owner, cityId);

            for (int i = 0; i < claims.Count; i++)
                total += GetClaimFine(claims[i]);

            return total;
        }

        public static void DepositFineToLedgers(Mobile owner)
        {
            DepositFineToLedgers(owner, -1);
        }

        public static void DepositFineToLedgers(Mobile owner, int cityId)
        {
            List<ReinoRentEvictionClaim> claims = GetClaims(owner, cityId);

            for (int i = 0; i < claims.Count; i++)
            {
                ReinoRentEvictionClaim claim = claims[i];
                if (claim == null || claim.CityId <= 0)
                    continue;

                ReinoResourceLedger ledger = ReinoExpansionSystem.GetLedger(claim.CityId);
                if (ledger != null)
                    ledger.Add(ReinoResourceType.Gold, GetClaimFine(claim));
            }
        }

        public static bool RedeemAll(PlayerMobile pm, out string message)
        {
            return RedeemAll(pm, -1, out message);
        }

        public static bool RedeemAll(PlayerMobile pm, int cityId, out string message)
        {
            message = null;

            if (pm == null || pm.Deleted || pm.Backpack == null)
            {
                message = "Você precisa ter uma mochila para receber seus itens.";
                return false;
            }

            List<ReinoRentEvictionClaim> claims = GetClaims(pm, cityId);
            if (claims.Count == 0)
            {
                message = "Você não possui itens retidos no depositário.";
                return false;
            }

            Bag recovery = new Bag();
            recovery.Name = "pertences recuperados";

            int totalFine = 0;

            for (int i = 0; i < claims.Count; i++)
            {
                ReinoRentEvictionClaim claim = claims[i];
                totalFine += GetClaimFine(claim);

                Container vault = World.FindItem((Serial)claim.VaultSerial) as Container;
                if (vault != null && !vault.Deleted)
                {
                    List<Item> items = new List<Item>(vault.Items);

                    for (int j = 0; j < items.Count; j++)
                    {
                        Item item = items[j];
                        if (item == null || item.Deleted)
                            continue;

                        try
                        {
                            recovery.DropItem(item);
                        }
                        catch
                        {
                            try { item.Delete(); } catch { }
                        }
                    }

                    vault.Delete();
                }

            }

            if (recovery.Items.Count == 0)
            {
                recovery.Delete();

                for (int i = m_Claims.Count - 1; i >= 0; i--)
                {
                    ReinoRentEvictionClaim c = m_Claims[i];
                    if (c != null && c.OwnerSerial == pm.Serial.Value && (cityId <= 0 || c.CityId == cityId))
                        m_Claims.RemoveAt(i);
                }

                message = "Seus registros de despejo foram limpos, mas não havia mais itens no vault.";
                return true;
            }

            pm.Backpack.AddItem(recovery);

            for (int i = m_Claims.Count - 1; i >= 0; i--)
            {
                ReinoRentEvictionClaim c = m_Claims[i];
                if (c != null && c.OwnerSerial == pm.Serial.Value)
                    m_Claims.RemoveAt(i);
            }

            message = String.Format("Você pagou {0} moedas e recebeu seus pertences de volta.", totalFine);
            return true;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;

            int fine = GetTotalFine(pm);
            if (fine > 0)
                pm.SendMessage("Você possui itens retidos no depositário. Para Recuperar seus itens, pague: {0} moedas.", fine);
        }

        public static void CleanupExpiredClaims()
        {
            DateTime now = DateTime.UtcNow;

            for (int i = m_Claims.Count - 1; i >= 0; i--)
            {
                ReinoRentEvictionClaim claim = m_Claims[i];
                if (claim == null)
                {
                    m_Claims.RemoveAt(i);
                    continue;
                }

                if (now - claim.StoredUtc < ExpireAfter)
                    continue;

                Container vault = World.FindItem((Serial)claim.VaultSerial) as Container;
                if (vault != null && !vault.Deleted)
                    vault.Delete();

                m_Claims.RemoveAt(i);
            }
        }

        private static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(0);
                    bw.Write(m_Claims.Count);
                    for (int i = 0; i < m_Claims.Count; i++)
                        m_Claims[i].Serialize(bw);
                }
            }
            catch
            {
            }
        }

        private static void Load()
        {
            m_Claims.Clear();

            if (!File.Exists(FilePath))
                return;

            try
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    int version = br.ReadInt32();
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                        m_Claims.Add(ReinoRentEvictionClaim.Deserialize(br));
                }
            }
            catch
            {
                m_Claims.Clear();
            }
        }
    }

    [CorpseName("um corpo sem vida")]
    public class ReinoEvictionStorageNpc : BaseCreature
    {
        private int m_CityId;

        [CommandProperty(AccessLevel.GameMaster)]
        public int CityId
        {
            get { return m_CityId; }
            set { m_CityId = value; InvalidateProperties(); }
        }

        [Constructable]
        public ReinoEvictionStorageNpc()
            : base(AIType.AI_Vendor, FightMode.None, 10, 1, 0.2, 0.4)
        {
            Blessed = true;
            CantWalk = true;
            Direction = Direction.South;
            Female = Utility.RandomBool();
            Body = Female ? 0x191 : 0x190;
            Hue = Utility.RandomSkinHue();
            Name = NameList.RandomName(Female ? "female" : "male") + " depositário";

            AddItem(new Shirt(Utility.RandomNeutralHue()) { Movable = false });
            AddItem(new LongPants(Utility.RandomNeutralHue()) { Movable = false });
            AddItem(new Boots(Utility.RandomNeutralHue()) { Movable = false });
            AddItem(new HalfApron(Utility.RandomNeutralHue()) { Movable = false });

            Container pack = new Backpack();
            pack.Movable = false;
            AddItem(pack);

            Utility.AssignRandomHair(this);
            m_CityId = -1;
        }

        public override bool IsInvulnerable { get { return true; } }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_CityId > 0)
                list.Add("Depositário de {0}", ReinoElectionsSystem.GetCityName(m_CityId));
        }

        public ReinoEvictionStorageNpc(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return;

            int fine = ReinoRentEvictionStorageSystem.GetTotalFine(pm, m_CityId);
            if (fine <= 0)
            {
                SayTo(pm, "Você não possui itens retidos aqui.");
                return;
            }

            SayTo(pm, "Você precisa pagar uma multa de {0} moedas para retirar seus itens.", fine);
            pm.Target = new ReinoEvictionPaymentTarget(this);
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            PlayerMobile pm = from as PlayerMobile;
            if (pm == null || dropped == null || dropped.Deleted)
                return false;

            return TryTakePayment(pm, dropped);
        }

        private bool TryTakePayment(PlayerMobile pm, Item payment)
        {
            int fine = ReinoRentEvictionStorageSystem.GetTotalFine(pm, m_CityId);
            if (fine <= 0)
            {
                pm.SendMessage("Você não possui itens retidos aqui.");
                return false;
            }

            bool validSource = payment.IsChildOf(pm.Backpack) || payment.RootParent == pm || payment.Parent == this || payment.Parent == Backpack;

            if (!validSource)
            {
                pm.SendMessage("O pagamento precisa sair de uma pilha de moedas que estava com você.");
                return false;
            }

            int available = 0;
            if (payment is Gold)
                available = ((Gold)payment).Amount;
            else if (payment is Copper)
                available = ((Copper)payment).Amount;
            else
            {
                pm.SendMessage("Use uma pilha de moedas para pagar a multa.");
                return false;
            }

            if (available < fine)
            {
                pm.SendMessage("A multa atual é de {0} moedas.", fine);
                return false;
            }

            ConsumeFromStack(payment, fine);
            ReinoRentEvictionStorageSystem.DepositFineToLedgers(pm, m_CityId);

            string message;
            if (!ReinoRentEvictionStorageSystem.RedeemAll(pm, m_CityId, out message))
            {
                pm.SendMessage(message);
                return false;
            }

            pm.SendMessage(message);
            return true;
        }

        private static void ConsumeFromStack(Item item, int amount)
        {
            if (item is Gold)
            {
                Gold gold = (Gold)item;
                gold.Amount -= amount;
                if (gold.Amount <= 0)
                    gold.Delete();
            }
            else if (item is Copper)
            {
                Copper copper = (Copper)item;
                copper.Amount -= amount;
                if (copper.Amount <= 0)
                    copper.Delete();
            }
        }

        private class ReinoEvictionPaymentTarget : Target
        {
            private readonly ReinoEvictionStorageNpc m_Npc;

            public ReinoEvictionPaymentTarget(ReinoEvictionStorageNpc npc)
                : base(12, false, TargetFlags.None)
            {
                m_Npc = npc;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = from as PlayerMobile;
                Item item = targeted as Item;

                if (pm == null || item == null || m_Npc == null || m_Npc.Deleted)
                    return;

                m_Npc.TryTakePayment(pm, item);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
            writer.Write(m_CityId);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 1)
                m_CityId = reader.ReadInt();
            else
                m_CityId = -1;
            Blessed = true;
            CantWalk = true;
        }
    }
}
