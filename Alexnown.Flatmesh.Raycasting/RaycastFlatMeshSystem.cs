using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

namespace Alexnown.Flatmesh.Raycasting
{
    public class RaycastFlatMeshSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct RaycastJob : IJobForEach<RaycastFlatMeshRequest, RaycastHitResult>
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<ArchetypeChunk> RaycastableChunks;
            [ReadOnly]
            public ArchetypeChunkBufferType<RaycastableElement> RaycastableElementsType;
            [ReadOnly]
            public ComponentDataFromEntity<FlatMeshBounds> RenderBounds;
            [ReadOnly]
            public ComponentDataFromEntity<FlatMeshBlobComponent> FlatMeshBlobs;

            private bool PointRaycastMesh(Vector2 hitPos, ref FlatMeshData data)
            {
                int trisCount = data.Triangles.Length / 3;
                for (int i = 0; i < trisCount; i++)
                {
                    var v1 = data.Vertices[data.Triangles[3 * i]];
                    var v2 = data.Vertices[data.Triangles[3 * i + 1]];
                    var v3 = data.Vertices[data.Triangles[3 * i + 2]];
                    if(v1 ==v2 || v2 == v3 || v1 == v3) continue;
                    if (PointInTriangle(hitPos, v1, v2, v3)) return true;
                }
                return false;
            }

            private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
            }

            private bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
            {
                float d1, d2, d3;
                bool has_neg, has_pos;

                d1 = Sign(pt, v1, v2);
                d2 = Sign(pt, v2, v3);
                d3 = Sign(pt, v3, v1);

                has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                return !(has_neg && has_pos);
            }

            public void Execute([ReadOnly]ref RaycastFlatMeshRequest request, [WriteOnly]ref RaycastHitResult hitResult)
            {
                for (int i = 0; i < RaycastableChunks.Length; i++)
                {
                    var buffers = RaycastableChunks[i].GetBufferAccessor(RaycastableElementsType);
                    for (int j = 0; j < buffers.Length; j++)
                    {
                        var raycastableElements = buffers[j];
                        for (int k = 0; k < raycastableElements.Length; k++)
                        {
                            var raycastableEntity = raycastableElements[k].Target;
                            var bounds = RenderBounds[raycastableEntity];
                            bool isContains = bounds.Contains(request.WorldRayPos);
                            if (!isContains) continue;
                            var meshBlobReference = FlatMeshBlobs[raycastableEntity].Data;
                            if (PointRaycastMesh(request.WorldRayPos, ref meshBlobReference.Value))
                            {
                                hitResult.Value = raycastableEntity;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private EntityQuery _requests;
        private EntityQuery _raycastables;
        protected override void OnCreate()
        {
            base.OnCreate();
            _requests = GetEntityQuery(
                ComponentType.ReadOnly<RaycastFlatMeshRequest>(),
                ComponentType.ReadWrite<RaycastHitResult>());
            RequireForUpdate(_requests);
            _raycastables = GetEntityQuery(ComponentType.ReadOnly<RaycastableElement>());
            RequireForUpdate(_raycastables);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle allocRequests;
            var requests = _raycastables.CreateArchetypeChunkArray(Allocator.TempJob, out allocRequests);

            return new RaycastJob
            {
                RaycastableChunks = requests,
                FlatMeshBlobs = GetComponentDataFromEntity<FlatMeshBlobComponent>(true),
                RenderBounds = GetComponentDataFromEntity<FlatMeshBounds>(true),
                RaycastableElementsType = GetArchetypeChunkBufferType<RaycastableElement>(true)
            }.ScheduleSingle(_requests, JobHandle.CombineDependencies(inputDeps, allocRequests));
        }
    }
}
