using SadConsole.UI;
using SadConsole.UI.Controls;
using System;
using System.Globalization;

namespace Noise.Visualizer
{
    internal class PanelScreen : ControlsConsole
    {
        private readonly NoiseScreen _noiseScreen;
        private readonly NumberBox _seedBox, _octavesBox;
        private readonly TextBox _scaleBox, _persistanceBox, _lacunarityBox;
        private readonly CheckBox _islandGradientOption;
        private readonly Random _random;

        public PanelScreen(int width, int height, NoiseScreen noiseScreen) : base(width, height) 
        {
            _noiseScreen = noiseScreen;
            _random = new Random();

            Border.CreateForSurface(this, "Settings");

            int y = 1;
            Controls.Add(new Label("Seed") { Position = new (1, y) });
            y++;
            _seedBox = new NumberBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Seed.ToString(CultureInfo.InvariantCulture), MaxLength = 5, NumberMinimum = -1000, NumberMaximum = 1000 };
            _seedBox.CaretPosition = _seedBox.Text.Length;
            y++;
            var seedBoxRButton = new Button(Width - 4) { Position = new(1, y), Text = "Randomize" };
            seedBoxRButton.Click += (sender, args) =>
            {
                _seedBox.Text = _random.Next(-1000, 1001).ToString();
                _seedBox.CaretPosition = _seedBox.Text.Length;
            };
            _seedBox.TextChanged += (sender, args) => GenerateClick();
            Controls.Add(seedBoxRButton);
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
                _octavesBox.IsDirty = true;
            };
            _octavesBox.TextChanged += (sender, args) => { slider2.Value = int.TryParse(_octavesBox.Text, out int oValue) ? oValue : 1; };
            y++;
            var octavesRButton = new Button(Width - 4) { Position = new(1, y), Text = "Randomize" };
            octavesRButton.Click += (sender, args) =>
            {
                _octavesBox.Text = _random.Next(1, 13).ToString();
                _octavesBox.CaretPosition = _octavesBox.Text.Length;
            };
            _octavesBox.TextChanged += (sender, args) => GenerateClick();
            Controls.Add(octavesRButton);
            Controls.Add(slider2);
            Controls.Add(_octavesBox);
            y += 2;

            Controls.Add(new Label("Scale") { Position = new(1, y) });
            y++;
            _scaleBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Scale.ToString(CultureInfo.InvariantCulture) };
            _scaleBox.CaretPosition = _scaleBox.Text.Length;
            y++;
            var scaleRButton = new Button(Width - 4) { Position = new(1, y), Text = "Randomize" };
            scaleRButton.Click += (sender, args) =>
            {
                var scale = Math.Round(_random.NextDouble() * _random.Next(-1, 10), 2);
                while (scale == 0)
                    scale = Math.Round(_random.NextDouble() * _random.Next(-1, 10), 2);
                _scaleBox.Text = scale.ToString(CultureInfo.InvariantCulture);
                _scaleBox.CaretPosition = _scaleBox.Text.Length;
                GenerateClick();
            };
            _scaleBox.TextChanged += (sender, args) => GenerateClick();
            Controls.Add(scaleRButton);
            Controls.Add(_scaleBox);
            y += 2;

            Controls.Add(new Label("Persistance") { Position = new(1, y) });
            y++;
            _persistanceBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Persistance.ToString(CultureInfo.InvariantCulture) };
            _persistanceBox.CaretPosition = _persistanceBox.Text.Length;
            y++;
            var persistanceRButton = new Button(Width - 4) { Position = new(1, y), Text = "Randomize" };
            persistanceRButton.Click += (sender, args) =>
            {
                _persistanceBox.Text = Math.Round(_random.NextDouble() * _random.Next(-10, 10), 2).ToString(CultureInfo.InvariantCulture);
                _persistanceBox.CaretPosition = _persistanceBox.Text.Length;
                GenerateClick();
            };
            _persistanceBox.TextChanged += (sender, args) => GenerateClick();
            Controls.Add(persistanceRButton);
            Controls.Add(_persistanceBox);
            y += 2;

            Controls.Add(new Label("Lacunarity") { Position = new(1, y) });
            y++;
            _lacunarityBox = new TextBox(Width - 4) { Position = new(1, y), Text = _noiseScreen.Lacunarity.ToString(CultureInfo.InvariantCulture) };
            y++;
            var lacunarityRButton = new Button(Width - 4) { Position = new(1, y), Text = "Randomize" };
            lacunarityRButton.Click += (sender, args) =>
            {
                _lacunarityBox.Text = Math.Round(_random.NextDouble() * _random.Next(-5, 10), 2).ToString(CultureInfo.InvariantCulture);
                _lacunarityBox.CaretPosition = _lacunarityBox.Text.Length;
                GenerateClick();
            };
            _lacunarityBox.TextChanged += (sender, args) => GenerateClick();
            Controls.Add(lacunarityRButton);
            Controls.Add(_lacunarityBox);
            _lacunarityBox.CaretPosition = _lacunarityBox.Text.Length;
            y += 2;

            Controls.Add(new Label("Apply island gradient") { Position = new(1, y) });
            y++;
            _islandGradientOption = new CheckBox(1, 1) { Position = new(1, y), IsSelected = _noiseScreen.ApplyIslandGradient };
            _islandGradientOption.IsSelectedChanged += (sender, args) => { GenerateClick(); };
            Controls.Add(_islandGradientOption);
            y += 2;

            var randomizeButton = new Button(Width - 4) { Position = new(1, y), Text = "Randomize All" };
            randomizeButton.Click += (sender, args) =>
            {
                RandomizeAllValues();
            };
            Controls.Add(randomizeButton);

            y += 2;
            var copyButton = new Button(Width - 4) { Position = new(1, y), Text = "Copy to clipboard" };
            copyButton.Click += (sender, args) =>
            {
                CopyToClipboard();
            };
            Controls.Add(copyButton);

            _noiseScreen.Draw();
        }

        private void CopyToClipboard()
        {
            ClipboardHelper.SetText($"GenerateNoiseMap({_octavesBox.Text}, {_scaleBox.Text}, {_persistanceBox.Text}, {_lacunarityBox.Text})");
        }

        private void RandomizeAllValues()
        {
            _seedBox.Text = _random.Next(-1000, 1001).ToString();
            _seedBox.CaretPosition = _seedBox.Text.Length;

            _octavesBox.Text = _random.Next(1, 13).ToString();
            _octavesBox.CaretPosition = _octavesBox.Text.Length;

            var scale = Math.Round(_random.NextDouble() * _random.Next(-1, 10), 2);
            while (scale == 0)
                scale = Math.Round(_random.NextDouble() * _random.Next(-1, 10), 2);
            _scaleBox.Text = scale.ToString(CultureInfo.InvariantCulture);
            _scaleBox.CaretPosition = _scaleBox.Text.Length;

            _persistanceBox.Text = Math.Round(_random.NextDouble() * _random.Next(-10, 10), 2).ToString(CultureInfo.InvariantCulture);
            _persistanceBox.CaretPosition = _persistanceBox.Text.Length;

            _lacunarityBox.Text = Math.Round(_random.NextDouble() * _random.Next(-5, 10), 2).ToString(CultureInfo.InvariantCulture);
            _lacunarityBox.CaretPosition = _lacunarityBox.Text.Length;

            GenerateClick();
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
            _noiseScreen.ApplyIslandGradient = _islandGradientOption.IsSelected;
            _noiseScreen.Draw();
        }
    }
}
