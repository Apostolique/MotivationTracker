using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            Window.AllowUserResizing = true;
            base.Initialize();
        }

        protected override void LoadContent() {
            _s = new SpriteBatch(GraphicsDevice);

            _r = new RenderTarget2D(GraphicsDevice, _width, _height);

            _maxDays = maxDays(_year);
            _daySize = Math.Min(RealWidth / _maxDays, RealHeight / 12);

            InputHelper.Setup(this);
        }

        protected override void Update(GameTime gameTime) {
            InputHelper.UpdateSetup();

            if (_quit.Pressed())
                Exit();

            _hovered = MouseToDay(InputHelper.NewMouse.Position);

            if (_click.Pressed()) {
                if (_hovered.HasValue) {
                    if (_highlightedDays.Contains(_hovered.Value)) {
                        _highlightedDays.Remove(_hovered.Value);
                    } else {
                        _highlightedDays.Add(_hovered.Value);
                    }
                }
            }

            InputHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.SetRenderTarget(_r);
            GraphicsDevice.Clear(_background);

            _s.Begin();

            for (int i = 0; i < 12; i++) {
                for (int j = 0; j < DateTime.DaysInMonth(_year, i + 1); j++) {
                    var rect = new Rectangle(_marginHorizontal + j * _daySize, _marginVertical + i * _daySize, (int)(_daySize * _cellMargin), (int)(_daySize * _cellMargin));
                    if (_highlightedDays.Contains(new Point(j, i)) || _hovered.HasValue && _hovered.Value.X == j && _hovered.Value.Y == i) {
                        _s.FillRectangle(rect, _fillActive);
                        _s.DrawRectangle(rect, _borderActive, 3);
                    } else {
                        _s.FillRectangle(rect, _fill);
                        _s.DrawRectangle(rect, _border, 3);
                    }
                }
            }

            _s.End();

            GraphicsDevice.SetRenderTarget(null);

            _s.Begin(samplerState: SamplerState.PointClamp);
            var bounds = new Rectangle(0, 0, (int)(_width * Ratio), (int)(_height * Ratio));
            _s.Draw(_r, bounds, Color.White);
            _s.DrawRectangle(bounds, Color.White * 0.2f, 4);
            _s.End();

            base.Draw(gameTime);
        }

        private int maxDays(int year) {
            int maxDays = 0;
            for (int i = 1; i <= 12; i++) {
                maxDays = Math.Max(maxDays, DateTime.DaysInMonth(year, i));
            }

            return maxDays;
        }
        private Point? MouseToDay(Point mouse) {
            var t = ((mouse.ToVector2() / new Vector2(Ratio)) - new Vector2(_marginHorizontal, _marginVertical)) / new Vector2(_daySize);

            if (t.X >= 0 && t.Y >= 0 && t.Y < 12 && t.X - MathF.Truncate(t.X) < _cellMargin && t.Y - MathF.Truncate(t.Y) < _cellMargin && t.X < DateTime.DaysInMonth(_year, (int)t.Y + 1)) {
                return t.ToPoint();
            }

            return null;
        }

        float Ratio => MathF.Min((float)Window.ClientBounds.Width / _width, (float)Window.ClientBounds.Height / _height);
        int RealWidth => _width - _marginHorizontal * 2;
        int RealHeight => _height - _marginVertical * 2;

        GraphicsDeviceManager _graphics;
        SpriteBatch _s;

        RenderTarget2D _r;
        int _width = 1920;
        int _height = 1080;

        int _year = 2020;
        int _maxDays;

        int _daySize;
        int _marginHorizontal = 50;
        int _marginVertical = 100;
        float _cellMargin = 1 - 0.2f;

        Color _fillActive = new Color(214, 31, 105);
        Color _borderActive = new Color(241, 126, 184);

        Color _fill = new Color(55, 65, 81);
        Color _border = new Color(75, 85, 99);

        Color _background = new Color(22, 30, 46);

        HashSet<Point> _highlightedDays = new HashSet<Point>();
        Point? _hovered = new Point();

        ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );

        ICondition _click = new MouseCondition(MouseButton.LeftButton);
    }
}
