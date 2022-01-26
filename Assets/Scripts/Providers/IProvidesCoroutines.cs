using NoZ.Modules;
using System.Collections;
using UnityEngine;

namespace NoZ.Zisle
{
    public interface IProvidesCoroutines : IModule<IProvidesCoroutines>
    {
        Coroutine StartCoroutine(IEnumerator routine);
    }
}
