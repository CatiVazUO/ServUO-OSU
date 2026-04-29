using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.Systems.Arena
{
    public class ArenaBombermanMovementHook
    {
        public static void Initialize()
        {
            EventSink.Movement += OnMovement;
        }

        private static void OnMovement(MovementEventArgs e)
        {
            Mobile m = e.Mobile;
            PlayerMobile pm = m as PlayerMobile;
            if (pm == null || pm.Map == null || pm.Map == Map.Internal)
                return;

            string key;
            int city;
            ArenaDefinition def;
            Server.Custom.Reinos.ReinoLotDefinition lot;

            if (!ArenaSystem.TryResolveArenaAt(pm.Location, pm.Map, out key, out city, out def, out lot))
                return;

            ArenaGameModes.BombermanSession s = ArenaGameModes.GetOrCreateBomberman(key);
            if (!s.Running || !s.IsParticipant(pm))
                return;

            int x = pm.X;
            int y = pm.Y;
            Server.Movement.Movement.Offset(e.Direction, ref x, ref y);

            // bloqueia ocupação de tile por outro jogador participante em todas as direções durante o jogo
            IPooledEnumerable eable = pm.Map.GetMobilesInRange(new Point3D(x, y, pm.Z), 0);
            foreach (Mobile mob in eable)
            {
                PlayerMobile other = mob as PlayerMobile;
                if (other == null || other == pm)
                    continue;

                if (s.IsParticipant(other))
                {
                    e.Blocked = true;
                    break;
                }
            }
            eable.Free();

            if (e.Blocked)
                return;

            IPooledEnumerable items = pm.Map.GetItemsInRange(new Point3D(x, y, pm.Z), 0);
            foreach (Item it in items)
            {
                if (it == null || it.Deleted)
                    continue;

                if (it is ArenaWallItem || it is ArenaCrateItem || it is ArenaBombItem || it is ArenaGateBlockItem)
                {
                    e.Blocked = true;
                    break;
                }
            }
            items.Free();
        }
    }
}
