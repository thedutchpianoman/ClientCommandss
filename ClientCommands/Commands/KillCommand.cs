using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.ThrowableProjectiles;
using RemoteAdmin;

namespace ClientCommands.Commands
{
    /// <summary>
    /// .kill — Kills the sender with a visual-only explosion effect.
    /// Nearby players receive NO damage from the explosion.
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class KillCommand : ICommand
    {
        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Command { get; } = "kill";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Kills yourself with a visual explosion. No area damage.";

        // ── Execute ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Only real players can use this command.
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command can only be used by a player in-game.";
                return false;
            }

            Player player = Player.Get(playerSender.ReferenceHub);

            if (player is null)
            {
                response = "Could not resolve your player object. Please try again.";
                return false;
            }

            if (!player.IsAlive)
            {
                response = "You are already dead.";
                return false;
            }

            // Spawn a visual-only explosion particle effect at the player's position.
            // ExplosionUtils.ServerSpawnEffect creates the particle without applying
            // any damage, so nearby players are completely safe.
            ExplosionUtils.ServerSpawnEffect(player.Position, ParticleEffectIntensity.Large);

            // Kill the player. The cause is shown in the death screen / round report.
            player.Kill(DamageType.Explosion);

            response = "💥 You killed yourself.";
            return true;
        }
    }
}
