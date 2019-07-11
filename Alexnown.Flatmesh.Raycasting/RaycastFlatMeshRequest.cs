using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Raycasting
{
    public struct RaycastFlatMeshRequest : IComponentData
    {
        public Vector2 WorldRayPos;
    }
}
