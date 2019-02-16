﻿using System.Linq;
using ColossalFramework.UI;
using ForestBrush.Resources;
using UnityEngine;

namespace ForestBrush.GUI
{
    public class TreeItem : UIPanel, IUIFastListRow
    {
        TreeInfo prefab;
        UICheckBox includeCheckBox;
        UILabel treeNameLabel;
        UITextureSprite thumbNailSprite;
        bool initialized;

        private void Initialize(TreeInfo info)
        {
            if (initialized) return;

            prefab = info;

            //General
            name = info.GetUncheckedLocalizedTitle() + "ListItem"; 
            atlas = ResourceLoader.GetAtlas("Ingame");
            width = parent.width;
            height = Constants.UIItemHeight;
            isVisible = true;
            isInteractive = true;

            //Thumbnail
            thumbNailSprite = AddUIComponent<UITextureSprite>();
            thumbNailSprite.texture = prefab.m_Atlas.sprites.Find(spr => spr.name == prefab.m_Thumbnail).texture;
            thumbNailSprite.size = new Vector2(50f, 54.5f);
            thumbNailSprite.relativePosition = new Vector3(Constants.UISpacing, (Constants.UIItemHeight - thumbNailSprite.height) / 2);

            //CheckBox
            includeCheckBox = AddUIComponent<UICheckBox>();
            includeCheckBox.size = Constants.UICheckboxSize;
            var sprite = includeCheckBox.AddUIComponent<UISprite>();
            sprite.atlas =  ResourceLoader.GetAtlas("Ingame");
            sprite.spriteName = Constants.CheckBoxSpriteUnchecked;
            sprite.size = includeCheckBox.size;
            sprite.relativePosition = Vector3.zero;
            includeCheckBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)includeCheckBox.checkedBoxObject).atlas =  ResourceLoader.GetAtlas("Ingame");
            ((UISprite)includeCheckBox.checkedBoxObject).spriteName = Constants.CheckBoxSpriteChecked;
            includeCheckBox.checkedBoxObject.size = includeCheckBox.size;
            includeCheckBox.checkedBoxObject.relativePosition = Vector3.zero;
            includeCheckBox.eventCheckChanged += EventIncludeTree;
            includeCheckBox.isChecked = ForestBrushMod.instance.Container.m_variations.Any(v => v.m_finalTree == prefab);
            includeCheckBox.relativePosition = new Vector3(width - (Constants.UISpacing * 2) - includeCheckBox.width, (height - includeCheckBox.height) / 2);

            //Label
            treeNameLabel = AddUIComponent<UILabel>();
            treeNameLabel.text = prefab.GetUncheckedLocalizedTitle();
            treeNameLabel.relativePosition = new Vector3(thumbNailSprite.width + Constants.UISpacing * 2, (height - treeNameLabel.height) / 2);

            initialized = true;
        }

        public void ToggleCheckbox(bool value)
        {
            includeCheckBox.isChecked = value;
        }

        public void UpdateCheckbox()
        {
            includeCheckBox.isChecked = ForestBrushMod.instance.Container.m_variations.Any(v => v.m_finalTree == prefab);
        }

        private void EventIncludeTree(UIComponent component, bool value)
        {
            bool updateAll = false;
            if (includeCheckBox.hasFocus && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightCommand)))
                updateAll = true;
            ForestBrushMod.instance.BrushTool.UpdateTreeList(prefab, value, updateAll);
        }

        public void Deselect(bool isRowOdd)
        {
            
        }

        public void Display(object data, bool isRowOdd)
        {
            if (data == null) return;
            try
            {
                prefab = data as TreeInfo;
                Initialize(prefab);
                includeCheckBox.isChecked = ForestBrushMod.instance.Container.m_variations.Any(v => v.m_finalTree == prefab);
                treeNameLabel.text = prefab.GetUncheckedLocalizedTitle();
                thumbNailSprite.texture = prefab.m_Atlas.sprites.Find(spr => spr.name == prefab.m_Thumbnail).texture;
                backgroundSprite = "";
                if (isRowOdd)
                    backgroundSprite = "ListItemHover";
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(exception + "Stacktrace: " + exception.StackTrace, this);
                
            }
        }

        public void Select(bool isRowOdd)
        {
            
        }       
    }
}
