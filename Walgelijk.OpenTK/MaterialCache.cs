using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    public class MaterialCache : Cache<Material, LoadedMaterial>
    {
        protected override LoadedMaterial CreateNew(Material material)
        {
            LoadedMaterial loaded;

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

            return loaded;
        }

        protected override void DisposeOf(LoadedMaterial loaded)
        {
            throw new NotImplementedException();
        }
    }
}
