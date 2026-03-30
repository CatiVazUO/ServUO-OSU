using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Custom.Systems.DefQual
{
    public static class OSUDefQualRegistry
    {
        private static List<OSUDefQualDefinition> _all;

        public static void EnsureLoaded()
        {
            if (_all != null)
                return;

            _all = new List<OSUDefQualDefinition>();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }
                catch { continue; }

                if (types == null) continue;

                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];
                    if (t == null || t.IsAbstract) continue;

                    if (!typeof(OSUDefQualDefinition).IsAssignableFrom(t))
                        continue;

                    try
                    {
                        var def = (OSUDefQualDefinition)Activator.CreateInstance(t);
                        if (def != null && !string.IsNullOrWhiteSpace(def.Id))
                            _all.Add(def);
                    }
                    catch { }
                }
            }

            // ordena por nome (você pode mudar depois)
            _all.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static List<OSUDefQualDefinition> GetAll()
        {
            EnsureLoaded();
            return _all;
        }

        public static OSUDefQualDefinition GetById(string id)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(id)) return null;

            for (int i = 0; i < _all.Count; i++)
                if (string.Equals(_all[i].Id, id, StringComparison.OrdinalIgnoreCase))
                    return _all[i];

            return null;
        }
    }
}
