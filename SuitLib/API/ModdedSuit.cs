﻿using System.Collections.Generic;
using UnityEngine;
using static SuitLib.ModdedSuitsManager;

namespace SuitLib
{
    public class ModdedSuit
    {
        public Dictionary<string, Texture2D> suitReplacementTexturePropertyPairs;
        public Dictionary<string, Texture2D> armsReplacementTexturePropertyPairs;
        public VanillaModel vanillaModel;
        public TechType itemTechType;
        public Modifications modifications;
        public ModificationValues modificationValues;
        public float deathrunCrushDepth;

        /// <param name="suitReplacementTexturePropertyPairs">The texture name (like _MainTex) and the texture pairs for the suit (not the arms!)</param>
        /// <param name="vanillaModel">The tech type for the suit you're replacing (like reinforcedSuit)</param>
        /// <param name="itemTechType">The tech type of the moddedsuit</param>
        public ModdedSuit(Dictionary<string, Texture2D> suitReplacementTexturePropertyPairs, Dictionary<string, Texture2D> armsReplacementTexturePropertyPairs, 
            VanillaModel vanillaModel, TechType itemTechType)
        {
            this.suitReplacementTexturePropertyPairs = suitReplacementTexturePropertyPairs;
            this.armsReplacementTexturePropertyPairs = armsReplacementTexturePropertyPairs;
            this.vanillaModel = vanillaModel;
            this.itemTechType = itemTechType;
        }

        public ModdedSuit(Dictionary<string, Texture2D> suitReplacementTexturePropertyPairs, Dictionary<string, Texture2D> armsReplacementTexturePropertyPairs,
            VanillaModel vanillaModel, TechType itemTechType, Modifications modifications, ModificationValues modificationValues = null)
        {
            this.suitReplacementTexturePropertyPairs = suitReplacementTexturePropertyPairs;
            this.armsReplacementTexturePropertyPairs = armsReplacementTexturePropertyPairs;
            this.vanillaModel = vanillaModel;
            this.itemTechType = itemTechType;
            this.modifications = modifications;
            this.modificationValues = modificationValues;
        }

        /// <param name="deathrunCrushDepth">Deathrun is required for this parameter</param>
        public ModdedSuit(Dictionary<string, Texture2D> suitReplacementTexturePropertyPairs, Dictionary<string, Texture2D> armsReplacementTexturePropertyPairs,
    VanillaModel vanillaModel, TechType itemTechType, float deathrunCrushDepth)
        {
            this.suitReplacementTexturePropertyPairs = suitReplacementTexturePropertyPairs;
            this.armsReplacementTexturePropertyPairs = armsReplacementTexturePropertyPairs;
            this.vanillaModel = vanillaModel;
            this.itemTechType = itemTechType;
            this.deathrunCrushDepth = deathrunCrushDepth;
        }
    }
}