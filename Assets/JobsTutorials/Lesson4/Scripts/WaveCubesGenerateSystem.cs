using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.DOD.Lesson4
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(Lesson4SystemGroups))]
    public partial struct WaveCubesGenerateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveCubeGenerator>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var generator = SystemAPI.GetSingleton<WaveCubeGenerator>();
            var cubeNum = generator.halfCountX * generator.halfCountZ * 4;
            var generateCubes = CollectionHelper.CreateNativeArray<Entity>(cubeNum, Allocator.Temp);
            state.EntityManager.Instantiate(generator.srcEntity, generateCubes);
            int count = 0;
            for (int i = -generator.halfCountX; i < generator.halfCountX; i++)
            {
                for (int j = -generator.halfCountZ; j < generator.halfCountZ; j++)
                {
                    var cube = generateCubes[count];
                    var cubeTransform = SystemAPI.GetComponentRW<LocalTransform>(cube);
                    cubeTransform.ValueRW.Position = new float3(i * 1.1f, 0, j * 1.1f);
                    var cubeTag = new WaveCubeDistance() { distance = math.distance(cubeTransform.ValueRO.Position, float3.zero) };
                    state.EntityManager.AddComponentData(cube, cubeTag);//添加tag组件用于查询
                    count++;
                }
            }
            generateCubes.Dispose();
            state.Enabled = false;//只执行一次
        }
    }
}       
