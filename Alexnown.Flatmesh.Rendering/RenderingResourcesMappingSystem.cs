using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Rendering
{
    [UpdateInGroup(typeof(InitRenderingSystemGroup))]
    public class RenderingResourcesMappingSystem : ComponentSystem
    {
        public Dictionary<Entity, Mesh> Meshes = new Dictionary<Entity, Mesh>();
        public Dictionary<Entity, Material> Materials = new Dictionary<Entity, Material>();
        public Dictionary<Entity, MaterialPropertyBlock> PropertyBlocks = new Dictionary<Entity, MaterialPropertyBlock>();

        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            //todo: process clear resources command?    
        }
    }
}
