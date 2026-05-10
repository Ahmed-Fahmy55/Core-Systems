using UnityEditor;
using UnityEngine;

namespace ASoliman.Utils.ClipboardPlus
{
    /// <summary>
    /// Centralized manager for all GUI styles, contents, and colors used in the Clipboard Plus window.
    /// </summary>
    public static class ClipboardThemeManager
    {
        
        #region Initialization State
        
        private static bool _isInitialized = false;
        public static bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Colors
        
        // Common colors
        private static readonly Color _hoverPasteColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color _hoverRemoveColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        private static readonly Color _favoriteActiveColor = new Color(1f, 0.92f, 0.016f, 1f);
        private static readonly Color _headerColor = new Color(0.235f, 0.235f, 0.235f, 1f);
        private static readonly Color _recordingColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        
        // Property accessors for colors
        public static Color HoverPasteColor => _hoverPasteColor;
        public static Color HoverRemoveColor => _hoverRemoveColor;
        public static Color FavoriteActiveColor => _favoriteActiveColor;
        public static Color HeaderColor => _headerColor;
        public static Color RecordingColor => _recordingColor;
        
        #endregion
        
        #region Common Styles
        
        private static GUIStyle _clipStyle;
        private static GUIStyle _targetIndicatorStyle;
        private static GUIStyle _closeButtonStyle;
        private static GUIStyle _filterLabelStyle;
        private static GUIStyle _modernButtonStyle;
        
        // Property accessors for common styles
        public static GUIStyle ClipStyle => _clipStyle;
        public static GUIStyle TargetIndicatorStyle => _targetIndicatorStyle;
        public static GUIStyle CloseButtonStyle => _closeButtonStyle;
        public static GUIStyle FilterLabelStyle => _filterLabelStyle;
        public static GUIStyle ModernButtonStyle => _modernButtonStyle;
        
        #endregion

        #region Clip Preview Styles
        private static GUIStyle _headerLabelStyle;
        private static GUIStyle _boxStyle;

        // Property accessors for clip preview styles
        public static GUIStyle HeaderLabelStyle => _headerLabelStyle;
        public static GUIStyle BoxStyle => _boxStyle;

        #endregion
        
        #region Play Mode Tab Styles
        
        private static GUIStyle _playModeItemStyle;
        private static GUIStyle _playModeHeaderStyle;
        private static GUIStyle _recordingIndicatorStyle;
        private static GUIStyle _componentHeaderStyle;
        private static GUIStyle _removeButtonStyle;
        
        // Property accessors for play mode tab styles
        public static GUIStyle PlayModeItemStyle => _playModeItemStyle;
        public static GUIStyle PlayModeHeaderStyle => _playModeHeaderStyle;
        public static GUIStyle RecordingIndicatorStyle => _recordingIndicatorStyle;
        public static GUIStyle ComponentHeaderStyle => _componentHeaderStyle;
        public static GUIStyle RemoveButtonStyle => _removeButtonStyle;
        
        #endregion
        
        #region GUIContent Cache
        
        private static GUIContent _pasteContent;
        private static GUIContent _removeContent;
        private static GUIContent _clearContent;
        private static GUIContent _favoriteContent;
        private static GUIContent _recordContent;
        
        // Property accessors for GUIContent
        public static GUIContent PasteContent => _pasteContent;
        public static GUIContent RemoveContent => _removeContent;
        public static GUIContent ClearContent => _clearContent;
        public static GUIContent FavoriteContent { get {return _favoriteContent;} set {_favoriteContent = value;} }
        public static GUIContent RecordContent { get {return _recordContent;} set {_recordContent = value;} }
        
        #endregion

        #region Texture2D Cache

        private static Texture2D _collapseIcon;
        private static Texture2D _expandIcon;
        private static Texture2D _arrowUpIcon;
        private static Texture2D _arrowDownIcon;
        private static Texture2D _sortNewestIcon;
        private static Texture2D _sortOldestIcon;
        private static Texture2D _clearSearchIcon;

        // Property accessors for Texture2D
        public static Texture2D CollapseIcon => _collapseIcon;
        public static Texture2D ExpandIcon => _expandIcon;
        public static Texture2D ArrowUpIcon => _arrowUpIcon;
        public static Texture2D ArrowDownIcon => _arrowDownIcon;
        public static Texture2D SortNewestIcon => _sortNewestIcon;
        public static Texture2D SortOldestIcon => _sortOldestIcon;
        public static Texture2D ClearSearchIcon => _clearSearchIcon;
        
        #endregion
        
        /// <summary>
        /// Initializes all styles, colors, and GUIContent objects.
        /// This method should be called before accessing any styles.
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise.</returns>
        public static bool InitializeStyles()
        {
            if (_isInitialized) return true;
            
            if (EditorStyles.helpBox == null) return false;
            
            try
            {
                InitializeCommonStyles();
                InitializeClipPreviewStyles();
                InitializePlayModeStyles();
                InitializeGUIContent();
                InitializeTexture2D();
                _isInitialized = true;
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to initialize GUI styles: {e.Message}");
                _isInitialized = false;
                return false;
            }
        }

        /// <summary>
        /// Reset the initialization state, forcing styles to be re-initialized.
        /// This is useful when the editor changes themes or when window is reopened.
        /// </summary>
        public static void ResetInitialization()
        {
            _isInitialized = false;
        }
        
        #region Initialization Implementation
        
        private static void InitializeCommonStyles()
        {
            // Clip style
            _clipStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
            
            // Target indicator style
            _targetIndicatorStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(0, 0, 2, 0),
                fontSize = 11,
                border = new RectOffset(1, 1, 1, 1),
                stretchWidth = false,
                fixedHeight = 26
            };
            _targetIndicatorStyle.normal.textColor = EditorGUIUtility.isProSkin ?
                new Color(0.9f, 0.9f, 0.9f) :
                new Color(0.2f, 0.2f, 0.2f);
            
            // Close button style
            _closeButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 1),
                margin = new RectOffset(4, 0, 7, 0),
                fixedWidth = 16,
                fixedHeight = 16
            };
            
            // Filter label style
            _filterLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(6, 6, 2, 2),
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { background = CreateBadgeTexture() }
            };
            _filterLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ?
                new Color(0.8f, 0.8f, 0.8f) :
                new Color(0.3f, 0.3f, 0.3f);
            
            // Modern button style
            _modernButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(2, 2, 2, 2),
                fontSize = 11,
                fixedHeight = 24,
                richText = true
            };
        }

        private static void InitializeClipPreviewStyles()
        {
            _headerLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                padding = new RectOffset(4, 4, 0, 0),
                fontSize = 11
            };

            _boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };
        }
        
        private static void InitializePlayModeStyles()
        {
            _playModeItemStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
            
            _playModeHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 0, 0)
            };
            
            _recordingIndicatorStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                normal = { textColor = _recordingColor }
            };
            
            _componentHeaderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(0, 0, 0, 5),
                fontSize = 11,
                fixedHeight = 40
            };
            
            _removeButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = 24,
                fixedHeight = 24,
                padding = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(2, 2, 0, 0)
            };
        }
        
        private static void InitializeGUIContent()
        {
            // Clipboard content
            _pasteContent = new GUIContent(" Paste", Resources.Load<Texture2D>("clip_paste"));
            _removeContent = new GUIContent(" Remove", EditorGUIUtility.IconContent("TreeEditor.Trash").image);
            _clearContent = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
            _clearContent.tooltip = "Clear All Non-Favorites";
            _favoriteContent = EditorGUIUtility.IconContent("Favorite");
            _favoriteContent.tooltip = "Favorite";
            
            // Play mode content
            _recordContent = EditorGUIUtility.IconContent("Record Off");
        }

        private static void InitializeTexture2D()
        {
            _collapseIcon = Resources.Load<Texture2D>("clip_collapse");
            _expandIcon = Resources.Load<Texture2D>("clip_expand");
            _arrowUpIcon = Resources.Load<Texture2D>("arrow_up");
            _arrowDownIcon = Resources.Load<Texture2D>("arrow_down");
            _sortNewestIcon = Resources.Load<Texture2D>("sort_newest");
            _sortOldestIcon = Resources.Load<Texture2D>("sort_oldest");
            _clearSearchIcon = Resources.Load<Texture2D>("clear_search");
        }
        
        /// <summary>
        /// Creates a badge-style background texture for filter labels.
        /// </summary>
        /// <returns>A single pixel texture used for filter label backgrounds</returns>
        private static Texture2D CreateBadgeTexture()
        {
            var tex = new Texture2D(1, 1);
            var color = EditorGUIUtility.isProSkin ?
                new Color(0.25f, 0.25f, 0.25f, 0.2f) :
                new Color(0.85f, 0.85f, 0.85f, 0.4f);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
        
        #endregion
    }
}