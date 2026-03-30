using System;
using Server.Items;

namespace Server.Custom.Systems.Rent
{
	public class RentalLicense : Item
	{
		private Mobile c_Owner;

		public Mobile Owner{ get{ return c_Owner; } set{ c_Owner = value; InvalidateProperties(); } }


		[Constructable]
		public RentalLicense() : base( 0x14F0 )
		{
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			if ( c_Owner != null )
				list.Add( "licensa de aluguel de " + c_Owner.Name );
			else
				list.Add( "licensa de aluguel" );
		}

		public override void OnDoubleClick( Mobile m )
		{
			if ( c_Owner == null )
				c_Owner = m;
		}

		public RentalLicense( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write(  0 ); // version

			writer.Write( c_Owner );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			c_Owner = reader.ReadMobile();
		}
	}
}
