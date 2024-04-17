using Unity.Burst;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs.DOD
{
    [BurstCompile]
    struct WaveCubesJob : IJobParallelForTransform
    {
        [ReadOnly]//表面数据为只读状态，不会出现线程安全问题
        public NativeArray<float> distances;
        [ReadOnly]
        public float time;
        /// <summary>
        /// 幅度
        /// </summary>
        [ReadOnly]
        public float amplitude;
        /// <summary>
        /// 频率
        /// </summary>
        [ReadOnly]
        public float frequency;
        /// <summary>
        /// 波长
        /// </summary>
        [ReadOnly]
        public float waveLength;
        public void Execute(int index, TransformAccess transform)
        {
            var perPos = transform.position;
            perPos.y = Mathf.Sin(time * frequency + distances[index] * waveLength) * amplitude;
            transform.localPosition = perPos;
        }
    }
    public class WaveCubesWithJobs : MonoBehaviour
    {
        public GameObject archGo;
        public int xRadio = 40;
        public int zRadio = 40;
        /// <summary>
        /// 幅度
        /// </summary>
        public float amplitude = 9;
        /// <summary>
        /// 频率
        /// </summary>
        public float frequency = 3;
        /// <summary>
        /// 波长
        /// </summary>
        public float waveLength = 0.2f;
        private int totalNum;
        private TransformAccessArray cubes;
        /// <summary>
        /// 记录每个立方体的距离
        /// </summary>
        private NativeArray<float> distances;
        /// <summary>
        /// 性能分析数据
        /// </summary>
        static readonly ProfilerMarker<int> profilerMarker = new("WaveCubesWithJobs UpdateTransform", "Objects Count");
        void Start()
        {
            totalNum = xRadio * zRadio * 4;
            cubes = new TransformAccessArray(totalNum);
            float[] distanceArr = new float[totalNum];
            for (int i = -xRadio; i < xRadio; i++)
            {
                for (int j = -zRadio; j < zRadio; j++)
                {
                    var instance = Instantiate(archGo).transform;
                    instance.position = new Vector3(1.1f * i, 0, 1.1f * j);
                    var index = (i + xRadio) * 2 * xRadio + j + zRadio;
                    distanceArr[index] = Vector3.Distance(Vector3.zero, instance.position);
                    cubes.Add(instance);
                }
            }
            distances = new NativeArray<float>(distanceArr, Allocator.Persistent);
            archGo.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            using(profilerMarker.Auto(totalNum))
            {
                var waveCubeJob = new WaveCubesJob
                {
                    amplitude = amplitude,
                    distances = distances,
                    frequency = frequency,
                    waveLength = waveLength,
                    time = Time.time
                };
                var jobHandle = waveCubeJob.Schedule(cubes);
                jobHandle.Complete();
            }            
        }

        private void OnDestroy()
        {
            cubes.Dispose();
            distances.Dispose();
        }
    }
}

