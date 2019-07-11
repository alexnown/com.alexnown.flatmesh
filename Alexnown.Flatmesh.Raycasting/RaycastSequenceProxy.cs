using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Alexnown.Flatmesh.Raycasting
{
    public class RaycastSequenceProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool RevertSequenceOrder;
        [Header("Custom sequence")]
        public bool UseCustomSequence;
        public List<GameObject> CustomSequence;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<RaycastableElement>(entity);
            if (UseCustomSequence)
            {
                int count = CustomSequence.Count;
                for (int i = 0; i < count; i++)
                {
                    int index = RevertSequenceOrder ? count - i - 1 : i;
                    var go = CustomSequence[index];
                    if (go == null) continue;
                    var childEntity = conversionSystem.GetPrimaryEntity(go);
                    buffer.Add(new RaycastableElement { Target = childEntity });
                }
            }
            else
            {
                int count = transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    int index = RevertSequenceOrder ? count - i - 1 : i;
                    var go = transform.GetChild(index);
                    var childEntity = conversionSystem.GetPrimaryEntity(go);
                    buffer.Add(new RaycastableElement { Target = childEntity });
                }
            }
        }
    }
}