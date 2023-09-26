using Astralis.Extended.Effects;
using Astralis.GameCode;
using SadConsole;
using SadRogue.Primitives;
using System;
using Venomaus.FlowVitae.Grids;

namespace Astralis.Scenes.GameplayScenes
{
    internal class OverworldScene : Scene
    {
        private readonly World _map;

        public OverworldScene()
        {
            RemoveControlLayer();

            // Set Aesomatica font for the overworld
            Surface.Font = Game.Instance.Fonts[Constants.Fonts.Aesomatica];

            // Generate world
            _map = GenerateOverworld(seed: 50);
            _map.OnCellUpdate += OnCellUpdate;

            if (!Constants.DebugMode)
            {
                var fadeEffect = new FadeEffect(Surface, TimeSpan.FromSeconds(2), FadeEffect.FadeMode.FadeIn, false);
                fadeEffect.OnFinished += GameStart;
                Effects.Add(fadeEffect);
            }
            else
            {
                GameStart();
            }
        }

        private void GameStart()
        {

        }

        private World GenerateOverworld(int seed)
        {
            var map = new World(Width, Height, seed, new WorldGenerator(seed));

            // Initial draw
            var viewPort = map.GetViewPortWorldCoordinates();
            var cells = map.GetCells(viewPort);
            foreach (var cell in cells)
            {
                var (x, y) = map.WorldToScreenCoordinate(cell.X, cell.Y);
                Surface.Surface[x, y].CopyAppearanceFrom(cell);
                Surface.Surface[x, y].IsVisible = cell.IsVisible;
            }

            return map;
        }

        public void OnCellUpdate(object sender, CellUpdateArgs<byte, Tile> args)
        {
            var surface = Surface.Surface;
            surface[args.ScreenX, args.ScreenY].CopyAppearanceFrom(args.Cell);
            surface[args.ScreenX, args.ScreenY].IsVisible = args.Cell.IsVisible;
        }
    }
}
