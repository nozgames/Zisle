using NoZ.Modules;
using System.Collections;

namespace NoZ.Zisle
{
    public interface IProvidesUnityServices : IModule<IProvidesUnityServices>
    {
        bool IsInitialized { get; }

        IEnumerator WaitForInitialize();
    }
}
