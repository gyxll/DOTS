using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DOTS.DOD.Lesson4
{
    struct WaveCubeGenerator:IComponentData
    {
        public Entity srcEntity;
        public int halfCountX;
        public int halfCountZ;
    }
    struct WaveCubeDistance : IComponentData
    {
        public float distance;
    }
    public class WaveCubeAuthoring : Singleton<WaveCubeAuthoring>
    {
        public GameObject prefab;
        [Range(10, 100)] public int xHalfCount = 40;
        [Range(10, 100)] public int zHalfCount = 40;

        class WaveCubeBaker : Baker<WaveCubeAuthoring>
        {
            public override void Bake(WaveCubeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var component = new WaveCubeGenerator()
                {
                    srcEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    halfCountX = authoring.xHalfCount,
                    halfCountZ = authoring.zHalfCount
                };
                AddComponent(entity, component);
            }
        }
    }
}

