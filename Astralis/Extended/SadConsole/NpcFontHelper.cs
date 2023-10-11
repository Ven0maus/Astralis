using SadConsole;
using SadConsole.FontEditing;
using System;
using SadRogue.Primitives;
using SadConsole.Renderers;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Astralis.Extended.SadConsoleExt
{
    /// <summary>
    /// Class used to combine npc character glyphs into one NPC font
    /// </summary>
    internal static class NpcFontHelper
    {
        private static readonly Dictionary<SadFont, SadFontInfo> _npcFontCache = new();

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

        public static SadFont CreateNewNpcFont(string fontConfigFile)
        {
            // Make a copy of the template
            var fontImageFile = fontConfigFile.Replace(".font", ".png", StringComparison.OrdinalIgnoreCase);
            File.Copy(Constants.Fonts.NpcFonts.TemplateNpcFont, fontConfigFile, true);
            File.Copy(Constants.Fonts.NpcFonts.TemplateNpcFont.Replace(".font", ".png"), fontImageFile, true);

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
            var imageFilePath = Path.Combine(Constants.Fonts.SavedataFontsPath, jsonData["FilePath"].Value<string>());

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
        public static void AddGlyphToFont(ScreenSurface surfaceObject, SadFont font)
        {
            if (surfaceObject.Width != 1 && surfaceObject.Height != 1)
                throw new Exception("This screen surface is not compatible. Must be 1x1");

            var originalFontSize = surfaceObject.FontSize;
            surfaceObject.FontSize = (font.GlyphWidth, font.GlyphHeight);
            surfaceObject.ForceRendererRefresh = true;
            surfaceObject.Render(TimeSpan.Zero);

            // Get the pixel data from the custom glyph after your surface was rendered. This is where it was rendered to.
            Color[] pixels = ((ScreenSurfaceRenderer)surfaceObject.Renderer).Output.GetPixels();
            Color[] cachedPixelsUnused = null;

            var availableIndex = _npcFontCache[font].NextAvailableIndex;
            _npcFontCache[font].NextAvailableIndex = availableIndex+1;

            // Add a new row when needed
            if (availableIndex == font.TotalGlyphs)
                font.Edit_AddRows(1);

            // Add the custom glyph to the available index in the font
            font.Edit_SetGlyph_Pixel(availableIndex, pixels, true, ref cachedPixelsUnused);

            // Reset fontsize to original
            surfaceObject.FontSize = originalFontSize;
        }

        public static void SaveFont(SadFont font)
        {
            if (!_npcFontCache.ContainsKey(font)) return;

            var texture = font.Image;
            Texture2D newTexture = new Texture2D(Resolution.Device.GraphicsDevice, texture.Width, texture.Height);
            newTexture.SetData(texture.GetPixels());

            using (var ms = new MemoryStream())
            {
                newTexture.SaveAsPng(ms, texture.Width, texture.Height);
                ms.Seek(0, SeekOrigin.Begin);
                File.WriteAllBytes(_npcFontCache[font].ImageFilePath, ms.ToArray());
            }
        }
    }
}
