using Server.Network;

namespace Server.Gumps
{
    public class GenericSignGump : Gump
    {
        public GenericSignGump() : base(0, 0)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddImage(400, 300, 571);
            AddLabel(473, 326, 0, "Estatua Generica");
        }
    }
}
