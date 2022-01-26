using UnityEngine;
using NoZ.Modules;
using Unity.Services.Authentication;
using System.Collections;

namespace NoZ.Zisle.Modules
{
    public class AuthenticationModule : IProvidesAuthentication
    {
        void IModule<IProvidesAuthentication>.OnLoaded()
        {
            Module<IProvidesCoroutines>.Instance.StartCoroutine(Authenticate());
        }

        void IModule<IProvidesAuthentication>.OnUnloaded()
        {
        }

        private IEnumerator Authenticate ()
        {
            // Wait until unity services are initialized
            yield return Module<IProvidesUnityServices>.Instance.WaitForInitialize();

            // Sign in anonymously
            var async = AuthenticationService.Instance.SignInAnonymouslyAsync();
            while (!async.IsCompleted)
                yield return async;

            if (AuthenticationService.Instance.IsSignedIn)
                Debug.Log("Anonymously signed in");
            else
                Debug.LogError("Failed to anonymously sign in");
        }
    }
}
