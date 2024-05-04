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
            //�Բ�ѯ����ʵ����� AnotherTag
            state.EntityManager.AddComponent<AnotherTag>(queryMissingTag);

            // ʡ����������ݻᵼ��ʵʱ�決�����г��ֲ�һ�µĽ������ʹɾ���� RotateSpeed ���������ӵ�AnotherTag�Իᱣ����ʵ���ϡ�

            var queryCleanupTag = SystemAPI.QueryBuilder()
                .WithAll<AnotherTag>()
                .WithNone<RotateSpeed>()
                .Build();

            state.EntityManager.RemoveComponent<AnotherTag>(queryCleanupTag);
        }
    }
}

