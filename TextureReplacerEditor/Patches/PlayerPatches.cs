﻿using HarmonyLib;
using TextureReplacerEditor.Monobehaviors;
using UnityEngine;
using Utilities = IndigocoderLib.Utilities;

namespace TextureReplacerEditor.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal class PlayerPatches
    {
        [HarmonyPatch(nameof(Player.Start)), HarmonyPostfix]
        private static void Start_Postfix(Player __instance)
        {
            GameObject editorCanvas = Main_Plugin.AssetBundle.LoadAsset<GameObject>("TextureReplacerEditorCanvas");
            GameObject editorCanvasInstance = GameObject.Instantiate(editorCanvas);
            editorCanvasInstance.SetActive(false);
            Main_Plugin.CurrentEditorWindowInstance = editorCanvasInstance;
        }

        [HarmonyPatch(nameof(Player.Update)), HarmonyPostfix]
        private static void Update_Postfix(Player __instance)
        {
            if (!Input.GetMouseButtonDown(2)) return;

            RaycastHit hitInfo;
            if (!Physics.Raycast(SNCameraRoot.main.transform.position, SNCameraRoot.main.transform.forward, out hitInfo)) return;

            var prefabIdentifier = hitInfo.collider.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null) return;

            TextureReplacerEditorWindow editorWindow = Main_Plugin.CurrentEditorWindowInstance.GetComponent<TextureReplacerEditorWindow>();
            editorWindow.prefabInfoWindow.prefabNameText.text = Utilities.GetNameWithCloneRemoved(prefabIdentifier.name);
            editorWindow.prefabInfoWindow.CreateChildHierarchy(prefabIdentifier.transform);

            Time.timeScale = 0;
            TextureReplacerEditorWindow.windowActive = true;

            Main_Plugin.CurrentEditorWindowInstance.SetActive(true);
        }
    }
}