using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Alexnown.Flatmesh
{
    public class FlatMeshProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        private readonly static List<ushort> _tempForTriangles = new List<ushort>();
        public Vector2[] Vertices;
        public ushort[] Triangles;

        public int RemoveBrokenTriangles()
        {
            int totalTriangles = Triangles.Length / 3;
            for (int i = 0; i < totalTriangles; i++)
            {
                var t1 = Triangles[3 * i];
                var t2 = Triangles[3 * i + 1];
                var t3 = Triangles[3 * i + 2];
                var v1 = Vertices[t1];
                var v2 = Vertices[t2];
                var v3 = Vertices[t3];
                if (v1 != v2 && v2 != v3 && v3 != v1)
                {
                    _tempForTriangles.Add(t1);
                    _tempForTriangles.Add(t2);
                    _tempForTriangles.Add(t3);
                }
            }
            int removedTriangles = 0;
            if (Triangles.Length != _tempForTriangles.Count)
            {
                removedTriangles = Triangles.Length - _tempForTriangles.Count;
                Triangles = _tempForTriangles.ToArray();
            }
            _tempForTriangles.Clear();
            return removedTriangles;
        }

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
            var verts = builder.Allocate(vertices.Length, ref root.Vertices);
            var tris = builder.Allocate(triangles.Length, ref root.Triangles);
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
