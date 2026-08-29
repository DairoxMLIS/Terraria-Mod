using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Dairox_Mod.Content.Dusts;

public class SkyFlameDust : ModDust
{
    private const float InitialVelocityMultiplier = 0.5f;
    private const float MinScaleMultiplier = 1.1f;
    private const float MaxScaleMultiplier = 1.6f;

    private const float RotationSpeed = 0.12f;
    private const float VelocityDamping = 0.97f;
    private const float ScaleDamping = 0.97f;

    private const float LightMultiplier = 0.6f;
    private const float LightRed = 0.25f;
    private const float LightGreen = 0.8f;
    private const float LightBlue = 1f;

    private const float MinimumScale = 0.45f;

    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= InitialVelocityMultiplier;
        dust.noGravity = true;
        dust.noLight = true;

        dust.scale *= Main.rand.NextFloat(
            MinScaleMultiplier,
            MaxScaleMultiplier
        );

        dust.rotation =
            Main.rand.NextFloat(MathHelper.TwoPi);


         dust.frame = new Rectangle(
            0,
            Main.rand.Next(FrameCount) * FrameHeight,
            FrameWidth,
            FrameHeight
            );
    }

    public override bool Update(Dust dust)
    {
        UpdateMovement(dust);
        UpdateRotation(dust);
        UpdateScale(dust);
        UpdateLighting(dust);
        UpdateFrame(dust);

        if (dust.scale < MinimumScale)
            dust.active = false;

        return false;
    }

    private void UpdateMovement(Dust dust)
    {
        dust.position += dust.velocity;
        dust.velocity *= VelocityDamping;
    }

    private void UpdateRotation(Dust dust)
    {
        dust.rotation +=
            dust.velocity.X * RotationSpeed;
    }

    private void UpdateScale(Dust dust)
    {
        dust.scale *= ScaleDamping;
    }

    private void UpdateLighting(Dust dust)
    {
        float light =
            LightMultiplier * dust.scale;

        Lighting.AddLight(
            dust.position,
            light * LightRed,
            light * LightGreen,
            light * LightBlue
        );
    }
    private const int FrameCount = 3;
    private const int FrameWidth = 11;
    private const int FrameHeight = 11;

    private void UpdateFrame(Dust dust)
    {
        dust.fadeIn++;

        if (dust.fadeIn < 6)
            return;

        dust.fadeIn = 0;

        int currentFrame = dust.frame.Y / FrameHeight;

        currentFrame++;

        if (currentFrame >= FrameCount)
            currentFrame = 0;

        dust.frame = new Rectangle(
            0,
            currentFrame * FrameHeight,
            FrameWidth,
            FrameHeight
        );
    }
}