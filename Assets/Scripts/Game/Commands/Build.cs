using UnityEngine;

namespace NoZ.Zisle.Commands
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Build")]
    public class Build : ActorCommand, IExecuteOnServer
    {
        public void ExecuteOnServer(Actor source, Actor target)
        {
            if(target is Building building)
                building.Build(source.GetAttributeValue(ActorAttribute.Build) * target.GetAttributeValue(ActorAttribute.Build));
        }
    }
}