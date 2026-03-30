using System;
using System.Collections;
using Server.Multis;
using Server.Items;
using Server.Custom.Systems.Rent;

namespace Server.Custom.Systems.Rent
{
	public class RentalContract : TownHouseSign
	{
		private Mobile c_RentalMaster;
		private Mobile c_RentalClient;
		private BaseHouse c_ParentHouse;
		private bool c_Completed, c_EntireHouse;

		public BaseHouse ParentHouse{ get{ return c_ParentHouse; } }
		public Mobile RentalClient{ get{ return c_RentalClient; } set{ c_RentalClient = value; InvalidateProperties(); } }
		public Mobile RentalMaster{ get{ return c_RentalMaster; } }
		public bool Completed{ get{ return c_Completed; } set{ c_Completed = value; } }
		public bool EntireHouse{ get{ return c_EntireHouse; } set{ c_EntireHouse = value; } }


		[Constructable]
		public RentalContract() : base()
		{
			ItemID = 0x14F0;
			Movable = true;
			RentByTime = TimeSpan.FromDays( 1 );
			RecurRent = true;
			MaxZ = MinZ;
		}

		public bool HasContractedArea( Rectangle2D rect, int z )
		{
			foreach( Item item in AllSigns)
				if ( item is RentalContract && item != this && item.Map == Map && c_ParentHouse == ((RentalContract)item).ParentHouse )
					foreach( Rectangle2D rect2 in ((RentalContract)item).Blocks )
						for( int x = rect.Start.X; x < rect.End.X; ++x )
							for( int y = rect.Start.Y; y < rect.End.Y; ++y )
								if ( rect2.Contains( new Point2D( x, y ) ) )
									if ( ((RentalContract)item).MinZ <= z && ((RentalContract)item).MaxZ >= z )
										return true;

			return false;
		}

		public bool HasContractedArea( int z )
		{
			foreach( Item item in AllSigns)
				if ( item is RentalContract && item != this && item.Map == Map && c_ParentHouse == ((RentalContract)item).ParentHouse )
					if ( ((RentalContract)item).MinZ <= z && ((RentalContract)item).MaxZ >= z )
						return true;

			return false;
		}

		public void DepositTo( Mobile m )
		{
			if ( m == null )
				return;

			if ( Free )
			{
				m.SendMessage( "Já que a casa foi grátis você não recebe um depósito" );
				return;
			}

			m.BankBox.DropItem( new Gold( Price ) );
			m.SendMessage( "Você recebeu {0} moedas pela sua casa", Price );
		}

		public override void ValidateOwnership()
		{
			if ( c_Completed && c_RentalMaster == null )
			{
				Delete();
				return;
			}

			if ( c_RentalClient != null && ( c_ParentHouse == null || c_ParentHouse.Deleted ) )
			{
				Delete();
				return;
			}

			if ( c_RentalClient != null && !Owned )
			{
				Delete();
				return;
			}

			if ( ParentHouse == null )
				return;

			if ( !ValidateLocSec() )
			{
				if ( DemolishTimer == null )
					BeginDemolishTimer( TimeSpan.FromHours( 48 ) );
			}
			else
				ClearDemolishTimer();
		}

		protected override void DemolishAlert()
		{
			if ( ParentHouse == null || c_RentalMaster == null || c_RentalClient == null )
				return;

			c_RentalMaster.SendMessage( "Você começou a usar o espaço de {0}, e a unidade de aluguel dele vai se desfazer em {1}.", c_RentalClient.Name, Math.Round( (DemolishTime-DateTime.Now).TotalHours, 2 ) );
			c_RentalClient.SendMessage( "Alerta ao proprietário {0}, que ele está usando seu espaço alugado. Isso violou o contrato de aluguel que vai acabar em {1}, caso nada seja feito", c_RentalMaster.Name, Math.Round( (DemolishTime-DateTime.Now).TotalHours, 2 ) );
		}

		public void FixLocSec()
		{
			int count = 0;

			if ( (count = General.RemainingSecures( c_ParentHouse )+Secures) < Secures )
				Secures = count;

			if ( (count = General.RemainingLocks( c_ParentHouse )+Locks) < Locks )
				Locks = count;
		}

		public bool ValidateLocSec()
		{
			if ( General.RemainingSecures( c_ParentHouse )+Secures < Secures )
				return false;

			if ( General.RemainingLocks( c_ParentHouse )+Locks < Locks )
				return false;

			return true;
		}

		public override void ConvertItems( bool keep )
		{
			if ( House == null || c_ParentHouse == null || c_RentalMaster == null )
				return;

			foreach( BaseDoor door in new ArrayList( c_ParentHouse.Doors ) )
				if ( door.Map == House.Map && House.Region.Contains( door.Location ) )
					ConvertDoor( door );

			foreach( SecureInfo info in new ArrayList( c_ParentHouse.Secures ) )
				if ( info.Item.Map == House.Map && House.Region.Contains( info.Item.Location ) )
					c_ParentHouse.Release( c_RentalMaster, info.Item );

            foreach (Item item in new ArrayList(c_ParentHouse.LockDowns.Keys))
                if (item.Map == House.Map && House.Region.Contains(item.Location))
                    c_ParentHouse.Release(c_RentalMaster, item);
        }

		public override void UnconvertDoors( )
		{
			if ( House == null || c_ParentHouse == null )
				return;

			foreach( BaseDoor door in new ArrayList( House.Doors ) )
				House.Doors.Remove( door );
		}

		protected override void OnRentPaid()
		{
			if ( c_RentalMaster == null || c_RentalClient == null )
				return;

			if ( Free )
				return;

			c_RentalMaster.BankBox.DropItem( new Gold( Price ) );
			c_RentalMaster.SendMessage( "O banco transferiu seu dinheiro de {0}.", c_RentalClient.Name );
		}

		public override void ClearHouse()
		{
			if ( !Deleted )
				Delete();

			base.ClearHouse();
		}

		public override void OnDoubleClick( Mobile m )
		{
			ValidateOwnership();

			if ( Deleted )
				return;

			if ( c_RentalMaster == null )
				c_RentalMaster = m;

			BaseHouse house = BaseHouse.FindHouseAt( m );

			if ( c_ParentHouse == null )
				c_ParentHouse = house;

			if ( house == null ||  house != c_ParentHouse && house != House  )
			{
				m.SendMessage( "Você precisa estar na casa pra ver o contrato" );
				return;
			}

			if ( m == c_RentalMaster
			 && !c_Completed
			 && house is TownHouse
			 && ((TownHouse)house).ForSaleSign.PriceType != "Sale" )
			{
				c_ParentHouse = null;
				m.SendMessage( "Você só pode alugar propriedade que você é proprietário" );
				return;
			}

			if ( m == c_RentalMaster && !c_Completed && General.EntireHouseContracted( c_ParentHouse ) )
			{
				m.SendMessage( "A casa toda ja foi alugada" );
				return;
			}

			if ( c_Completed )
				new ContractConfirmGump( m, this );
			else if ( m == c_RentalMaster )
				new ContractSetupGump( m, this );
			else
				m.SendMessage( "Esse contrato ainda não foi finalizado" );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			if ( c_RentalClient != null )
				list.Add( "um contrato de aluguel com " + c_RentalClient.Name );
			else if ( c_Completed )
				list.Add("um contrato de aluguel pronto com");
			else
				list.Add("um contrato de aluguel não finalizado com");
		}

		public override void Delete()
		{
			if ( c_ParentHouse == null )
			{
				base.Delete();
				return;
			}

			if ( !Owned && !c_ParentHouse.IsFriend( c_RentalClient ) )
			{
				if ( c_RentalClient != null && c_RentalMaster != null )
				{
					c_RentalMaster.SendMessage( "{0} terminou seu contrato de aluguel. Como o contrato foi revogado, seu ultimo pagamento foi devolvido.", c_RentalMaster.Name );
					c_RentalClient.SendMessage( "Você terminou um contrato de aluguel com {0}. Por seu acesso ter sido revogado, seu ultimo pagamento foi devolvido.", c_RentalClient.Name );
				}

				DepositTo( c_RentalClient );
			}
			else if ( Owned )
			{
				if ( c_RentalClient != null && c_RentalMaster != null )
				{
					c_RentalClient.SendMessage("{0} terminou seu contrato de aluguel. Como quebraram seu contrato, seu pagamento foi devolvido.", c_RentalMaster.Name );
					c_RentalMaster.SendMessage("Você terminou um contrato de aluguel com {0}. Será devolvido seu ultimo pagamento.", c_RentalClient.Name );
				}

				DepositTo( c_RentalClient );

				PackUpHouse();
			}
			else
			{
				if ( c_RentalClient != null && c_RentalMaster != null )
				{
					c_RentalMaster.SendMessage("{0} terminou seu contrato de aluguel.", c_RentalClient.Name );
					c_RentalClient.SendMessage("Você terminou um contrato de aluguel com {0}.", c_RentalMaster.Name );
				}

				DepositTo( c_RentalMaster );
			}

			ClearRentTimer();
			base.Delete();
		}

		public RentalContract( Serial serial ) : base( serial )
		{
			RecurRent = true;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write(  1 ); // version

			// Version 1

			writer.Write( c_EntireHouse );

			writer.Write( c_RentalMaster );
			writer.Write( c_RentalClient );
			writer.Write( c_ParentHouse );
			writer.Write( c_Completed );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( version >= 1 )
				c_EntireHouse = reader.ReadBool();

			c_RentalMaster = reader.ReadMobile();
			c_RentalClient = reader.ReadMobile();
			c_ParentHouse = reader.ReadItem() as BaseHouse;
			c_Completed = reader.ReadBool();
		}
	}
}
