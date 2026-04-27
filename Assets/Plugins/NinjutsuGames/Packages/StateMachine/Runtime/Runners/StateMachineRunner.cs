using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NinjutsuGames.StateMachine.Runtime
{
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/state-machine-2/state-machine-runner")]
    [AddComponentMenu("Game Creator/State Machine/State Machine Runner")]
    public class StateMachineRunner : TLocalVariables, INameVariable
    {
        public bool isEmbedded;
        public StateMachineAsset cloneAsset;
        public StateMachineAsset originalAsset;
        public StateMachineAsset stateMachineAsset;
        private BaseGraphProcessor processor;
        
        
        // MEMBERS: -------------------------------------------------------------------------------
    
        [SerializeReference] private NameVariableRuntime m_Runtime = new();
        [SerializeReference] private NameVariableRuntime m_SubStatesRuntime = new();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        public IdString UniqueId => m_SaveUniqueID.Get;

        public NameVariableRuntime Runtime => m_Runtime;
        public NameVariableRuntime SubRuntime => m_SubStatesRuntime;
        
        // EVENTS: --------------------------------------------------------------------------------
        
        private event Action<string> EventChange;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void Awake()
        {
            Initialize();
            if(StateMachineRunnerInstances.Instance) StateMachineRunnerInstances.Instance.Register(stateMachineAsset, this);
        }

        private void OnEnable()
        {
            if(!Application.isPlaying) return;
            
            // Validate embedded graph setup
            if (isEmbedded)
            {
                ValidateEmbeddedGraph();
            }
        }
        
        private void ValidateEmbeddedGraph()
        {
            // Only perform minimal validation to avoid conflicts with automatic system
            if (isEmbedded && stateMachineAsset && !stateMachineAsset.IsLinkedToScene())
            {
                var scene = gameObject.scene;
                if (!scene.IsValid())
                {
                    scene = SceneManager.GetActiveScene();
                }
                
                // Silently fix scene linking for embedded graphs
                if (scene.IsValid())
                {
                    stateMachineAsset.LinkToScene(scene);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(!Application.isPlaying) return;
            if(StateMachineRunnerInstances.Instance) StateMachineRunnerInstances.Instance.Unregister(stateMachineAsset);

            // Clean up processor resources
            if (processor != null && processor is IDisposable disposableProcessor)
            {
                disposableProcessor.Dispose();
                processor = null;
            }

            // Clean up event handlers
            m_Runtime.EventChange -= OnRuntimeChange;
            EventChange = null;
        }

        private void OnValidate()
        {
            // Only validate embedded graphs in editor, not at runtime
            if (Application.isPlaying)
            {
                OnEnable();
            }
        }

        private void Start()
        {
            processor?.Run(new Args(gameObject), null);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize(bool run = false)
        {
            m_Runtime.OnStartup();
            m_Runtime.Merge(m_SubStatesRuntime);
            m_Runtime.EventChange += OnRuntimeChange;
            
            base.Awake();
            
            if (stateMachineAsset != null) processor = new StateMachineGraphProcessor(stateMachineAsset, gameObject);
            if(run) processor?.Run(new Args(gameObject), null);
        }
        
        /// <summary>
        /// Returns true if the variable exists 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Exists(string name)
        {
            return m_Runtime.Exists(name);
        }
        
        /// <summary>
        /// Returns the value of the variable 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Get(string name)
        {
            return m_Runtime.Get(name);
        }

        /// <summary>
        /// Sets the value of the variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Set(string name, object value)
        {
            m_Runtime.Set(name, value);
        }

        /// <summary>
        /// Registers a callback to be invoked when the variable changes
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="target"></param>
        public void Register(Action<string> callback, GameObject target)
        {
            EventChange += callback;
        }

        /// <summary>
        /// Unregisters a callback to be invoked when the variable changes
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="target"></param>
        public void Unregister(Action<string> callback, GameObject target)
        {
            EventChange -= callback;
        }

        /// <summary>
        /// Registers a callback to be invoked when the variable changes
        /// </summary>
        /// <param name="callback"></param>
        public void Register(Action<string> callback)
        {
            EventChange += callback;
        }
        
        /// <summary>
        /// Unregisters a callback to be invoked when the variable changes
        /// </summary>
        /// <param name="callback"></param>
        public void Unregister(Action<string> callback)
        {
            EventChange -= callback;
        }
        
        /// <summary>
        /// Runs a node.
        /// If the context is null the action run in the State Machine asset. 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="context"></param>
        public void RunNode(string nodeId, Args args)
        {
            processor?.RunNode(nodeId, args);
        }
        
        /// <summary>
        /// Stops a node.
        /// If the context is null the action run in the State Machine asset. 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="context"></param>
        public void StopNode(string nodeId, GameObject context)
        {
            processor?.StopNode(nodeId, context);
        }

        /// <summary>
        /// Disables a node.
        /// If the context is null the action run in the State Machine asset. 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="context"></param>
        public void DisableNode(string nodeId, GameObject context)
        {
            processor?.DisableNode(nodeId, context);
        }

        /// <summary>
        /// Enables a node.
        /// If the context is null the action run in the State Machine asset. 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="context"></param>
        public void EnableNode(string nodeId, GameObject context)
        {
            processor?.EnableNode(nodeId, context);
        }
        
        public bool IsNodeEnabled(string nodeId, GameObject context)
        {
            return processor.IsNodeEnabled(nodeId, context);
        }
        
        public bool IsNodeRunning(string nodeId, GameObject context)
        {
            return processor.IsNodeRunning(nodeId, context);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void OnRuntimeChange(string name)
        {
            EventChange?.Invoke(name);
        }

        // IGAMESAVE: -----------------------------------------------------------------------------

        public override Type SaveType => typeof(SaveSingleNameVariables);

        public override object GetSaveData(bool includeNonSavable)
        {
            return m_SaveUniqueID.SaveValue
                ? new SaveSingleNameVariables(m_Runtime)
                : null;   
        }

        public override Task OnLoad(object value)
        {
            var saveData = value as SaveSingleNameVariables;
            if (saveData == null || !m_SaveUniqueID.SaveValue)
            {
                return Task.FromResult(saveData != null || !m_SaveUniqueID.SaveValue);
            }
            var candidates = saveData.Variables.ToArray();
            foreach (var candidate in candidates)
            {
                if (!m_Runtime.Exists(candidate.Name)) continue;
                m_Runtime.Set(candidate.Name, candidate.Value);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the provided state machine asset within a dynamically created GameObject.
        /// </summary>
        /// <param name="stateMachineAsset">The state machine asset to be executed.</param>
        /// <param name="dontDestroyOnload">Determines if the created GameObject persists across scene loads.</param>
        public static GameObject Run(StateMachineAsset stateMachineAsset, bool dontDestroyOnload)
        {
            if (stateMachineAsset == null) return null;
            var gameObject = new GameObject(stateMachineAsset.name);
            if (dontDestroyOnload) DontDestroyOnLoad(gameObject);
            var runner = gameObject.AddComponent<StateMachineRunner>();
            runner.stateMachineAsset = stateMachineAsset;
            runner.m_Runtime = new NameVariableRuntime(stateMachineAsset.NameList);
            runner.Initialize();
            return gameObject;
        }
    }
}