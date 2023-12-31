﻿using Astralis.Scenes.Screens;
using System;

namespace Astralis.Scenes
{
    internal class MainMenuScene : Scene
    {
        private GameplayScene _overworldScene;
        private MainMenuScreen _mainMenuScreen;

        public MainMenuScene()
        {
            _overworldScene = new GameplayScene(true); Children.Add(_overworldScene);
            _mainMenuScreen = new MainMenuScreen(_overworldScene); Children.Add(_mainMenuScreen);

            // Load the game in the background of the main menu
            _overworldScene.OnFadeFinished += InitializeMainMenu;
            _overworldScene.FadeIn(1);
        }

        ~MainMenuScene()
        {
            Dispose();
        }

        private void InitializeMainMenu(object sender, WorldScreen args)
        {
            args.MainMenuCamera = true;

            _overworldScene.OnFadeFinished -= InitializeMainMenu;
            _mainMenuScreen.Initialize();
        }

        public override void Dispose()
        {
            IsFocused = false;

            if (_overworldScene != null)
            {
                _overworldScene.IsFocused = false;
                _overworldScene.Dispose();
                _overworldScene = null;
            }

            if (_mainMenuScreen != null)
            {
                _mainMenuScreen.IsFocused = false;
                _mainMenuScreen.Dispose();
                _mainMenuScreen = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
