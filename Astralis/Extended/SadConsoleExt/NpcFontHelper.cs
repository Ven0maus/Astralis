using Astralis.GameCode.Npcs;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SadConsole;
using SadConsole.FontEditing;
using SadConsole.Renderers;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Astralis.Extended.SadConsoleExt
{
    /// <summary>
    /// Class used to combine npc character glyphs into one NPC font
    /// </summary>
    internal static class NpcFontHelper
    {
        private static readonly Dictionary<SadFont, SadFontInfo> _npcFontCache = new();
        private static Dictionary<string, NpcConfiguration[]> _npcConfig;

        private class SadFontInfo
        {
            public int NextAvailableIndex { get; set; }
            public string ConfigFilePath { get; }
            public string ImageFilePath { get; }

            public SadFontInfo(string configFilePath, string imageFilePath, int availableIndex)
            {
                ConfigFilePath = configFilePath;
                NextAvailableIndex = availableIndex;
                ImageFilePath = imageFilePath;
            }
        }

        readonly struct NpcCombination : IEquatable<NpcCombination>
        {
            public readonly Color SkinColor;
            public readonly Color HairColor;
            public readonly Color ShirtColor;
            public readonly Color PantsColor;
            public readonly Gender Gender;

            public NpcCombination(Gender gender, Color skinColor, Color hairColor, Color shirtColor, Color pantsColor)
            {
                SkinColor = skinColor;
                HairColor = hairColor;
                ShirtColor = shirtColor;
                PantsColor = pantsColor;
                Gender = gender;
            }

            public bool Equals(NpcCombination other)
            {
                return SkinColor.Equals(other.SkinColor) &&
                    HairColor.Equals(other.HairColor) &&
                    ShirtColor.Equals(other.ShirtColor) &&
                    PantsColor.Equals(other.PantsColor) &&
                    Gender.Equals(other.Gender);
            }

            public override bool Equals(object obj)
            {
                return obj is NpcCombination combination && Equals(combination);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(SkinColor, HairColor, ShirtColor, PantsColor, Gender);
            }
        }

        private static Color GetRandomColor()
        {
            return Constants.Fonts.NpcFonts.PredefinedColors[Constants.Random.Next(0, Constants.Fonts.NpcFonts.PredefinedColors.Length)];
        }

        public static int[] GenerateRandomNpcGlyphs(string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentException("Param savePath cannot be null or empty.", nameof(savePath));

            var skinColorStartEnd = Constants.Fonts.NpcFonts.GetSkinColors(Race.Human);
            var skinColors = skinColorStartEnd[0].LerpSteps(skinColorStartEnd[1], 15);

            var facings = new[] { Facing.Forward, Facing.Left, Facing.Backwards };
            var uniqueCombinations = new HashSet<NpcCombination>();
            var npcGlyphs = new List<int>();
            const int total = 76;
            int half = total / 2;
            // Generate glyphs
            for (int i = 0; i < total; i++)
            {
                var skinColor = skinColors[Constants.Random.Next(0, skinColors.Length)];
                var hairColor = GetRandomColor();
                var shirtColor = GetRandomColor();
                var pantsColor = GetRandomColor();
                var gender = i < half ? Gender.Male : Gender.Female;

                // Make sure they are all unique combinations
                var combination = new NpcCombination(gender, skinColor, hairColor, shirtColor, pantsColor);
                while (uniqueCombinations.Contains(combination))
                {
                    skinColor = skinColors[Constants.Random.Next(0, skinColors.Length)];
                    hairColor = GetRandomColor();
                    shirtColor = GetRandomColor();
                    pantsColor = GetRandomColor();
                    gender = i < half ? Gender.Male : Gender.Female;

                    combination = new NpcCombination(gender, skinColor, hairColor, shirtColor, pantsColor);
                }

                uniqueCombinations.Add(combination);

                var indexes = new List<int>();
                foreach (var facing in facings)
                {
                    var index = CreateNpcGlyph(facing, gender, skinColor, hairColor, shirtColor, pantsColor, true);
                    indexes.Add(index);
                }
                npcGlyphs.Add(indexes[0]);
            }
            SaveFont(GetGamedataNpcFont(savePath, true));
            SaveNpcFonts();

            return npcGlyphs.ToArray();
        }

        public static int CreateNpcGlyph(
            Facing facing,
            Gender gender,
            Color skinColor,
            Color hairColor,
            Color shirtColor,
            Color pantsColor,
            bool addGlyphToFont,
            ScreenSurface surface = null)
        {
            bool wasNull = surface == null;
            if (surface == null)
            {
                surface = new ScreenSurface(1, 1)
                {
                    Font = Game.Instance.Fonts[Constants.Fonts.NpcFonts.CharacterCreationBaseFont],
                    FontSize = new Point(16, 16),
                };
                surface.Surface.DefaultBackground = Color.Transparent;
                surface.Surface[0].Decorators = CellDecoratorHelpers.Pool.Rent();
                surface.Surface[0].Decorators.AddRange(new CellDecorator[] { default, default, default });
            }

            var facingValue = facing.ToString();
            if (facingValue == Facing.Left.ToString() || facingValue == Facing.Right.ToString())
                facingValue = "Sideways";

            var npcGroupGenderConfig = GetGenderConfiguration(facingValue, gender);
            var main = npcGroupGenderConfig.First(a => a.ToString().Split('_').Length == 2);
            var hair = npcGroupGenderConfig.First(a => a.ToString().Split('_')[1].Equals("Hair"));
            var shirt = npcGroupGenderConfig.First(a => a.ToString().Split('_')[1].Equals("Shirt"));
            var pants = npcGroupGenderConfig.First(a => a.ToString().Split('_')[1].Equals("Pants"));

            var mirror = !facingValue.Equals("Sideways") ? Mirror.None :
                facing == Facing.Right ? Mirror.Horizontal : Mirror.None;

            surface.Surface[0].Glyph = (int)main;
            surface.Surface[0].Foreground = skinColor;
            surface.Surface[0].Mirror = mirror;

            // Adjust indexes with new decorators
            surface.Surface[0].Decorators[0] = new CellDecorator(hairColor, (int)hair, mirror);
            surface.Surface[0].Decorators[1] = new CellDecorator(shirtColor, (int)shirt, mirror);
            surface.Surface[0].Decorators[2] = new CellDecorator(pantsColor, (int)pants, mirror);

            // Add glyph to font
            int index = -1;
            if (addGlyphToFont)
            {
                index = AddGlyphToFont(surface, GetGamedataNpcFont());
            }

            if (wasNull)
            {
                CellDecoratorHelpers.Pool.Return(surface.Surface[0].Decorators);
                surface.Dispose();
            }

            return index;
        }

        private static NpcConfiguration[] GetGenderConfiguration(string facing, Gender gender)
        {
            _npcConfig ??= Enum.GetValues<NpcConfiguration>()
                    .GroupBy(a => a.ToString().Split('_').Last())
                    .ToDictionary(a => a.Key, a => a.ToArray());

            return _npcConfig[facing]
                    .GroupBy(a => a.ToString().Split('_')[0])
                    .First(a => a.Key.Equals(gender.ToString()))
                    .ToArray();
        }

        private static SadFont _npcsFont;
        public static SadFont GetGamedataNpcFont(string savePath = null, bool overwrite = false)
        {
            if (!overwrite && _npcsFont != null) return _npcsFont;

            var path = savePath ?? Constants.Fonts.NpcFonts.GamedataNpcFont;

            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);

            SadFont sadFont;
            if (!File.Exists(path))
                sadFont = CreateNewNpcFont(path);
            else if (!IsLoaded(path))
                sadFont = ReadExistingNpcFont(path);
            else
                sadFont = (SadFont)Game.Instance.Fonts[path];
            return _npcsFont = sadFont;
        }

        public static SadFont CreateNewNpcFont(string fontConfigFile)
        {
            // Make a copy of the template
            var fontImageFile = fontConfigFile.Replace(".font", ".png", StringComparison.OrdinalIgnoreCase);
            File.Copy(Constants.Fonts.NpcFonts.NpcFont, fontConfigFile, true);
            File.Copy(Constants.Fonts.NpcFonts.NpcFont.Replace(".font", ".png"), fontImageFile, true);

            // Index for where npc glyph adding starts
            const int startIndex = 2;

            // Adjust config file to match with the new name
            var jsonData = JObject.Parse(File.ReadAllText(fontConfigFile));
            jsonData["FilePath"] = Path.GetFileName(fontImageFile);
            jsonData["Name"] = fontConfigFile;
            jsonData["NextAvailableIndex"] = startIndex;
            File.WriteAllText(fontConfigFile, jsonData.ToString());

            // Load font into sadconsole and cache it
            var font = (SadFont)Game.Instance.LoadFont(fontConfigFile);
            _npcFontCache[font] = new SadFontInfo(fontConfigFile, fontImageFile, startIndex);
            return font;
        }

        public static SadFont ReadExistingNpcFont(string fontConfigFilePath)
        {
            var jsonData = JObject.Parse(File.ReadAllText(fontConfigFilePath));
            var font = (SadFont)Game.Instance.LoadFont(fontConfigFilePath);
            var imageFilePath = Path.Combine(Constants.Fonts.DynamicFontsPath, jsonData["FilePath"].Value<string>());

            // Add to cache
            _npcFontCache[font] = new SadFontInfo(fontConfigFilePath, imageFilePath, jsonData["NextAvailableIndex"].Value<int>());

            // Load font into sadconsole
            return (SadFont)Game.Instance.LoadFont(fontConfigFilePath);
        }

        public static bool IsLoaded(string fontConfigFilePath)
        {
            return _npcFontCache.Any(a => a.Value.ConfigFilePath.Equals(fontConfigFilePath, StringComparison.OrdinalIgnoreCase));
        }

        public static void SaveNpcFonts()
        {
            foreach (var font in _npcFontCache)
            {
                var jsonData = JObject.Parse(File.ReadAllText(font.Value.ConfigFilePath));
                jsonData["NextAvailableIndex"] = font.Value.NextAvailableIndex;
                File.WriteAllText(font.Value.ConfigFilePath, jsonData.ToString());
            }
        }

        /// <summary>
        /// Adds a new glyph to an existing font from a 1x1 surface
        /// </summary>
        /// <param name="surfaceObject"></param>
        /// <param name="font"></param>
        /// <exception cref="Exception"></exception>
        public static int AddGlyphToFont(ScreenSurface surfaceObject, SadFont font)
        {
            if (surfaceObject.Width != 1 && surfaceObject.Height != 1)
                throw new Exception("This screen surface is not compatible. Must be 1x1");

            var originalFontSize = surfaceObject.FontSize;
            surfaceObject.FontSize = (font.GlyphWidth, font.GlyphHeight);
            surfaceObject.ForceRendererRefresh = true;
            surfaceObject.Render(TimeSpan.Zero);

            // Get the pixel data from the custom glyph after your surface was rendered. This is where it was rendered to.
            Color[] pixels = ((ScreenSurfaceRenderer)surfaceObject.Renderer).Output.GetPixels();

            var availableIndex = _npcFontCache[font].NextAvailableIndex;
            _npcFontCache[font].NextAvailableIndex = availableIndex + 1;

            // Add a new row when needed
            if (availableIndex == font.TotalGlyphs)
                font.Edit_AddRows(1);

            // Add the custom glyph to the available index in the font
            Edit_SetGlyph_Pixel(font, availableIndex, pixels);

            // Reset fontsize to original
            surfaceObject.FontSize = originalFontSize;

            return availableIndex;
        }

        public static void Edit_SetGlyph_Pixel(this IFont font, int glyphIndex, Color[] pixels)
        {
            if (pixels.Length != font.GlyphWidth * font.GlyphHeight)
                throw new ArgumentOutOfRangeException(nameof(pixels), $"Amount of pixels must match font glyph width * height: {font.GlyphWidth * font.GlyphHeight}.");

            var cachedFontTexturePixels = font.Image.GetPixels();

            Rectangle rect = font.GetGlyphSourceRectangle(glyphIndex);

            int indexCounter = 0;
            for (int y = rect.Position.Y; y < rect.Position.Y + rect.Height; y++)
            {
                for (int x = rect.Position.X; x < rect.Position.X + rect.Width; x++)
                {
                    cachedFontTexturePixels[y * font.Image.Width + x] = pixels[indexCounter];
                    indexCounter++;
                }
            }

            font.Image.SetPixels(cachedFontTexturePixels);
        }

        public static void SaveFont(SadFont font)
        {
            if (!_npcFontCache.ContainsKey(font)) return;
            ((SadConsole.Host.GameTexture)font.Image).Texture.Save(_npcFontCache[font].ImageFilePath);
        }
    }
}
