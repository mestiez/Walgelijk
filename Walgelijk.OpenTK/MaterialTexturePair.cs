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
                   EqualityComparer<LoadedMaterial>.Default.Equals(Material, pair.Material) &&
                   EqualityComparer<LoadedTexture>.Default.Equals(Texture, pair.Texture);
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
