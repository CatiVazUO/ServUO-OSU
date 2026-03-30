// Check PackUpHouse() for that crash on item delete.  It causes a crash in RemoveMulti (Core)

using System;
using System.Collections;
using Server;
using Server.Custom.Systems.Rent;
using Server.Multis;

namespace Server.Custom.Systems.Rent
{
	public class General
	{
		public static string Version{ get { return "2.01"; } }

		// This setting determines the suggested gold value for a single square of a home
		//  which then derives price, lockdowns and secures.
		public static int SuggestionFactor { get{ return 600; } }

		// This setting determines if players need License in order to rent out their property
		public static bool RequireRenterLicense{ get{ return false; } }

		public static void Configure()
		{
			EventSink.WorldSave += new WorldSaveEventHandler( OnSave );
		}

		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler( OnLogin );
			EventSink.Speech += new SpeechEventHandler( HandleSpeech );
			EventSink.ServerStarted += new ServerStartedEventHandler( OnStarted );
		}

        private static void OnStarted()
        {
            foreach (TownHouse house in TownHouse.AllTownHouses)
            {
                if (house == null || house.Deleted)
                    continue;

                try
                {
                    house.InitSectorDefinition();

                    if (house.ForSaleSign != null && !house.ForSaleSign.Deleted)
                        RUOVersion.UpdateRegion(house.ForSaleSign);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OSU.Rent.OnStarted: erro ao iniciar house: " + ex.Message);
                }
            }
        }

        public static void OnSave( WorldSaveEventArgs e )
		{
			foreach( TownHouseSign sign in new ArrayList( TownHouseSign.AllSigns ) )
				sign.ValidateOwnership();

			foreach( TownHouse house in new ArrayList( TownHouse.AllTownHouses ) )
				if ( house.Deleted )
				{
					TownHouse.AllTownHouses.Remove( house );
					continue;
				}
		}

        private static void OnLogin(LoginEventArgs e)
        {
            if (e == null || e.Mobile == null)
                return;

            ArrayList houses = new ArrayList(BaseHouse.GetHouses(e.Mobile));

            if (houses == null || houses.Count == 0)
                return;

            foreach (BaseHouse house in houses)
            {
                TownHouse th = house as TownHouse;

                if (th == null || th.Deleted)
                    continue;

                if (th.ForSaleSign == null || th.ForSaleSign.Deleted)
                    continue;

                try
                {
                    th.ForSaleSign.CheckDemolishTimer();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OSU.Rent.OnLogin: erro ao checar demolish timer: " + ex.Message);
                }
            }
        }

        private static void HandleSpeech( SpeechEventArgs e )
		{
			ArrayList houses = new ArrayList(BaseHouse.GetHouses( e.Mobile ));

			if ( houses == null )
				return;

			foreach( BaseHouse house in houses )
			{
                if (!RUOVersion.RegionContains(house.Region, e.Mobile))
                    continue;

				if ( house is TownHouse )
					house.OnSpeech( e );

				if ( house.Owner == e.Mobile
				 && e.Speech.ToLower() == "contrato de aluguel"
				 && CanRent( e.Mobile, house, true ) )
				{
					e.Mobile.AddToBackpack( new RentalContract() );
					e.Mobile.SendMessage("Um contrato de aluguel foi posto na sua mochila.");
				}

				if ( house.Owner == e.Mobile
				 && e.Speech.ToLower() == "checar espaço" )
				{
					int count = 0;

					e.Mobile.SendMessage( "Você tem {0} itens e {1} recipientes seguros faltando.", RemainingSecures( house ), RemainingLocks( house ) );

					if ( (count = AllRentalLocks( house )) != 0 )
						e.Mobile.SendMessage( "Os alugueis estão usando {0} dos seus itens.", count );
					if ( (count = AllRentalSecures( house )) != 0 )
						e.Mobile.SendMessage("Os alugueis estão usando {0} dos seus recipientes seguros.", count );
				}
			}
		}

		private static bool CanRent( Mobile m, BaseHouse house, bool say )
		{
			if ( house is TownHouse && ((TownHouse)house).ForSaleSign.PriceType != "Sale" )
			{
				if ( say )
					m.SendMessage( "Você tem que ser o proprietário para alugar" );

				return false;
			}

			if ( RequireRenterLicense )
			{
				RentalLicense lic = m.Backpack.FindItemByType( typeof( RentalLicense ) ) as RentalLicense;

				if ( lic != null && lic.Owner == null )
					lic.Owner = m;

				if ( lic == null || lic.Owner != m )
				{
					if ( say )
						m.SendMessage( "Você tem que ter uma licensa para alugar sua casa" );

					return false;
				}
			}

			if ( EntireHouseContracted( house ) )
			{
				if ( say )
					m.SendMessage( "A casa toda já tem contratos de aluguel" );

				return false;
			}

			if ( RemainingSecures( house ) < 0 || RemainingLocks( house ) < 0 )
			{
				if ( say )
					m.SendMessage( "Você não tem espaço suficiente par alugar essa casa" );

				return false;
			}

			return true;
		}

		#region Rental Info

		public static bool EntireHouseContracted( BaseHouse house )
		{
			foreach( Item item in TownHouseSign.AllSigns )
				if ( item is RentalContract && house == ((RentalContract)item).ParentHouse )
					if ( ((RentalContract)item).EntireHouse )
						return true;

			return false;
		}

		public static bool HasContract( BaseHouse house )
		{
			foreach( Item item in TownHouseSign.AllSigns )
				if ( item is RentalContract && house == ((RentalContract)item).ParentHouse )
					return true;

			return false;
		}

		public static bool HasOtherContract( BaseHouse house, RentalContract contract )
		{
			foreach( Item item in TownHouseSign.AllSigns )
				if ( item is RentalContract && item != contract && house == ((RentalContract)item).ParentHouse )
					return true;

			return false;
		}

		public static int RemainingSecures( BaseHouse house )
		{ 
			if ( house == null )
				return 0;

			int a, b, c, d;

			return (Core.AOS ? house.GetAosMaxSecures() - house.GetAosCurSecures( out a, out b, out c, out d ) : house.MaxSecures - house.SecureCount) - AllRentalSecures( house );
		}

		public static int RemainingLocks( BaseHouse house )
		{ 
			if ( house == null )
				return 0;

            if (house is TownHouse)
                return (house.MaxLockDowns - (house.GetLockdowns() + house.SecureCount)) - AllRentalLocks(house);

            return (Core.AOS ? house.GetAosMaxLockdowns() - house.GetAosCurLockdowns() : house.MaxLockDowns - house.LockDownCount) - AllRentalLocks(house);
        }

		public static int AllRentalSecures( BaseHouse house )
		{
			int count = 0;

			foreach( TownHouseSign sign in TownHouseSign.AllSigns )
				if ( sign is RentalContract && ((RentalContract)sign).ParentHouse == house )
					count+=sign.Secures;

			return count;
		}

		public static int AllRentalLocks( BaseHouse house )
		{
			int count = 0;

			foreach( TownHouseSign sign in TownHouseSign.AllSigns )
				if ( sign is RentalContract && ((RentalContract)sign).ParentHouse == house )
					count+=sign.Locks;

			return count;
		}

		#endregion
	}
}
