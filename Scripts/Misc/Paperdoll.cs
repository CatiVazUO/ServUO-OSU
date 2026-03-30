using System;
using System.Collections.Generic;
using Server.Network;
using Server.Mobiles;

namespace Server.Misc
{
    public class Paperdoll
    {
        public static void Initialize()
        {
            EventSink.PaperdollRequest += new PaperdollRequestEventHandler(EventSink_PaperdollRequest);
        }

        private static string BuildOSUPaperdollText(Mobile beholder, Mobile beheld)
        {
            string title = Titles.ComputeTitle(beholder, beheld);

            PlayerMobile pm = beheld as PlayerMobile;

            if (pm == null || pm.OSUCreation == null || !pm.OSUCreationCompleted)
                return title;

            int bodyVariant = pm.OSUCreation.BodyVariant;
            int faceIndex = pm.OSUCreation.FaceIndex;

            if (bodyVariant < 0 || bodyVariant > 1 || faceIndex < 0 || faceIndex > 8)
                return title;

            if (bodyVariant < 0)
                bodyVariant = 0;
            else if (bodyVariant > 1)
                bodyVariant = 1;

            if (faceIndex < 0)
                faceIndex = 0;
            else if (faceIndex > 8)
                faceIndex = 8;

            string tag = String.Format("[OSUPD:{0}:{1}] ", bodyVariant, faceIndex);

            // O pacote de paperdoll aceita só 60 caracteres.
            // Então colocamos a tag no COMEÇO, para ela nunca ser cortada.
            int maxTitleLen = 60 - tag.Length;

            if (maxTitleLen < 0)
                maxTitleLen = 0;

            if (title == null)
                title = String.Empty;

            if (title.Length > maxTitleLen)
                title = title.Substring(0, maxTitleLen);

            return tag + title;
        }

        public static void EventSink_PaperdollRequest(PaperdollRequestEventArgs e)
        {
            Mobile beholder = e.Beholder;
            Mobile beheld = e.Beheld;

            string paperdollText = BuildOSUPaperdollText(beholder, beheld);

            beholder.Send(new DisplayPaperdoll(beheld, paperdollText, beheld.AllowEquipFrom(beholder)));

            if (beholder.ViewOPL)
            {
                List<Item> items = beheld.Items;

                for (int i = 0; i < items.Count; ++i)
                    beholder.Send(items[i].OPLPacket);
            }
        }
    }
}
