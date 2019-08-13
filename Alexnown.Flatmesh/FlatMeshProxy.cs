using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Alexnown.Flatmesh
{
    public class FlatMeshProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Vector2[] Vertices;
        public ushort[] Triangles;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int vertsCount = Vertices?.Length ?? 0;
            int triangleCount = Triangles?.Length ?? 0;
            if (vertsCount == 0 || triangleCount == 0) throw new InvalidOperationException(
                $"Cant convnert mesh {name} with verts={vertsCount}, trinagles={triangleCount}");
            var meshData = ConstructBlob(Vertices, Triangles);
            dstManager.AddComponentData(entity, new FlatMeshBlobComponent { Data = meshData });
            float minX = Single.MaxValue, minY = Single.MaxValue;
            float maxX = Single.MinValue, maxY = Single.MinValue;
            foreach (var point in Vertices)
            {
                if (point.x < minX) minX = point.x;
                else if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                else if (point.y > maxY) maxY = point.y;
            }
            dstManager.AddComponentData(entity, new FlatMeshBounds
            {
                Min = new float2(minX, minY),
                Max = new float2(maxX, maxY)
            });
        }

        public unsafe BlobAssetReference<FlatMeshData> ConstructBlob(Vector2[] vertices, ushort[] triangles)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<FlatMeshData>();
            var verts = builder.Allocate(ref root.Vertices, vertices.Length);
            var tris = builder.Allocate(ref root.Triangles, triangles.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                verts[i] = vertices[i];
            }
            for (int i = 0; i < triangles.Length; i++)
            {
                tris[i] = triangles[i];
            }

            var blobAsset = builder.CreateBlobAssetReference<FlatMeshData>(Allocator.Temp);

            builder.Dispose();

            return blobAsset;
        }
    }
}
