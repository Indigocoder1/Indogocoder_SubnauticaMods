﻿using System.Collections.Generic;
using System.Linq;
using TextureReplacerEditor.Monobehaviors.Items;
using TextureReplacerEditor.Monobehaviors.PropertyWindowHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace TextureReplacerEditor.Monobehaviors.Windows
{
    internal class MaterialWindow : DraggableWindow
    {
        private const float MIN_SEARCH_BAR_WAIT_TIME = 0.5f;

        public MaterialItem currentMaterialItem { get; private set; }
        public Dictionary<MaterialEditData, List<PropertyEditData>> materialEdits { get; private set; } = new();
        public MaterialEditData CurrentMaterialEditData
        {
            get
            {
                if (currentMaterialItem)
                {
                    return new(currentMaterialItem.material, currentMaterialItem.MaterialIndex, currentMaterialItem.prefabIdentifierRoot, currentMaterialItem.pathToRenderer);
                }

                return _currentMaterialEditData;
            }
            private set
            {
                _currentMaterialEditData = value;
            }
        }

        private MaterialEditData _currentMaterialEditData;

        public TMP_InputField searchBar;
        public TextMeshProUGUI matNameText;
        public GameObject propertyItemPrefab;
        public Transform propertyItemsParent;
        public Transform tutorialLineTarget;
        public GameObject tutorialHighlight;

        private Material material;
        private float currentSearchCallDelay;
        private bool hasSearched;
        private bool overrideEditData;

        public void SetMaterial(Material material, MaterialItem item)
        {
            this.material = material;
            currentMaterialItem = item;
            overrideEditData = false;
            matNameText.text = material.name;

            SpawnPropertyItems();

            CurrentMaterialEditData = new(item.material, item.MaterialIndex, item.prefabIdentifierRoot, item.pathToRenderer);
            searchBar.onValueChanged.AddListener((_) => OnSearchBarValueChanged());
        }

        public void SetMaterial(Material material, MaterialEditData editData)
        {
            this.material = material;
            currentMaterialItem = null;
            overrideEditData = true;
            SpawnPropertyItems();

            CurrentMaterialEditData = new(editData.material, editData.materialIndex, editData.prefabIdentifier, editData.pathToRend);
            searchBar.onValueChanged.AddListener((_) => OnSearchBarValueChanged());
        }

        private void Update()
        {
            if (currentSearchCallDelay > 0)
            {
                currentSearchCallDelay -= Time.unscaledDeltaTime;
            }
            else if (currentSearchCallDelay <= 0 && !hasSearched)
            {
                Search();

                hasSearched = true;
            }
        }

        public void OnPropertyChanged(object sender, OnPropertyChangedEventArgs e)
        {
            if (currentMaterialItem == null && !overrideEditData) return;

            if (!materialEdits.ContainsKey(CurrentMaterialEditData))
            {
                materialEdits.Add(CurrentMaterialEditData, new List<PropertyEditData>());
            }

            PropertyItem propertyItem = (sender as MonoBehaviour).GetComponentInParent<PropertyItem>();

            if (e.originalValue.Equals(e.newValue))
            {
                if (materialEdits[CurrentMaterialEditData].Any(i => i.propertyItem.propertyName == propertyItem.propertyName))
                {
                    PropertyEditData item = materialEdits[CurrentMaterialEditData].First(i => i.propertyItem.propertyName == propertyItem.propertyName);
                    materialEdits[CurrentMaterialEditData].Remove(item);
                }

                return;
            }

            if (materialEdits[CurrentMaterialEditData].Any(i => i.propertyItem.propertyName == propertyItem.propertyName))
            {
                PropertyEditData item = materialEdits[CurrentMaterialEditData].First(i => i.propertyItem.propertyName == propertyItem.propertyName);
                int index = materialEdits[CurrentMaterialEditData].IndexOf(item);
                materialEdits[CurrentMaterialEditData][index] = new PropertyEditData(propertyItem, e.originalValue, e.newValue, propertyItem.propertyName, e.changedType);
            }
            else
            {
                materialEdits[CurrentMaterialEditData].Add(new PropertyEditData(propertyItem, e.originalValue, e.newValue, propertyItem.propertyName, e.changedType));
            }
        }

        public void OpenConfigWindow()
        {
            ConfigViewerWindow configWindow = TextureReplacerEditorWindow.Instance.configViewerWindow;
            configWindow.OpenWindow();

            if (!configWindow.addedItems.Any(i => i.materialEditData.Equals(CurrentMaterialEditData)))
            {
                configWindow.AddConfigItem();
            }
            else
            {
                CustomConfigItem item = configWindow.addedItems.First(i => i.materialEditData.Equals(CurrentMaterialEditData));
                configWindow.UpdateConfigItem(item);
            }

            TextureReplacerEditorWindow.Instance.tutorialHandler.TriggerTutorialItem("OnConfigWindowOpened");
        }

        public void SetTutorialHighlightActive(bool active)
        {
            tutorialHighlight.SetActive(active);
        }

        private void ClearPropertyItems()
        {
            foreach (Transform child in propertyItemsParent)
            {
                Destroy(child.gameObject);
            }
        }

        private void SpawnPropertyItems()
        {
            ClearPropertyItems();

            int propertyCount = material.shader.GetPropertyCount();
            for (int i = 0; i < propertyCount; i++)
            {
                ShaderPropertyType type = material.shader.GetPropertyType(i);
                string propertyName = material.shader.GetPropertyName(i);
                PropertyItem propertyItem = Instantiate(propertyItemPrefab, propertyItemsParent).GetComponent<PropertyItem>();

                if (materialEdits.Any(i => i.Value.Any(a => a.propertyName == propertyName)))
                {
                    List<PropertyEditData> editDatas = materialEdits.FirstOrDefault(i => i.Value.Any(a => a.propertyName == propertyName)).Value;
                    PropertyEditData data = editDatas.FirstOrDefault(i => i.propertyName == propertyName);
                    propertyItem.SetInfo(type, material, propertyName, data.originalValue);
                }
                else
                {
                    propertyItem.SetInfo(type, material, propertyName);
                }
            }
        }

        private void OnSearchBarValueChanged()
        {
            currentSearchCallDelay = MIN_SEARCH_BAR_WAIT_TIME;
            hasSearched = false;
        }

        private void Search()
        {
            foreach (var propertyItem in propertyItemsParent.GetComponentsInChildren<PropertyItem>(true))
            {
                if (string.IsNullOrEmpty(searchBar.text))
                {
                    propertyItem.gameObject.SetActive(true);
                    continue;
                }

                if (propertyItem.propertyName.ToLower().Contains(searchBar.text.ToLower()))
                {
                    propertyItem.gameObject.SetActive(true);
                }
                else
                {
                    propertyItem.gameObject.SetActive(false);
                }
            }
        }

        public struct PropertyEditData
        {
            public PropertyItem propertyItem;
            public object originalValue;
            public object newValue;
            public string propertyName;
            public ShaderPropertyType type;

            public PropertyEditData(PropertyItem propertyItem, object originalValue, object newValue, string propertyName, ShaderPropertyType type)
            {
                this.propertyItem = propertyItem;
                this.originalValue = originalValue;
                this.newValue = newValue;
                this.propertyName = propertyName;
                this.type = type;
            }
        }

        public struct MaterialEditData
        {
            public Material material;
            public int materialIndex;
            public PrefabIdentifier prefabIdentifier;
            public string pathToRend;

            public MaterialEditData(Material material, int index, PrefabIdentifier identifier, string pathToRend)
            {
                this.material = material;
                materialIndex = index;
                prefabIdentifier = identifier;
                this.pathToRend = pathToRend;
            }
        }
    }
}
