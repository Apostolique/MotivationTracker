using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

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

            Assets.Setup(Content, GraphicsDevice);

            Calendar c = LoadJson<Calendar>(GetPath(_calendarName));

            foreach (Calendar.Point cp in c.ActiveDays) {
                Point p = new Point(cp.X, cp.Y);
                if (!_highlightedDays.Contains(p)) {
                    _highlightedDays.Add(p);
                }
            }

            InputHelper.Setup(this);
        }

        protected override void Update(GameTime gameTime) {
            InputHelper.UpdateSetup();

            if (_quit.Pressed())
                Exit();

            _hovered = mouseToDay(InputHelper.NewMouse.Position);

            if (_click.Pressed()) {
                if (_hovered.HasValue) {
                    if (_highlightedDays.Contains(_hovered.Value)) {
                        _highlightedDays.Remove(_hovered.Value);
                    } else {
                        _highlightedDays.Add(_hovered.Value);
                    }
                }
            }

            if (_save.Pressed()) {
                DateTime now = DateTime.Now;
                int year = now.Year;
                int month = now.Month;
                int day = now.Day;
                string name = $"{year}-{month}-{day}";
                string imageName = $"{name}.png";
                using Stream file = File.OpenWrite(GetPath(imageName));
                _r.SaveAsPng(file, _width, _height);

                Calendar c = new Calendar();

                foreach (Point p in _highlightedDays) {
                    Calendar.Point cp = new Calendar.Point {
                        X = p.X,
                        Y = p.Y
                    };
                    c.ActiveDays.Add(cp);
                }

                CreateJson<Calendar>(GetPath(_calendarName), c);
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
                    if (_highlightedDays.Contains(new Point(j, i)) && _hovered.HasValue && _hovered.Value.X == j && _hovered.Value.Y == i) {
                        _s.FillRectangle(rect, _fillBoth);
                        _s.DrawRectangle(rect, _borderBoth, 3);
                    } else if (_hovered.HasValue && _hovered.Value.X == j && _hovered.Value.Y == i) {
                        _s.FillRectangle(rect, _fillHover);
                        _s.DrawRectangle(rect, _borderHover, 3);
                    } else if (_highlightedDays.Contains(new Point(j, i))) {
                        _s.FillRectangle(rect, _fillActive);
                        _s.DrawRectangle(rect, _borderActive, 3);
                    } else {
                        _s.FillRectangle(rect, _fill);
                        _s.DrawRectangle(rect, _border, 3);
                    }
                }
            }
            _s.DrawString(Assets.Font, DateTime.Now.ToLongDateString(), new Vector2(50, 50), Color.White);

            _s.End();

            GraphicsDevice.SetRenderTarget(null);

            _s.Begin(samplerState: SamplerState.PointClamp);
            var bounds = new Rectangle(0, 0, (int)(_width * Ratio), (int)(_height * Ratio));
            _s.Draw(_r, bounds, Color.White);
            _s.DrawRectangle(bounds, Color.White * 0.2f, 4);
            if (_hovered.HasValue) {
                _s.DrawString(Assets.Font, pointToDateName(_hovered.Value), new Vector2(50, Window.ClientBounds.Height - 50), Color.White);
            }
            _s.End();

            base.Draw(gameTime);
        }

        public static string RootPath => AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
        public static string GetPath(string name) => Path.Combine(RootPath, name);
        public static T LoadJson<T>(string name) where T : new() {
            T json;

            string jsonPath = GetPath(name);

            if (File.Exists(jsonPath)) {
                json = JsonSerializer.Deserialize<T>(File.ReadAllText(jsonPath));
            } else {
                json = new T();
            }

            return json;
        }
        public static void CreateJson<T>(string name, T content) where T : new() {
            string jsonPath = GetPath(name);

            var options = new JsonSerializerOptions {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(content, options);
            File.WriteAllText(jsonPath, jsonString);
        }

        private int maxDays(int year) {
            int maxDays = 0;
            for (int i = 1; i <= 12; i++) {
                maxDays = Math.Max(maxDays, DateTime.DaysInMonth(year, i));
            }

            return maxDays;
        }
        private Point? mouseToDay(Point mouse) {
            var v = ((mouse.ToVector2() / new Vector2(Ratio)) - new Vector2(_marginHorizontal, _marginVertical)) / new Vector2(_daySize);

            if (isVectorInMonth(v)) {
                return v.ToPoint();
            }

            return null;
        }
        private string pointToDateName(Point p) {
            if (isVectorInMonth(p.ToVector2())) {
                return new DateTime(_year, p.Y + 1, p.X + 1).ToLongDateString();
            }
            return "";
        }
        private bool isVectorInMonth(Vector2 v) {
            return v.X >= 0 && v.Y >= 0 && v.Y < 12 && v.X - MathF.Truncate(v.X) < _cellMargin && v.Y - MathF.Truncate(v.Y) < _cellMargin && v.X < DateTime.DaysInMonth(_year, (int)v.Y + 1);
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

        Color _fillBoth = new Color(231, 70, 148);
        Color _borderBoth = new Color(248, 180, 217);

        Color _fillActive = new Color(214, 31, 105);
        Color _borderActive = new Color(241, 126, 184);

        Color _fillHover = new Color(107, 114, 128);
        Color _borderHover = new Color(210, 214, 220);

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

        ICondition _save =
            new AllCondition(
                new AnyCondition(
                    new KeyboardCondition(Keys.LeftControl),
                    new KeyboardCondition(Keys.RightControl)
                ),
                new KeyboardCondition(Keys.S)
            );

        string _calendarName = "calendar.json";
    }
}
