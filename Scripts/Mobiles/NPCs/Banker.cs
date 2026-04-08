#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server.Accounting;
using Server.ContextMenus;
using Server.Custom.Reinos;
using Server.Items;
using Server.Network;

using Acc = Server.Accounting.Account;
#endregion

namespace Server.Mobiles
{
    public class Banker : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        [Constructable]
        public Banker()
            : base("the banker")
        { }

        public Banker(Serial serial)
            : base(serial)
        { }

        public override NpcGuild NpcGuild { get { return NpcGuild.MerchantsGuild; } }

        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        // ===== OSU: Cidade do banco =====
        [CommandProperty(AccessLevel.GameMaster)]
        public string OSUBankCityId { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string OSUBankCityName { get; set; }

        private string GetBankCityName()
        {
            return String.IsNullOrWhiteSpace(OSUBankCityName) ? OSUBankCityId : OSUBankCityName;
        }

        private bool CanUseThisBank(Mobile from)
        {
            string reason;
            return CanUseThisBank(from, out reason);
        }

        private bool CanUseThisBank(Mobile from, out string reason)
        {
            reason = String.Empty;

            if (String.IsNullOrWhiteSpace(OSUBankCityId))
                return true;

            PlayerMobile pm = from as PlayerMobile;
            if (pm == null)
                return true;

            int cityId = ReinoDiplomacySystem.ResolveCityId(OSUBankCityId);

            if (cityId >= 0)
                return ReinoDiplomacySystem.CanUseBank(pm, cityId, out reason);

            bool isCitizen = String.Equals(pm.OSUCitizenCityId, OSUBankCityId, StringComparison.OrdinalIgnoreCase);

            if (!isCitizen)
                reason = "Você não possui permissão para usar o banco desta cidade.";

            return isCitizen;
        }

        private void SayNoBankAccess(Mobile from)
        {
            string reason;
            if (!CanUseThisBank(from, out reason) && !String.IsNullOrWhiteSpace(reason))
            {
                Say(reason);
                return;
            }

            string cityName = GetBankCityName();
            if (String.IsNullOrWhiteSpace(cityName))
                cityName = "esta cidade";

            Say("Você não possui permissão para acessar o banco de " + cityName + ".");
        }

        public static int GetBalance(Mobile m)
        {
            double balance = 0;

            if (AccountGold.Enabled && m.Account != null)
            {
                int goldStub;
                m.Account.GetGoldBalance(out goldStub, out balance);

                if (balance > Int32.MaxValue)
                {
                    return Int32.MaxValue;
                }
            }

            Container bank = m.Player ? m.BankBox : m.FindBankNoCreate();

            if (bank != null)
            {
                var gold = bank.FindItemsByType<Gold>();
                var checks = bank.FindItemsByType<BankCheck>();

                balance += gold.Aggregate(0.0, (c, t) => c + t.Amount);
                balance += checks.Aggregate(0.0, (c, t) => c + t.Worth);
            }

            return (int)Math.Max(0, Math.Min(Int32.MaxValue, balance));
        }

        public static int GetBalance(Mobile m, out Item[] gold, out Item[] checks)
        {
            double balance = 0;

            if (AccountGold.Enabled && m.Account != null)
            {
                int goldStub;
                m.Account.GetGoldBalance(out goldStub, out balance);

                if (balance > Int32.MaxValue)
                {
                    gold = checks = new Item[0];
                    return Int32.MaxValue;
                }
            }

            Container bank = m.Player ? m.BankBox : m.FindBankNoCreate();

            if (bank != null)
            {
                gold = bank.FindItemsByType(typeof(Gold));
                checks = bank.FindItemsByType(typeof(BankCheck));

                balance += gold.OfType<Gold>().Aggregate(0.0, (c, t) => c + t.Amount);
                balance += checks.OfType<BankCheck>().Aggregate(0.0, (c, t) => c + t.Worth);
            }
            else
            {
                gold = checks = new Item[0];
            }

            return (int)Math.Max(0, Math.Min(Int32.MaxValue, balance));
        }

        public static bool Withdraw(Mobile from, int amount, bool message = false)
        {
            // If for whatever reason the TOL checks fail, we should still try old methods for withdrawing currency.
            if (AccountGold.Enabled && from.Account != null && from.Account.WithdrawGold(amount))
            {
                if (message)
                    from.SendLocalizedMessage(1155856, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold has been removed from your bank box.

                return true;
            }

            Item[] gold, checks;
            var balance = GetBalance(from, out gold, out checks);

            if (balance < amount)
            {
                return false;
            }

            for (var i = 0; amount > 0 && i < gold.Length; ++i)
            {
                if (gold[i].Amount <= amount)
                {
                    amount -= gold[i].Amount;
                    gold[i].Delete();
                }
                else
                {
                    gold[i].Amount -= amount;
                    amount = 0;
                }
            }

            for (var i = 0; amount > 0 && i < checks.Length; ++i)
            {
                var check = (BankCheck)checks[i];

                if (check.Worth <= amount)
                {
                    amount -= check.Worth;
                    check.Delete();
                }
                else
                {
                    check.Worth -= amount;
                    amount = 0;
                }
            }

            if (message)
                from.SendLocalizedMessage(1155856, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold has been removed from your bank box.

            return true;
        }

        public static bool Deposit(Mobile from, int amount, bool message = false)
        {
            // If for whatever reason the TOL checks fail, we should still try old methods for depositing currency.
            if (AccountGold.Enabled && from.Account != null && from.Account.DepositGold(amount))
            {
                if (message)
                    from.SendLocalizedMessage(1042763, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold was deposited in your account.

                return true;
            }

            var box = from.Player ? from.BankBox : from.FindBankNoCreate();

            if (box == null)
            {
                return false;
            }

            var items = new List<Item>();

            while (amount > 0)
            {
                Item item;
                if (amount < 5000)
                {
                    item = new Gold(amount);
                    amount = 0;
                }
                else if (amount <= 1000000)
                {
                    item = new BankCheck(amount);
                    amount = 0;
                }
                else
                {
                    item = new BankCheck(1000000);
                    amount -= 1000000;
                }

                if (box.TryDropItem(from, item, false))
                {
                    items.Add(item);
                }
                else
                {
                    item.Delete();
                    foreach (var curItem in items)
                    {
                        curItem.Delete();
                    }

                    return false;
                }
            }

            if (message)
                from.SendLocalizedMessage(1042763, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold was deposited in your account.

            return true;
        }

        public static int DepositUpTo(Mobile from, int amount, bool message = false)
        {
            // If for whatever reason the TOL checks fail, we should still try old methods for depositing currency.
            if (AccountGold.Enabled && from.Account != null && from.Account.DepositGold(amount))
            {
                if (message)
                    from.SendLocalizedMessage(1042763, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold was deposited in your account.

                return amount;
            }

            var box = from.Player ? from.BankBox : from.FindBankNoCreate();

            if (box == null)
            {
                return 0;
            }

            var amountLeft = amount;
            while (amountLeft > 0)
            {
                Item item;
                int amountGiven;

                if (amountLeft < 5000)
                {
                    item = new Gold(amountLeft);
                    amountGiven = amountLeft;
                }
                else if (amountLeft <= 1000000)
                {
                    item = new BankCheck(amountLeft);
                    amountGiven = amountLeft;
                }
                else
                {
                    item = new BankCheck(1000000);
                    amountGiven = 1000000;
                }

                if (box.TryDropItem(from, item, false))
                {
                    amountLeft -= amountGiven;
                }
                else
                {
                    item.Delete();
                    break;
                }
            }

            return amount - amountLeft;
        }

        public static void Deposit(Container cont, int amount)
        {
            while (amount > 0)
            {
                Item item;

                if (amount < 5000)
                {
                    item = new Gold(amount);
                    amount = 0;
                }
                else if (amount <= 1000000)
                {
                    item = new BankCheck(amount);
                    amount = 0;
                }
                else
                {
                    item = new BankCheck(1000000);
                    amount -= 1000000;
                }

                cont.DropItem(item);
            }
        }
        public override bool IsActiveVendor { get { return false; } }

        public override void InitSBInfo()
        {
         //   m_SBInfos.Add(new SBBanker());
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (from.InRange(Location, 12))
            {
                return true;
            }

            return base.HandlesOnSpeech(from);
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            HandleSpeech(this, e);
            base.OnSpeech(e);
        }

        public static void HandleSpeech(Mobile vendor, SpeechEventArgs e)
        {
            Banker banker = vendor as Banker;

            if (!e.Handled && e.Mobile.InRange(vendor, 12))
            {
                if (e.Mobile.Map.Rules != MapRules.FeluccaRules && vendor is BaseVendor && !((BaseVendor)vendor).CheckVendorAccess(e.Mobile))
                {
                    return;
                }

                // OSU: bloqueio total do banco para não-cidadãos (para QUALQUER comando bancário)
                if (banker != null && !banker.CanUseThisBank(e.Mobile))
                {
                    foreach (var keyword in e.Keywords)
                    {
                        // keywords bancárias: withdraw, balance, bank, check (as que este arquivo trata)
                        if (keyword == 0x0000 || keyword == 0x0001 || keyword == 0x0002 || keyword == 0x0003)
                        {
                            e.Handled = true;
                            banker.SayNoBankAccess(e.Mobile);
                            return;
                        }
                    }
                }

                foreach (var keyword in e.Keywords)
                {
                    switch (keyword)
                    {
                        case 0x0000: // *withdraw*
                            {
                                e.Handled = true;

                                if (e.Mobile.Criminal)
                                {
                                    vendor.Say(500389);
                                    break;
                                }

                                var split = e.Speech.Split(' ');

                                if (split.Length >= 2)
                                {
                                    int amount;

                                    var pack = e.Mobile.Backpack;

                                    if (!int.TryParse(split[1], out amount))
                                    {
                                        break;
                                    }

                                    if ((!Core.ML && amount > 5000) || (Core.ML && amount > 60000))
                                    {
                                        vendor.Say(500381);
                                    }
                                    else if (pack == null || pack.Deleted || !(pack.TotalWeight < pack.MaxWeight) ||
                                             !(pack.TotalItems < pack.MaxItems))
                                    {
                                        vendor.Say(1048147);
                                    }
                                    else if (amount > 0)
                                    {
                                        var box = e.Mobile.Player ? e.Mobile.BankBox : e.Mobile.FindBankNoCreate();

                                        if (box == null || !Withdraw(e.Mobile, amount))
                                        {
                                            vendor.Say(500384);
                                        }
                                        else
                                        {
                                            pack.DropItem(new Gold(amount));
                                            vendor.Say(1010005);
                                        }
                                    }
                                }
                            }
                            break;

                        case 0x0001: // *balance*
                            {
                                e.Handled = true;

                                if (e.Mobile.Criminal)
                                {
                                    vendor.Say(500389);
                                    break;
                                }

                                if (AccountGold.Enabled && e.Mobile.Account is Account)
                                {
                                    vendor.Say(1155855, String.Format("{0:#,0}\t{1:#,0}",
                                        e.Mobile.Account.TotalPlat,
                                        e.Mobile.Account.TotalGold), 0x3BC);

                                    vendor.Say(1155848, String.Format("{0:#,0}", ((Account)e.Mobile.Account).GetSecureAccountAmount(e.Mobile)), 0x3BC);
                                }
                                else
                                {
                                    vendor.Say(1042759, GetBalance(e.Mobile).ToString("#,0"));
                                }
                            }
                            break;

                        case 0x0002: // *bank*
                            {
                                e.Handled = true;

                                if (e.Mobile.Criminal)
                                {
                                    vendor.Say(500378);
                                    break;
                                }

                                // OSU: bloqueio aqui também (caso alguém chame "bank" sem keyword detection acima)
                                if (banker != null && !banker.CanUseThisBank(e.Mobile))
                                {
                                    banker.SayNoBankAccess(e.Mobile);
                                    break;
                                }

                                e.Mobile.BankBox.Open();
                            }
                            break;

                        case 0x0003: // *check*
                            {
                                e.Handled = true;

                                if (e.Mobile.Criminal)
                                {
                                    vendor.Say(500389);
                                    break;
                                }

                                // OSU: bloqueio
                                if (banker != null && !banker.CanUseThisBank(e.Mobile))
                                {
                                    banker.SayNoBankAccess(e.Mobile);
                                    break;
                                }

                                if (AccountGold.Enabled && e.Mobile.Account != null)
                                {
                                    vendor.Say("We no longer offer a checking service.");
                                    break;
                                }

                                var split = e.Speech.Split(' ');

                                if (split.Length >= 2)
                                {
                                    int amount;

                                    if (!int.TryParse(split[1], out amount))
                                    {
                                        break;
                                    }

                                    if (amount < 5000)
                                    {
                                        vendor.Say(1010006);
                                    }
                                    else if (amount > 1000000)
                                    {
                                        vendor.Say(1010007);
                                    }
                                    else
                                    {
                                        var check = new BankCheck(amount);

                                        var box = e.Mobile.BankBox;

                                        if (!box.TryDropItem(e.Mobile, check, false))
                                        {
                                            vendor.Say(500386);
                                            check.Delete();
                                        }
                                        else if (!box.ConsumeTotal(typeof(Gold), amount))
                                        {
                                            vendor.Say(500384);
                                            check.Delete();
                                        }
                                        else
                                        {
                                            vendor.Say(1042673, AffixType.Append, amount.ToString("#,0"), "");
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        // OSU: bloquear deposit via drag & drop
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!CanUseThisBank(from))
            {
                SayNoBankAccess(from);
                return false;
            }

            return base.OnDragDrop(from, dropped);
        }

        // OSU: Context Menu - se não for cidadão, em vez de abrir banco, mostra mensagem
        private class DeniedBankEntry : ContextMenuEntry
        {
            private readonly Banker _banker;
            private readonly Mobile _from;

            public DeniedBankEntry(Banker banker, Mobile from)
                : base(6105) // "Open Bankbox" (id padrão do menu)
            {
                _banker = banker;
                _from = from;
                Enabled = true;
            }

            public override void OnClick()
            {
                if (_banker != null && _from != null)
                    _banker.SayNoBankAccess(_from);
            }
        }

        public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
        {
            if (from.Alive)
            {
                if (!CanUseThisBank(from))
                {
                    list.Add(new DeniedBankEntry(this, from));
                }
                else
                {
                    var entry = new OpenBankEntry(this);
                    entry.Enabled = from.Map.Rules == MapRules.FeluccaRules || CheckVendorAccess(from);
                    list.Add(entry);
                }
            }

            base.AddCustomContextEntries(from, list);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
            writer.Write(OSUBankCityId);
            writer.Write(OSUBankCityName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
            {
                OSUBankCityId = reader.ReadString();
                OSUBankCityName = reader.ReadString();
            }
        }
    }
}
