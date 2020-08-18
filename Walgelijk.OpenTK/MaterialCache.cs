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

            loaded = new LoadedMaterial(material); //TODO vang de shit die deze gek gooit
            loadedMaterials.Add(material, loaded);
            return loaded;
        }
    }
}
