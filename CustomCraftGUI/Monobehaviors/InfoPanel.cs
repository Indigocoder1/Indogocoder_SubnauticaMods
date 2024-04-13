﻿using CustomCraftGUI.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static HandReticle;

namespace CustomCraftGUI.Monobehaviors
{
    public class InfoPanel : MonoBehaviour
    {
        public TextMeshProUGUI itemNameText;
        public uGUI_ItemIcon itemIcon;

        public GameObject ingredientItemPrefab;
        public Transform ingredientsPrefabParent;
        public Transform linkedItemsPrefabsParent;

        private List<IngredientItem> linkedItems = new();

        private void Start()
        {
            ClearItemsLists();
        }

        public void SetCurrentItem(ItemIcon icon)
        {
            ClearItemsLists();

            Atlas.Sprite sprite = SpriteManager.Get(icon.techType);

            itemNameText.text = icon.itemName;
            itemIcon.SetForegroundSprite(sprite);
            itemIcon.foreground.transform.localScale = SpriteSizeFormatter.GetSpriteShrinkScalar(sprite);

            ITechData techData = CraftData.Get(icon.techType, true);
            if (techData == null) return;

            for (int i = 0; i < techData.ingredientCount; i++)
            {
                IIngredient ingredient = techData.GetIngredient(i);
                IngredientItem ingredientItem = Instantiate(ingredientItemPrefab, ingredientsPrefabParent).GetComponent<IngredientItem>();
                ingredientItem.SetInfo(SpriteManager.Get(ingredient.techType), ingredient.techType, ingredient.amount);
            }

            for (int i = 0; i < techData.linkedItemCount; i++)
            {
                TechType techType = techData.GetLinkedItem(i);
                IngredientItem ingredientItem = Instantiate(ingredientItemPrefab, linkedItemsPrefabsParent).GetComponent<IngredientItem>();
                ingredientItem.SetInfo(SpriteManager.Get(techType), techType, 1);

                linkedItems.Add(ingredientItem);
            }

            TryCollapseLinkedItems();
        }

        public void ClearItemsLists()
        {
            foreach (Transform child in ingredientsPrefabParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in linkedItemsPrefabsParent)
            {
                Destroy(child.gameObject);
            }

            linkedItems.Clear();
        }

        private void TryCollapseLinkedItems()
        {
            Dictionary<TechType, int> linkedItemValues = new();

            foreach (IngredientItem item in linkedItems)
            {
                if(!linkedItemValues.ContainsKey(item.techType))
                {
                    linkedItemValues.Add(item.techType, 1);
                }
                else
                {
                    linkedItemValues[item.techType]++;
                }
            }

            foreach (Transform child in linkedItemsPrefabsParent)
            {
                Destroy(child.gameObject);
            }

            linkedItems.Clear();

            foreach (TechType key in linkedItemValues.Keys)
            {
                IngredientItem ingredientItem = Instantiate(ingredientItemPrefab, linkedItemsPrefabsParent).GetComponent<IngredientItem>();
                ingredientItem.SetInfo(SpriteManager.Get(key), key, linkedItemValues[key]);

                linkedItems.Add(ingredientItem);
            }
        }
    }
}