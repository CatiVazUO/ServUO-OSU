using Server.Custom.Systems.Rent;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Custom.Systems.Rent
{
	public enum Intu{ Neither, No, Yes }

	[Flipable( 0xC0B, 0xC0C )]
	public class TownHouseSign : Item
	{
		private static ArrayList s_TownHouseSigns = new ArrayList();
		public static ArrayList AllSigns{ get{ return s_TownHouseSigns; } }

		private Point3D c_BanLoc, c_SignLoc;
		private int c_Locks, c_Secures, c_Price, c_MinZ, c_MaxZ, c_MinTotalSkill, c_MaxTotalSkill, c_ItemsPrice, c_RTOPayments;
		private bool c_YoungOnly, c_RecurRent, c_Relock, c_KeepItems, c_LeaveItems, c_RentToOwn, c_Free, c_ForcePrivate, c_ForcePublic, c_NoTrade, c_NoBanning;
		private string c_Skill;
		private double c_SkillReq;
		private ArrayList c_Blocks, c_DecoreItemInfos, c_PreviewItems;
		private TownHouse c_House;
		private Timer c_DemolishTimer, c_RentTimer, c_PreviewTimer;
		private DateTime c_DemolishTime, c_RentTime;
		private TimeSpan c_RentByTime, c_OriginalRentTime;
		private Intu c_Murderers;
		//private Nation m_Nation;
		private Container m_Treasury;
		private bool m_Flip;
        private OSUPropertyType m_PropertyType;
        private string m_AllowedCulture;
        private string m_AllowedCulturesCsv;
        private string m_CitizenCityId;
        private bool m_GovernmentManaged;
        private int m_GovernmentCityId;
        private bool m_GovernorConfigured;

        private int m_TombSelectedItemID;
        private int m_TombSelectedGumpID;
        private int m_TombExtraCost;

        private string m_TombDeadName;
        private string m_TombBirthYear;
        private string m_TombDeathYear;
        private string m_TombMessage;

        private bool m_TombFinalized;

        [CommandProperty(AccessLevel.GameMaster)]
        public OSUPropertyType PropertyType
        {
            get { return m_PropertyType; }
            set
            {
                m_PropertyType = value;

                if (m_PropertyType != OSUPropertyType.House)
                    Secures = 0;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TombSelectedItemID
        {
            get { return m_TombSelectedItemID; }
            set { m_TombSelectedItemID = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TombSelectedGumpID
        {
            get { return m_TombSelectedGumpID; }
            set { m_TombSelectedGumpID = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TombExtraCost
        {
            get { return m_TombExtraCost; }
            set { m_TombExtraCost = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string TombDeadName
        {
            get { return m_TombDeadName; }
            set { m_TombDeadName = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string TombBirthYear
        {
            get { return m_TombBirthYear; }
            set { m_TombBirthYear = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string TombDeathYear
        {
            get { return m_TombDeathYear; }
            set { m_TombDeathYear = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string TombMessage
        {
            get { return m_TombMessage; }
            set { m_TombMessage = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool TombFinalized
        {
            get { return m_TombFinalized; }
            set { m_TombFinalized = value; InvalidateProperties(); }
        }


        [CommandProperty( AccessLevel.GameMaster )]
		public string AllowedCulture
		{
			get{ return m_AllowedCulture; }
			set{ m_AllowedCulture = value; InvalidateProperties(); }
		}

        [CommandProperty(AccessLevel.GameMaster)]
        public string CitizenCityId
        {
            get { return m_CitizenCityId; }
            set { m_CitizenCityId = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string AllowedCulturesCsv
        {
            get { return m_AllowedCulturesCsv; }
            set { m_AllowedCulturesCsv = NormalizeCulturesCsv(value); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GovernmentManaged
        {
            get { return m_GovernmentManaged; }
            set { m_GovernmentManaged = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GovernmentCityId
        {
            get { return m_GovernmentCityId; }
            set { m_GovernmentCityId = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GovernorConfigured
        {
            get { return m_GovernorConfigured; }
            set { m_GovernorConfigured = value; InvalidateProperties(); }
        }

        [CommandProperty( AccessLevel.GameMaster )]
		public bool Flip
		{
			get{ return m_Flip; }
			set
			{
				if( value != m_Flip )
				{
					UpdateSignItem(value);
				}
				
				m_Flip = value;
			}
		}
		
		[CommandProperty( AccessLevel.GameMaster )]
		//public Nation Nation
		//{
		//	get{ return m_Nation; }
		//	set{ m_Nation = value; }
		//}

		//[CommandProperty( AccessLevel.GameMaster )]
		//public Container Treasury
		//{
		//	get{ return m_Treasury; }
		//	set{ m_Treasury = value; }
		//}

		public Point3D BanLoc
		{
			get{ return c_BanLoc; }
			set
			{
				c_BanLoc = value;
				InvalidateProperties();
				if ( Owned )
					c_House.Region.GoLocation = value;
			}
		}

		public Point3D SignLoc
		{
			get{ return c_SignLoc; }
			set
			{
				c_SignLoc = value;
				InvalidateProperties();

				if ( Owned )
				{
					c_House.Sign.Location = value;
					//c_House.Hanger.Location = value;
				}
			}
		}

		public int Locks
		{
			get{ return c_Locks; }
			set
			{
				c_Locks = value;
				InvalidateProperties();
				if ( Owned )
					c_House.MaxLockDowns = value;
			}
		}

		public int Secures
		{
			get{ return c_Secures; }
			set
			{
				c_Secures = value;
				InvalidateProperties();
				if ( Owned )
					c_House.MaxSecures = value;
			}
		}

		public int Price
		{
			get{ return c_Price; }
			set
			{
				c_Price = value;
				InvalidateProperties();
			}
		}

		public int MinZ
		{
			get{ return c_MinZ; }
			set
			{
				if ( value > c_MaxZ )
					c_MaxZ = value+1;

				c_MinZ = value;
                if (Owned)
                    RUOVersion.UpdateRegion(this);
            }
		}

		public int MaxZ
		{
			get{ return c_MaxZ; }
			set
			{
				if ( value < c_MinZ )
					value = c_MinZ;

				c_MaxZ = value;
                if (Owned)
                    RUOVersion.UpdateRegion(this);
            }
		}

		public int MinTotalSkill
		{
			get{ return c_MinTotalSkill; }
			set
			{
				if ( value > c_MaxTotalSkill )
					value = c_MaxTotalSkill;

				c_MinTotalSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public int MaxTotalSkill
		{
			get{ return c_MaxTotalSkill; }
			set
			{
				if ( value < c_MinTotalSkill )
					value = c_MinTotalSkill;

				c_MaxTotalSkill = value;
				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public bool YoungOnly
		{
			get{ return c_YoungOnly; }
			set
			{
				c_YoungOnly = value;

				if ( c_YoungOnly )
					c_Murderers = Intu.Neither;

				ValidateOwnership();
				InvalidateProperties();
			}
		}

		public TimeSpan RentByTime
		{
			get{ return c_RentByTime; }
			set
			{
				c_RentByTime = value;
				c_OriginalRentTime = value;

				if ( value == TimeSpan.Zero )
                    ClearRentTimer();
				else
				{
					ClearRentTimer();
					BeginRentTimer( value );
				}

				InvalidateProperties();
			}
		}

		public bool RecurRent
		{
			get{ return c_RecurRent; }
			set
			{
				c_RecurRent = value;

				if ( !value )
					c_RentToOwn = value;

				InvalidateProperties();
			}
		}

		public bool KeepItems
		{
			get{ return c_KeepItems; }
			set
			{
				c_LeaveItems = false;
				c_KeepItems = value;
				InvalidateProperties();
			}
		}

		public bool Free
		{
			get{ return c_Free; }
			set
			{
				c_Free = value;
				c_Price = 1;
				InvalidateProperties();
			}
		}

		public Intu Murderers
		{
			get{ return c_Murderers; }
			set
			{
				c_Murderers = value;

				ValidateOwnership();
				InvalidateProperties();
			}
		}

        public bool ForcePrivate
        { 
            get { return c_ForcePrivate; }
            set
            { 
                c_ForcePrivate = value;

                if (value)
                {
                    c_ForcePublic = false;

                    if (c_House != null)
                        c_House.Public = false;
                }
            } 
        }
        
        public bool ForcePublic
        { 
            get { return c_ForcePublic; }
            set
            { 
                c_ForcePublic = value;

                if (value)
                {
                    c_ForcePrivate = false;

                    if (c_House != null)
                        c_House.Public = true;
                }
            }
        }

        public bool NoBanning
        { 
            get { return c_NoBanning; }
            set
            {
                c_NoBanning = value;

                if (value && c_House != null)
                    c_House.Bans.Clear();
            }
        }

        public ArrayList Blocks { get { return c_Blocks; } set { c_Blocks = value; } }
        public string Skill { get { return c_Skill; } set { c_Skill = value; ValidateOwnership(); InvalidateProperties(); } }
        public double SkillReq { get { return c_SkillReq; } set { c_SkillReq = value; ValidateOwnership(); InvalidateProperties(); } }
		public bool LeaveItems{ get{ return c_LeaveItems; } set{ c_LeaveItems = value; InvalidateProperties(); } }
		public bool RentToOwn{ get{ return c_RentToOwn; } set{ c_RentToOwn = value; InvalidateProperties(); } }
        public bool Relock { get { return c_Relock; } set { c_Relock = value; } }
        public bool NoTrade { get { return c_NoTrade; } set { c_NoTrade = value; } }
        public int ItemsPrice { get { return c_ItemsPrice; } set { c_ItemsPrice = value; InvalidateProperties(); } }
		public TownHouse House{ get{ return c_House; } set{ c_House = value; } }
		public Timer DemolishTimer{ get{ return c_DemolishTimer; } }
		public DateTime DemolishTime{ get{ return c_DemolishTime; } }

		public bool Owned{ get{ return c_House != null && !c_House.Deleted; } }
		public int Floors{ get{ return (c_MaxZ-c_MinZ)/20+1; } }

		public bool BlocksReady{ get{ return Blocks.Count != 0; } }
		public bool FloorsReady{ get{ return  BlocksReady && MinZ != short.MinValue ; } }
		public bool SignReady{ get{ return  FloorsReady && SignLoc != Point3D.Zero ; } }
		public bool BanReady{ get{ return SignReady; } }
		public bool LocSecReady{ get{ return SignReady; } }
		public bool ItemsReady{ get{ return LocSecReady; } }
		public bool LengthReady{ get{ return ItemsReady; } }
		public bool PriceReady{ get{ return  LengthReady && Price != 0 ; } }

        public bool IsTomb
        {
            get
            {
                return PropertyType == OSUPropertyType.Tomb;
            }
        }

        public string PriceType
		{
			get
			{
				if ( c_RentByTime == TimeSpan.Zero )
					return "Sale";
				if ( c_RentByTime == TimeSpan.FromDays( 1 ) )
					return "Daily";
				if ( c_RentByTime == TimeSpan.FromDays( 7 ) )
					return "Weekly";
				if ( c_RentByTime == TimeSpan.FromDays( 30 ) )
					return "Monthly";

				return "Sale";
			}
		}

		public string PriceTypeShort
		{
			get
			{
				if ( c_RentByTime == TimeSpan.Zero )
					return "Sale";
				if ( c_RentByTime == TimeSpan.FromDays( 1 ) )
					return "Day";
				if ( c_RentByTime == TimeSpan.FromDays( 7 ) )
					return "Week";
				if ( c_RentByTime == TimeSpan.FromDays( 30 ) )
					return "Month";

				return "Sale";
			}
		}

		[Constructable]
		public TownHouseSign() : base( 0x0BD2 )
		{
			Name = "This building is for rent!";
			Movable = false;

			c_BanLoc = Point3D.Zero;
			c_SignLoc = Point3D.Zero;
			c_Skill = "";
			c_Blocks = new ArrayList();
			c_DecoreItemInfos = new ArrayList();
			c_PreviewItems = new ArrayList();
			c_DemolishTime = DateTime.Now;
			c_RentTime = DateTime.Now;
			c_RentByTime = TimeSpan.Zero;
			c_RecurRent = true;
            m_CitizenCityId = String.Empty;
            m_AllowedCulturesCsv = "Todos";
            m_GovernmentManaged = false;
            m_GovernmentCityId = -1;
            m_GovernorConfigured = true;

            m_PropertyType = OSUPropertyType.House;
			m_AllowedCulture = "Todos";

			c_MinZ = short.MinValue;
			c_MaxZ = short.MaxValue;

			s_TownHouseSigns.Add( this );
		}


		public int GetSignItemID()
		{
			if (PropertyType == OSUPropertyType.Tomb)
				return Flip ? 0x1166 : 0x1165;

			return Flip ? 0x0BD1 : 0x0BD2;
		}

		public void UpdateSignItem()
		{
			UpdateSignItem( Flip );
		}

		public void UpdateSignItem(bool south)
		{
			m_Flip = south;
			ItemID = GetSignItemID();
		}

		private void SearchForHouse()
		{
			foreach( TownHouse house in TownHouse.AllTownHouses )
				if (house.ForSaleSign == this )
					c_House = house;
		}

		public void UpdateBlocks()
		{
			if ( !Owned )
				return;

            if (c_Blocks.Count == 0)
				UnconvertDoors();

            RUOVersion.UpdateRegion(this);
            ConvertItems(false);
			c_House.InitSectorDefinition();
		}

		public void ShowAreaPreview( Mobile m )
		{
			ClearPreview();

			Point2D point = Point2D.Zero;
			ArrayList blocks = new ArrayList();

			foreach( Rectangle2D rect in c_Blocks )
				for( int x = rect.Start.X; x < rect.End.X; ++x )
					for( int y = rect.Start.Y; y < rect.End.Y; ++y )
					{
						point = new Point2D( x, y );
						if ( !blocks.Contains( point ) )
							blocks.Add( point );
					}

            if (blocks.Count > 500)
            {
                m.SendMessage("Due to size of the area, skipping the preview.");
                return;
            }

			Item item = null;
            int avgz = 0;
			foreach( Point2D p in blocks )
			{
                avgz = Map.GetAverageZ(p.X, p.Y);

				item = new Item( 0x1766 );
				item.Name = "Area Preview";
				item.Movable = false;
				item.Location = new Point3D( p.X, p.Y, avgz <= m.Z ? m.Z+2 : avgz+2  );
				item.Map = Map;

				c_PreviewItems.Add( item );
			}

			c_PreviewTimer = Timer.DelayCall( TimeSpan.FromSeconds( 100 ), new TimerCallback( ClearPreview ) );
		}

		public void ShowSignPreview()
		{
			ClearPreview();

			Item sign = new Item( GetSignItemID() );
			sign.Name = "Sign Preview";
			sign.Movable = false;
			sign.Location = SignLoc;
			sign.Map = Map;

			c_PreviewItems.Add( sign );

			/*sign = new Item( 0xB98 );
			sign.Name = "Sign Preview";
			sign.Movable = false;
			sign.Location = SignLoc;
			sign.Map = Map;

			c_PreviewItems.Add( sign ); useless hanger*/

			c_PreviewTimer = Timer.DelayCall( TimeSpan.FromSeconds( 100 ), new TimerCallback( ClearPreview ) );
		}

		public void ShowBanPreview()
		{
			ClearPreview();

			Item ban = new Item( 0x17EE );
			ban.Name = "Ban Loc Preview";
			ban.Movable = false;
			ban.Location = BanLoc;
			ban.Map = Map;

			c_PreviewItems.Add( ban );

			c_PreviewTimer = Timer.DelayCall( TimeSpan.FromSeconds( 100 ), new TimerCallback( ClearPreview ) );
		}

        public void ShowFloorsPreview(Mobile m)
        {
            ClearPreview();

            Item item = new Item(0x7BD);
            item.Name = "Bottom Floor Preview";
            item.Movable = false;
            item.Location = m.Location;
            item.Z = c_MinZ;
            item.Map = Map;

            c_PreviewItems.Add(item);

            item = new Item(0x7BD);
            item.Name = "Top Floor Preview";
            item.Movable = false;
            item.Location = m.Location;
            item.Z = c_MaxZ;
            item.Map = Map;

            c_PreviewItems.Add(item);

            c_PreviewTimer = Timer.DelayCall(TimeSpan.FromSeconds(100), new TimerCallback(ClearPreview));
        }

        public void ClearPreview()
		{
			foreach( Item item in new ArrayList( c_PreviewItems ) )
			{
				c_PreviewItems.Remove( item );
				item.Delete();
			}

			if ( c_PreviewTimer != null )
				c_PreviewTimer.Stop();

			c_PreviewTimer = null;
		}

		public void Purchase( Mobile m )
		{
            Purchase( m, false );
		}

		public void Purchase( Mobile m, bool sellitems )
		{
            if (IsTomb)
            {
                TombDeadName = "";
                TombBirthYear = "";
                TombDeathYear = "";
                TombMessage = "";
                TombFinalized = false;

                if (TombSelectedItemID > 0)
                    ItemID = TombSelectedItemID;
            }

            try
            {
                if (Owned)
                {
                    m.SendMessage("Someone already owns this house!");
                    return;
                }

                if (!CanOwnThisProperty(m))
                {
                    m.SendMessage(CannotOwnMessage(m));
                    return;
                }

                if (!PriceReady)
                {
                    m.SendMessage("The setup for this house is not yet complete.");
                    return;
                }

                if (IsTomb)
                {
                    m_TombDeadName = "";
                    m_TombBirthYear = "";
                    m_TombDeathYear = "";
                    m_TombMessage = "";
                    m_TombFinalized = false;

                    if (m_TombSelectedItemID > 0)
                        ItemID = m_TombSelectedItemID;
                }

                int price = c_Price + (sellitems ? c_ItemsPrice : 0);

                if (c_Free)
                    price = 0;

                if (m.AccessLevel == AccessLevel.Player && !Banker.Withdraw(m, price))
                {
                    m.SendMessage("You cannot afford this house.");
                    return;
                }

                if (m.AccessLevel == AccessLevel.Player)
                {
                    m.SendMessage( "Um total de " + price.ToString() + " moedas de outro foi retirado do seu banco." );
					m.SendMessage("Para trancar as portas, clique em casa uma delas e defina as permissões.");
                    OnRentPaid();
                }

                if (IsTomb)
                    Visible = true;
                else
                    Visible = false;

                int minX = ((Rectangle2D)c_Blocks[0]).Start.X;
                int minY = ((Rectangle2D)c_Blocks[0]).Start.Y;
                int maxX = ((Rectangle2D)c_Blocks[0]).End.X;
                int maxY = ((Rectangle2D)c_Blocks[0]).End.Y;

                foreach (Rectangle2D rect in c_Blocks)
                {
                    if (rect.Start.X < minX)
                        minX = rect.Start.X;
                    if (rect.Start.Y < minY)
                        minY = rect.Start.Y;
                    if (rect.End.X > maxX)
                        maxX = rect.End.X;
                    if (rect.End.Y > maxY)
                        maxY = rect.End.Y;
                }

                c_House = new TownHouse(m, this, c_Locks, c_Secures);

                c_House.Components.Resize( maxX-minX, maxY-minY );
                c_House.Components.Add( 0x520, c_House.Components.Width-1, c_House.Components.Height-1, -5 );

                c_House.Location = new Point3D(minX, minY, Map.GetAverageZ(minX, minY));
                c_House.Map = Map;
                c_House.Region.GoLocation = c_BanLoc;
                c_House.Sign.Location = c_SignLoc;
              //  c_House.Sign.OriginalName = m.Name;
              //  c_House.Sign.TownHouseSign = this;
                //c_House.Hanger = new Item(0xB98);
                //c_House.Hanger.Location = c_SignLoc;
                //c_House.Hanger.Map = Map;
                //c_House.Hanger.Movable = false;

                if (c_ForcePublic)
                    c_House.Public = true;

                c_House.Price = RentByTime == TimeSpan.FromDays(0) ? c_Price : 1;

                RUOVersion.UpdateRegion(this);

                if (c_House.Price == 0)
                    c_House.Price = 1;

                if (c_RentByTime != TimeSpan.Zero)
                    BeginRentTimer(c_RentByTime);

                c_RTOPayments = 1;

                HideOtherSigns();

                c_DecoreItemInfos = new ArrayList();

                ConvertItems(sellitems);

                if (IsTomb)
                {
                    // a própria lápide continua sendo o item clicável
                    Location = c_SignLoc;
                    Map = Map;
                    Visible = true;

                    if (c_House.Sign != null && !c_House.Sign.Deleted)
                        c_House.Sign.Visible = false;
                }
                else
                {
                    c_House.Sign.ItemID = GetSignItemID();
                }
            }
            catch(Exception e)
            {
                Errors.Report(string.Format("An error occurred during home purchasing.  More information available on the console."));
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static string NormalizeCulturesCsv(string csv)
        {
            if (String.IsNullOrWhiteSpace(csv))
                return "Todos";

            string[] parts = csv.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (part.Length == 0)
                    continue;
                if (String.Equals(part, "Todos", StringComparison.OrdinalIgnoreCase))
                    return "Todos";
                if (!list.Exists(s => String.Equals(s, part, StringComparison.OrdinalIgnoreCase)))
                    list.Add(part);
            }

            return list.Count == 0 ? "Todos" : String.Join(",", list.ToArray());
        }

        public static bool ContainsCulture(string csv, string culture)
        {
            csv = NormalizeCulturesCsv(csv);
            if (String.Equals(csv, "Todos", StringComparison.OrdinalIgnoreCase))
                return true;
            if (String.IsNullOrWhiteSpace(culture))
                return false;

            string[] parts = csv.Split(',');
            for (int i = 0; i < parts.Length; i++)
                if (String.Equals(parts[i].Trim(), culture, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public static string ToggleCulture(string csv, string culture)
        {
            csv = NormalizeCulturesCsv(csv);
            if (String.IsNullOrWhiteSpace(culture))
                return csv;
            if (String.Equals(culture, "Todos", StringComparison.OrdinalIgnoreCase))
                return "Todos";

            List<string> list = new List<string>();
            if (!String.Equals(csv, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = csv.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i].Trim();
                    if (part.Length > 0 && !list.Exists(s => String.Equals(s, part, StringComparison.OrdinalIgnoreCase)))
                        list.Add(part);
                }
            }

            int idx = list.FindIndex(s => String.Equals(s, culture, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                list.RemoveAt(idx);
            else
                list.Add(culture);

            return list.Count == 0 ? "Todos" : String.Join(",", list.ToArray());
        }

        public bool IsGovernmentManager(Mobile m)
        {
            if (!(m is PlayerMobile))
                return false;

            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (!m_GovernmentManaged)
                return false;

            return Server.Custom.Systems.Reinos.ReinoAccessHelper.HasGovernmentAccess((PlayerMobile)m, m_GovernmentCityId);
        }

        public bool IsCultureAllowed(Mobile m)
        {
            if (m == null)
                return false;

            PlayerMobile pm = m as PlayerMobile;
            if (pm == null)
                return false;

            string csv = NormalizeCulturesCsv(m_AllowedCulturesCsv);
            if (!String.IsNullOrWhiteSpace(csv) && !String.Equals(csv, "Todos", StringComparison.OrdinalIgnoreCase))
                return ContainsCulture(csv, pm.OSUCultureId);

            return String.IsNullOrWhiteSpace(m_AllowedCulture) || m_AllowedCulture == "Todos" || String.Equals(pm.OSUCultureId, m_AllowedCulture, StringComparison.OrdinalIgnoreCase);
        }

        private void HideOtherSigns()
		{
			foreach( Item item in c_House.Sign.GetItemsInRange( 0 ) )
				if ( !(item is HouseSign) )
					if ( item.ItemID == 0xB95
					|| item.ItemID == 0xB96
					|| item.ItemID == 0xC43
					|| item.ItemID == 0xC44
					||  item.ItemID > 0xBA3 && item.ItemID < 0xC0E  )
						item.Visible = false;
		}

		public virtual void ConvertItems( bool keep )
		{
			if ( c_House == null )
				return;

            ArrayList items = new ArrayList();
            foreach(Rectangle2D rect in c_Blocks)
                foreach (Item item in Map.GetItemsInBounds(rect))
                    if (c_House.Region.Contains(item.Location) && item.RootParent == null && !items.Contains(item))
                        items.Add(item);

            foreach (Item item in new ArrayList(items))
            {
                if (item is HouseSign
                || item is BaseMulti
                || item is BaseAddon
                || item is AddonComponent
                || item == this
                || item == c_House.Sign
                || item == c_House.Hanger
                || !item.Visible
                || item.IsLockedDown
                || item.IsSecure
                || item.Movable
                || c_PreviewItems.Contains(item))
                    continue;

                if (item is BaseDoor)
                    ConvertDoor((BaseDoor)item);
                
                else if (!c_LeaveItems)
                {
                    c_DecoreItemInfos.Add(new DecoreItemInfo(item.GetType().ToString(), item.Name, item.ItemID, item.Hue, item.Location, item.Map));
					item.Delete();
                }
            }
        }

		protected void ConvertDoor( BaseDoor door )
		{
			if ( !Owned )
				return;

			if ( door is ISecurable )
			{
				door.Locked = false;
				c_House.Doors.Add( door );
                return;
			}

			door.Open = false;

			GenericHouseDoor newdoor = new GenericHouseDoor( 0, door.ClosedID, door.OpenedSound, door.ClosedSound );
			newdoor.Offset = door.Offset;
			newdoor.ClosedID = door.ClosedID;
			newdoor.OpenedID = door.OpenedID;
			newdoor.Location = door.Location;
			newdoor.Map = door.Map;

			door.Delete();

			foreach( Item inneritem in newdoor.GetItemsInRange( 1 ) )
				if ( inneritem is BaseDoor && inneritem != newdoor && inneritem.Z == newdoor.Z )
				{
					((BaseDoor)inneritem).Link = newdoor;
					newdoor.Link = (BaseDoor)inneritem;
				}

            c_House.Doors.Add(newdoor);
        }
        public virtual void UnconvertDoors()
		{
			if ( c_House == null )
				return;

			BaseDoor newdoor = null;
            
            foreach (BaseDoor door in new ArrayList(c_House.Doors))
			{
                door.Open = false;

				if ( c_Relock )
					door.Locked = true;

				newdoor = new StrongWoodDoor( (DoorFacing)0 );
				newdoor.ItemID = door.ItemID;
				newdoor.ClosedID = door.ClosedID;
				newdoor.OpenedID = door.OpenedID;
				newdoor.OpenedSound = door.OpenedSound;
				newdoor.ClosedSound = door.ClosedSound;
				newdoor.Offset = door.Offset;
				newdoor.Location = door.Location;
				newdoor.Map = door.Map;

				door.Delete();

				foreach( Item inneritem in newdoor.GetItemsInRange( 1 ) )
					if ( inneritem is BaseDoor && inneritem != newdoor && inneritem.Z == newdoor.Z )
					{
						( (BaseDoor)inneritem ).Link = newdoor;
						newdoor.Link = (BaseDoor)inneritem;
					}

				c_House.Doors.Remove( door );
			}
		}

		public void RecreateItems()
		{
			Item item = null;
			foreach( DecoreItemInfo info in c_DecoreItemInfos )
			{
				item = null;

				if ( info.TypeString.ToLower().IndexOf( "static" ) != -1 )
					item = new Static( info.ItemID );
				else
				{
					try{
					item = Activator.CreateInstance( ScriptCompiler.FindTypeByFullName( info.TypeString ) ) as Item;
					}catch{ continue; }
				}

				if ( item == null )
					continue;

				item.ItemID = info.ItemID;
				item.Name = info.Name;
				item.Hue = info.Hue;
				item.Location = info.Location;
				item.Map = info.Map;
				item.Movable = false;
			}
		}

		public virtual void ClearHouse()
		{

            if (IsTomb)
            {
                if (c_House == null)
                    return;

                ArrayList list = new ArrayList();

                foreach (Item item in new ArrayList(c_House.LockDowns.Keys))
                {
                    if (item != null && !item.Deleted && !list.Contains(item))
                        list.Add(item);
                }

                foreach (SecureInfo info in new ArrayList(c_House.Secures))
                {
                    if (info != null && info.Item != null && !info.Item.Deleted && !list.Contains(info.Item))
                        list.Add(info.Item);
                }

                foreach (Rectangle2D rect in c_Blocks)
                {
                    ArrayList l = new ArrayList();

                    foreach (Item item in Map.GetItemsInBounds(rect))
                        l.Add(item);

                    foreach (Item item in l)
                    {
                        if (item is HouseSign
                        || item is BaseDoor
                        || item is BaseMulti
                        || item is BaseAddon
                        || item is AddonComponent
                        || item == this
                        || item == c_House.Sign
                        || item == c_House.Hanger
                        || item.Map != c_House.Map
                        || !c_House.Region.Contains(item.Location)
                        || !item.Visible)
                            continue;

                        if (!list.Contains(item))
                            list.Add(item);
                    }
                }

                foreach (Item item in list)
                {
                    try
                    {
                        if (item != null && !item.Deleted)
                            item.Delete();
                    }
                    catch
                    {
                    }
                }

                return;
            }

            UnconvertDoors();
			ClearDemolishTimer();
			ClearRentTimer();
			PackUpItems();
			c_House = null;
			Visible = true;

			if ( c_RentToOwn )
				c_RentByTime = c_OriginalRentTime;
		}

		public virtual void ValidateOwnership()
		{
			if ( !Owned )
				return;

			if ( c_House.Owner == null )
			{
				c_House.Delete();
				return;
			}

			if ( c_House.Owner.AccessLevel != AccessLevel.Player )
				return;

			if ( !CanBuyHouse( c_House.Owner ) && c_DemolishTimer == null )
				BeginDemolishTimer();
			else
				ClearDemolishTimer();
		}

		public int CalcVolume()
		{
			int floors = 1;
			if ( c_MaxZ - c_MinZ < 100 )
				floors = 1 + Math.Abs( (c_MaxZ - c_MinZ)/20 );

			Point3D point = Point3D.Zero;
			ArrayList blocks = new ArrayList();

			foreach( Rectangle2D rect in c_Blocks )
				for( int x = rect.Start.X; x < rect.End.X; ++x )
					for( int y = rect.Start.Y; y < rect.End.Y; ++y )
						for( int z = 0; z < floors; z++ )
						{
							point = new Point3D( x, y, z );
							if ( !blocks.Contains( point ) )
								blocks.Add( point );
						}
			return blocks.Count;
		}

        private void StartTimers()
        {
            if (c_DemolishTime > DateTime.Now)
                BeginDemolishTimer(c_DemolishTime - DateTime.Now);
            //else if (c_RentByTime != TimeSpan.Zero)
                //BeginRentTimer(c_RentByTime);
            
            else if ( c_RentTime > DateTime.Now )
				BeginRentTimer( c_RentTime-DateTime.Now );
        }

		#region Demolish

		public void ClearDemolishTimer()
		{
			if ( c_DemolishTimer == null )
				return;

			c_DemolishTimer.Stop();
			c_DemolishTimer = null;
			c_DemolishTime = DateTime.Now;

			if ( !c_House.Deleted && Owned )
				c_House.Owner.SendMessage( "Demolition canceled." );
		}

		public void CheckDemolishTimer()
		{
			if ( c_DemolishTimer == null || !Owned )
				return;

			DemolishAlert();
		}

		protected void BeginDemolishTimer()
		{
			BeginDemolishTimer( TimeSpan.FromHours( 24 ) );
		}

		protected void BeginDemolishTimer( TimeSpan time )
		{
			if ( !Owned )
				return;

			c_DemolishTime = DateTime.Now + time;
			c_DemolishTimer = Timer.DelayCall( time, new TimerCallback( PackUpHouse ) );

			DemolishAlert();
		}

		protected virtual void DemolishAlert()
		{
			c_House.Owner.SendMessage( "You no longer meet the requirements for your town house, which will be demolished automatically in {0}:{1}:{2}.", (c_DemolishTime-DateTime.Now).Hours, (c_DemolishTime-DateTime.Now).Minutes, (c_DemolishTime-DateTime.Now).Seconds );
		}

		protected void PackUpHouse()
		{
			if ( !Owned || c_House.Deleted )
				return;

			PackUpItems();

            try
            {
                c_House.Delete();
            }
            catch
            {
                Errors.Report("The infamous SVN bug has occured.");
            }

		}

		protected void PackUpItems()
		{
			if ( c_House == null )
				return;

			ArrayList list = new ArrayList();

            foreach (Item item in new ArrayList(c_House.LockDowns.Keys))
            {
                item.IsLockedDown = false;
                item.Movable = true;
                c_House.LockDowns.Remove(item);
                list.Add(item);
            }

            foreach ( SecureInfo info in new ArrayList( c_House.Secures ) )
			{
				info.Item.IsLockedDown = false;
				info.Item.IsSecure = false;
				info.Item.Movable = true;
				info.Item.SetLastMoved();
				c_House.Secures.Remove( info );
				list.Add( info.Item );
			}

            foreach (Rectangle2D rect in c_Blocks)
            {
                ArrayList l = new ArrayList();
                foreach (Item item in Map.GetItemsInBounds(rect))
                    l.Add(item);

                foreach (Item item in l)
                {
                    if (item is HouseSign
                    || item is BaseDoor
                    || item is BaseMulti
                    || item is BaseAddon
                    || item is AddonComponent
                    || !item.Visible
                    || item.IsLockedDown
                    || item.IsSecure
                    || !item.Movable
                    || item.Map != c_House.Map
                    || !c_House.Region.Contains(item.Location))
                        continue;

                    list.Add( item );
                }
            }

            Mobile owner = c_House.Owner;
            Container bank = owner != null ? owner.BankBox : null;
            Container pack = owner != null ? owner.Backpack : null;

            int movedToBank = 0;
            int movedToPack = 0;
            int droppedAtFeet = 0;

            Bag recoveryBag = null;

            for (int i = 0; i < list.Count; i++)
            {
                Item item = list[i] as Item;

                if (item == null || item.Deleted)
                    continue;

                try
                {
                    item.Movable = true;
                    item.IsLockedDown = false;
                    item.IsSecure = false;

                    bool placed = false;

                    // tenta criar/usar uma bag de recuperação dentro do banco
                    if (bank != null)
                    {
                        if (recoveryBag == null || recoveryBag.Deleted || recoveryBag.Parent != bank || recoveryBag.TotalItems >= 120)
                        {
                            recoveryBag = new Bag();
                            recoveryBag.Name = "House Recovery";

                            if (!bank.TryDropItem(owner, recoveryBag, false))
                            {
                                recoveryBag.Delete();
                                recoveryBag = null;
                            }
                        }

                        if (recoveryBag != null)
                        {
                            recoveryBag.DropItem(item);
                            movedToBank++;
                            placed = true;
                        }
                    }

                    // se não conseguiu ir pro banco, tenta mochila
                    if (!placed && pack != null)
                    {
                        if (pack.TryDropItem(owner, item, false))
                        {
                            movedToPack++;
                            placed = true;
                        }
                    }

                    // se não conseguiu nem banco nem mochila, joga no chão aos pés do dono
                    if (!placed && owner != null && owner.Map != null)
                    {
                        item.MoveToWorld(owner.Location, owner.Map);
                        droppedAtFeet++;
                        placed = true;
                    }

                    // último recurso: se nem dono/map existir, apaga
                    if (!placed)
                        item.Delete();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (owner != null)
            {
                if (movedToBank > 0)
                    owner.SendMessage("{0} item(ns) da casa foram enviados para o seu banco.", movedToBank);

                if (movedToPack > 0)
                    owner.SendMessage("{0} item(ns) da casa foram enviados para a sua mochila.", movedToPack);

                if (droppedAtFeet > 0)
                    owner.SendMessage("{0} item(ns) da casa foram colocados aos seus pés porque não couberam no banco/mochila.", droppedAtFeet);
            }
        }

		#endregion

		#region Rent

		public void ClearRentTimer()
		{
			if ( c_RentTimer != null )
			{
				c_RentTimer.Stop();
				c_RentTimer = null;
			}

			c_RentTime = DateTime.Now;
		}

		private void BeginRentTimer()
		{
			BeginRentTimer( TimeSpan.FromDays( 1 ) );
		}

		private void BeginRentTimer( TimeSpan time )
		{
			if ( !Owned )
				return;

			c_RentTimer = Timer.DelayCall( time, new TimerCallback( RentDue ) );
			c_RentTime = DateTime.Now + time;
		}

		public void CheckRentTimer( Mobile from )
		{
			if ( c_RentTimer == null || !Owned )
				return;

			from.SendMessage( "This rent cycle ends in {0} days, {1}:{2}:{3}. At that occasion, {4} copper pieces will be withdrawn from your bank account.", (c_RentTime-DateTime.Now).Days, (c_RentTime-DateTime.Now).Hours, (c_RentTime-DateTime.Now).Minutes, (c_RentTime-DateTime.Now).Seconds, c_Price );
		}

		private void RentDue()
		{
			if ( this == null || !Owned || c_House.Owner == null )
				return;
			
			if ( !c_RecurRent )
			{
				c_House.Owner.SendMessage( "Your town house rental contract has expired, and the bank has once again taken possession." );
				PackUpHouse();
				return;
			}

			if ( !c_Free && c_House.Owner.AccessLevel == AccessLevel.Player && !Banker.Withdraw( c_House.Owner, c_Price ) )
			{
				c_House.Owner.SendMessage( "Since you can not afford the rent, the bank has reclaimed your town house." );
				PackUpHouse();
				return;
			}

			if ( !c_Free )
				c_House.Owner.SendMessage( "The bank has withdrawn {0} copper rent for your town house.", c_Price );

			OnRentPaid();

			if ( c_RentToOwn )
			{
				c_RTOPayments++;

				bool complete = false;

				if ( c_RentByTime == TimeSpan.FromDays( 1 ) && c_RTOPayments >= 60 )
				{
					complete = true;
					c_House.Price = c_Price*60;
				}

				if ( c_RentByTime == TimeSpan.FromDays( 7 ) && c_RTOPayments >= 9 )
				{
					complete = true;
					c_House.Price = c_Price*9;
				}

				if ( c_RentByTime == TimeSpan.FromDays( 30 ) && c_RTOPayments >= 2 )
				{
					complete = true;
					c_House.Price = c_Price*2;
				}

				if ( complete )
				{
					c_House.Owner.SendMessage( "You now own your rental home." );
					c_RentByTime = TimeSpan.FromDays( 0 );
					return;
				}
			}

			BeginRentTimer( c_RentByTime );
		}

		//GOVERNMENT
		protected virtual void OnRentPaid()
		{
			//if( AssignTreasury() )
			//{
			//	Copper copper = new Copper();
			//	copper.Amount = Price;
			//	
			//	if( Treasury is BaseContainer )
			//		( (BaseContainer)Treasury ).DropAndStack( copper );
			//}
		}
		
		//public bool AssignTreasury()
	//	{
           // foreach (Treasury t in Server.Items.Treasury.Treasuries)
           // {
               // if (t.Nation == Nation)
               // {
               //     Treasury = t;
               //     break;
               // }
           // }

		//	return Treasury != null;
		//}

		public void NextPriceType()
		{
			if ( m_PropertyType == OSUPropertyType.House )
			{
				if ( c_RentByTime == TimeSpan.Zero )
					RentByTime = TimeSpan.FromDays( 1 );
				else if ( c_RentByTime == TimeSpan.FromDays( 1 ) )
					RentByTime = TimeSpan.FromDays( 7 );
				else if ( c_RentByTime == TimeSpan.FromDays( 7 ) )
					RentByTime = TimeSpan.FromDays( 30 );
				else
					RentByTime = TimeSpan.Zero;
			}
			else
			{
				if ( c_RentByTime == TimeSpan.Zero )
					RentByTime = TimeSpan.FromDays( 1 );
				else if ( c_RentByTime == TimeSpan.FromDays( 1 ) )
					RentByTime = TimeSpan.FromDays( 7 );
				else if ( c_RentByTime == TimeSpan.FromDays( 7 ) )
					RentByTime = TimeSpan.FromDays( 30 );
				else
					RentByTime = TimeSpan.FromDays( 1 );
			}
		}

		public void PrevPriceType()
		{
			if ( m_PropertyType == OSUPropertyType.House )
			{
				if ( c_RentByTime == TimeSpan.Zero )
					RentByTime = TimeSpan.FromDays( 30 );
				else if ( c_RentByTime == TimeSpan.FromDays( 30 ) )
					RentByTime = TimeSpan.FromDays( 7 );
				else if ( c_RentByTime == TimeSpan.FromDays( 7 ) )
					RentByTime = TimeSpan.FromDays( 1 );
				else
					RentByTime = TimeSpan.Zero;
			}
			else
			{
				if ( c_RentByTime == TimeSpan.Zero )
					RentByTime = TimeSpan.FromDays( 30 );
				else if ( c_RentByTime == TimeSpan.FromDays( 30 ) )
					RentByTime = TimeSpan.FromDays( 7 );
				else if ( c_RentByTime == TimeSpan.FromDays( 7 ) )
					RentByTime = TimeSpan.FromDays( 1 );
				else
					RentByTime = TimeSpan.FromDays( 30 );
			}
		}

		#endregion

		public static bool HasOwnedPropertyType(Mobile m, OSUPropertyType type)
		{
			if (m == null)
				return false;

			foreach (TownHouse house in TownHouse.AllTownHouses)
			{
				if (house == null || house.Deleted)
					continue;

				TownHouseSign sign = house.ForSaleSign;

				if (sign == null || sign.Deleted)
					continue;

				if (house.Owner == m && sign.PropertyType == type)
					return true;
			}

			return false;
		}

		public bool CanOwnThisProperty(Mobile m)
		{
			if (m == null)
				return false;

			switch (PropertyType)
			{
				case OSUPropertyType.House:
					return !HasOwnedPropertyType(m, OSUPropertyType.House);
				case OSUPropertyType.Commercial:
					return !HasOwnedPropertyType(m, OSUPropertyType.Commercial);
				case OSUPropertyType.Tomb:
					return true;
				default:
					return true;
			}
		}

		public string CannotOwnMessage(Mobile m)
		{
			switch (PropertyType)
			{
				case OSUPropertyType.House:
					return "Você já possui uma casa.";
				case OSUPropertyType.Commercial:
					return "Você já possui uma casa comercial.";
				case OSUPropertyType.Tomb:
					return string.Empty;
				default:
					return "Você já possui uma propriedade deste tipo.";
			}
		}

		public bool CanBuyHouse( Mobile m )
		{
			if ( c_Skill != "" )
			{
				try
				{
					SkillName index = (SkillName)Enum.Parse( typeof( SkillName ), c_Skill, true );
					if ( m.Skills[index].Value < c_SkillReq )
						return false;
				}
				catch
				{
					return false;
				}
			}

			if ( c_MinTotalSkill != 0 && m.SkillsTotal/10 < c_MinTotalSkill )
				return false;

			if ( c_MaxTotalSkill != 0 && m.SkillsTotal/10 > c_MaxTotalSkill )
				return false;

			if ( c_YoungOnly && m.Player && !((PlayerMobile)m).Young )
				return false;

			if ( c_Murderers == Intu.Yes && m.Kills < 5 )
				return false;

			if ( c_Murderers == Intu.No && m.Kills >= 5 )
				return false;

			return true;
		}


        public TombstoneDefinition TombSelectedDefinition
        {
            get
            {
                if (m_TombSelectedItemID <= 0)
                    return null;

                return TombstoneRegistry.Find(m_TombSelectedItemID);
            }
        }

        public List<TombstoneDefinition> GetAvailableTombDefinitions()
        {
            return TombstoneRegistry.GetByBaseItemID(GetSignItemID());
        }

        public int GetTombInitialCost()
        {
            return c_Price + m_TombExtraCost;
        }

        public int GetTombWeeklyRent()
        {
            return c_Price;
        }

        public void RentTomb(Mobile from, TombstoneDefinition def)
        {
            if (def == null)
                return;

            m_TombSelectedItemID = def.ItemID;
            m_TombSelectedGumpID = def.GumpID;
            m_TombExtraCost = def.ExtraCost;

            m_TombDeadName = "";
            m_TombBirthYear = "";
            m_TombDeathYear = "";
            m_TombMessage = "";
            m_TombFinalized = false;

            ItemID = def.ItemID;

            Purchase(from, false);

            PropertyType = OSUPropertyType.Tomb;

            if (c_House != null && c_House.IsOwner(from) && !m_TombFinalized)
            {
                from.CloseGump(typeof(CemeteryManageGump));
                from.SendGump(new CemeteryManageGump(from, this));
            }
        }

        public void FinalizeTomb(string deadName, string birthYear, string deathYear, string message)
        {
            m_TombDeadName = deadName;
            m_TombBirthYear = birthYear;
            m_TombDeathYear = deathYear;
            m_TombMessage = message;
            m_TombFinalized = true;

            InvalidateProperties();
        }

        public void ResetTombState()
        {
            m_TombDeadName = "";
            m_TombBirthYear = "";
            m_TombDeathYear = "";
            m_TombMessage = "";
            m_TombFinalized = false;
            m_TombExtraCost = 0;
            m_TombSelectedGumpID = 0;

            if (TombstoneRegistry.IsEastFamily(ItemID))
                ItemID = 0x1165;
            else
                ItemID = 0x1166;

            m_TombSelectedItemID = ItemID;

            if (c_House != null && !c_House.Deleted)
            {
                try
                {
                    c_House.Public = true;
                }
                catch
                {
                }
            }

            Visible = true;
            InvalidateProperties();
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            if (IsTomb && !string.IsNullOrEmpty(m_TombDeadName))
            {
                TombstoneDefinition def = TombSelectedDefinition;

                string deadName = FitTombText(m_TombDeadName, def != null ? def.MaxNameLength : 30);
                list.Add(deadName);
                return;
            }

            base.AddNameProperties(list);
        }

        public string FitTombText(string text, int max)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            if (max <= 0)
                return "";

            text = text.Trim();

            if (text.Length > max)
                text = text.Substring(0, max);

            return text;
        }

        public string GetTombDisplayName()
        {
            TombstoneDefinition def = TombSelectedDefinition;
            return FitTombText(m_TombDeadName, def != null ? def.MaxNameLength : 30);
        }

        public string GetTombDisplayBirthYear()
        {
            TombstoneDefinition def = TombSelectedDefinition;
            return FitTombText(m_TombBirthYear, def != null ? def.MaxDateLength : 4);
        }

        public string GetTombDisplayDeathYear()
        {
            TombstoneDefinition def = TombSelectedDefinition;
            return FitTombText(m_TombDeathYear, def != null ? def.MaxDateLength : 4);
        }

        public string GetTombDisplayMessage()
        {
            TombstoneDefinition def = TombSelectedDefinition;
            return FitTombText(m_TombMessage, def != null ? def.MaxMessageLength : 40);
        }

        public string GetTombDisplayDate()
        {
            TombstoneDefinition def = TombSelectedDefinition;

            if (def == null || !def.HasDate)
                return "";

            string birth = GetTombDisplayBirthYear();
            string death = GetTombDisplayDeathYear();

            if (def.DateLayout == TombstoneDateLayout.Stacked)
                return birth + "<BR>" + death;

            return birth + " - " + death;
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m == null)
                return;

            if (m.AccessLevel != AccessLevel.Player)
            {
                new TownHouseSetupGump(m, this, false);

                if (Owned && c_House != null && c_House.Owner == m)
                    m.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Information, m, c_House));

                return;
            }

            if (!Visible)
                return;

            if (!m.InRange(GetWorldLocation(), 2))
            {
                m.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            if (m_GovernmentManaged && IsGovernmentManager(m))
            {
                new TownHouseSetupGump(m, this, true);
                new TownHouseConfirmGump(m, this);
                return;
            }

            if (m_GovernmentManaged && !m_GovernorConfigured)
            {
                m.SendMessage("Este imóvel ainda não foi liberado pelo governo.");
                return;
            }

            if (IsTomb)
            {
                if (!IsCultureAllowed(m))
                {
                    m.SendMessage("Você só pode alugar tumbas no cemitério de sua própria cultura.");
                    return;
                }

                if (!Owned || c_House == null)
                {
                    m.CloseGump(typeof(CemeteryRentGump));
                    m.SendGump(new CemeteryRentGump(m, this, 0, false));
                    return;
                }

                if (c_House.IsOwner(m))
                {
                    m.CloseGump(typeof(CemeteryRentGump));
                    m.SendGump(new CemeteryRentGump(m, this, 0, true));
                    return;
                }

                m.CloseGump(typeof(CemeteryPreviewGump));
                m.SendGump(new CemeteryPreviewGump(m, this));
                return;
            }

            if (!IsCultureAllowed(m))
                new TownHouseConfirmGump(m, this);
            else if (CanBuyHouse(m) && CanOwnThisProperty(m))
                new TownHouseConfirmGump(m, this);
            else if (!CanOwnThisProperty(m))
                m.SendMessage(CannotOwnMessage(m));
            else
                m.SendMessage("You cannot purchase this house.");
        }

        public override void Delete()
		{
			if ( c_House == null || c_House.Deleted )
				base.Delete();
			else
				PublicOverheadMessage( Network.MessageType.Regular, 0x0, true, "You cannot delete this while the home is owned." );

			if ( Deleted )
				s_TownHouseSigns.Remove( this );
		}

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (c_Free)
                list.Add(1060658, "Price\tFree");
            if (c_SkillReq != 0.0)
                list.Add(1060661, "Requires\t{0}", c_SkillReq + " in " + c_Skill);
            if (c_MinTotalSkill != 0)
                list.Add(1060662, "Requires more than\t{0} total skills", c_MinTotalSkill);
            if (c_MaxTotalSkill != 0)
                list.Add(1060663, "Requires less than\t{0} total skills", c_MaxTotalSkill);
            if (c_YoungOnly)
                list.Add(1063483, "Must be\tYoung");
            else if (c_Murderers == Intu.Yes)
                list.Add(1063483, "Must be\ta murderer");
            else if (c_Murderers == Intu.No)
                list.Add(1063483, "Must be\tinnocent");

            if (IsTomb)
            {
                TombstoneDefinition def = TombSelectedDefinition;

                if (def != null)
                {
                    if (def.HasDate)
                    {
                        string birth = GetTombDisplayBirthYear();
                        string death = GetTombDisplayDeathYear();

                        if (!string.IsNullOrEmpty(birth) || !string.IsNullOrEmpty(death))
                            list.Add("Data: " + birth + " - " + death);
                    }

                    if (def.HasMessage)
                    {
                        string msg = GetTombDisplayMessage();

                        if (!string.IsNullOrEmpty(msg))
                            list.Add("Mensagem: " + msg);
                    }
                }
            }
            else if (c_RentByTime == TimeSpan.Zero)
                list.Add(1060658, "Price\t{0}{1}", c_Price, "");
            else if (c_RecurRent)
                list.Add(1060658, "{0}\t{1}\r{2}", PriceType + (c_RentToOwn ? " Rent-to-Own" : " Tax"), c_Price, "");
            else
                list.Add(1060658, "One {0}\t{1}{2}", PriceTypeShort, c_Price, "");
        }

		public TownHouseSign( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( 21 );

			writer.Write(  m_Flip );
			writer.Write( (int)m_PropertyType );
			writer.Write( m_AllowedCulture );
            writer.Write((string)m_CitizenCityId);
            writer.Write(m_AllowedCulturesCsv ?? "Todos");
            writer.Write(m_GovernmentManaged);
            writer.Write(m_GovernmentCityId);
            writer.Write(m_GovernorConfigured);
            writer.Write(m_TombSelectedItemID);
            writer.Write(m_TombSelectedGumpID);
            writer.Write(m_TombExtraCost);
            writer.Write(m_TombDeadName);
            writer.Write(m_TombBirthYear);
            writer.Write(m_TombDeathYear);
            writer.Write(m_TombMessage);
            writer.Write(m_TombFinalized);
            //writer.Write( (int) m_Nation );
            writer.Write(  m_Treasury );
			
            // Version 13

            writer.Write(c_ForcePrivate);
            writer.Write(c_ForcePublic);
            writer.Write(c_NoTrade);

            // Version 12

			writer.Write( c_Free );

			// Version 11

			writer.Write( (int)c_Murderers );

			// Version 10

			writer.Write( c_LeaveItems );

			// Version 9
			writer.Write( c_RentToOwn );
			writer.Write( c_OriginalRentTime );
			writer.Write( c_RTOPayments );

			// Version 7
			writer.WriteItemList( c_PreviewItems, true );

			// Version 6
			writer.Write( c_ItemsPrice );
			writer.Write( c_KeepItems );

			// Version 5
			writer.Write( c_DecoreItemInfos.Count );
			foreach( DecoreItemInfo info in c_DecoreItemInfos )
				info.Save( writer );

			writer.Write( c_Relock );

			// Version 4
			writer.Write( c_RecurRent );
			writer.Write( c_RentByTime );
			writer.Write( c_RentTime );
			writer.Write( c_DemolishTime );
			writer.Write( c_YoungOnly );
			writer.Write( c_MinTotalSkill );
			writer.Write( c_MaxTotalSkill );

			// Version 3
			writer.Write( c_MinZ );
			writer.Write( c_MaxZ );

			// Version 2
			writer.Write( c_House );

			// Version 1
			writer.Write( c_Price );
			writer.Write( c_Locks );
			writer.Write( c_Secures );
			writer.Write( c_BanLoc );
			writer.Write( c_SignLoc );
			writer.Write( c_Skill );
			writer.Write( c_SkillReq );
			writer.Write( c_Blocks.Count );
			foreach( Rectangle2D rect in c_Blocks )
				writer.Write( rect );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

            int version = reader.ReadInt();

            m_CitizenCityId = String.Empty;
            m_Flip = false;
            m_PropertyType = OSUPropertyType.House;
            m_AllowedCulture = "Todos";
            m_AllowedCulturesCsv = "Todos";
            m_GovernmentManaged = false;
            m_GovernmentCityId = -1;
            m_GovernorConfigured = true;
            m_TombSelectedItemID = 0;
            m_TombSelectedGumpID = 0;
            m_TombExtraCost = 0;
            m_TombDeadName = "";
            m_TombBirthYear = "";
            m_TombDeathYear = "";
            m_TombMessage = "";
            m_TombFinalized = false;
            m_Treasury = null;

            // v21 em diante = formato correto novo com governo e múltiplos povos
            if (version >= 21)
            {
                m_Flip = reader.ReadBool();
                m_PropertyType = (OSUPropertyType)reader.ReadInt();
                m_AllowedCulture = reader.ReadString();
                m_CitizenCityId = reader.ReadString();
                m_AllowedCulturesCsv = reader.ReadString();
                m_GovernmentManaged = reader.ReadBool();
                m_GovernmentCityId = reader.ReadInt();
                m_GovernorConfigured = reader.ReadBool();

                if (version >= 18)
                {
                    m_TombSelectedItemID = reader.ReadInt();
                    m_TombSelectedGumpID = reader.ReadInt();
                    m_TombExtraCost = reader.ReadInt();
                    m_TombDeadName = reader.ReadString();
                    m_TombBirthYear = reader.ReadString();
                    m_TombDeathYear = reader.ReadString();
                    m_TombMessage = reader.ReadString();
                    m_TombFinalized = reader.ReadBool();
                }

                if (version >= 14)
                    m_Treasury = (Container)reader.ReadItem();
            }
            else if (version >= 20)
            {
                if (version >= 15)
                    m_Flip = reader.ReadBool();

                if (version >= 16)
                    m_PropertyType = (OSUPropertyType)reader.ReadInt();

                if (version >= 17)
                    m_AllowedCulture = reader.ReadString();

                m_CitizenCityId = reader.ReadString();

                if (version >= 18)
                {
                    m_TombSelectedItemID = reader.ReadInt();
                    m_TombSelectedGumpID = reader.ReadInt();
                    m_TombExtraCost = reader.ReadInt();
                    m_TombDeadName = reader.ReadString();
                    m_TombBirthYear = reader.ReadString();
                    m_TombDeathYear = reader.ReadString();
                    m_TombMessage = reader.ReadString();
                    m_TombFinalized = reader.ReadBool();
                }

                if (version >= 14)
                    m_Treasury = (Container)reader.ReadItem();
            }
            // v19 = formato salvo com a ordem bugada do patch anterior
            else if (version == 19)
            {
                m_Flip = reader.ReadBool();
                m_PropertyType = (OSUPropertyType)reader.ReadInt();
                m_AllowedCulture = reader.ReadString();
                m_CitizenCityId = reader.ReadString();

                m_TombSelectedItemID = reader.ReadInt();
                m_TombSelectedGumpID = reader.ReadInt();
                m_TombExtraCost = reader.ReadInt();
                m_TombDeadName = reader.ReadString();
                m_TombBirthYear = reader.ReadString();
                m_TombDeathYear = reader.ReadString();
                m_TombMessage = reader.ReadString();
                m_TombFinalized = reader.ReadBool();

                m_Treasury = (Container)reader.ReadItem();
            }
            // v18 ou menor = formato antigo
            else
            {
                if (version >= 15)
                    m_Flip = reader.ReadBool();

                if (version >= 16)
                    m_PropertyType = (OSUPropertyType)reader.ReadInt();

                if (version >= 17)
                    m_AllowedCulture = reader.ReadString();

                if (version >= 18)
                {
                    m_TombSelectedItemID = reader.ReadInt();
                    m_TombSelectedGumpID = reader.ReadInt();
                    m_TombExtraCost = reader.ReadInt();
                    m_TombDeadName = reader.ReadString();
                    m_TombBirthYear = reader.ReadString();
                    m_TombDeathYear = reader.ReadString();
                    m_TombMessage = reader.ReadString();
                    m_TombFinalized = reader.ReadBool();
                }

                if (version >= 14)
                    m_Treasury = (Container)reader.ReadItem();
            }

            m_AllowedCulturesCsv = NormalizeCulturesCsv(m_AllowedCulturesCsv);
            if (String.IsNullOrWhiteSpace(m_AllowedCulture))
                m_AllowedCulture = "Todos";

            if (version >= 13)
            {
                c_ForcePrivate = reader.ReadBool();
                c_ForcePublic = reader.ReadBool();
                c_NoTrade = reader.ReadBool();
            }
            
            if (version >= 12)
				c_Free = reader.ReadBool();

			if ( version >= 11 )
				c_Murderers = (Intu)reader.ReadInt();

			if ( version >= 10 )
				c_LeaveItems = reader.ReadBool();

			if ( version >= 9 )
			{
				c_RentToOwn = reader.ReadBool();
				c_OriginalRentTime = reader.ReadTimeSpan();
				c_RTOPayments = reader.ReadInt();
			}

			c_PreviewItems = new ArrayList();
			if ( version >= 7 )
				c_PreviewItems = reader.ReadItemList();

			if ( version >= 6 )
			{
				c_ItemsPrice = reader.ReadInt();
				c_KeepItems = reader.ReadBool();
			}

			c_DecoreItemInfos = new ArrayList();
			if ( version >= 5 )
			{
				int decorecount = reader.ReadInt();
				DecoreItemInfo info;
				for( int i = 0; i < decorecount; ++i )
				{
					info = new DecoreItemInfo();
					info.Load( reader );
					c_DecoreItemInfos.Add( info );
				}

				c_Relock = reader.ReadBool();
			}

			if ( version >= 4 )
			{
				c_RecurRent = reader.ReadBool();
				c_RentByTime = reader.ReadTimeSpan();
				c_RentTime = reader.ReadDateTime();
				c_DemolishTime = reader.ReadDateTime();
				c_YoungOnly = reader.ReadBool();
				c_MinTotalSkill = reader.ReadInt();
				c_MaxTotalSkill = reader.ReadInt();
			}

			if ( version >= 3 )
			{
				c_MinZ = reader.ReadInt();
				c_MaxZ = reader.ReadInt();
			}

			if ( version >= 2 )
				c_House = (TownHouse)reader.ReadItem();

			c_Price = reader.ReadInt();
			c_Locks = reader.ReadInt();
			c_Secures = reader.ReadInt();
			c_BanLoc = reader.ReadPoint3D();
			c_SignLoc = reader.ReadPoint3D();
			c_Skill = reader.ReadString();
			c_SkillReq = reader.ReadDouble();

			c_Blocks = new ArrayList();
			int count = reader.ReadInt();
			for ( int i = 0; i < count; ++i )
				c_Blocks.Add( reader.ReadRect2D() );

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(StartTimers));

			ClearPreview();

			s_TownHouseSigns.Add( this );
		}
	}
}
