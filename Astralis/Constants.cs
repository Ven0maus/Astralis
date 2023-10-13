using Astralis.Extended;
using Astralis.GameCode.Npcs;
using SadRogue.Primitives;
using System;

namespace Astralis
{
    public class Constants
    {
        public static int GameSeed = 601;
        public const bool DebugMode = false;
        public const bool FullScreen = false;

        public static Random Random;

        public static int ScreenWidth { get { return Resolution.ScreenCellsX; } }
        public static int ScreenHeight { get { return Resolution.ScreenCellsY; } }

        public static readonly Color GameTitleColor = Color.Ivory;
        public static readonly Color GameTitleShadowColor = Color.Lerp(Color.Black, GameTitleColor, 0.35f);
        public const string GameTitle = "Astralis";
        public const string GameTitleFancy = @"
     _            _                   _   _       
    / \     ___  | |_   _ __   __ _  | | (_)  ___ 
   / _ \   / __| | __| | '__| / _` | | | | | / __|
  / ___ \  \__ \ | |_  | |   | (_| | | | | | \__ \
 /_/   \_\ |___/  \__| |_|    \__,_| |_| |_| |___/";

        public static class Fonts
        {
            /// <summary>
            /// Fonts that are created and used during gameplay are stored here.
            /// </summary>
            public const string DynamicFontsPath = $"{Configuration.GamedataPath}/Fonts";

            public static class UserInterfaceFonts
            {
                public const string Anno = "Resources/Tilesets/Anno_16x16.font";
            }

            public static class WorldFonts
            {
                public const string WorldObjects = "Resources/Tilesets/WorldObjects_16x16.font";
            }

            public static class NpcFonts
            {
                public const string TemplateNpcFont = "Resources/Npcs/TemplateNpc_16x16.font";
                public const string BaseNpc = "Resources/Npcs/BaseNpc_16x16.font";
                public const string ProceduralNpcsFont = $"{DynamicFontsPath}/ProceduralNpcs_16x16.font";

                internal static Color[] PredefinedColors = new Color[]
                {
                    Color.Coral,
                    Color.Green,
                    Color.Blue,
                    Color.DarkOrange,
                    Color.OliveDrab,
                    Color.AnsiYellowBright,
                    Color.Cyan,
                    Color.Magenta,
                    Color.Brown,
                    Color.Teal,
                    Color.Gray,
                    Color.Lime,
                    Color.Thistle,
                    Color.DarkRed,
                    Color.Indigo
                };

                internal static Color[] GetSkinColors(Race race)
                {
                    switch (race)
                    {
                        case Race.Orc:
                            return new[] { "#4D2600".HexToColor(), "#006600".HexToColor() };
                        case Race.Human:
                        case Race.Elf:
                        case Race.Dwarf:
                            return new[] { "#e6bc98".HexToColor(), "#3b2219".HexToColor() };
                        default:
                            throw new NotImplementedException($"Skin color for race '{race}' not implemented.");
                    }
                }
            }
        }

        public static class WorldGeneration
        {
            public const float WorldZoomFentity = 1.75f;
            public const bool DrawBordersOnDebugMode = false;
            public const int ExtraChunkRadius = 2;
            public const int ChunkSize = 32;
        }

        public static class Configuration
        {
            public const string JsonFilesPath = "Configuration/Data";
            public const string GamedataPath = "Gamedata";

            // Give each trait points (+1, -1) etc
            // The player starts with a specific amount of points they can use
            // In the end the player should be at 0 or positive points to start the game
            // TODO: Move traits and their points into JSON config
            // Define also a few presets that have some traits selected (positive and negative)
            // When displaying the traits, make sure the color is green or red based on positive/negative
            // Display points that the player has available
            // When selecting negative traits, the points should be added to the player available points
            // Also deducted when selecting a positive trait or removing a negative trait from selected traits

            public enum Traits
            {
                // Positive traits
                // Abilities
                Beast_Whisperer, // Can tame and command certain beasts / animals / monsters
                Keen_Instincts, // Can sense traps nearby
                Steelmind, // Unaffected by illusions or fear
                Shadowstalker, // Summons a stalker minion to protect its owner during the night
                Lucky, // Gets an extra dice roll for loot drops, taking the better roll of the two.
                Supportive, // Applies all character buffs to other players nearby

                // Modifiers
                Stealthy, // 25% bonus to gaining experience for stealth skill
                Resourceful, // 25% chance not to deplete a resource when depletion check happens
                Quick_Reflexes, // 15% chance to dodge incoming melee hits
                Hardy, // 10% reduced damage from incoming melee hits
                Intuitive, // 25% bonus to gaining experience for magical skills
                Marksman, // Non-magic projectiles have 25% less chance to break on impact, and 10% less chance to miss
                Eagle_Eyed, // Has a 30% larger vision range
                Quick_Learner, // Learns skills 25% faster than others
                Night_Owl, // Vision range expanded when in the dark, npcs highlighted at night
                Fast_Healer, // Recovers health and broken limbs 25% faster,
                Vampiric, // When killing a bleeding enemy, 25% chance to recover 5-10% of current missing health
                Scammer, // 10% increased prices when selling to traders
                Polyglot, // 10% increased experience gain of social skill

                // Negative traits
                // Abilities
                Unlucky, // Gets an extra dice roll for loot drops, taking the worst roll of the two.
                Ethnic_Kinship, // Will not harm others of the same race
                Unfriendly, // 10% reduced experience gain of social skill 

                // Modifiers
                Lone_Wolf, // Reduced effectiveness of all skills by 15% when not alone
                Psychotic, // Will inflict cuts on himself occassionaly, applying a bleed debuff on itself
                Noctophobia, // Reduced effectiveness of combat skills by 25% at night
                Frail_Bones, // Has a 10% higher chance to break a limb when hit by a strong attack
                Hemophobia, // 10% reduced effectiveness of combat skills when bleeding
                Nomads_Curse, // 25% reduced effectiveness of all skills when in the same area for more than 2 days
                Slow_Healer, // Recovers health and broken limbs 25% slower
                Undead, // Everything is hostile to you except undead or creatures of the night
                Misanthrope, // 20% increased prices when buying from traders
                Ailment_Magnet, // 10% more chance to contract an illness or debuff
            }

            public enum TraitPresets
            {
                None,
                Forester,
            }
        }
    }
}
