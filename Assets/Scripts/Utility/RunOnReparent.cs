using Unity.Netcode;

namespace NoZ.Zisle
{
    public abstract class RunOnReparent : NetworkBehaviour
    {
        public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
        {
            base.OnNetworkObjectParentChanged(parentNetworkObject);

            Run();
        }

        protected abstract void Run();
    }
}
