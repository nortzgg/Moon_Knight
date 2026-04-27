using System;
using System.Collections.Generic;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
    [Serializable]
    internal class AssetEntry
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] private AssetVersion version = new();
        [SerializeField] private AssetRelease release = new();
        [SerializeField] private string[] changes = Array.Empty<string>(); // Legacy support
        [SerializeField] private AssetChanges assetChanges = new(); // New sub-categories structure
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public AssetVersion Version => version;
        public AssetRelease Release => release;
        
        /// <summary>
        /// Legacy changes array for backward compatibility
        /// </summary>
        public string[] Changes => changes;
        
        /// <summary>
        /// New sub-categorized changes structure
        /// </summary>
        public AssetChanges AssetChanges => assetChanges;
        
        /// <summary>
        /// Returns true if the new sub-categories structure has any content
        /// </summary>
        public bool HasSubCategorizedChanges => 
            assetChanges.New.Length > 0 || 
            assetChanges.Enhanced.Length > 0 || 
            assetChanges.Changed.Length > 0 || 
            assetChanges.Removed.Length > 0 || 
            assetChanges.Fixed.Length > 0;

        [field: NonSerialized] public State State { get; set; } = State.Loading;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public AssetEntry()
        { }

        public AssetEntry(State state) : this()
        {
            State = state;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility with single changes list
        /// </summary>
        public AssetEntry(string assetVersion, string date, List<string> allChanges)
        {
            version = new AssetVersion(assetVersion);
            release = new AssetRelease(date);
            changes = allChanges.ToArray();
            assetChanges = new AssetChanges(); // Initialize empty sub-categories
        }
        
        /// <summary>
        /// New constructor with sub-categorized changes
        /// </summary>
        public AssetEntry(string assetVersion, string date, AssetChanges categorizedChanges)
        {
            version = new AssetVersion(assetVersion);
            release = new AssetRelease(date);
            assetChanges = categorizedChanges;
            
            // For backward compatibility, combine all changes into the legacy array
            var allChanges = new List<string>();
            allChanges.AddRange(categorizedChanges.New);
            allChanges.AddRange(categorizedChanges.Enhanced);
            allChanges.AddRange(categorizedChanges.Changed);
            allChanges.AddRange(categorizedChanges.Removed);
            allChanges.AddRange(categorizedChanges.Fixed);
            changes = allChanges.ToArray();
        }
        
        public AssetEntry(AssetVersion assetVersion)
        {
            version = assetVersion;
        }
    }
}