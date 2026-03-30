using Server;
using Server.Gumps;

namespace Server.Gumps
{
    public abstract class OSUBaseGump : Gump
    {
        protected const int LabelHue = 1152;

        // guarda posição atual
        public int GumpX { get; private set; }
        public int GumpY { get; private set; }

        protected OSUBaseGump(int x, int y) : base(x, y)
        {
            GumpX = x;
            GumpY = y;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);

            DrawSkin();
        }

        // ✅ aqui é o "skin" do seu OSUSkillGump novo
        protected virtual void DrawSkin()
        {
            AddImageTiled(523, 273, 480, 607, 375);
            AddImage(905, 772, 341);
            AddImageTiled(514, 382, 33, 391, 345);
            AddImage(509, 772, 342);
            AddImageTiled(982, 384, 33, 388, 346);
            AddImageTiled(613, 260, 304, 48, 355);
            AddImage(509, 258, 339);
            AddImage(905, 258, 340);
            AddImageTiled(619, 847, 289, 48, 356);

            AddImageTiled(566, 449, 399, 25, 443);
            AddImage(563, 332, 447);
            AddImage(562, 403, 447);
            AddImageTiled(566, 788, 399, 25, 443);
        }
    }
}
