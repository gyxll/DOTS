using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;

namespace Jobs.DOD
{
    public class SurgeWithJob : MonoBehaviour
    {
        /// <summary>
        /// 距离判断阈值
        /// </summary>
        public const float DistanceFlag = 0.05f;
        /// <summary>
        /// 立方体的原型
        /// </summary>
        public GameObject cubeArchtype;
        /// <summary>
        /// 出生区域
        /// </summary>
        public BoxCollider startArea;
        private Vector3 startPos;
        private Vector3 startRange;
        /// <summary>
        /// 终点区域
        /// </summary>
        public BoxCollider endArea;
        private Vector3 endPos;
        private Vector3 endRange;
        /// <summary>
        /// 总共生成数量，要根据生成的间隔tickTime、每次生成的数量generationNumPerTicktime、和移动速度moveSpeed动态设置，否则会出现生成停滞的问题
        /// </summary>
        [Range(10, 10000)] public int generationTotalNum = 500;
        /// <summary>
        /// 每次间隔时间生成的数量
        /// </summary>
        [Range(1, 60)] public int generationNumPerTicktime = 5;
        /// <summary>
        /// 生成的时间间隔
        /// </summary>
        [Range(0.1f, 1.0f)] public float tickTime = 0.2f;
        /// <summary>
        /// 旋转速度范围
        /// </summary>
        [Range(90, 180)] public float rotateRange = 180;
        /// <summary>
        /// 移动速度
        /// </summary>
        public float moveSpeed = 5;
        /// <summary>
        /// 立方体的对象池
        /// </summary>
        private ObjectPool<GameObject> cubesPool;
        /// <summary>
        /// 下次更新时间
        /// </summary>
        private float nextUpdateTime = 0;
        #region Job相关数据
        /// <summary>
        /// 提供给Jobs访问的数据结构
        /// </summary>
        private TransformAccessArray cubesAccessArray;
        /// <summary>
        /// GameObject实例和实例在 cubesAccessArray 中的索引映射字典
        /// </summary>
        private Dictionary<int, int> insanceIndexDic;
        /// <summary>
        /// cubesAccessArray 内索引和GameObject实例的映射字典
        /// </summary>
        private Dictionary<int, GameObject> indexObjDic;
        private Vector3[] targetPosArr;
        private float[] rotateSpeedArr;
        private float[] moveSpeedArr;
        /// <summary>
        /// 是否抵达目的地
        /// </summary>
        private bool[] isReachArr;
        private NativeArray<Vector3> targetPosNativeArr;
        private NativeArray<float> rotateSpeedNativeArr;
        private NativeArray<float> moveSpeedNativeArr;
        /// <summary>
        /// 是否抵达目的地
        /// </summary>
        private NativeArray<bool> isReachNativeArr;
        #endregion
        static readonly ProfilerMarker profilerMarker = new("CubesMarchWithJob");
        // Start is called before the first frame update：
        void Start()
        {
            cubesPool = new ObjectPool<GameObject>(CreteCube, OnGetCube, OnRecycleCube, OnDestroyCube, true, 10, generationTotalNum);
            startPos = startArea.transform.position;
            startRange = startArea.size * 0.5f;
            endPos = endArea.transform.position;
            endRange = endArea.size * 0.5f;
            //初始化Jobs相关数据
            cubesAccessArray = new TransformAccessArray(generationTotalNum);
            insanceIndexDic = new Dictionary<int, int>(generationTotalNum);
            indexObjDic = new Dictionary<int, GameObject>(generationTotalNum);
            targetPosArr = new Vector3[generationTotalNum];
            rotateSpeedArr = new float[generationTotalNum];
            moveSpeedArr = new float[generationTotalNum];
            isReachArr = new bool[generationTotalNum];
            for (int i = 0; i < generationTotalNum; i++)
            {
                isReachArr[i] = true;
            }
            targetPosNativeArr = new NativeArray<Vector3>(generationTotalNum, Allocator.Persistent);
            rotateSpeedNativeArr = new NativeArray<float>(generationTotalNum, Allocator.Persistent);
            moveSpeedNativeArr = new NativeArray<float>(generationTotalNum, Allocator.Persistent);
            isReachNativeArr = new NativeArray<bool>(generationTotalNum, Allocator.Persistent);
        }

        private void OnDestroyCube(GameObject obj)
        {
            Destroy(obj);
        }

        private void OnRecycleCube(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void OnGetCube(GameObject obj)
        {
            obj.SetActive(true);
            RandomMoveInfo(obj);            
        }
        /// <summary>
        /// 创建单个立方体实例
        /// </summary>
        /// <returns></returns>
        private GameObject CreteCube()
        {
            var newCube = Instantiate(cubeArchtype);
            newCube.hideFlags = HideFlags.HideInHierarchy;
            cubesAccessArray.Add(newCube.transform);
            insanceIndexDic.Add(newCube.GetInstanceID(), cubesAccessArray.length - 1);
            indexObjDic.Add(cubesAccessArray.length - 1, newCube);
            return newCube;
        }

        private Vector3 tempVec3 = Vector3.zero;
        /// <summary>
        /// 设置随机目标
        /// </summary>
        /// <param name="newCube"></param>
        private void RandomMoveInfo(GameObject newCube)
        {
            tempVec3.Set(UnityEngine.Random.Range(-startRange.x, startRange.x), 0, UnityEngine.Random.Range(-startRange.z, startRange.z));
            newCube.transform.position = startPos + tempVec3;
            tempVec3.Set(UnityEngine.Random.Range(-endRange.x, endRange.x), 0, UnityEngine.Random.Range(-endRange.z, endRange.z));
            var targetPos = endPos + tempVec3;
            var rotateSpeed = UnityEngine.Random.Range(-rotateRange, rotateRange);
            //将数据报错到和Jobs的NativeArray对应的数组中
            int nativeIndex = insanceIndexDic[newCube.GetInstanceID()];
            targetPosArr[nativeIndex] = targetPos;
            rotateSpeedArr[nativeIndex] = rotateSpeed;
            moveSpeedArr[nativeIndex] = moveSpeed;
            isReachArr[nativeIndex] = false;
        }

        private void Update()
        {
            using (profilerMarker.Auto())
            {
                if (Time.time > nextUpdateTime)
                {
                    for (int i = 0; i < generationNumPerTicktime; i++)
                    {
                        if (cubesPool.CountAll < generationTotalNum)
                        {
                            cubesPool.Get();
                        }
                        else
                        {
                            nextUpdateTime += tickTime;
                            break;
                        }
                    }
                    nextUpdateTime += tickTime;
                }
                //采样数据拷贝方式速度比一个一个设置快得多
                targetPosNativeArr.CopyFrom(targetPosArr);
                rotateSpeedNativeArr.CopyFrom(rotateSpeedArr);
                moveSpeedNativeArr.CopyFrom(moveSpeedArr);
                isReachNativeArr.CopyFrom(isReachArr);
                var randomMoveAndRotateJob = new RandomMoveAndRotateJob
                {
                    targetPosArr = targetPosNativeArr,
                    rotateSpeedArr = rotateSpeedNativeArr,
                    moveSpeedArr = moveSpeedNativeArr,
                    isReachArr = isReachNativeArr,
                    distanceFlag = DistanceFlag,
                    deltaTime = Time.deltaTime
                };
                var jobHandle = randomMoveAndRotateJob.Schedule(cubesAccessArray);
                jobHandle.Complete();
                //将Job执行完成后的数据读回 isReachArr，将已抵达的内容放回对象池
                isReachNativeArr.CopyTo(isReachArr);
                for (int i = 0; i < generationTotalNum; i++)
                {
                    if (isReachArr[i] && indexObjDic.ContainsKey(i) && indexObjDic[i].activeSelf)
                    {
                        cubesPool.Release(indexObjDic[i]);
                    }
                }
            }                
        }

        private void OnDestroy()
        {
            cubesPool.Dispose();
            cubesAccessArray.Dispose();
            targetPosNativeArr.Dispose();
            rotateSpeedNativeArr.Dispose();
            moveSpeedNativeArr.Dispose();
            isReachNativeArr.Dispose();
        }
    }
}

