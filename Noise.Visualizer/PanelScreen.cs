using Newtonsoft.Json.Linq;
using SadConsole.UI;
using SadConsole.UI.Controls;
using System.Globalization;

namespace Noise.Visualizer
{
    internal class PanelScreen : ControlsConsole
    {
        private readonly NoiseScreen _noiseScreen;
        public PanelScreen(int width, int height, NoiseScreen noiseScreen) : base(width, height) 
        {
            _noiseScreen = noiseScreen;

            Border.CreateForSurface(this, "Settings");

            int y = 1;
            Controls.Add(new Label("Seed") { Position = new (1, y) });
            y++;
            var seedBox = new NumberBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Seed.ToString(CultureInfo.InvariantCulture), MaxLength = int.MaxValue, NumberMinimum = 0, NumberMaximum = int.MaxValue };
            seedBox.CaretPosition = seedBox.Text.Length;
            Controls.Add(seedBox);
            y += 2;

            Controls.Add(new Label("Octaves") { Position = new(1, y) });
            y++;
            var octavesBox = new NumberBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Octaves.ToString(CultureInfo.InvariantCulture), MaxLength = int.MaxValue, NumberMinimum = 1, NumberMaximum = int.MaxValue };
            octavesBox.CaretPosition = octavesBox.Text.Length;
            Controls.Add(octavesBox);
            y += 2;

            Controls.Add(new Label("Scale") { Position = new(1, y) });
            y++;
            var scaleBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Scale.ToString(CultureInfo.InvariantCulture) };
            scaleBox.CaretPosition = scaleBox.Text.Length;
            Controls.Add(scaleBox);
            y += 2;

            Controls.Add(new Label("Persistance") { Position = new(1, y) });
            y++;
            var persistanceBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Persistance.ToString(CultureInfo.InvariantCulture) };
            persistanceBox.CaretPosition = persistanceBox.Text.Length;
            Controls.Add(persistanceBox);
            y += 2;

            Controls.Add(new Label("Lacunarity") { Position = new(1, y) });
            y++;
            var lacunarityBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Lacunarity.ToString(CultureInfo.InvariantCulture) };
            Controls.Add(lacunarityBox);
            lacunarityBox.CaretPosition = lacunarityBox.Text.Length;
            y += 2;

            Controls.Add(new Label("Apply island gradient") { Position = new(1, y) });
            y++;
            var islandGradientCheckbox = new CheckBox(1, 1) { Position = new(1, y), IsSelected = _noiseScreen.ApplyIslandGradient };
            Controls.Add(islandGradientCheckbox);
            y += 2;

            var button = new Button(Width - 4) { Position = new(1, y), Text = "Generate" };
            button.Click += (sender, args) => 
            {
                if (int.TryParse(seedBox.Text, out int value))
                    _noiseScreen.Seed = value;
                if (int.TryParse(octavesBox.Text, out value))
                    _noiseScreen.Octaves = value;
                if (float.TryParse(scaleBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float fValue))
                    _noiseScreen.Scale = fValue;
                if (float.TryParse(persistanceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue))
                    _noiseScreen.Persistance = fValue;
                if (float.TryParse(lacunarityBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue))
                    _noiseScreen.Lacunarity = fValue;
                _noiseScreen.ApplyIslandGradient = islandGradientCheckbox.IsSelected;
                _noiseScreen.Draw(); 
            };
            Controls.Add(button);

            _noiseScreen.Draw();
        }
    }
}
