using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Jobs.DOD
{
    [BurstCompile]
    //添加下面的标签要求System中的Update在查询通过后才执行，即需要查询到CubeGeneratorByPrefab单例才会执行后面的内容
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(CreateEntitiesByPrefabSystemGroup))]
    partial struct CubeGenerateByPrefabSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) 
        {
            //下面的代码要求存在特定Component存在时才会执行System
            //通过 RequireForUpdate 添加的条件都会覆盖本System已查询的结果。
            //换句话说，如果任何RequireForUpdate设置的条件未满足时，即使该System的其它查询（显式或隐式）匹配成功，Update也会被跳过。
            //请注意，对于实现了 IEnableComponent 的Component，该方法不会考虑Component是否已启用，而只会检查组件是否存在。
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

