using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    public class MaterialCache
    {
        private Dictionary<Material, LoadedMaterial> loadedMaterials = new Dictionary<Material, LoadedMaterial>();

        public LoadedMaterial Load(Material material)
        {
            if (loadedMaterials.TryGetValue(material, out var loaded))
                return loaded;
            try
            {
                loaded = new LoadedMaterial(material);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Loaded default material instead");
                // TODO *DIT GAAT EEN ONEINDIGE LOOP WORDEN ALS DE DEFAULT MATERIAL KAPOT IS!*
                loaded = Load(Material.DefaultMaterial);
            }

            loadedMaterials.Add(material, loaded);
            return loaded;
        }
    }
}
