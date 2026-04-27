using System;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
    [Serializable]
    internal class AssetChanges
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private string[] @new = Array.Empty<string>();
        [SerializeField] private string[] enhanced = Array.Empty<string>();
        [SerializeField] private string[] changed = Array.Empty<string>();
        [SerializeField] private string[] removed = Array.Empty<string>();
        [SerializeField] private string[] @fixed = Array.Empty<string>();

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public string[] New => this.@new;
        public string[] Enhanced => this.enhanced;
        public string[] Changed => this.changed;
        public string[] Removed => this.removed;
        public string[] Fixed => this.@fixed;
        
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public AssetChanges()
        {
        }
        
        public AssetChanges(string[] newChanges, string[] enhancedChanges, string[] changedChanges, 
                           string[] removedChanges, string[] fixedChanges)
        {
            this.@new = newChanges ?? Array.Empty<string>();
            this.enhanced = enhancedChanges ?? Array.Empty<string>();
            this.changed = changedChanges ?? Array.Empty<string>();
            this.removed = removedChanges ?? Array.Empty<string>();
            this.@fixed = fixedChanges ?? Array.Empty<string>();
        }
    }
}