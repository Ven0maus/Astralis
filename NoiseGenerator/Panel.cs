using Astralis;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System.Globalization;

namespace NoiseGenerator
{
    internal class Panel : ControlsConsole
    {
        private enum PropertyValue
        {
            Seed,
            Octaves,
            Scale,
            Persistence,
            Lacunarity
        }

        private readonly Random _random;
        private readonly Dictionary<PropertyValue, TextBox> _values;

        public event EventHandler<GenerateArgs>? OnGenerate;

        public Panel() : base(19, 23)
        {
            Font = Game.Instance.Fonts[Constants.Fonts.LCD];
            Surface.DrawBox(new Rectangle(0, 0, Width, Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, new ColoredGlyph(Color.Blue, Color.Black)));

            _random = new Random();
            _values = new Dictionary<PropertyValue, TextBox>();

            var values = (PropertyValue[])Enum.GetValues(typeof(PropertyValue));
            var startY = 1;
            int currentY = startY;
            for (int i = 0; i < values.Length; i++)
            {
                var label = new Label(values[i].ToString().ToUpper())
                {
                    Position = new Point(1, currentY)
                };
                Controls.Add(label);

                currentY += 1;
                var textBox = new TextBox(13)
                {
                    Position = new Point(1, currentY)
                };
                Controls.Add(textBox);
                _values.Add(values[i], textBox);

                var rButton = new Button(3)
                {
                    Text = "R",
                    Position = new Point(textBox.Width + 2, currentY),
                    ShowEnds = true,
                    AutoSize = false
                };
                var propType = values[i];
                rButton.Click += (sender, args) => { Randomize(propType, true); };
                Controls.Add(rButton);

                currentY += 2;
            }

            var generateButton = new Button("GENERATE ")
            {
                Position = new Point(1, currentY)
            };
            generateButton.Click += (sender, args) => InvokeGenerate();
            Controls.Add(generateButton);

            currentY += 2;

            var randomizeButton = new Button("RANDOMIZE")
            {
                Position = new Point(1, currentY)
            };
            randomizeButton.Click += (sender, args) =>
            {
                foreach (var item in _values)
                    Randomize(item.Key, false);
                InvokeGenerate();
            };
            Controls.Add(randomizeButton);


            // Start values randomized
            randomizeButton.InvokeClick();
        }

        public void InvokeGenerate()
        {
            var args = GetEventArgs();
            if (args == null) return;
            OnGenerate?.Invoke(this, args);
        }

        private GenerateArgs? GetEventArgs()
        {
            var seed = (int)(GetValue(PropertyValue.Seed) ?? 1);
            var octaves = (int)(GetValue(PropertyValue.Octaves) ?? 1);
            var scale = GetValue(PropertyValue.Scale);
            var persistence = GetValue(PropertyValue.Persistence);
            var lacunarity = GetValue(PropertyValue.Lacunarity);

            if (scale == null || persistence == null || lacunarity == null)
            {
                Window.Message("Invalid values selected.", "Close");
                return null;
            }

            return new GenerateArgs(seed, octaves, scale.Value, persistence.Value, lacunarity.Value);
        }

        private float? GetValue(PropertyValue prop)
        {
            if (!float.TryParse(_values[prop].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return null;
            return value;
        }

        private void Randomize(PropertyValue prop, bool generate)
        {
            switch (prop)
            {
                case PropertyValue.Seed:
                    _values[prop].Text = _random.Next(-100000, 100000).ToString();
                    break;
                case PropertyValue.Octaves:
                    _values[prop].Text = _random.Next(1, 9).ToString();
                    break;
                case PropertyValue.Scale:
                    _values[prop].Text = GetRandomFloat(0.1f, 100f).ToString(CultureInfo.InvariantCulture);
                    break;
                case PropertyValue.Persistence:
                    _values[prop].Text = GetRandomFloat(0.2f, 0.8f).ToString(CultureInfo.InvariantCulture);
                    break;
                case PropertyValue.Lacunarity:
                    _values[prop].Text = GetRandomFloat(1.5f, 4.0f).ToString(CultureInfo.InvariantCulture);
                    break;
            }

            // Update caret position and set dirty
            _values[prop].CaretPosition = _values[prop].Text.Length;
            _values[prop].IsDirty = true;

            if (generate)
                InvokeGenerate();
        }

        private float GetRandomFloat(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        public sealed class GenerateArgs : EventArgs
        {
            public int Seed { get; }
            public int Octaves { get; }
            public float Scale { get; }
            public float Persistence { get; }
            public float Lacunarity { get; }

            public GenerateArgs(int seed, int octaves, float scale, float persistence, float lacunarity)
            {
                Seed = seed;
                Octaves = octaves;
                Scale = scale;
                Persistence = persistence;
                Lacunarity = lacunarity;
            }
        }
    }
}
