namespace NoZ.Zisle
{
    public interface IExecuteOnServer
    {
        public void ExecuteOnServer(Actor source, Actor target);
    }

    public interface IExecuteOnClient
    {
        public void ExecuteOnClient(Actor source, Actor target);
    }

    public abstract class ActorCommand : NetworkScriptableObject<ActorCommand>
    {
    }
}
