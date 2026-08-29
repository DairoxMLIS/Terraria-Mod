using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.NPCs.Demigod_of_Sky_Flame_Azaroth;

public class AzarothArena : GlobalNPC
{
    public override bool InstancePerEntity => true;

    #region Constants

    private const float ArenaWidth = 2200f;
    private const float ArenaHeight = 2200f;

    private const float KillMargin = 10f;

    private const string WallMidTexturePath =
        "Dairox_Mod/Content/NPCs/Demigod_of_Sky_Flame_Azaroth/AzarothWallMid";

    private const string WallCornerTexturePath =
        "Dairox_Mod/Content/NPCs/Demigod_of_Sky_Flame_Azaroth/AzarothWallCorner";

    #endregion

    #region Fields

    private Vector2 arenaCenter;
    private bool arenaInitialized;

    #endregion

    #region Spawn

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (npc.type != ModContent.NPCType<AzarothBody>())
            return;

        arenaCenter = npc.Center;
        arenaInitialized = true;
    }

    #endregion

    #region AI

    public override void AI(NPC npc)
    {
        if (npc.type != ModContent.NPCType<AzarothBody>())
            return;

        if (!npc.active)
            return;

        if (!arenaInitialized)
        {
            arenaCenter = npc.Center;
            arenaInitialized = true;
        }

        Player player = Main.player[npc.target];

        if (player == null ||
            !player.active ||
            player.dead)
        {
            return;
        }

        UpdatePlayerPosition(player);
    }

    private void UpdatePlayerPosition(Player player)
    {
        float halfWidth =
            ArenaWidth / 2f;

        float halfHeight =
            ArenaHeight / 2f;

        float left =
            arenaCenter.X - halfWidth;

        float right =
            arenaCenter.X + halfWidth;

        float top =
            arenaCenter.Y - halfHeight;

        float bottom =
            arenaCenter.Y + halfHeight;

        Vector2 playerCenter =
            player.Center;

        // Если игрок каким-то образом оказался
        // сильно за пределами арены — убиваем его.
        if (playerCenter.X < left - KillMargin ||
            playerCenter.X > right + KillMargin ||
            playerCenter.Y < top - KillMargin ||
            playerCenter.Y > bottom + KillMargin)
        {
            player.KillMe(
                PlayerDeathReason.ByCustomReason(
                    $"{player.name} was consumed by Azaroth's arena."
                ),
                999999,
                0
            );

            return;
        }

        // Обычное движение упирается в границу арены.
        float minX =
            left + player.width / 2f;

        float maxX =
            right - player.width / 2f;

        float minY =
            top + player.height / 2f;

        float maxY =
            bottom - player.height / 2f;

        player.Center = new Vector2(
            MathHelper.Clamp(
                player.Center.X,
                minX,
                maxX
            ),
            MathHelper.Clamp(
                player.Center.Y,
                minY,
                maxY
            )
        );
    }

    #endregion

    #region Rendering

    public override void PostDraw(
        NPC npc,
        SpriteBatch spriteBatch,
        Vector2 screenPos,
        Color drawColor)
    {
        if (npc.type != ModContent.NPCType<AzarothBody>())
            return;

        if (!npc.active)
            return;

        if (!arenaInitialized)
            return;

        DrawArena(
            spriteBatch,
            screenPos
        );
    }

    private void DrawArena(
        SpriteBatch spriteBatch,
        Vector2 screenPos)
    {
        Texture2D midTexture =
            ModContent.Request<Texture2D>(
                WallMidTexturePath
            ).Value;

        Texture2D cornerTexture =
            ModContent.Request<Texture2D>(
                WallCornerTexturePath
            ).Value;

        float halfWidth =
            ArenaWidth / 2f;

        float halfHeight =
            ArenaHeight / 2f;

        float left =
            arenaCenter.X - halfWidth;

        float right =
            arenaCenter.X + halfWidth;

        float top =
            arenaCenter.Y - halfHeight;

        float bottom =
            arenaCenter.Y + halfHeight;

        int cornerSize =
            cornerTexture.Width;

        // Верхняя сторона
        DrawHorizontalWall(
            spriteBatch,
            midTexture,
            left + cornerSize,
            right - cornerSize,
            top,
            screenPos,
            SpriteEffects.None
        );

        // Нижняя сторона
        DrawHorizontalWall(
            spriteBatch,
            midTexture,
            left + cornerSize,
            right - cornerSize,
            bottom - midTexture.Height,
            screenPos,
            SpriteEffects.FlipVertically
        );

        // Левая сторона
        DrawVerticalWall(
            spriteBatch,
            midTexture,
            left,
            top + cornerSize,
            bottom - cornerSize,
            screenPos,
            SpriteEffects.None
        );

        // Правая сторона
        DrawVerticalWall(
            spriteBatch,
            midTexture,
            right-5f,
            top + cornerSize,
            bottom - cornerSize,
            screenPos,
            SpriteEffects.FlipHorizontally
        );

        // Левый верхний
        DrawCorner(
            spriteBatch,
            cornerTexture,
            left,
            top,
            screenPos,
            SpriteEffects.None
        );

        // Правый верхний
        DrawCorner(
            spriteBatch,
            cornerTexture,
            right - cornerSize,
            top,
            screenPos,
            SpriteEffects.FlipHorizontally
        );

        // Левый нижний
        DrawCorner(
            spriteBatch,
            cornerTexture,
            left,
            bottom - cornerSize,
            screenPos,
            SpriteEffects.FlipVertically
        );

        // Правый нижний
        DrawCorner(
            spriteBatch,
            cornerTexture,
            right - cornerSize,
            bottom - cornerSize,
            screenPos,
            SpriteEffects.FlipHorizontally |
            SpriteEffects.FlipVertically
        );
    }

    private void DrawHorizontalWall(
        SpriteBatch spriteBatch,
        Texture2D texture,
        float startX,
        float endX,
        float y,
        Vector2 screenPos,
        SpriteEffects effects)
    {
        float width =
            texture.Width;

        for (float x = startX;
             x < endX;
             x += width)
        {
            float remaining =
                endX - x;

            int drawWidth =
                (int)MathHelper.Min(
                    width,
                    remaining
                );

            Rectangle source =
                new Rectangle(
                    0,
                    0,
                    drawWidth,
                    texture.Height
                );

            spriteBatch.Draw(
                texture,
                new Vector2(
                    x,
                    y
                ) - screenPos,
                source,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                effects,
                0f
            );
        }
    }

    private void DrawVerticalWall(
        SpriteBatch spriteBatch,
        Texture2D texture,
        float x,
        float startY,
        float endY,
        Vector2 screenPos,
        SpriteEffects effects)
    {
        float height =
            texture.Width;

        float segmentHeight =
            texture.Width;

        for (float y = startY;
             y < endY;
             y += segmentHeight)
        {
            float centerX =
                x + texture.Height / 2f;

            float centerY =
                y + segmentHeight / 2f;

            spriteBatch.Draw(
                texture,
                new Vector2(
                    centerX,
                    centerY
                ) - screenPos,
                null,
                Color.White,
                MathHelper.PiOver2,
                new Vector2(
                    texture.Width / 2f,
                    texture.Height / 2f
                ),
                1f,
                effects,
                0f
            );
        }
    }

    private void DrawCorner(
        SpriteBatch spriteBatch,
        Texture2D texture,
        float x,
        float y,
        Vector2 screenPos,
        SpriteEffects effects)
    {
        spriteBatch.Draw(
            texture,
            new Vector2(
                x,
                y
            ) - screenPos,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            1f,
            effects,
            0f
        );
    }

    #endregion
}