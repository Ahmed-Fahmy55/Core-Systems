using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace ASoliman.Utils.ClipboardPlus
{
    /// <summary>
    /// A custom editor window for handling clipboard operations in Unity,  
    /// enabling seamless copying, pasting, and organization of component references.
    /// </summary>
    public class ClipboardEditorWindow : EditorWindow
    {
        // UI-related fields
        private Vector2 _scrollPosition;
        private string _searchQuery = "";
        private bool _showFavoritesOnly = false;
        private bool _isTypeFiltered = false;
        private bool _reverseOrder = false;
        private bool _isAltKeyDown = false;
        private string _clipBeingEdited = null;
        private string _editableHeaderText = "";

        // State-related fields
        private Component _selectedComponent;

        // Tab-related fields
        private int _selectedTab;
        private readonly string[] _tabNames = { "Clipboard", "Play Mode" };
        private PlayModePersistenceTab _playModeTab;

        [MenuItem("Window/Clipboard Plus &w")]
        public static ClipboardEditorWindow ShowWindow()
        {
            var window = GetWindow<ClipboardEditorWindow>("Clipboard");
            window.minSize = new Vector2(300, 200);
            var icon = Resources.Load<Texture2D>("clipboard");
            window.titleContent.image = icon;
            return window;
        }

        private void OnEnable()
        {
            ClipboardThemeManager.ResetInitialization();
            _playModeTab = new PlayModePersistenceTab();

            if (ClipboardManager.Instance != null)
            {
                ClipboardManager.Instance.ValidateClips();
            }

            Undo.undoRedoPerformed += Repaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
            ClipboardThemeManager.ResetInitialization();
            ClipPreviewRenderer.ClearPreviewCache();
        }

        private void OnGUI()
        {
            if (!ClipboardThemeManager.IsInitialized)
            {
                InitializeStyles();
                if (!ClipboardThemeManager.IsInitialized)
                {
                    EditorGUILayout.HelpBox("Initializing styles...", MessageType.Info);
                    return;
                }
            }

            DrawTabs();

            if (_selectedTab == 0)
            {
                CheckForAltKeyStatus();
                DrawSelectedComponentIndicator();
                HandleDragAndDrop();

                if (_selectedComponent != null)
                {
                    DrawTopBorder();
                }

                DrawToolbar();
                DrawTypeFilterIndicator();
                DrawClipList();

                CheckForGlobalMouseEvents();
            }
            else
            {
                _playModeTab.OnGUI();
            }
        }

        private void OnSelectionChange()
        {
            _selectedComponent = Selection.activeObject as Component;
            Repaint();
        }
        private void InitializeStyles()
        {
            ClipboardThemeManager.InitializeStyles();
        }

        /// <summary>
        /// Forces all ClipboardEditorWindow instances to repaint.
        /// This is useful when the clipboard data changes outside of the window's normal update cycle.
        /// </summary>
        public static void ForceRepaint()
        {
            var windows = Resources.FindObjectsOfTypeAll<ClipboardEditorWindow>();
            foreach (var window in windows)
            {
                if (window != null)
                {
                    window.Repaint();
                }
            }
        }

        /// <summary>
        /// Sets the target object for paste operations and updates the window's state accordingly.
        /// </summary>
        /// <param name="target">The Unity Object to set as the paste target</param>
        public void SetPasteTarget(UnityEngine.Object target)
        {
            _selectedComponent = null;
            _isTypeFiltered = false;

            if (target is Component component)
            {
                _selectedComponent = component; // This is the component we intend to paste onto
                _isTypeFiltered = true; // Enable filtering based on this component
            }
            Repaint();
        }

        private void DrawTabs()
        {
            // Cache original GUI colors
            var originalBgColor = GUI.backgroundColor;
            var originalContentColor = GUI.contentColor;
            
            EditorGUILayout.BeginHorizontal();
            
            var tabStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = 22,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(12, 12, 4, 4),
                margin = new RectOffset(0, 0, 0, 0),
                fontSize = 12,
                richText = true,
                border = new RectOffset(2, 2, 2, 2),
                stretchWidth = false
            };

            var isDarkTheme = EditorGUIUtility.isProSkin;
            var unselectedTabBgColor = isDarkTheme ? 
                new Color(0.7f, 0.7f, 0.7f) : 
                new Color(0.8f, 0.8f, 0.8f);
            
            var selectedTabTextColor = isDarkTheme ?
                new Color(1f, 1f, 1f) :
                new Color(0.1f, 0.1f, 0.1f);

            var unselectedTabTextColor = isDarkTheme ?
                new Color(0.7f, 0.7f, 0.7f) :
                new Color(0.3f, 0.3f, 0.3f);

            var tabAreaRect = EditorGUILayout.GetControlRect(false, 24);
            float tabWidth = tabAreaRect.width / 2;

            // Draw tabs
            for (int i = 0; i < _tabNames.Length; i++)
            {
                bool isSelected = _selectedTab == i;
                
                // Invert the background color logic - darker for unselected
                GUI.backgroundColor = isSelected ? Color.clear : unselectedTabBgColor;
                GUI.contentColor = isSelected ? selectedTabTextColor : unselectedTabTextColor;

                // Calculate tab position
                var tabRect = new Rect(tabAreaRect.x + (i * tabWidth), tabAreaRect.y, tabWidth, 22);
                
                if (GUI.Button(tabRect, _tabNames[i], tabStyle))
                {
                    _selectedTab = i;
                    GUI.FocusControl(null);
                }
            }

            // Draw single separator line
            EditorGUI.DrawRect(
                new Rect(tabAreaRect.x, tabAreaRect.y + 22, tabAreaRect.width, 1),
                isDarkTheme ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.5f, 0.5f, 0.5f)
            );

            // Restore original colors
            GUI.backgroundColor = originalBgColor;
            GUI.contentColor = originalContentColor;
            
            EditorGUILayout.EndHorizontal();
        }


        private void DrawTopBorder()
        {
            var borderRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(borderRect, EditorGUIUtility.isProSkin ?
                new Color(0.1f, 0.1f, 0.1f) :
                new Color(0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// Handles drag and drop operations for components onto the clipboard window.
        /// Accepts only Component types and adds them as new clipboard entries.
        /// </summary>
        private void HandleDragAndDrop()
        {
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                var draggedObject = DragAndDrop.objectReferences.FirstOrDefault();
                if (draggedObject is Component component)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        if (ClipboardManager.Instance != null)
                        {
                            ClipboardManager.Instance.AddClip(component, ClipType.Component);
                        }
                        Repaint();
                    }
                    Event.current.Use();
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawSearchField();
            GUILayout.FlexibleSpace();

            // Add order toggle button
            GUIContent orderContent = new GUIContent(
                _reverseOrder ? ClipboardThemeManager.SortOldestIcon : ClipboardThemeManager.SortNewestIcon,
                _reverseOrder ? "Sort Newest First": "Sort Oldest First"
            );
            
            if (GUILayout.Button(orderContent, EditorStyles.toolbarButton, GUILayout.Width(28)))
            {
                _reverseOrder = !_reverseOrder;
                Repaint();
            }
            
            DrawClearButton();
            DrawFavoritesToggle();
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal();
            
            var searchRect = EditorGUILayout.GetControlRect(false, 18, EditorStyles.toolbarSearchField, GUILayout.Width(180));
            
            _searchQuery = EditorGUI.TextField(searchRect, _searchQuery, EditorStyles.toolbarSearchField);
            
            // Only show clear button when we have text
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                GUIStyle clearButtonStyle = new GUIStyle(EditorStyles.label);
                clearButtonStyle.alignment = TextAnchor.UpperRight;
                clearButtonStyle.fontSize = 12;
                clearButtonStyle.normal.textColor = Color.white;
                
                if (GUILayout.Button("x", clearButtonStyle, GUILayout.Width(8), GUILayout.Height(18)))
                {
                    _searchQuery = "";
                    GUI.FocusControl(null);
                    Repaint();
                }
            }
            else
            {
                GUILayout.Space(20);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawClearButton()
        {
            var normalButtonColor = GUI.backgroundColor;
            var clearRect = GUILayoutUtility.GetRect(28, 24);
            var isHoveringClear = clearRect.Contains(Event.current.mousePosition);

            GUI.backgroundColor = isHoveringClear ? new Color(0.8f, 0.3f, 0.3f, 1f) : normalButtonColor;

            if (GUI.Button(clearRect, ClipboardThemeManager.ClearContent, EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Clear Clipboard",
                    "Are you sure you want to clear all non-favorited clips?",
                    "Yes", "No"))
                {
                    ClipboardManager.Instance.ClearAllClips();
                }
            }

            GUI.backgroundColor = normalButtonColor;
        }

        private void DrawFavoritesToggle()
        {
            var favContent = EditorGUIUtility.IconContent("Favorite");
            favContent.tooltip = _showFavoritesOnly ? "Unfilter Favorites" : "Show Favorites Only";
            var favButtonStyle = new GUIStyle(EditorStyles.toolbarButton);

            Color originalGUIColor = GUI.color;
            if (_showFavoritesOnly)
            {
                GUI.color = ClipboardThemeManager.FavoriteActiveColor;
            }

            _showFavoritesOnly = GUILayout.Toggle(_showFavoritesOnly, favContent, favButtonStyle, GUILayout.Width(28));
            GUI.color = originalGUIColor;
        }

        private void DrawClipList()
        {
            if (!ClipboardThemeManager.IsInitialized || ClipboardManager.Instance == null) return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var clips = ClipboardManager.Instance.GetClips()
                ?.Where(clip => FilterClip(clip))
                ?.ToList();

            if (clips != null && clips.Any())
            {
                // Apply reverse order if needed
                if (_reverseOrder)
                {
                    clips.Reverse();
                }

                for (int i = 0; i < clips.Count; i++)
                {
                    var clip = clips[i];
                    if (clip != null)
                    {
                        // Pass the index and total count to DrawClipEntry
                        DrawClipEntry(clip, i, clips.Count);
                    }
                }
            }
            else
            {
                string helpBoxText = _showFavoritesOnly ? "No favorite clips available. Try adding clips to your favorites." : "No clips available. Drag and drop components here to create clips.";
                EditorGUILayout.HelpBox(helpBoxText, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Filters clipboard entries based on current search, favorites, and type filter settings.
        /// Returns true if the clip should be displayed, false otherwise.
        /// </summary>
        /// <param name="clip">The clip data to evaluate</param>
        /// <returns>Boolean indicating if the clip should be displayed</returns>
        private bool FilterClip(ClipData clip)
        {
            if (_showFavoritesOnly && !clip.isFavorite)
                return false;

            if (_isTypeFiltered && _selectedComponent != null)
            {
                if (clip.DataType != _selectedComponent.GetType())
                    return false;
            }

            if (string.IsNullOrEmpty(_searchQuery))
                return true;

            return clip.sourceType.ToLower().Contains(_searchQuery.ToLower()) ||
                clip.sourcePath.ToLower().Contains(_searchQuery.ToLower());
        }

        /// <summary>
        /// Draws a single clipboard entry including its header, preview, and action buttons.
        /// </summary>
        /// <param name="clip">The clip data to display</param>
        private void DrawClipEntry(ClipData clip, int index, int totalCount)
        {
            if (clip == null)
            {
                ClipboardManager.Instance.RemoveClip(clip.id, false);
                return;
            }
            EditorGUILayout.BeginVertical(ClipboardThemeManager.ClipStyle);
            DrawClipHeader(clip, index, totalCount);
            DrawClipPreview(clip);
            DrawClipButtons(clip);
            EditorGUILayout.EndVertical();
        }



        private void DrawClipHeader(ClipData clip, int index, int totalCount)
        {
            EditorGUILayout.BeginHorizontal();

            Rect dragRect = EditorGUILayout.GetControlRect(GUILayout.Height(24));
            var dragColor = EditorGUIUtility.isProSkin ?
                new Color(0.3f, 0.3f, 0.3f, 0.3f) :
                new Color(0.8f, 0.8f, 0.8f, 0.3f);

            EditorGUI.DrawRect(dragRect, dragColor);

            var style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12
            };

            float buttonSize = 20;
            float padding = 2;

            // Create rects for buttons
            Rect upButtonRect = new Rect(dragRect.xMax - (buttonSize * 4 + padding), dragRect.y + 2, buttonSize, buttonSize);
            Rect downButtonRect = new Rect(dragRect.xMax - (buttonSize * 3 + padding), dragRect.y + 2, buttonSize, buttonSize);
            Rect foldRect = new Rect(dragRect.xMax - (buttonSize * 2 + padding), dragRect.y + 2, buttonSize, buttonSize);
            Rect favoriteRect = new Rect(dragRect.xMax - buttonSize, dragRect.y + 2, buttonSize, buttonSize);

            // Calculate proper label width based on ALT key state
            float labelWidth = dragRect.width - (buttonSize * 2 + padding + 10);
            if (_isAltKeyDown)
            {
                labelWidth -= (buttonSize * 2); // Reduce width when showing up/down buttons
            }

            // Create icon area and label area
            var iconArea = new Rect(dragRect.x + 5, dragRect.y + 4, 16, 16);
            var labelRect = new Rect(dragRect.x + 25, dragRect.y, labelWidth - 20, dragRect.height);

            // Draw component icon with tooltip
            Texture componentIcon = null;
            string componentTypeName = "Unknown Component";
            
            if (clip.DataType != null)
            {
                componentIcon = EditorGUIUtility.ObjectContent(null, clip.DataType).image;
                componentTypeName = clip.DataType.Name;

                if (componentIcon == null)
                {
                    componentIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
                }
            }

            if (componentIcon != null)
            {
                // Draw component icon with tooltip
                GUI.DrawTexture(iconArea, componentIcon);
                
                // Add tooltip separately using invisible button
                EditorGUI.LabelField(iconArea, new GUIContent("", componentTypeName));
            }

            // Handle clip title editing with proper area that doesn't include the reorder buttons
            bool isEditingThisClip = _clipBeingEdited == clip.id;
            
            // Determine edit area
            var editRect = new Rect(labelRect.x, labelRect.y + 2, labelRect.width - 10, labelRect.height - 4);
            
            // Check for mouse clicks on the label area only - don't include the area where buttons would be
            if (!isEditingThisClip && !_isAltKeyDown && Event.current.type == EventType.MouseDown && 
                labelRect.Contains(Event.current.mousePosition))
            {
                _clipBeingEdited = clip.id;
                _editableHeaderText = clip.sourceType;
                GUI.FocusControl("ClipHeaderEdit_" + clip.id);
                Event.current.Use();
            }
            
            if (isEditingThisClip)
            {
                // Create a unique control name for this text field
                GUI.SetNextControlName("ClipHeaderEdit_" + clip.id);
                
                // Text field for editing
                _editableHeaderText = EditorGUI.TextField(editRect, _editableHeaderText, EditorStyles.textField);
                
                // Handle Enter key to confirm edit
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    // Apply the name change
                    if (!string.IsNullOrWhiteSpace(_editableHeaderText))
                    {
                        clip.sourceType = _editableHeaderText;
                        EditorUtility.SetDirty(clip);
                    }
                    
                    _clipBeingEdited = null;
                    GUI.FocusControl(null);
                    Event.current.Use();
                }
                
                // Handle Escape key to cancel edit
                else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    _clipBeingEdited = null;
                    GUI.FocusControl(null);
                    Event.current.Use();
                }
                
                // Check for clicks outside to end editing
                else if (Event.current.type == EventType.MouseDown && !editRect.Contains(Event.current.mousePosition))
                {
                    _clipBeingEdited = null;
                    GUI.FocusControl(null);
                    // Don't use the event here, as we want it to propagate for other controls
                }
            }
            else
            {
                // Just display the label normally when not editing
                GUI.Label(labelRect, $"{clip.sourceType}", style);
            }

            // Draw up/down buttons only when ALT is held with window focus
            if (_isAltKeyDown)
            {
                // Up button - disabled if it's the first item
                bool canMoveUp = index > 0;
                using (new EditorGUI.DisabledScope(!canMoveUp))
                {
                    GUIContent upContent = new GUIContent(ClipboardThemeManager.ArrowUpIcon, "Move Up");
                    if (GUI.Button(upButtonRect, upContent, EditorStyles.label) && canMoveUp)
                    {
                        MoveClipUp(clip);
                        GUIUtility.ExitGUI(); // Prevent layout issues after reordering
                    }
                }

                // Down button - disabled if it's the last item
                bool canMoveDown = index < totalCount - 1;
                using (new EditorGUI.DisabledScope(!canMoveDown))
                {
                    GUIContent downContent = new GUIContent(ClipboardThemeManager.ArrowDownIcon, "Move Down");
                    if (GUI.Button(downButtonRect, downContent, EditorStyles.label) && canMoveDown)
                    {
                        MoveClipDown(clip);
                        GUIUtility.ExitGUI(); // Prevent layout issues after reordering
                    }
                }
            }

            Texture2D foldTexture = clip.isExpanded ? ClipboardThemeManager.CollapseIcon : ClipboardThemeManager.ExpandIcon;

            GUIContent foldContent = new GUIContent(foldTexture, clip.isExpanded ? "Collapse" : "Expand");
            if (GUI.Button(foldRect, foldContent, EditorStyles.label))
            {
                clip.isExpanded = !clip.isExpanded;
                EditorUtility.SetDirty(clip);
            }

            ClipboardThemeManager.FavoriteContent.tooltip = clip.isFavorite ? "Unfavorite" : "Favorite";
            var originalColor = GUI.color;
            if (clip.isFavorite)
            {
                GUI.color = ClipboardThemeManager.FavoriteActiveColor;
            }
            if (GUI.Button(favoriteRect, ClipboardThemeManager.FavoriteContent, EditorStyles.label))
            {
                ClipboardManager.Instance.ToggleFavorite(clip.id);
            }
            GUI.color = originalColor;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawClipPreview(ClipData clip)
        {
            float previewHeight = ClipPreviewRenderer.GetPreviewHeight(clip);
            var previewRect = GUILayoutUtility.GetRect(0, previewHeight, GUILayout.ExpandWidth(true));
            ClipPreviewRenderer.DrawPreview(clip, previewRect);
        }



        private void DrawClipButtons(ClipData clip)
        {
            if (clip == null) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var normalButtonColor = GUI.backgroundColor;
            var buttonStyle = ClipboardThemeManager.ModernButtonStyle;

            // Determine if paste should be possible based on _selectedComponent
            bool canPaste = _selectedComponent != null &&
                            clip.DataType != null &&
                            _selectedComponent.GetType() == clip.DataType;

            // Paste Button
            var pasteRect = GUILayoutUtility.GetRect(80, 24);
            var isHoveringPaste = pasteRect.Contains(Event.current.mousePosition);
            GUI.backgroundColor = canPaste && isHoveringPaste ? ClipboardThemeManager.HoverPasteColor : normalButtonColor;

            using (new EditorGUI.DisabledScope(!canPaste))
            {
                if (GUI.Button(pasteRect, ClipboardThemeManager.PasteContent, buttonStyle))
                {
                    // Use _selectedComponent for the paste operation
                    if (_selectedComponent != null) // Double check just in case
                    {
                        ClipboardManager.Instance.PasteClip(clip, _selectedComponent);
                        // Consider adding feedback here if needed
                    }
                }
            }
            GUI.backgroundColor = normalButtonColor;

            GUILayout.Space(5);

            // Remove Button (Logic remains the same)
            var removeRect = GUILayoutUtility.GetRect(80, 24);
            var isHoveringRemove = removeRect.Contains(Event.current.mousePosition);
            GUI.backgroundColor = isHoveringRemove ? ClipboardThemeManager.HoverRemoveColor : normalButtonColor;
            if (GUI.Button(removeRect, ClipboardThemeManager.RemoveContent, buttonStyle))
            {
                ClipboardManager.Instance.RemoveClip(clip.id);
                GUIUtility.ExitGUI(); // Exit GUI to avoid layout errors after removal
            }
            GUI.backgroundColor = normalButtonColor;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4); // Add spacing after buttons
        }

        /// <summary>
        /// Draws an indicator showing the currently selected component and type filtering status.
        /// Includes component icon, name, and type information with a close button to clear selection.
        /// </summary>
        private void DrawSelectedComponentIndicator()
        {
            if (_selectedComponent != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var combinedName = $"{_selectedComponent.gameObject.name}.{_selectedComponent.GetType().Name}";
                var contentWidth = EditorStyles.label.CalcSize(new GUIContent(combinedName)).x + 40; // Adjusted width

                EditorGUILayout.BeginHorizontal(ClipboardThemeManager.TargetIndicatorStyle, GUILayout.Width(contentWidth));

                var componentIcon = EditorGUIUtility.ObjectContent(_selectedComponent, _selectedComponent.GetType()).image;
                if (componentIcon == null)
                {
                    componentIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
                }
                var iconRect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
                iconRect.y += 2;
                GUI.DrawTexture(iconRect, componentIcon, ScaleMode.ScaleToFit);

                GUILayout.Space(4);

                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 0, 2, 0)
                };
                GUILayout.Label(combinedName, labelStyle);

                EditorGUILayout.EndHorizontal();

                // Close button clears the selection and filter
                if (GUILayout.Button("×", ClipboardThemeManager.CloseButtonStyle, GUILayout.Width(20)))
                {
                    _selectedComponent = null;
                    _isTypeFiltered = false;
                    Repaint();
                    GUIUtility.ExitGUI(); // Prevent potential layout issues after state change
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        /// <summary>
        /// Displays a filter indicator when type filtering is active, showing the selected component type
        /// and the number of filtered clips compared to total clips.
        /// </summary>
        private void DrawTypeFilterIndicator()
        {
            if (_selectedComponent != null && _isTypeFiltered) 
            {
                var componentType = _selectedComponent.GetType();
                var allClips = ClipboardManager.Instance?.GetClips() ?? new List<ClipData>();
                var totalClips = allClips.Count;
                // Ensure null checks for clips and DataType
                var filteredClips = allClips.Count(c => c != null && c.DataType == componentType);

                var filterText = $"Pasting to: {componentType.Name} ({filteredClips} matching clips)";
                var content = new GUIContent(filterText);
                EditorGUILayout.LabelField(content, ClipboardThemeManager.FilterLabelStyle, GUILayout.Height(20));
            }
        }

        private void MoveClipUp(ClipData clip)
        {
            if (ClipboardManager.Instance == null) return;
            
            var allClips = ClipboardManager.Instance.GetClips()
                ?.Where(c => FilterClip(c))
                ?.ToList();
            
            if (_reverseOrder)
            {
                allClips.Reverse(); // Adjust for reversed order
            }
            
            if (allClips == null || allClips.Count <= 1) return;
            
            int index = allClips.IndexOf(clip);
            if (index <= 0) return; // Already at the top
            
            // Swap the clips in the original collection
            ClipboardManager.Instance.SwapClips(clip.id, allClips[index - 1].id);
        }

        private void MoveClipDown(ClipData clip)
        {
            if (ClipboardManager.Instance == null) return;
            
            var allClips = ClipboardManager.Instance.GetClips()
                ?.Where(c => FilterClip(c))
                ?.ToList();
                
            if (_reverseOrder)
            {
                allClips.Reverse(); // Adjust for reversed order
            }
            
            if (allClips == null || allClips.Count <= 1) return;
            
            int index = allClips.IndexOf(clip);
            if (index < 0 || index >= allClips.Count - 1) return; // Already at the bottom
            
            // Swap the clips in the original collection
            ClipboardManager.Instance.SwapClips(clip.id, allClips[index + 1].id);
        }

        private void CheckForGlobalMouseEvents()
        {
            // If we're editing a clip and clicked elsewhere in the window (not handled by specific controls)
            if (_clipBeingEdited != null && Event.current.type == EventType.MouseDown)
            {
                _clipBeingEdited = null;
                GUI.FocusControl(null);
                Repaint();
            }
        }

        private void CheckForAltKeyStatus()
        {
            // Only track ALT key when window has focus
            if (!EditorWindow.focusedWindow || EditorWindow.focusedWindow != this)
            {
                _isAltKeyDown = false;
                return;
            }

            // Check actual key state rather than just events
            if (Event.current.alt)
            {
                if (!_isAltKeyDown)
                {
                    _isAltKeyDown = true;
                    Repaint();
                }
            }
            else if (_isAltKeyDown)
            {
                _isAltKeyDown = false;
                Repaint();
            }
        }

    }

    public class PlayModePersistenceTab
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, List<string>> _gameObjectComponentOrder = new Dictionary<string, List<string>>();

        public void OnGUI()
        {
            DrawHeader();
            HandleDragAndDrop();
            DrawCapturesList();
        }

        private void HandleDragAndDrop()
        {
            var currentEvent = Event.current;
            
            if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
            {
                var draggedObject = DragAndDrop.objectReferences.FirstOrDefault();
                if (draggedObject is Component component)
                {
                    // Only show copy cursor when recording is active
                    if (PlayModePersistenceManager.Instance.IsRecording && EditorApplication.isPlaying)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (currentEvent.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            
                            // Create menu for drag operation
                            var menu = new GenericMenu();
                            
                            menu.AddItem(new GUIContent("Auto Save"), false, () => 
                            {
                                PlayModePersistenceManager.Instance.StartAutoRecording(component);
                                // Automatically capture initial state
                                PlayModePersistenceManager.Instance.CaptureComponent(component);
                            });
                            
                            menu.AddItem(new GUIContent("Save Snapshot"), false, () => 
                            {
                                PlayModePersistenceManager.Instance.CaptureComponent(component);
                            });
                            
                            menu.ShowAsContext();
                        }
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    
                    currentEvent.Use();
                }
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            DrawRecordingButton();
            GUILayout.FlexibleSpace();
            DrawClearButton();

            EditorGUILayout.EndHorizontal();

            DrawHeaderMessage();
        }

        private void DrawRecordingButton()
        {
            ClipboardThemeManager.RecordContent = new GUIContent(
                EditorApplication.isPlaying ? 
                    (PlayModePersistenceManager.Instance.IsRecording ? " Recording" : " Ready") : 
                    " Ready",
                EditorGUIUtility.IconContent("Record Off").image
            );

            var normalButtonColor = GUI.backgroundColor;
            if (PlayModePersistenceManager.Instance.IsRecording)
            {
                GUI.backgroundColor = ClipboardThemeManager.RecordingColor;
                ClipboardThemeManager.RecordContent.image = EditorGUIUtility.IconContent("Record On").image;
            }

            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            if (GUILayout.Button(ClipboardThemeManager.RecordContent, EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                PlayModePersistenceManager.Instance.ToggleRecording();
            }
            EditorGUI.EndDisabledGroup();

            GUI.backgroundColor = normalButtonColor;
        }

        private void DrawClearButton()
        {
            if (PlayModePersistenceManager.Instance.CapturedComponents.Count > 0)
            {
                var clearContent = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
                clearContent.tooltip = "Clear All Captures";

                var normalButtonColor = GUI.backgroundColor;
                var clearRect = GUILayoutUtility.GetRect(32, 24);
                var isHoveringClear = clearRect.Contains(Event.current.mousePosition);

                GUI.backgroundColor = isHoveringClear ? ClipboardThemeManager.HoverRemoveColor : normalButtonColor;

                if (GUI.Button(clearRect, clearContent, EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Clear Captures",
                        "Are you sure you want to clear all captured components?",
                        "Yes", "No"))
                    {
                        PlayModePersistenceManager.Instance.StopAutoRecording();
                        PlayModePersistenceManager.Instance.ClearCaptures();
                    }
                }

                GUI.backgroundColor = normalButtonColor;
            }
        }

        private void DrawHeaderMessage()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to start recording component values.",
                    MessageType.Info
                );
                return;
            }
            if (!PlayModePersistenceManager.Instance.IsRecording)
            {
                EditorGUILayout.HelpBox(
                    "Click the Recording button to start capturing component values.",
                    MessageType.Info
                );
                return;
            }
            var captures = PlayModePersistenceManager.Instance.CapturedComponents;
            if (captures.Count == 0 && PlayModePersistenceManager.Instance.IsRecording && EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Drag and drop a component or use the context menu (right-click) to auto save or capture a snapshot of its current state.",
                    MessageType.Info
                );
            }
        }

        private void DrawCapturesList()
        {
            var captures = PlayModePersistenceManager.Instance.CapturedComponents;
            if (captures.Count == 0) return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Group captures by GameObject path and maintain order
            var groupedCaptures = captures
                .GroupBy(c => c.gameObjectPath)
                .OrderByDescending(g => GetGameObjectFirstCaptureTime(g.Key, g.ToList()));

            foreach (var group in groupedCaptures)
            {
                DrawGameObjectGroup(group.Key, group.ToList());
            }

            EditorGUILayout.EndScrollView();
        }

        private DateTime GetGameObjectFirstCaptureTime(string gameObjectPath, List<PlayModePersistenceData> components)
        {
            // Get the earliest capture time among all components
            return components.Min(c => c.captureTime);
        }

        private void DrawGameObjectGroup(string gameObjectPath, List<PlayModePersistenceData> components)
        {
            // Initialize order tracking for new GameObjects
            if (!_gameObjectComponentOrder.ContainsKey(gameObjectPath))
            {
                _gameObjectComponentOrder[gameObjectPath] = new List<string>();
            }

            // Add any new components to the top of the order
            foreach (var component in components)
            {
                if (!_gameObjectComponentOrder[gameObjectPath].Contains(component.componentId))
                {
                    _gameObjectComponentOrder[gameObjectPath].Insert(0, component.componentId);
                }
            }

            // Remove any components that no longer exist
            _gameObjectComponentOrder[gameObjectPath] = _gameObjectComponentOrder[gameObjectPath]
                .Where(id => components.Any(c => c.componentId == id))
                .ToList();

            // Draw the GameObject group
            EditorGUILayout.BeginVertical(ClipboardThemeManager.PlayModeItemStyle);

            // GameObject header
            EditorGUILayout.BeginHorizontal();
            var goIcon = EditorGUIUtility.IconContent("GameObject Icon").image;
            var iconRect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
            GUI.DrawTexture(iconRect, goIcon);
            
            EditorGUILayout.LabelField(
                Path.GetFileName(gameObjectPath),
                ClipboardThemeManager.PlayModeHeaderStyle
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(gameObjectPath, EditorStyles.miniLabel);

            // Draw components in the maintained order
            foreach (var componentId in _gameObjectComponentOrder[gameObjectPath])
            {
                var component = components.First(c => c.componentId == componentId);
                DrawComponentCapture(component);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawComponentCapture(PlayModePersistenceData capture)
        {
            EditorGUILayout.BeginVertical(ClipboardThemeManager.ComponentHeaderStyle);
            
            EditorGUILayout.BeginHorizontal();
            
            var componentType = Type.GetType(capture.componentType);
            var icon = EditorGUIUtility.ObjectContent(null, componentType).image;
            
            if (icon == null)
            {
                icon = EditorGUIUtility.IconContent("cs Script Icon").image;
            }

            var iconRect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
            GUI.DrawTexture(iconRect, icon);
            
            EditorGUILayout.LabelField(
                componentType?.Name ?? "Unknown",
                ClipboardThemeManager.PlayModeHeaderStyle,
                GUILayout.MinWidth(100),
                GUILayout.ExpandWidth(true)
            );

            if (PlayModePersistenceManager.Instance.IsAutoRecordedComponent(capture.componentId))
            {
                GUILayout.Label(
                    EditorGUIUtility.IconContent("Record On").image, 
                    ClipboardThemeManager.RecordingIndicatorStyle, 
                    GUILayout.Width(16)
                );
            }
            
            EditorGUILayout.LabelField(
                capture.captureTime.ToString("hh:mm:ss tt"),
                EditorStyles.miniLabel,
                GUILayout.Width(70)
            );            

            var removeContent = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
            removeContent.tooltip = "Remove Capture";
            
            var buttonRect = GUILayoutUtility.GetRect(24, 24, ClipboardThemeManager.RemoveButtonStyle);
            var isHoveringRemove = buttonRect.Contains(Event.current.mousePosition);

            var normalButtonColor = GUI.backgroundColor;
            
            GUI.backgroundColor = isHoveringRemove ? ClipboardThemeManager.HoverRemoveColor : normalButtonColor;
            
            if (GUI.Button(buttonRect, removeContent, ClipboardThemeManager.RemoveButtonStyle))
            {
                var isAutoRecorded = PlayModePersistenceManager.Instance.IsAutoRecordedComponent(capture.componentId);
                var message = isAutoRecorded ? 
                    "Would you like to stop recording this component?" : 
                    "Remove this capture?";
                
                if (EditorUtility.DisplayDialog("Remove Capture", message, "Yes", "No"))
                {
                    if (isAutoRecorded)
                    {
                        var component = EditorUtility.InstanceIDToObject(int.Parse(capture.componentId)) as Component;
                        if (component != null)
                        {
                            PlayModePersistenceManager.Instance.StopAutoRecording(component);
                        }
                    }
                    PlayModePersistenceManager.Instance.RemoveCapture(capture.componentId);
                }
            }

            GUI.backgroundColor = normalButtonColor;
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}