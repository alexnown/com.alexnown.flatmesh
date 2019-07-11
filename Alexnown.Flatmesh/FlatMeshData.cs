using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh
{
    public struct FlatMeshBlobComponent : IComponentData
    {
        public BlobAssetReference<FlatMeshData> Data;
    }

    public struct FlatMeshData
    {
        public BlobArray<Vector2> Vertices;
        public BlobArray<ushort> Triangles;
    }
}