using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Rendering
{
    public class RenderingSequenceProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Header("Custom sequence")]
        public bool UseCustomRenderingSequence;
        public List<GameObject> CustomRenderingSequence;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderedArray = dstManager.AddBuffer<RenderingElement>(entity);
            if (UseCustomRenderingSequence)
            {
                foreach (var renderedGo in CustomRenderingSequence)
                {
                    if (renderedGo == null) continue;
                    var childEntity = conversionSystem.GetPrimaryEntity(renderedGo);
                    renderedArray.Add(new RenderingElement { Target = childEntity });
                }
            }
            else
            {
                foreach (Transform child in transform)
                {
                    var childEntity = conversionSystem.GetPrimaryEntity(child.gameObject);
                    renderedArray.Add(new RenderingElement { Target = childEntity });
                }
            }
        }
    }
}