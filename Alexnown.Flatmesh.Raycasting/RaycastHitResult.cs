using Unity.Entities;

namespace Alexnown.Flatmesh.Raycasting
{
    public struct RaycastHitResult : IComponentData
    {
        public Entity Value;
    }
}
