using Astralis;
using Astralis.Extended;
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
            Lacunarity,
            NoiseType,
            OffsetX,
            OffsetY,
        }

        private readonly Random _random;
        private readonly Dictionary<PropertyValue, TextBox> _values;
        private readonly ComboBox? _noiseType;
        private readonly CheckBox _ridges;

        public event EventHandler<GenerateArgs>? OnGenerate;
        public event EventHandler? OnResetOffset;

        public Panel() : base(19, 34)
        {
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
                SetLabelTheme(label);
                Controls.Add(label);

                currentY += 1;

                if (values[i] == PropertyValue.NoiseType)
                {
                    var noiseTypes = (FastNoiseLite.NoiseType[])Enum.GetValues(typeof(FastNoiseLite.NoiseType));
                    var maxWidth = noiseTypes.Select(a => a.ToString()).Max(a => a.Length) + 4;
                    _noiseType = new ComboBox(maxWidth, maxWidth, noiseTypes.Length, noiseTypes.Cast<object>().ToArray())
                    {
                        Position = new Point(1, currentY),
                    };
                    _noiseType.SelectedItemChanged += OnNoiseTypeChanged;
                    Controls.Add(_noiseType);
                }
                else
                {
                    var textBox = new TextBox(13)
                    {
                        Position = new Point(1, currentY)
                    };
                    if (values[i].ToString().StartsWith("Offset"))
                    {
                        textBox.IsEnabled = false;
                        textBox.Text = "0";
                    }
                    Controls.Add(textBox);
                    _values.Add(values[i], textBox);

                    // Add R buttons for all except offsets
                    if (!values[i].ToString().StartsWith("Offset"))
                    {
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
                    }
                }

                currentY += 2;
            }

            _ridges = new CheckBox("SHARP RIDGES")
            {
                Position = new Point(1, currentY),
                LeftBracketGlyph = '(',
                RightBracketGlyph = ')',
                CheckedIconGlyph = 'X'
            };
            Controls.Add(_ridges);
            currentY += 2;

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

            currentY += 2;

            var resetOffsetButton = new Button("RESET OFFSET")
            {
                Position = new Point(1, currentY)
            };
            resetOffsetButton.Click += (sender, args) =>
            {
                OnResetOffset?.Invoke(this, EventArgs.Empty);
            };
            Controls.Add(resetOffsetButton);

            // Start values randomized
            randomizeButton.InvokeClick();
        }

        private void OnNoiseTypeChanged(object? sender, ListBox.SelectedItemEventArgs e)
        {
            InvokeGenerate();
        }

        private Colors _labelTheme;
        private void SetLabelTheme(Label label)
        {
            if (_labelTheme == null)
            {
                Color color = Color.DarkGoldenrod;
                _labelTheme = label.FindThemeColors().Clone();
                _labelTheme.ControlForegroundNormal.SetColor(color);
                _labelTheme.ControlForegroundSelected.SetColor(color);
                _labelTheme.ControlForegroundMouseOver.SetColor(color);
                _labelTheme.ControlForegroundFocused.SetColor(color);
                _labelTheme.RebuildAppearances();
            }
            label.SetThemeColors(_labelTheme);
        }

        public void InvokeGenerate()
        {
            var args = GetEventArgs();
            if (args == null) return;
            OnGenerate?.Invoke(this, args);
        }

        public void SetOffset(Point camera)
        {
            _values[PropertyValue.OffsetX].Text = camera.X.ToString();
            _values[PropertyValue.OffsetX].IsDirty = true;
            _values[PropertyValue.OffsetY].Text = camera.Y.ToString();
            _values[PropertyValue.OffsetY].IsDirty = true;
            InvokeGenerate();
        }

        private GenerateArgs? GetEventArgs()
        {
            var seed = (int)(GetValue(PropertyValue.Seed) ?? 1);
            var octaves = (int)(GetValue(PropertyValue.Octaves) ?? 1);
            var scale = GetValue(PropertyValue.Scale);
            var persistence = GetValue(PropertyValue.Persistence);
            var lacunarity = GetValue(PropertyValue.Lacunarity);
            var offsetX = (int)(GetValue(PropertyValue.OffsetX) ?? 0);
            var offsetY = (int)(GetValue(PropertyValue.OffsetY) ?? 0);

            if (scale == null || persistence == null || lacunarity == null)
            {
                Window.Message("Invalid values selected.", "Close");
                return null;
            }

            var noiseType = _noiseType?.SelectedItem != null ? (FastNoiseLite.NoiseType)_noiseType.SelectedItem : default;

            return new GenerateArgs(seed, octaves, scale.Value, persistence.Value, lacunarity.Value, noiseType, (offsetX, offsetY), _ridges.IsSelected);
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
            public FastNoiseLite.NoiseType NoiseType { get; }
            public bool SharpRidges { get; }
            public Point Offset { get; }

            public GenerateArgs(int seed, int octaves, float scale, float persistence, float lacunarity, FastNoiseLite.NoiseType noiseType, Point offset, bool sharpRidges)
            {
                Seed = seed;
                Octaves = octaves;
                Scale = scale;
                Persistence = persistence;
                Lacunarity = lacunarity;
                NoiseType = noiseType;
                Offset = offset;
                SharpRidges = sharpRidges;
            }
        }
    }
}
