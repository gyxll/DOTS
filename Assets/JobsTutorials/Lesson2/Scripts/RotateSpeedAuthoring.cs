using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace Jobs.DOD
{
    struct RotateSpeed :IComponentData
    {
        public float rotateSpeed;
        //public string param3;
    }
    public class RotateSpeedAuthoring : MonoBehaviour
    {
        [Range(0, 360)] public float rotateSpeed = 360.0f;

        public class RotateSpeedBaker : Baker<RotateSpeedAuthoring>
        {
            public override void Bake(RotateSpeedAuthoring authoring)
            {
                var data = new RotateSpeed
                {
                    rotateSpeed = authoring.rotateSpeed
                };
                var entities = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entities, data);
            }
        }
    }

    public struct AnotherTag : IComponentData { }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    partial struct AddTagToRotationBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var queryMissingTag = SystemAPI.QueryBuilder()
                .WithAll<RotateSpeed>()
                .WithNone<AnotherTag>()
                .Build();
            //对查询到的实体添加 AnotherTag
            state.EntityManager.AddComponent<AnotherTag>(queryMissingTag);

            // 省略下面的内容会导致实时烘焙过程中出现不一致的结果。即使删除了 RotateSpeed 组件，已添加的AnotherTag仍会保留在实体上。

            var queryCleanupTag = SystemAPI.QueryBuilder()
                .WithAll<AnotherTag>()
                .WithNone<RotateSpeed>()
                .Build();

            state.EntityManager.RemoveComponent<AnotherTag>(queryCleanupTag);
        }
    }
}

