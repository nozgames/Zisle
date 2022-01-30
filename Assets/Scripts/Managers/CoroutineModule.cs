using NoZ.Modules;
using System.Collections;
using UnityEngine;

namespace NoZ.Zisle.Modules
{
    public class CoroutineModule : IProvidesCoroutines
    {
        private class Host : MonoBehaviour { }

        private Host _host;

        void IModule<IProvidesCoroutines>.OnLoaded()
        {
            _host = (new GameObject()).AddComponent<Host>(); ;
            _host.gameObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(_host.gameObject);
        }

        void IModule<IProvidesCoroutines>.OnUnloaded()
        {
            UnityEngine.Object.Destroy(_host.gameObject);
            _host = null;
        }

        Coroutine IProvidesCoroutines.StartCoroutine(IEnumerator routine) => 
            _host.StartCoroutine(routine);
    }
}
