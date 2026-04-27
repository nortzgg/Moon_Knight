using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;

namespace NinjutsuGames.StateMachine.Runtime
{
    public class StateMachineSettings : AssetRepository<StateMachineRepository>
    {
        public override IIcon Icon => new IconStateMachine(ColorTheme.Type.TextLight);
        public override string Name => "State Machines";
    }

}