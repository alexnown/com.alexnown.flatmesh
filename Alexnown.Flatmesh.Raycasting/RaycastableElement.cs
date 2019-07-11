using Unity.Entities;

namespace Alexnown.Flatmesh.Raycasting
{
    [InternalBufferCapacity(0)]
    public struct RaycastableElement : IBufferElementData
    {
        public Entity Target;
    }
}
