using System;

namespace Walgelijk.OpenTK
{
    public struct MaterialTexturePair
    {
        public LoadedMaterial Material;
        public LoadedTexture Texture;
        public int UniformLocation;

        public MaterialTexturePair(LoadedMaterial material, LoadedTexture texture, int uniformLocation)
        {
            Material = material;
            Texture = texture;
            UniformLocation = uniformLocation;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialTexturePair pair &&
                   pair.Material.ProgramHandle == Material.ProgramHandle &&
                   pair.UniformLocation == UniformLocation &&
                   pair.Texture.Index == Texture.Index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Material, Texture);
        }

        public static bool operator ==(MaterialTexturePair left, MaterialTexturePair right)
        {
            return left.Material.ProgramHandle == right.Material.ProgramHandle &&
                   left.UniformLocation == right.UniformLocation &&
                   left.Texture.Index == right.Texture.Index;
        }

        public static bool operator !=(MaterialTexturePair left, MaterialTexturePair right)
        {
            return !(left == right);
        }
    }
}
