using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

using Dairox_Mod.Content.NPCs.Demigod_of_Sky_Flame_Azaroth;

namespace Dairox_Mod.Content.BossBars;

public class AzarothBossBar : ModBossBar
{
    private bool showCrossIcon;

    public override Asset<Texture2D> GetIconTexture(
        ref Rectangle? iconFrame)
    {
        iconFrame = null;

        return ModContent.Request<Texture2D>(
            showCrossIcon
                ? "Dairox_Mod/Content/NPCs/Demigod_of_Sky_Flame_Azaroth/AzarothBody_Head_Boss_SecondFaze"
                : "Dairox_Mod/Content/NPCs/Demigod_of_Sky_Flame_Azaroth/AzarothBody_Head_Boss"
        );
    }

    public void SetCrossIcon(bool value)
    {
        showCrossIcon = value;
    }

    public override bool? ModifyInfo(
        ref BigProgressBarInfo info,
        ref float life,
        ref float lifeMax,
        ref float shield,
        ref float shieldMax)
    {
        if (info.npcIndexToAimAt < 0 ||
            info.npcIndexToAimAt >= Main.maxNPCs)
        {
            return false;
        }

        NPC azaroth = Main.npc[info.npcIndexToAimAt];

        if (!azaroth.active ||
            azaroth.type != ModContent.NPCType<AzarothBody>())
        {
            return false;
        }

        if (azaroth.ModNPC is not AzarothBody boss)
            return false;

        NPC cross = FindCross();

        float crossHealth =
            cross != null && cross.life > 1
                ? cross.life
                : 0f;

        life = azaroth.life;
        lifeMax = azaroth.lifeMax;

        shield = crossHealth;
        shieldMax = boss.CrossMaxHealth;

        return true;
    }

    private NPC FindCross()
    {
        int crossType =
            ModContent.NPCType<AzarothCross>();

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (!npc.active)
                continue;

            if (npc.type == crossType)
                return npc;
        }

        return null;
    }
}