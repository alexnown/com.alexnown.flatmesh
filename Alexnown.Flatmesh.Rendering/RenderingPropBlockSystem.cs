using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Rendering
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderingPropBlockSystem : ComponentSystem
    {
        public readonly List<RenderingBatch> Batches = new List<RenderingBatch>();
        public int DrawLayer;

        protected override void OnUpdate()
        {
            if (Batches.Count == 0) return;
            foreach (var batch in Batches)
            {
                if (!batch.Enabled) continue;
                foreach (var pass in batch.Passes)
                {
                    Graphics.DrawMesh(pass.Mesh, pass.Matrix, pass.Material, DrawLayer, null, 0, pass.PropertyBlock);
                }
            }
        }
    }

    public class RenderingBatch
    {
        public bool Enabled = true;
        public LinkedList<DrawPass> Passes = new LinkedList<DrawPass>();
    }

    public class DrawPass
    {
        public Mesh Mesh;
        public Matrix4x4 Matrix;
        public Material Material;
        public MaterialPropertyBlock PropertyBlock;
    }
}