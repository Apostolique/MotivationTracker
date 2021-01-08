using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameProject {
    public static class Assets {
        public static void Setup(ContentManager content, GraphicsDevice graphicsDevice) {
            var FontSystem = FontSystemFactory.Create(graphicsDevice, 2048, 2048);
            FontSystem.AddFont(TitleContainer.OpenStream($"{content.RootDirectory}/source-code-pro-medium.ttf"));
            Font = FontSystem.GetFont(28);
        }

        public static DynamicSpriteFont Font;
    }
}
