using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Jobs.DOD
{
    [BurstCompile]
    //�������ı�ǩҪ��System�е�Update�ڲ�ѯͨ�����ִ�У�����Ҫ��ѯ��CubeGeneratorByPrefab�����Ż�ִ�к��������
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(CreateEntitiesByPrefabSystemGroup))]
    partial struct CubeGenerateByPrefabSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) 
        {
            //����Ĵ���Ҫ������ض�Component����ʱ�Ż�ִ��System
            //ͨ�� RequireForUpdate ��ӵ��������Ḳ�Ǳ�System�Ѳ�ѯ�Ľ����
            //���仰˵������κ�RequireForUpdate���õ�����δ����ʱ����ʹ��System��������ѯ����ʽ����ʽ��ƥ��ɹ���UpdateҲ�ᱻ������
            //��ע�⣬����ʵ���� IEnableComponent ��Component���÷������ῼ��Component�Ƿ������ã���ֻ��������Ƿ���ڡ�
            state.RequireForUpdate<CubeGeneratorByPrefab>();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state) 
        {
            var generatorPrefab = SystemAPI.GetSingleton<CubeGeneratorByPrefab>();
            var entityArr = CollectionHelper.CreateNativeArray<Entity>(generatorPrefab.count, Allocator.Temp);
            state.EntityManager.Instantiate(generatorPrefab.cubePrototype, entityArr);
            int count = 0;
            foreach (var entity in entityArr)
            {
                var randomRotate = new RotateSpeed() { rotateSpeed = math.radians(count * 180) };
                state.EntityManager.AddComponentData(entity, randomRotate);
                var position = new float3((count - generatorPrefab.count * 0.5f) * 1.2f, 0, 0);
                SystemAPI.GetComponentRW<LocalTransform>(entity).ValueRW.Position = position;
                count++;
            }

            entityArr.Dispose();
            state.Enabled = false;
        }
    }
}

