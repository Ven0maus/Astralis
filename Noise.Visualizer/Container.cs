using Astralis;
using Astralis.Extended;
using SadConsole;

namespace Noise.Visualizer
{
    internal class Container : ScreenObject
    {
        private readonly NoiseScreen _noiseScreen;
        private readonly PanelScreen _panelScreen;

        public Container()
        {
            var noiseScreenWidth = Mathf.PercentOf(Constants.ScreenWidth, 80);
            _noiseScreen = new NoiseScreen(noiseScreenWidth, Constants.ScreenHeight) { IsFocused = false };
            //_noiseScreen.Surface.Fill(background: Color.Blue);
            _panelScreen = new PanelScreen(Constants.ScreenWidth - noiseScreenWidth, Constants.ScreenHeight, _noiseScreen)
            {
                Position = new(noiseScreenWidth, 0),
                IsFocused = true
            };
            //_panelScreen.Surface.Fill(background: Color.Green);
            Children.Add(_noiseScreen);
            Children.Add(_panelScreen);

            UseKeyboard = false;
            UseMouse = false;
        }
    }
}
