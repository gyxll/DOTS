using DOTS.DOD;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace Jobs.DOD
{
    struct CubeGeneratorByPrefab:IComponentData
    {
        public Entity cubePrototype;
        public int count;
    }
    public class CubeGeneratorByPrefabAuthoring : Singleton<CubeGeneratorByPrefabAuthoring>
    {
        public GameObject prefab;
        [Range(1,10)]
        public int cubeCount = 6;


        class CubeGeneratorBaker:Baker<CubeGeneratorByPrefabAuthoring>
        {
            public override void Bake(CubeGeneratorByPrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var prefabComponent = new CubeGeneratorByPrefab() { cubePrototype = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic), count = authoring.cubeCount };
                AddComponent(entity, prefabComponent);
            }
        }
    }
}
