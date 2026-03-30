using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Custom.Systems.Culture
{
    public static class OSUCultureRegistry
    {
        private static Dictionary<string, OSUCultureDefinition> _byId;
        private static List<OSUCultureDefinition> _ordered;

        public static void EnsureLoaded()
        {
            if (_byId != null && _ordered != null)
                return;

            _byId = new Dictionary<string, OSUCultureDefinition>(StringComparer.OrdinalIgnoreCase);
            _ordered = new List<OSUCultureDefinition>();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }
                catch
                {
                    continue;
                }

                if (types == null)
                    continue;

                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];
                    if (t == null || t.IsAbstract)
                        continue;

                    if (!typeof(OSUCultureDefinition).IsAssignableFrom(t))
                        continue;

                    try
                    {
                        OSUCultureDefinition def = (OSUCultureDefinition)Activator.CreateInstance(t);
                        if (def == null || string.IsNullOrWhiteSpace(def.Id))
                            continue;

                        _byId[def.Id] = def;
                        _ordered.Add(def);
                    }
                    catch
                    {
                    }
                }
            }

            _ordered.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
        }

        public static List<OSUCultureDefinition> GetOrdered(int max = 6)
        {
            EnsureLoaded();

            List<OSUCultureDefinition> list = new List<OSUCultureDefinition>();
            for (int i = 0; i < _ordered.Count && list.Count < max; i++)
                list.Add(_ordered[i]);

            return list;
        }

        public static OSUCultureDefinition GetById(string id)
        {
            EnsureLoaded();

            if (string.IsNullOrWhiteSpace(id))
                return null;

            _byId.TryGetValue(id, out OSUCultureDefinition def);
            return def;
        }
    }
}
