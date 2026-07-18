using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.Usables.Scp330;
using MEC;

namespace ClientCommands
{
    /// <summary>
    /// Handles all server- and player-level events for the ClientCommands plugin.
    /// Also owns the automatic candy distribution coroutine and the .msg cooldown map.
    /// </summary>
    public sealed class EventHandlers
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly Config _config;

        /// <summary>
        /// Per-player timestamps of the last successful .msg use (UTC ticks).
        /// Keyed by <see cref="Player.UserId"/>.
        /// </summary>
        internal readonly Dictionary<string, DateTime> MsgCooldowns = new();

        // ── Candy coroutine handle ────────────────────────────────────────────

        private CoroutineHandle _candyCoroutine;

        // ── All available SCP:SL candy kinds ─────────────────────────────────

        private static readonly CandyKindID[] AllCandies =
        {
            CandyKindID.Red,
            CandyKindID.Green,
            CandyKindID.Blue,
            CandyKindID.Purple,
            CandyKindID.Rainbow,
            CandyKindID.Yellow,
            CandyKindID.Pink,
        };

        private static readonly Random Rng = new();

        // ── Constructor ───────────────────────────────────────────────────────

        public EventHandlers(Config config)
        {
            _config = config;
        }

        // ── Server events ─────────────────────────────────────────────────────

        /// <summary>
        /// Called when the round starts. Kicks off the candy distribution coroutine
        /// if the candy feature is enabled.
        /// </summary>
        public void OnRoundStarted()
        {
            if (_config.Candy.IsEnabled)
            {
                _candyCoroutine = Timing.RunCoroutine(CandyDistributionCoroutine());
                Log.Debug("[ClientCommands] Candy distribution coroutine started.");
            }
        }

        /// <summary>
        /// Called when the round ends. Stops the candy coroutine and clears cooldowns.
        /// </summary>
        public void OnRoundEnded(RoundEndedEventArgs _)
        {
            KillCandyCoroutine();
            MsgCooldowns.Clear();
            Log.Debug("[ClientCommands] Candy coroutine stopped and cooldowns cleared.");
        }

        // ── Player events ─────────────────────────────────────────────────────

        /// <summary>
        /// Removes the player's cooldown entry when they leave so stale entries
        /// don't accumulate across reconnects.
        /// </summary>
        public void OnPlayerLeft(LeftEventArgs ev)
        {
            MsgCooldowns.Remove(ev.Player.UserId);
        }

        // ── Candy coroutine ───────────────────────────────────────────────────

        /// <summary>
        /// Coroutine that waits <see cref="CandyConfig.Interval"/> seconds and then
        /// gives every alive player one random candy, repeating until the round ends.
        /// </summary>
        private IEnumerator<float> CandyDistributionCoroutine()
        {
            while (true)
            {
                // Wait for the configured interval before each handout.
                yield return Timing.WaitForSeconds(_config.Candy.Interval);

                foreach (Player player in Player.List)
                {
                    // Only give candy to alive, non-NPC players.
                    if (!player.IsAlive || player.IsNPC)
                        continue;

                    CandyKindID candy = PickRandomCandy();
                    GiveCandyToPlayer(player, candy);

                    player.Broadcast(
                        duration: 5,
                        message: $"🍬 You received a free <color=yellow>{GetCandyName(candy)}</color> candy!",
                        type: Broadcast.BroadcastFlags.Normal,
                        shouldClearPrevious: false);
                }
            }
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        /// <summary>Stops the running candy coroutine if one exists.</summary>
        internal void KillCandyCoroutine()
        {
            if (_candyCoroutine.IsRunning)
                Timing.KillCoroutines(_candyCoroutine);
        }

        /// <summary>
        /// Gives a single candy of the specified kind to <paramref name="player"/>.
        /// Uses the SCP-330 bag API: adds the bag item and inserts the chosen candy.
        /// </summary>
        internal static void GiveCandyToPlayer(Player player, CandyKindID candyKind)
        {
            try
            {
                // Add an SCP-330 bag item to the player's inventory.
                var item = player.AddItem(ItemType.SCP330);

                // Cast to the game's Scp330Bag and insert the specific candy kind.
                if (item?.Base is Scp330Bag bag)
                {
                    // Clear default candies generated by the constructor, then add ours.
                    bag.Candies.Clear();
                    bag.TryAddSpecificCandy(candyKind);
                    bag.ServerRefreshBag();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[ClientCommands] GiveCandyToPlayer failed: {ex.Message}");
            }
        }

        /// <summary>Returns a random <see cref="CandyKindID"/> from <see cref="AllCandies"/>.</summary>
        internal static CandyKindID PickRandomCandy()
            => AllCandies[Rng.Next(AllCandies.Length)];

        /// <summary>Maps a <see cref="CandyKindID"/> to a human-readable name.</summary>
        internal static string GetCandyName(CandyKindID kind) => kind switch
        {
            CandyKindID.Red     => "Red",
            CandyKindID.Green   => "Green",
            CandyKindID.Blue    => "Blue",
            CandyKindID.Purple  => "Purple",
            CandyKindID.Rainbow => "Rainbow",
            CandyKindID.Yellow  => "Yellow",
            CandyKindID.Pink    => "Pink",
            _                   => kind.ToString(),
        };

        /// <summary>
        /// Strips rich-text / color tags from a string so they are not counted
        /// toward the max-length limit and cannot be abused for formatting exploits.
        /// </summary>
        internal static string StripRichText(string input)
            => Regex.Replace(input, @"<[^>]*>", string.Empty);
    }
}
