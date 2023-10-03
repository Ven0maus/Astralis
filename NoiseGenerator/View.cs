using Astralis;
using Astralis.Extended;
using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;

namespace NoiseGenerator
{
    internal class View : ControlsConsole
    {
        private readonly Panel _panel;

        public View(int width, int height) : base(width, height)
        {
            Font = Game.Instance.Fonts[Constants.Fonts.LCD];

            _panel = new Panel();
            _panel.OnGenerate += (sender, args) => UpdateNoise(args);
            _panel.InvokeGenerate();
            Children.Add(_panel);
        }

        private void UpdateNoise(Panel.GenerateArgs args)
        {
            NoiseHelper noise = new(args.Seed);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float xValue = x;
                    float yValue = y;
                    float value = noise.GetNoise(xValue, yValue, args.Octaves, args.Scale, args.Persistence, args.Lacunarity);

                    Surface.SetBackground(x, y, Color.Lerp(Color.White, Color.Black, value));
                }
            }
            Surface.IsDirty = true;
        }
    }
}
