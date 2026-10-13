using System;

namespace SpaceBourne2ModTool.Models
{
    /// <summary>
    /// Represents a single cheat / trainer option inspired by existing WeMod & Cheat Engine tables
    /// for SpaceBourne 2 (health, armor, ship shields, money, attributes, no overheat, etc.).
    /// </summary>
    public class TrainerOption
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }   // Player, Ship, Weapons, Stats, Game
        public string Description { get; set; }
        public TrainerValueType ValueType { get; set; }
        public object DefaultValue { get; set; }
        public object CurrentValue { get; set; }
        public string HotkeyHint { get; set; }
        public bool IsToggle { get; set; }
        public bool Enabled { get; set; }

        // For documentation / CE script generation
        public string SuggestedPointerPath { get; set; }
        public string Notes { get; set; }
    }

    public enum TrainerValueType
    {
        BooleanToggle,   // Unlimited Health, No Overheat
        IntegerAdd,      // Add 100 Health, Add 100K Money
        FloatMultiplier, // Game Speed
        IntegerSet       // Set Attribute Points
    }
}
