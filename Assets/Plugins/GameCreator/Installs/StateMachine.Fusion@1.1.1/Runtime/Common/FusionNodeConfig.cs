using System;
using Fusion;
using NinjutsuGames.StateMachine.Runtime;

namespace NinjutsuGames.StateMachine.Fusion.Runtime
{
    [Serializable]
    public class FusionNodeConfig : TNetworkConfig
    {
        public RpcTargets rpcTargets = RpcTargets.Proxies;
        [field:NonSerialized] public PlayerRef Sender { get; set; }
        [field:NonSerialized] public int Index { get; set; }
    }
}