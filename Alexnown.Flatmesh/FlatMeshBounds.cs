using Unity.Entities;
using Unity.Mathematics;

namespace Alexnown.Flatmesh
{
    public struct FlatMeshBounds : IComponentData
    {
        public float2 Min;
        public float2 Max;

        public bool Contains(float2 pos)
        {
            return pos.x >= Min.x && pos.x <= Max.x && pos.y >= Min.y && pos.y <= Max.y;
        }
    }
}
