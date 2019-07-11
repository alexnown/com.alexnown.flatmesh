using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Rendering
{
    public struct AddedToLightweightRendering : ISystemStateComponentData { }

    [UpdateInGroup(typeof(InitRenderingSystemGroup))]
    [UpdateAfter(typeof(RenderingResourcesMappingSystem))]
    public class RegisterRenderBatchSystem : ComponentSystem
    {
        private RenderingPropBlockSystem _renderSystem;
        private RenderingResourcesMappingSystem _resourcesSystem;

        private EntityQuery _notDeclaredYet;
        private EntityQuery _deletedQuery;
        private readonly Dictionary<Entity, RenderingBatch> _declaredBatches = new Dictionary<Entity, RenderingBatch>();
        private EntityQueryBuilder.F_EB<RenderingElement> _declareRenderingForEach;
        private EntityQueryBuilder.F_E _deleteRenderingForEach;

        protected override void OnCreate()
        {
            _renderSystem = World.GetOrCreateSystem<RenderingPropBlockSystem>();
            _resourcesSystem = World.GetOrCreateSystem<RenderingResourcesMappingSystem>();
            _notDeclaredYet = GetEntityQuery(
                ComponentType.ReadOnly<RenderingElement>(),
                ComponentType.Exclude<AddedToLightweightRendering>());
            _deletedQuery = GetEntityQuery(
                ComponentType.ReadOnly<AddedToLightweightRendering>(),
                ComponentType.Exclude<RenderingElement>());
            _declareRenderingForEach = DeclateForRendering;
            _deleteRenderingForEach = DeleteFromRendering;
        }

        private void DeclateForRendering(Entity entity, DynamicBuffer<RenderingElement> elements)
        {
            var batch = new RenderingBatch();
            for (int i = 0; i < elements.Length; i++)
            {
                var renderedEntity = elements[i].Target;
                Mesh mesh;
                Material mat;
                MaterialPropertyBlock propertyBlock;
                _resourcesSystem.Meshes.TryGetValue(renderedEntity, out mesh);
                _resourcesSystem.Materials.TryGetValue(renderedEntity, out mat);
                _resourcesSystem.PropertyBlocks.TryGetValue(renderedEntity, out propertyBlock);
                var drawPass = new DrawPass
                {
                    Mesh = mesh,
                    Matrix = Matrix4x4.Translate(new Vector3(0, 0, -0.001f * i)),
                    Material = mat,
                    PropertyBlock = propertyBlock
                };
                batch.Passes.AddLast(drawPass);
            }
            if (batch.Passes.Count > 0)
            {
                _declaredBatches[entity] = batch;
                _renderSystem.Batches.Add(batch);
            }
        }

        private void DeleteFromRendering(Entity entity)
        {
            RenderingBatch batch;
            if (_declaredBatches.TryGetValue(entity, out batch))
            {
                _renderSystem.Batches.Remove(batch);
            }
            _declaredBatches.Remove(entity);
        }

        protected override void OnUpdate()
        {
            Entities.With(_notDeclaredYet).ForEach(_declareRenderingForEach);
            PostUpdateCommands.AddComponent(_notDeclaredYet, ComponentType.ReadOnly<AddedToLightweightRendering>());

            Entities.With(_deletedQuery).ForEach(_deleteRenderingForEach);
            EntityManager.RemoveComponent(_deletedQuery, ComponentType.ReadOnly<AddedToLightweightRendering>());
        }
    }
}
