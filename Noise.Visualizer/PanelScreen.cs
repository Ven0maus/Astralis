using Newtonsoft.Json.Linq;
using SadConsole.UI;
using SadConsole.UI.Controls;
using System.Globalization;

namespace Noise.Visualizer
{
    internal class PanelScreen : ControlsConsole
    {
        private readonly NoiseScreen _noiseScreen;
        private readonly NumberBox _seedBox, _octavesBox;
        private readonly TextBox _scaleBox, _persistanceBox, _lacunarityBox;
        private readonly CheckBox _islandGradientCheckbox;

        public PanelScreen(int width, int height, NoiseScreen noiseScreen) : base(width, height) 
        {
            _noiseScreen = noiseScreen;

            Border.CreateForSurface(this, "Settings");

            int y = 1;
            Controls.Add(new Label("Seed") { Position = new (1, y) });
            y++;
            _seedBox = new NumberBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Seed.ToString(CultureInfo.InvariantCulture), MaxLength = int.MaxValue, NumberMinimum = 0, NumberMaximum = int.MaxValue };
            _seedBox.CaretPosition = _seedBox.Text.Length;
            y++;
            var slider = new ScrollBar(SadConsole.Orientation.Horizontal, Width - 4) { Position = new(1, y), Maximum = 1337, Value = _noiseScreen.Seed };
            slider.ValueChanged += (sender, args) => { _seedBox.Text = slider.Value.ToString(); GenerateClick(); _seedBox.IsDirty = true; };
            _seedBox.TextChanged += (sender, args) => { slider.Value = int.TryParse(_seedBox.Text, out int oValue) ? oValue : 0; };
            Controls.Add(slider);
            Controls.Add(_seedBox);
            y += 2;


            Controls.Add(new Label("Octaves") { Position = new(1, y) });
            y++;
            _octavesBox = new NumberBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Octaves.ToString(CultureInfo.InvariantCulture), MaxLength = int.MaxValue, NumberMinimum = 1, NumberMaximum = int.MaxValue };
            _octavesBox.CaretPosition = _octavesBox.Text.Length;
            y++;
            var slider2 = new ScrollBar(SadConsole.Orientation.Horizontal, Width - 4) { Position = new(1, y), Maximum = 12, Value = _noiseScreen.Octaves };
            slider2.ValueChanged += (sender, args) => 
            { 
                if (slider2.Value == 0)
                    slider2.Value = 1; 
                _octavesBox.Text = slider2.Value.ToString(); 
                GenerateClick(); 
                _octavesBox.IsDirty = true; 
            };
            _octavesBox.TextChanged += (sender, args) => { slider2.Value = int.TryParse(_octavesBox.Text, out int oValue) ? oValue : 1; };
            Controls.Add(slider2);
            Controls.Add(_octavesBox);
            y += 2;

            Controls.Add(new Label("Scale") { Position = new(1, y) });
            y++;
            _scaleBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Scale.ToString(CultureInfo.InvariantCulture) };
            _scaleBox.CaretPosition = _scaleBox.Text.Length;
            Controls.Add(_scaleBox);
            y += 2;

            Controls.Add(new Label("Persistance") { Position = new(1, y) });
            y++;
            _persistanceBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Persistance.ToString(CultureInfo.InvariantCulture) };
            _persistanceBox.CaretPosition = _persistanceBox.Text.Length;
            Controls.Add(_persistanceBox);
            y += 2;

            Controls.Add(new Label("Lacunarity") { Position = new(1, y) });
            y++;
            _lacunarityBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Lacunarity.ToString(CultureInfo.InvariantCulture) };
            Controls.Add(_lacunarityBox);
            _lacunarityBox.CaretPosition = _lacunarityBox.Text.Length;
            y += 2;

            Controls.Add(new Label("Apply island gradient") { Position = new(1, y) });
            y++;
            _islandGradientCheckbox = new CheckBox(1, 1) { Position = new(1, y), IsSelected = _noiseScreen.ApplyIslandGradient };
            Controls.Add(_islandGradientCheckbox);
            y += 2;

            var button = new Button(Width - 4) { Position = new(1, y), Text = "Generate" };
            button.Click += (sender, args) => 
            {
                GenerateClick();
            };
            Controls.Add(button);

            _noiseScreen.Draw();
        }

        private void GenerateClick()
        {
            if (int.TryParse(_seedBox.Text, out int value))
                _noiseScreen.Seed = value;
            if (int.TryParse(_octavesBox.Text, out value))
                _noiseScreen.Octaves = value;
            if (float.TryParse(_scaleBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float fValue))
                _noiseScreen.Scale = fValue;
            if (float.TryParse(_persistanceBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue))
                _noiseScreen.Persistance = fValue;
            if (float.TryParse(_lacunarityBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out fValue))
                _noiseScreen.Lacunarity = fValue;
            _noiseScreen.ApplyIslandGradient = _islandGradientCheckbox.IsSelected;
            _noiseScreen.Draw();
        }
    }
}
