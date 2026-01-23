using Cysharp.Threading.Tasks;
using Firebase;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public class FirebaseInitializer : IAsyncStartable
{
    public async UniTask StartAsync(CancellationToken cancellation = default)
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase 초기화 완료");
        }
        else
        {
            Debug.LogError($"Firebase 초기화 실패 이유 : {dependencyStatus}");
        }
    }
}
