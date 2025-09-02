using UnityEngine;
// https://developer.android.com/games/engines/unity/memory-advice
// https://stackoverflow.com/a/9428660/1111918
public class MemoryAdvisor : MonoBehaviour
{
    private readonly int[] _advisorSDKs = { 33, 32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22 };
    private const double CRITICAL_HEAP = 0.2d;
    private const long CRITICAL_RAM_MB = 400L;
    private const double NEAR_CRITICAL_HEAP = 0.3d;
    private const long NEAR_CRITICAL_RAM_MB = 500L;
    private const string MemoryAdvisorString = "MemoryAdvisor";
    public enum MemoryState
    {
        OK,
        ApproachingLimit,
        Critical
    }
    private static MemoryAdvisor sInstance;
    private MemoryState memoryState = MemoryState.OK;
    public MemoryState GetMemoryState(bool recalculate = false)
    {
        if (recalculate)
        {
            CalculateMemoryState();
            ////Debug.LogError("aaa memState=" + memoryState);
        }
        return memoryState;
    }
    public delegate void MemoryStateChanged(MemoryState _memoryState, long avlRam);
    public static event MemoryStateChanged onMemoryStateChanged;
    public void AddOnMemoryStateChanged(MemoryStateChanged callback = null)
    {
        onMemoryStateChanged += callback;
    }
    public void RemoveOnMemoryStateChanged(MemoryStateChanged callback = null)
    {
        onMemoryStateChanged -= callback;
    }

    /*public static MemoryAdvisor Instance
    {
        get
        {
            if (sInstance == null)
            {
                sInstance = GameObject.FindObjectOfType<MemoryAdvisor>();
                if (sInstance == null)
                {
                    sInstance = new GameObject(MemoryAdvisorString).AddComponent<MemoryAdvisor>();
                }
                sInstance.Awake();
            }
            return sInstance;
        }
    }*/

    private bool isAwakeDone;

    private void Awake()
    {
        if (sInstance && sInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        //long maxRam = 3619;
        //long avlRam = 500;//132;
        //long usedRamInBytes = maxRam - avlRam;
        //double availableRamInPercentage = 1d - (double)usedRamInBytes / (double)maxRam;
        ////Debug.LogError("aaa avl ram= " + availableRamInPercentage);

        if (!isAwakeDone)
        {
            sInstance = this;
            isAwakeDone = true;
            DontDestroyOnLoad(this.gameObject);
        }

    }

    private void Start()
    {
        CalculateMemoryState();
    }

    private void OnRemoteConfigLoad()
    {
        CalculateMemoryState();
    }

    private void CalculateMemoryState()
    {
#if UNITY_EDITOR
        isSDKMatched(29);
#elif UNITY_ANDROID
    calculateForAndroid();
#endif
    }

    private AndroidJavaClass clsRuntime;
    private AndroidJavaObject objRuntime;

    private AndroidJavaClass unityPlayerClass;
    private AndroidJavaObject currentActivity, systemService, memoryInfo;
    private int _sdkVer = -1;
    private long _maxHeap;
    private long _totalMemory;
    private long _freeMemory;
    private long _usedMemory;

    private double _availableHeapPercentage;

    //------------------- Ram
    private long _maxRam;
    private long _avlRam;

    private long _avlRamMb;
    private long _usedRamInBytes;
    //long usedRamInPercentage = usedRamInBytes * 100 / maxRam;
    private double _availableRamInPercentage;
    private const string Str1 = "android.os.Build$VERSION", SDK_INT = "SDK_INT", Str2 = "java.lang.Runtime", GetRuntime = "getRuntime",
        Str3 = "com.unity3d.player.UnityPlayer", CurrentActivity = "currentActivity", Activity = "activity", GetSystemService = "getSystemService",
        Str4 = "android.app.ActivityManager$MemoryInfo", GetMemoryInfo = "getMemoryInfo", MaxMemory = "maxMemory", TotalMemory = "totalMemory",
        FreeMemory = "freeMemory", TotalMem = "totalMem", AvailMem = "availMem";
    private void calculateForAndroid()
    {

        if (memoryInfo == null)
        {
            AndroidJavaClass objVersion = new AndroidJavaClass(Str1);
            _sdkVer = objVersion.GetStatic<int>(SDK_INT);
            ////Debug.LogError("aaa sdkVer=" + sdkVer);
            if (isSDKMatched(_sdkVer))
            {
                clsRuntime = new AndroidJavaClass(Str2);
                objRuntime = clsRuntime.CallStatic<AndroidJavaObject>(GetRuntime);

                unityPlayerClass = new AndroidJavaClass(Str3);
                currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>(CurrentActivity);
                systemService = currentActivity.Call<AndroidJavaObject>(GetSystemService, Activity);
                memoryInfo = new AndroidJavaObject(Str4);
            }
        }

        if (isSDKMatched(_sdkVer))
        {
            systemService.Call(GetMemoryInfo, memoryInfo);
             _maxHeap = objRuntime.Call<long>(MaxMemory);// maximum heap size
             _totalMemory = objRuntime.Call<long>(TotalMemory);
            _freeMemory = objRuntime.Call<long>(FreeMemory);
           _usedMemory = _totalMemory - _freeMemory;

            _availableHeapPercentage = 1d - (double)_usedMemory / (double)_maxHeap;

            //------------------- Ram
             _maxRam = memoryInfo.Get<long>(TotalMem);
             _avlRam = memoryInfo.Get<long>(AvailMem);

             _avlRamMb = ((_avlRam / 1024L) / 1024L);
             _usedRamInBytes = _maxRam - _avlRam;
            //long usedRamInPercentage = usedRamInBytes * 100 / maxRam;
             _availableRamInPercentage = 1d - (double)_usedRamInBytes / (double)_maxRam;

            //Debug.LogError("aaa Heap maxMemory=" + maxHeap + "-" + ((maxHeap / 1024L) / 1024L) + "MB-" + (((maxHeap / 1024L) / 1024L) / 1024) + "GB, totalMemory=" + totalMemory + "-" + ((totalMemory / 1024L) / 1024L) + "MB-" + (((totalMemory / 1024L) / 1024L) / 1024L) + "GB, freeMemory=" + freeMemory + "-" + ((freeMemory / 1024L) / 1024L) + "MB-" + (((freeMemory / 1024L) / 1024L) / 1024L) + "GB, usedMemory=" + usedMemory + "-" + ((usedMemory / 1024L) / 1024L) + "MB-" + (((usedMemory / 1024L) / 1024L) / 1024L) + "GB, avper=" + (float)availableHeapPercentage);
            //Debug.LogError("aaa Ram maxRam=" + ((maxRam / 1024L) / 1024L) + "MB-" + (((maxRam / 1024L) / 1024L) / 1024) + "GB, avlRam=" + avlRamMb + "MB-" + (((avlRam / 1024L) / 1024L) / 1024L) + "GB, usedRamInBytes=" + ((usedRamInBytes / 1024L) / 1024L) + "MB-" + (((usedRamInBytes / 1024L) / 1024L) / 1024L) + "GB, avlRamPercent=" + availableRamInPercentage);

            if (_availableHeapPercentage < CRITICAL_HEAP || _avlRamMb < CRITICAL_RAM_MB || (_maxHeap - _usedMemory) > _avlRam)
            {
                if (memoryState != MemoryState.Critical && onMemoryStateChanged != null)
                {
                    onMemoryStateChanged(MemoryState.Critical, _avlRam);
                }
                memoryState = MemoryState.Critical;
                //Debug.LogError("MemoryState.Critical");
            }
            else if (_availableHeapPercentage < NEAR_CRITICAL_HEAP || _avlRamMb < NEAR_CRITICAL_RAM_MB || (_maxHeap - _usedMemory) > _avlRam)
            {
                if (memoryState != MemoryState.ApproachingLimit && onMemoryStateChanged != null)
                {
                    onMemoryStateChanged(MemoryState.ApproachingLimit, _avlRam);
                }
                memoryState = MemoryState.ApproachingLimit;
                //Debug.LogError("MemoryState.ApproachingLimit");
            }
            else
            {
                if (memoryState != MemoryState.OK && onMemoryStateChanged != null)
                {
                    onMemoryStateChanged(MemoryState.OK, _avlRam);
                }
                memoryState = MemoryState.OK;
                //Debug.LogError(" MemoryState.OK");
            }
        }
        else
        {
            if (memoryState != MemoryState.OK && onMemoryStateChanged != null)
            {
                onMemoryStateChanged(MemoryState.OK, -1L);
            }
            memoryState = MemoryState.OK;
        }
    }
    private bool isSDKMatched(int _sdkVer)
    {
        for (int i = 0; i < _advisorSDKs.Length; i++)
        {
            if (_advisorSDKs[i] == _sdkVer)
            {
                return true;
            }
        }
        return false;
    }
}