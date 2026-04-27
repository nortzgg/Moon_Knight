using System;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using RuntimePaths = NinjutsuGames.StateMachine.Runtime.RuntimePaths;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(StateMachineVersion))]
    public class StateMachineVersionDrawer : PropertyDrawer
    {
        private const string ID = "state-machine-2";
        private static readonly string DOCS_LINK = $"https://docs.ninjutsugames.com/game-creator-2/{ID}";
        
        private const string USS_PATH = RuntimePaths.EDITOR + "Settings/Versions/StyleSheets/VersionInfo";
        private const string STORE_LINK = "https://www.ninjutsugames.com/link/{0}";
        
        private static readonly IIcon ICON = new IconCubeOutline(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_DOCS = new IconInfoOutline(ColorTheme.Type.TextLight);
        
        private static readonly IIcon ICON_INSTALLED_YES = new IconCircleSolid(ColorTheme.Type.Green);
        private static readonly IIcon ICON_INSTALLED_UPD = new IconCircleSolid(ColorTheme.Type.Yellow);
        private static readonly IIcon ICON_INSTALLED_NEW = new IconCircleSolid(ColorTheme.Type.Teal);
        
        private const string NAME_LOADING = "GC-Updates-Loading";
        private const string NAME_CONTAINER_ROOT = "GC-Updates-Container-Root";
        private const string NAME_CONTAINER_BODY = "GC-Updates-Container-Body";
        private const string NAME_CONTAINER_FOOT = "GC-Updates-Container-Foot";
        private const string NAME_ARROW = "Head-Arrow";
        
        private const string NAME_ASSET_ROOT = "GC-Updates-Asset-Root";
        private const string NAME_ASSET_HEAD = "GC-Updates-Asset-Head";
        private const string NAME_ASSET_BODY = "GC-Updates-Asset-Body";
        
        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Root;
        private VisualElement m_Body;
        private VisualElement m_Foot;
        
        // PAINT METHOD: --------------------------------------------------------------------------

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VersionsManager.Initialize();

            m_Root = new VisualElement { name = NAME_CONTAINER_ROOT };
            m_Body = new VisualElement { name = NAME_CONTAINER_BODY };
            m_Foot = new VisualElement
            {
                name = NAME_CONTAINER_FOOT,
                style =
                {
                    display = DisplayStyle.None
                }
            };

            var styleSheets = StyleSheetUtils.Load(USS_PATH);
            foreach (var sheet in styleSheets) m_Root.styleSheets.Add(sheet);

            RefreshFoot();
            RefreshBody();
            VersionsManager.EventChange += RefreshBody;
            
            m_Root.Add(m_Body);
            m_Root.Add(m_Foot);
            m_Root.Add(new SpaceSmallest());
            
            return m_Root;
        }
        
        private void RefreshFoot()
        {
            var remindUpdates = new Toggle
            {
                value = VersionsNotifications.RemindUpdates
            };

            var remindLabel = new Label("Remind me when there is a new update");

            remindUpdates.RegisterValueChangedCallback(changeEvent =>
            {
                VersionsNotifications.RemindUpdates = changeEvent.newValue;
            });
            
            m_Foot.Add(remindUpdates);
            m_Foot.Add(remindLabel);
            
            var buttonDocumentation = new Button();
            var docIcon = new Image { image = ICON_DOCS.Texture };
            buttonDocumentation.Add(docIcon);
            buttonDocumentation.clicked += () =>
            {
                Application.OpenURL(DOCS_LINK);
            };
            m_Foot.Add(buttonDocumentation);        }
        
        private void RefreshBody()
        {
            m_Body.Clear();

            switch (VersionsManager.Latest.State)
            {
                case State.Loading: RefreshLoading(); break;
                case State.Ready: RefreshReady(); break;
                case State.Error: RefreshError(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void RefreshLoading()
        {
            OfflineVersion(true);
        }
        
        private void RefreshError()
        {
            OfflineVersion(false);
        }

        private void OfflineVersion(bool isLoading)
        {
            RefreshAsset(ID, new AssetEntry(VersionsManager.GetInstalledVersion(ID)), isLoading);
        }
        
        private void RefreshReady()
        {
            foreach (var entry in VersionsManager.LatestEntries)
            {
                RefreshAsset(entry.Key, entry.Value);
            }
        }

        private void RefreshAsset(string id, AssetEntry asset, bool isLoading = false)
        {
            var root = new VisualElement { name = NAME_ASSET_ROOT };
            var head = new VisualElement { name = NAME_ASSET_HEAD };
            var body = new VisualElement { name = NAME_ASSET_BODY };

            root.Add(head);
            root.Add(body);
            
            m_Body.Add(root);
            
            CreateHead(id, asset, head, body);
            CreateBody(id, asset, body, isLoading);
        }

        private void CreateHead(string id, AssetEntry asset, VisualElement head, VisualElement body)
        {
            var installedVersion = VersionsManager.GetInstalledVersion(id);
            var isInstalledOlder = installedVersion?.IsOlderThan(asset?.Version) ?? false;
            var isInstalledNewer = installedVersion?.IsNewerThan(asset?.Version) ?? false;

            Texture icon = isInstalledOlder ? ICON_INSTALLED_UPD.Texture : ICON_INSTALLED_YES.Texture;
            
            if(isInstalledNewer) icon = ICON_INSTALLED_NEW.Texture;
            
            var headImage = new Image { 
                name = NAME_ARROW, 
                image = ICON.Texture 
            }; 
            head.Add(headImage);
            
            head.RegisterCallback<ClickEvent>(clickEvent =>
            {
                body.style.display = body.style.display == DisplayStyle.None
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;  

                m_Foot.style.display = body.style.display;
                
                if(body.style.display == DisplayStyle.Flex) head.AddToClassList("expanded");
                else head.RemoveFromClassList("expanded");
            });
            
            var btnUpdate = new Button
            {
                text = "Update",
                style =
                {
                    width = new Length(60, LengthUnit.Pixel),
                    borderLeftWidth = new StyleFloat(1)
                }
            };
            
            btnUpdate.clicked += () =>
            {
                Application.OpenURL(string.Format(STORE_LINK, id));
            };

            btnUpdate.style.display = isInstalledOlder ? DisplayStyle.Flex : DisplayStyle.None;
            
            var label = asset?.Version.ToString();
            if(isInstalledNewer || asset?.Version == null)
            {
                label = $"{installedVersion}";
            }
            if (isInstalledOlder)
            {
                label = $"{installedVersion} → {label}";
            }

            head.Add(new LabelTitle(TextUtils.Humanize(id)));
            head.Add(new Label(label));
            head.Add(new Image { image = icon });
            head.Add(btnUpdate);
        }

        private static Label CreateWrappedLabel(string text)
        {
            var label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.flexWrap = Wrap.Wrap;
            return label;
        }

        private void CreateBody(string id, AssetEntry asset, VisualElement body, bool isLoading)
        {
            if(isLoading)
            {
                var loading = new Label("Fetching information...")
                {
                    name = NAME_LOADING
                };

                body.Add(loading);
                body.style.display = DisplayStyle.None;
                return;
            }
            
            if (asset?.Release?.Date == null)
            {
                body.Add(new Label("Coming soon"));
                body.style.display = DisplayStyle.None;
                return;
            }
            
            body.Add(new LabelTitle($"Released on {asset.Release.Date}"));
            
            // Use sub-categorized changes if available, otherwise fall back to legacy changes
            if (asset.HasSubCategorizedChanges)
            {
                // Display sub-categorized changes
                if (asset.AssetChanges.New.Length > 0)
                {
                    body.Add(new SpaceSmall());
                    body.Add(new LabelTitle("New"));
                    foreach (string log in asset.AssetChanges.New)
                    {
                        body.Add(CreateWrappedLabel($"- {log}"));
                    }
                }
                
                if (asset.AssetChanges.Enhanced.Length > 0)
                {
                    body.Add(new SpaceSmall());
                    body.Add(new LabelTitle("Enhanced"));
                    foreach (string log in asset.AssetChanges.Enhanced)
                    {
                        body.Add(CreateWrappedLabel($"- {log}"));
                    }
                }
                
                if (asset.AssetChanges.Changed.Length > 0)
                {
                    body.Add(new SpaceSmall());
                    body.Add(new LabelTitle("Changed"));
                    foreach (string log in asset.AssetChanges.Changed)
                    {
                        body.Add(CreateWrappedLabel($"- {log}"));
                    }
                }
                
                if (asset.AssetChanges.Removed.Length > 0)
                {
                    body.Add(new SpaceSmall());
                    body.Add(new LabelTitle("Removed"));
                    foreach (string log in asset.AssetChanges.Removed)
                    {
                        body.Add(CreateWrappedLabel($"- {log}"));
                    }
                }
                
                if (asset.AssetChanges.Fixed.Length > 0)
                {
                    body.Add(new SpaceSmall());
                    body.Add(new LabelTitle("Fixed"));
                    foreach (string log in asset.AssetChanges.Fixed)
                    {
                        body.Add(CreateWrappedLabel($"- {log}"));
                    }
                }
            }
            else if (asset.Changes.Length > 0)
            {
                // Fall back to legacy single list display
                body.Add(new SpaceSmall());
                body.Add(new LabelTitle("Changes"));
                foreach (var log in asset.Changes)
                {
                    body.Add(CreateWrappedLabel($"- {log}"));
                }
            }

            body.style.display = DisplayStyle.None;
        }
    }
}