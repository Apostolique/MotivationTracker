using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SpriteFontPlus;

namespace GameProject {
    public static class Assets {
        public static void Setup(ContentManager content) {
            Font = DynamicSpriteFont.FromTtf(TitleContainer.OpenStream($"{content.RootDirectory}/source-code-pro-medium.ttf"), 28);
        }

        public static DynamicSpriteFont Font;
    }
}
