using System;

namespace Server.Custom.Systems.Climate
{
    // Aqui você coloca regiões "fixas" por coordenadas, para não precisar clicar tudo.
    // Isso NÃO remove o comando de clicar — é só um atalho.
    public static class OSUClimateDefaults
    {
        public static void Initialize()
        {
            // Se você NÃO quer carregar defaults automaticamente, deixe este return.
            // Quando quiser usar, apague a linha abaixo.
            // return;

            // Exemplo (APAGUE/EDITE):
            // Add("NorthCold_01", -3, false, 0, 1094, 109, 1586, 341);
            // Add("NorthCold_02", -2, false, 0, 1162, 341, 1455, 533);
        }

        private static void Add(string name, int baseTemp, bool isStatic, int mapIndex, int x1, int y1, int x2, int y2)
        {
            var r = new OSUClimateRegion(name, baseTemp, isStatic, mapIndex, x1, y1, x2, y2);

            string err;
            if (!OSUClimateRegions.TryAddRegion(r, out err))
            {
                Console.WriteLine("[OSUClimate] ERRO: " + err);
            }
        }
    }
}
