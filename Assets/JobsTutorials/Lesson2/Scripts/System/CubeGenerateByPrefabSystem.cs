using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Jobs.DOD
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(CreateEntitiesByPrefabSystemGroup))]
    partial struct CubeGenerateByPrefabSystem : ISystem
    {
        public void OnCreate(ref SystemState state) 
        {
            state.RequireForUpdate<CubeGeneratorByPrefab>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state) 
        {
            var generatorPrefab = SystemAPI.GetSingleton<CubeGeneratorByPrefab>();
            var entityArr = CollectionHelper.CreateNativeArray<Entity>(generatorPrefab.count, Allocator.Temp);
            state.EntityManager.Instantiate(generatorPrefab.cubePrototype, entityArr);
            int count = 0;
            foreach (var entity in entityArr)
            {
                var randomRotate = new RotateSpeed() { rotateSpeed = math.radians(count) };
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

