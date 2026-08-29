using Terraria;
using Terraria.ModLoader;

namespace Dairox_Mod.Content.NPCs.Globals;

public class BossSpawnGlobalNPC : GlobalNPC
{
    public override void EditSpawnRate(
        Player player,
        ref int spawnRate,
        ref int maxSpawns)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (!npc.active || !npc.boss)
                continue;

            spawnRate = int.MaxValue;
            maxSpawns = 0;
            return;
        }
    }
}