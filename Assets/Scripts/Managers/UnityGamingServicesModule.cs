using NoZ.Modules;
using System.Collections;
using Unity.Services.Core;

namespace NoZ.Zisle.Modules
{
    public class UnityServicesModule : IProvidesUnityServices
    {
        public bool IsInitialized => UnityServices.State == ServicesInitializationState.Initialized;

        void IModule<IProvidesUnityServices>.OnLoaded()
        {
            Module<IProvidesCoroutines>.Instance.StartCoroutine(Initialize());
        }

        void IModule<IProvidesUnityServices>.OnUnloaded()
        {
        }

        private IEnumerator Initialize ()
        {
            var asyncInit = UnityServices.InitializeAsync();
            while (!asyncInit.IsCompleted)
                yield return asyncInit;

            if(IsInitialized)
                UnityEngine.Debug.Log("UnityServices initialized");
            else
                UnityEngine.Debug.LogError("UnityServices failed to initialize");
        }

        IEnumerator IProvidesUnityServices.WaitForInitialize()
        {
            while(!IsInitialized)
                yield return null;
        }
    }
}
