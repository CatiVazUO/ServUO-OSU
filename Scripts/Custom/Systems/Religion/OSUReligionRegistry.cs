using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Custom.Systems.Religion
{
    public static class OSUReligionRegistry
    {
        private static List<OSUReligionDefinition> _all;

        public static void EnsureLoaded()
        {
            if (_all != null)
                return;

            _all = new List<OSUReligionDefinition>();

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

                    if (!typeof(OSUReligionDefinition).IsAssignableFrom(t))
                        continue;

                    try
                    {
                        var def = (OSUReligionDefinition)Activator.CreateInstance(t);
                        if (def != null && !string.IsNullOrWhiteSpace(def.Id))
                            _all.Add(def);
                    }
                    catch { }
                }
            }

            _all.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
        }

        public static List<OSUReligionDefinition> GetAll()
        {
            EnsureLoaded();
            return _all;
        }

        public static OSUReligionDefinition GetById(string id)
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
