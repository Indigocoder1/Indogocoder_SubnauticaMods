﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SuitLib.ModdedSuitsManager;
using static TechStringCache;

namespace SuitLib.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal static class Player_Patches
    {
        private static Dictionary<string, Texture> diveSuitTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, Texture> radiationSuitTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, Texture> reinforcedSuitTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, Texture> filtrationSuitTextures = new Dictionary<string, Texture>();

        private static Dictionary<string, Texture> diveGlovesTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, Texture> radiationGlovesTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, Texture> reinforcedGlovesTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, Texture> filtrationGlovesTextures = new Dictionary<string, Texture>();

        private static GameObject diveSuitBodyGO;
        private static GameObject diveSuitGlovesGO;

        private static GameObject radiationSuitGO;
        private static GameObject radiationGlovesGO;

        private static GameObject reinforcedSuitGO;
        private static GameObject reinforcedGlovesGO;

        private static GameObject filtrationSuitGO;
        private static Equipment playerEquipment;

        [HarmonyPatch(nameof(Player.Start)), HarmonyPrefix]
        private static void Start()
        {
            playerEquipment = Inventory.main.equipment;
            InitializeModelGOs();
            InitializeVanillaTextures();
        }

        private static void InitializeVanillaTextures()
        {
            HashSet<string> suitTextureNamesHash = new HashSet<string>();
            for (int suitsIndex = 0; suitsIndex < ModdedSuitsManager.moddedSuitsList.Count; suitsIndex++)
            {
                ModdedSuit suit = ModdedSuitsManager.moddedSuitsList[suitsIndex];
                List<string> keys = new List<string>(suit.suitReplacementTexturePropertyPairs.Keys);
                foreach (string key in keys)
                {
                    suitTextureNamesHash.Add(key);
                }
            }

            HashSet<string> gloveTextureNamesHash = new HashSet<string>();
            for (int glovesIndex = 0; glovesIndex < ModdedSuitsManager.moddedGlovesList.Count; glovesIndex++)
            {
                ModdedGloves gloves = ModdedSuitsManager.moddedGlovesList[glovesIndex];
                List<string> keys = new List<string>(gloves.replacementTexturePropertyPairs.Keys);
                foreach (string key in keys)
                {
                    gloveTextureNamesHash.Add(key);
                }
            }

            for (int suitTexIndex = 0; suitTexIndex < suitTextureNamesHash.Count; suitTexIndex++)
            {
                string texName = suitTextureNamesHash.ElementAt(suitTexIndex);

                Texture diveSuitTex = diveSuitBodyGO.GetComponent<Renderer>().material.GetTexture(texName);
                diveSuitTextures.Add(texName, diveSuitTex);

                Texture radiationSuitTex = radiationSuitGO.GetComponent<Renderer>().material.GetTexture(texName);
                radiationSuitTextures.Add(texName, radiationSuitTex);

                Texture reinforcedSuitTex = reinforcedSuitGO.GetComponent<Renderer>().material.GetTexture(texName);
                reinforcedSuitTextures.Add(texName, reinforcedSuitTex);

                Texture filtrationSuitTex = filtrationSuitGO.GetComponent<Renderer>().material.GetTexture(texName);
                filtrationSuitTextures.Add(texName, filtrationSuitTex);
            }

            for (int glovesTexIndex = 0; glovesTexIndex < gloveTextureNamesHash.Count; glovesTexIndex++)
            {
                string texName = gloveTextureNamesHash.ElementAt(glovesTexIndex);

                Texture diveGlovesTex = diveSuitGlovesGO.GetComponent<Renderer>().material.GetTexture(texName);
                diveGlovesTextures.Add(texName, diveGlovesTex);

                Texture radiationGlovesTex = radiationGlovesGO.GetComponent<Renderer>().material.GetTexture(texName);
                radiationGlovesTextures.Add(texName, radiationGlovesTex);

                Texture reinforcedGlovesTex = reinforcedGlovesGO.GetComponent<Renderer>().material.GetTexture(texName);
                reinforcedGlovesTextures.Add(texName, reinforcedGlovesTex);
            }
        }

        private static void InitializeModelGOs()
        {
            Transform geo = Player.main.transform.Find("body/player_view/male_geo");

            Transform diveSuit = geo.Find("diveSuit");
            diveSuitBodyGO = diveSuit.Find("diveSuit_body_geo").gameObject;
            diveSuitGlovesGO = diveSuit.Find("diveSuit_hands_geo").gameObject;

            Transform radiationSuit = geo.Find("radiationSuit");
            radiationSuitGO = radiationSuit.Find("radiationSuit_body_geo").gameObject;
            radiationGlovesGO = radiationSuit.Find("radiationSuit_gloves_geo").gameObject;

            Transform reinforcedSuit = geo.Find("reinforcedSuit");
            reinforcedSuitGO = reinforcedSuit.Find("reinforced_suit_01_body_geo").gameObject;
            reinforcedGlovesGO = reinforcedSuit.Find("reinforced_suit_01_glove_geo").gameObject;

            filtrationSuitGO = geo.Find("stillSuit/still_suit_01_body_geo").gameObject;
        }

        [HarmonyPatch(nameof(Player.EquipmentChanged)), HarmonyPostfix]
        private static void EquipmentChanged_Patch()
        {
            TechType typeInBodySlot = playerEquipment.GetTechTypeInSlot("Body");
            TechType typeInGlovesSlot = playerEquipment.GetTechTypeInSlot("Gloves");

            bool wearingModdedSuit =
                (typeInBodySlot != TechType.None) &&
                (typeInBodySlot != TechType.RadiationSuit) &&
                (typeInBodySlot != TechType.ReinforcedDiveSuit) &&
                (typeInBodySlot != TechType.WaterFiltrationSuit);

            bool wearingModdedGloves =
                (typeInGlovesSlot != TechType.None) &&
                (typeInGlovesSlot != TechType.RadiationGloves) &&
                (typeInGlovesSlot != TechType.ReinforcedGloves);

            bool wearingAnySuit = typeInBodySlot != TechType.None;
            bool wearingAnyGloves = typeInGlovesSlot != TechType.None;

            SetGOsActive(typeInBodySlot, typeInGlovesSlot, wearingAnySuit, wearingAnyGloves);

            SetGloveTextures(typeInGlovesSlot, wearingModdedGloves);
            SetSuitTextures(typeInBodySlot, wearingModdedSuit);
        }

        [HarmonyPatch(nameof(Player.HasReinforcedSuit)), HarmonyPostfix]
        private static void HasReinforcedSuit_Patch(bool __result)
        {
            foreach (ModdedSuit suit in moddedSuitsList)
            {
                if (!WearingItem(suit.itemTechType, "Body"))
                {
                    continue;
                }

                if ((suit.modifications & Modifications.Reinforced) != 0)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(nameof(Player.HasReinforcedGloves)), HarmonyPostfix]
        private static void HasReinforcedGloves_Patch(bool __result)
        {
            foreach (ModdedGloves gloves in moddedGlovesList)
            {
                if (!WearingItem(gloves.itemTechType, "Gloves"))
                {
                    continue;
                }

                if ((gloves.modifications & Modifications.Reinforced) != 0)
                {
                    __result = true;
                }
            }
        }

        private static void SetGOsActive(TechType typeInBodySlot, TechType typeInGlovesSlot, bool wearingAnySuit, bool wearingAnyGloves)
        {
            if (wearingAnySuit)
            {
                diveSuitBodyGO.SetActive(false);

                foreach (ModdedSuit suit in ModdedSuitsManager.moddedSuitsList)
                {
                    if (suit.itemTechType != typeInBodySlot)
                    {
                        continue;
                    }

                    GameObject model = GetModel(suit.vanillaModel, true);
                    model.SetActive(true);
                }
            }
            else
            {
                diveSuitBodyGO.SetActive(true);
            }

            if (wearingAnyGloves)
            {
                diveSuitGlovesGO.SetActive(false);

                foreach (ModdedGloves gloves in ModdedSuitsManager.moddedGlovesList)
                {
                    if (gloves.itemTechType != typeInGlovesSlot)
                    {
                        continue;
                    }

                    GameObject model = GetModel(gloves.vanillaModel, false);
                    model.SetActive(true);
                }
            }
            else
            {
                diveSuitGlovesGO.SetActive(true);
            }
        }

        private static void SetSuitTextures(TechType typeInSuitSlot, bool wearingModdedSuit)
        {
            if (wearingModdedSuit)
            {
                foreach (ModdedSuit suit in ModdedSuitsManager.moddedSuitsList)
                {
                    if (!WearingItem(suit.itemTechType, "Body"))
                    {
                        continue;
                    }

                    Renderer modelRenderer = GetModel(suit.vanillaModel, true).GetComponent<Renderer>();
                    foreach (string key in suit.suitReplacementTexturePropertyPairs.Keys)
                    {
                        Texture2D moddedTex = suit.suitReplacementTexturePropertyPairs[key];
                        modelRenderer.materials[0].SetTexture(key, moddedTex);

                    }

                    foreach (string key in suit.armsReplacementTexturePropertyPairs.Keys)
                    {
                        Texture2D armsTex = suit.armsReplacementTexturePropertyPairs[key];
                        modelRenderer.materials[1].SetTexture(key, armsTex);
                    }
                }
            }
            else
            {
                foreach (string key in diveSuitTextures.Keys)
                {
                    Texture2D tex = (Texture2D)diveSuitTextures[key];
                    diveSuitBodyGO.GetComponent<Renderer>().material.SetTexture(key, tex);
                }

                foreach (string key in radiationSuitTextures.Keys)
                {
                    Texture2D tex = (Texture2D)radiationSuitTextures[key];
                    Renderer rend = radiationSuitGO.GetComponent<Renderer>();
                    for (int i = 0; i < rend.materials.Length; i++)
                    {
                        rend.materials[i].SetTexture(key, tex);
                    }
                }

                foreach (string key in reinforcedSuitTextures.Keys)
                {
                    Texture2D tex = (Texture2D)reinforcedSuitTextures[key];
                    Renderer rend = reinforcedSuitGO.GetComponent<Renderer>();
                    for (int i = 0; i < rend.materials.Length; i++)
                    {
                        rend.materials[i].SetTexture(key, tex);
                    }
                }

                foreach (string key in filtrationSuitTextures.Keys)
                {
                    Texture2D tex = (Texture2D)filtrationSuitTextures[key];
                    Renderer rend = filtrationSuitGO.GetComponent<Renderer>();
                    for (int i = 0; i < rend.materials.Length; i++)
                    {
                        rend.materials[i].SetTexture(key, tex);
                    }
                }
            }
        }

        private static void SetGloveTextures(TechType typeInGlovesSlot, bool wearingModdedGloves)
        {
            if (wearingModdedGloves)
            {
                foreach (ModdedGloves gloves in ModdedSuitsManager.moddedGlovesList)
                {
                    if (!WearingItem(gloves.itemTechType, "Gloves"))
                    {
                        continue;
                    }

                    Renderer glovesModelRenderer = GetModel(gloves.vanillaModel, false).GetComponent<Renderer>();

                    foreach (string key in gloves.replacementTexturePropertyPairs.Keys)
                    {
                        Texture2D moddedTex = gloves.replacementTexturePropertyPairs[key];
                        glovesModelRenderer.material.SetTexture(key, moddedTex);
                    }
                }
            }
            else
            {
                foreach (string key in diveGlovesTextures.Keys)
                {
                    Texture2D tex = (Texture2D)diveGlovesTextures[key];
                    diveSuitGlovesGO.GetComponent<Renderer>().material.SetTexture(key, tex);
                }

                foreach (string key in radiationGlovesTextures.Keys)
                {
                    Texture2D tex = (Texture2D)radiationGlovesTextures[key];
                    radiationGlovesGO.GetComponent<Renderer>().material.SetTexture(key, tex);
                }

                foreach (string key in reinforcedGlovesTextures.Keys)
                {
                    Texture2D tex = (Texture2D)reinforcedGlovesTextures[key];
                    reinforcedGlovesGO.GetComponent<Renderer>().material.SetTexture(key, tex);
                }
            }
        }

        private static GameObject GetModel(VanillaModel type, bool isSuit)
        {
            switch (type)
            {
                case VanillaModel.Dive:
                    return isSuit ? diveSuitBodyGO : diveSuitGlovesGO;
                case VanillaModel.Radiation:
                    return isSuit ? radiationSuitGO : radiationGlovesGO;
                case VanillaModel.Reinforced:
                    return isSuit ? reinforcedSuitGO : reinforcedGlovesGO;
                case VanillaModel.WaterFiltration:
                    return isSuit ? filtrationSuitGO : null;
                default:
                    return null;
            }
        }

        public static bool WearingItem(TechType item, string slot)
        {
            return item == playerEquipment.GetTechTypeInSlot(slot);
        }
    }
}