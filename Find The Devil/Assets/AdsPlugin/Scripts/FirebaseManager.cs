using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
public class FirebaseManager : MonoBehaviour
{
    private static void PrintStatus(string msg)
    {
        AnalyticsManager.Instance.ShowLogs(msg);
    }
    #region Firebase

    private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
    private static bool _firebaseInitialized = false;
    public static bool IsFirebaseInitialized()
    {
        return _firebaseInitialized;
    }
    public void OnFireBase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            _dependencyStatus = task.Result;
            if (_dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + _dependencyStatus);
            }
        });
    }
    private void InitializeFirebase()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseAnalytics.SetUserProperty(FirebaseAnalytics.UserPropertySignUpMethod, "Google");
        _firebaseInitialized = true;
        var app = FirebaseApp.DefaultInstance;
        FirebaseApp.LogLevel = AnalyticsManager.Instance.AreLogsEnabled() ? LogLevel.Verbose : LogLevel.Error;
        var defaults = new Dictionary<string, object>
        {
            {AdsConstant.AdsTimer, 30}
        };
        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
            .ContinueWithOnMainThread(task =>
            {
                FetchDataAsync();
            });
    }
    private Task FetchDataAsync()
    {
        PrintStatus("Fetching data...");
        var fetchTask =
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    private void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            PrintStatus("Fetch canceled.");
        }
        else if (fetchTask.IsFaulted)
        {
            PrintStatus("Fetch encountered an error.");
        }
        else if (fetchTask.IsCompleted)
        {
            PrintStatus("Fetch completed successfully!");
        }
        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
        switch (info.LastFetchStatus)
        {
            case Firebase.RemoteConfig.LastFetchStatus.Success:

                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        PrintStatus($"Remote data loaded and ready (last fetch time {info.FetchTime}).");
                        GetRemoteData();
                    });
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case Firebase.RemoteConfig.FetchFailureReason.Error:
                        PrintStatus("Fetch failed for unknown reason");
                        break;
                    case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                        PrintStatus("Fetch throttled until " + info.ThrottledEndTime);
                        break;
                }
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Pending:
                PrintStatus("Latest Fetch call still pending.");
                break;
        }
    }
    private static void GetRemoteData()
    {
        GameInitializer.Instance.adTimer = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(
            AdsConstant.AdsTimer).LongValue;
    }
    #endregion
}