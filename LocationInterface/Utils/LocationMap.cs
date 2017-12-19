﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;

namespace LocationInterface.Utils
{
    public enum PointColour
    {
        Red,
        Green,
        Blue,
        Black
    }

    public class LocationMap : WpfGame
    {
        private SpriteBatch _spriteBatch;
        private IGraphicsDeviceService _graphicsDeviceManager;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;
        private Vector2[] _circlePositions = new Vector2[0];
        private Dictionary<PointColour, Texture2D> _pointTextures;
        private Random _random;
        private PointColour _currentColour;
        private Camera _camera;
        protected KeyListener _sKeyBind;

        protected override void Initialize()
        {
            // must be initialized. required by Content loading and rendering (will add itself to the Services)
            _graphicsDeviceManager = new WpfGraphicsDeviceService(this);

            // wpf and keyboard need reference to the host control in order to receive input
            // this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _camera = new Camera(0, 0);

            // must be called after the WpfGraphicsDeviceService instance was created
            base.Initialize();

            Dictionary<PointColour, Color> pointColours = new Dictionary<PointColour, Color>
            {
                [PointColour.Red] = Color.Red,
                [PointColour.Green] = Color.Green,
                [PointColour.Blue] = Color.Blue,
                [PointColour.Black] = Color.Black
            };

            int radius = 5, diameter = radius * 2;

            _pointTextures = new Dictionary<PointColour, Texture2D>();
            foreach (PointColour pointColour in Enum.GetValues(typeof(PointColour)))
            {
                _pointTextures[pointColour] = new Texture2D(GraphicsDevice, diameter, diameter);

                Color[] circleData = new Color[diameter * diameter];
                int i = 0;
                for (int x = -radius; x < radius; x++)
                    for (int y = -radius; y < radius; y++)
                    {
                        if ((x * x) + (y * y) < radius * radius) circleData[i] = pointColours[pointColour];
                        else circleData[i] = new Color(0, 0, 0, 0);
                        i++;
                    }

                _pointTextures[pointColour].SetData(circleData);
            }

            _random = new Random();
            _circlePositions = new Vector2[10];
            for (int i = 0; i < _circlePositions.Length; i++)
                _circlePositions[i] = new Vector2(_random.Next(500), _random.Next(500));

            _sKeyBind = new KeyListener(Keys.S, SaveInfo);
        }

        protected void SaveInfo()
        {
            if (_keyboard.GetState().IsKeyDown(Keys.LeftControl))
                App.ImageIndex.SaveIndex();
        }

        double colourTimer = 0;
        protected override void Update(GameTime time)
        {
            var mouseState = _mouse.GetState();
            var keyboardState = _keyboard.GetState();

            colourTimer += time.ElapsedGameTime.TotalMilliseconds;
            if (colourTimer > 250)
            {
                _currentColour = (PointColour)_random.Next(4);
                colourTimer = 0;
            }

            _sKeyBind.Update(keyboardState);
            bool shiftDown = keyboardState.IsKeyDown(Keys.LeftShift);
            if (keyboardState.IsKeyDown(Keys.A)) _camera.Move(5, 0);
            if (keyboardState.IsKeyDown(Keys.D)) _camera.Move(-5, 0);
            if (keyboardState.IsKeyDown(Keys.W)) _camera.Move(0, 5);
            if (keyboardState.IsKeyDown(Keys.S) && !keyboardState.IsKeyDown(Keys.LeftControl)) _camera.Move(0, -5);
            if (keyboardState.IsKeyDown(Keys.R)) ScalePoints(shiftDown ? -.05 : -.001);
            if (keyboardState.IsKeyDown(Keys.Y)) ScalePoints(shiftDown ? +.05 : +.001);
            if (keyboardState.IsKeyDown(Keys.T)) TranslatePoints(0, shiftDown ? -4 : -1);
            if (keyboardState.IsKeyDown(Keys.F)) TranslatePoints(shiftDown ? -4 : -1, 0);
            if (keyboardState.IsKeyDown(Keys.G)) TranslatePoints(0, shiftDown ? +4 : +1);
            if (keyboardState.IsKeyDown(Keys.H)) TranslatePoints(shiftDown ? +4 : +1, 0);
        }

        public void ScalePoints(double factor)
        {

        }

        public void TranslatePoints(float x, float y)
        {

        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();
            for (int i = 0; i < _circlePositions.Length; i++)
                _spriteBatch.Draw(_pointTextures[_currentColour], _circlePositions[i]);
            _spriteBatch.End();
        }
    }
}
