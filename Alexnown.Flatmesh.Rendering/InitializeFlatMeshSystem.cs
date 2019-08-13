using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Rendering
{
    public struct MeshInitialized : ISystemStateComponentData { }

    [UpdateInGroup(typeof(InitRenderingSystemGroup))]
    [UpdateBefore(typeof(RenderingResourcesMappingSystem))]
    public class InitializeFlatMeshSystem : ComponentSystem
    {
        private RenderingResourcesMappingSystem _resourcesMapping;
        private EntityQuery _meshesForCreating;
        private EntityQuery _removedMeshes;
        private EntityQueryBuilder.F_ED<FlatMeshBlobComponent> _cachedForEach;

        protected override void OnCreate()
        {
            _resourcesMapping = World.GetOrCreateSystem<RenderingResourcesMappingSystem>();
            _meshesForCreating = GetEntityQuery(
                ComponentType.ReadOnly<FlatMeshBlobComponent>(),
                ComponentType.Exclude<MeshInitialized>());
            _removedMeshes = GetEntityQuery(
                ComponentType.Exclude<FlatMeshBlobComponent>(),
                ComponentType.ReadOnly<MeshInitialized>());
            _cachedForEach = CreateMeshFromMeshData;
        }

        protected override void OnUpdate()
        {
            if (!_meshesForCreating.IsEmptyIgnoreFilter)
            {
                Entities.With(_meshesForCreating).ForEach(_cachedForEach);
                PostUpdateCommands.AddComponent(_meshesForCreating, ComponentType.ReadOnly<MeshInitialized>());
            }

            if (!_removedMeshes.IsEmptyIgnoreFilter)
            {
                Entities.With(_removedMeshes).ForEach(entity => _resourcesMapping.Meshes.Remove(entity));
                PostUpdateCommands.RemoveComponent(_removedMeshes, ComponentType.ReadOnly<MeshInitialized>());
            }
        }

        private void CreateMeshFromMeshData(Entity entity, ref FlatMeshBlobComponent data)
        {
            _resourcesMapping.Meshes[entity] = CreateMeshFromBlob(ref data.Data.Value);
        }

        protected Mesh CreateMeshFromBlob(ref FlatMeshData data)
        {
            var verts = new Vector3[data.Vertices.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                var x = data.Vertices[i].x;
                var y = data.Vertices[i].y;
                verts[i] = new Vector3(x, y);
            }
            var triangles = new int[data.Triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = data.Triangles[i];
            }
            return new Mesh
            {
                vertices = verts,
                triangles = triangles
            };
        }
    }
}