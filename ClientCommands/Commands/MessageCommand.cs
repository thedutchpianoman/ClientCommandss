using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using RemoteAdmin;

namespace ClientCommands.Commands
{
    /// <summary>
    /// .msg &lt;message&gt; — Displays the message above the sender's head for the configured duration.
    /// <list type="bullet">
    ///   <item>Rich-text / color tags are stripped before the length check and display.</item>
    ///   <item>A per-player cooldown prevents spam.</item>
    ///   <item>Multiple players can have active messages simultaneously.</item>
    ///   <item>The original display nickname is restored automatically after the duration.</item>
    /// </list>
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class MessageCommand : ICommand
    {
        // ── ICommand ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Command { get; } = "msg";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Display a message above your head for several seconds. Usage: .msg <text>";

        // ── Execute ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Only real in-game players can use this command.
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

            // ── Validate arguments ─────────────────────────────────────────────

            if (arguments.Count == 0)
            {
                response = "Usage: .msg <message>";
                return false;
            }

            // Reconstruct the full message from all arguments.
            string raw = string.Join(" ", arguments);

            // Strip rich-text tags before checking length to prevent bypass.
            string stripped = EventHandlers.StripRichText(raw);

            if (string.IsNullOrWhiteSpace(stripped))
            {
                response = "Your message cannot be empty.";
                return false;
            }

            int maxLen = Plugin.Instance.Config.Message.MaxLength;
            if (stripped.Length > maxLen)
            {
                response = $"Your message is too long. Maximum {maxLen} characters (after stripping tags). " +
                           $"Current: {stripped.Length}";
                return false;
            }

            // ── Cooldown check ────────────────────────────────────────────────

            Dictionary<string, DateTime> cooldowns = Plugin.Instance.Handlers!.MsgCooldowns;
            float cooldown = Plugin.Instance.Config.Message.Cooldown;

            if (cooldowns.TryGetValue(player.UserId, out DateTime lastUsed))
            {
                double secondsRemaining = cooldown - (DateTime.UtcNow - lastUsed).TotalSeconds;
                if (secondsRemaining > 0)
                {
                    response = $"You must wait {Math.Ceiling(secondsRemaining)} second(s) before using .msg again.";
                    return false;
                }
            }

            // Record this use for the cooldown.
            cooldowns[player.UserId] = DateTime.UtcNow;

            // ── Display the message ───────────────────────────────────────────

            float duration = Plugin.Instance.Config.Message.Duration;

            // Run a coroutine that temporarily appends the message to the player's
            // displayed nickname so all nearby players can see it above their head.
            Timing.RunCoroutine(ShowMessageCoroutine(player, stripped, duration));

            response = $"💬 Message displayed for {duration} second(s).";
            return true;
        }

        // ── Coroutine ─────────────────────────────────────────────────────────

        /// <summary>
        /// Temporarily sets the player's <see cref="Player.DisplayNickname"/> to include
        /// the message text (so it appears above their head for all nearby players),
        /// then restores the original value after <paramref name="duration"/> seconds.
        /// </summary>
        private static IEnumerator<float> ShowMessageCoroutine(Player player, string message, float duration)
        {
            // Save whatever nickname was shown before (may be null for default).
            string originalNickname = player.DisplayNickname ?? player.Nickname;

            // Append the message on a new line so the name is still visible.
            player.DisplayNickname = $"{originalNickname}\n💬 {message}";

            // Wait for the display duration.
            yield return Timing.WaitForSeconds(duration);

            // Restore the original nickname if the player is still connected.
            if (player is { IsConnected: true })
                player.DisplayNickname = originalNickname;
        }
    }
}
