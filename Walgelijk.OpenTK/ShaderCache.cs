using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    public class ShaderCache
    {
        private Dictionary<Shader, LoadedShader> loadedShaders = new Dictionary<Shader, LoadedShader>();

        public LoadedShader Load(Shader shader)
        {
            if (loadedShaders.TryGetValue(shader, out var loaded))
                return loaded;

            loaded = new LoadedShader(shader); //TODO vang de shit die deze gek gooit
            loadedShaders.Add(shader, loaded);
            return loaded;
        }
    }
}
