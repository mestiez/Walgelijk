using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK
{
    public struct MaterialTexturePair
    {
        public LoadedMaterial Material;
        public LoadedTexture Texture;

        public MaterialTexturePair(LoadedMaterial material, LoadedTexture texture)
        {
            Material = material;
            Texture = texture;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialTexturePair pair &&
                   pair.Material.ProgramHandle == Material.ProgramHandle &&
                   pair.Texture.Index == Texture.Index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Material, Texture);
        }

        public static bool operator ==(MaterialTexturePair left, MaterialTexturePair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MaterialTexturePair left, MaterialTexturePair right)
        {
            return !(left == right);
        }
    }
}
