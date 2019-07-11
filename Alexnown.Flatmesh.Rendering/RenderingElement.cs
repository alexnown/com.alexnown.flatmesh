using Unity.Entities;

namespace Alexnown.Flatmesh.Rendering
{
    [InternalBufferCapacity(0)]
    public struct RenderingElement : IBufferElementData
    {
        public Entity Target;
    }
}