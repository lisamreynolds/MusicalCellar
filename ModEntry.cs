using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;

namespace MusicalCellar
{
    public class ModEntry : Mod
    {
        private const string CELLAR_NAME = "Cellar";
        private const string FARMHOUSE_NAME = "FarmHouse";

        private string currentlyPlaying = "";

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Player.Warped += Warped;
            Helper.Events.GameLoop.DayEnding += DayEnding;
            Helper.Events.GameLoop.UpdateTicked += UpdateTicked;
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.currentLocation?.Name != FARMHOUSE_NAME) return;

            GameLocation farmHouse = Game1.currentLocation;
            if (!string.IsNullOrEmpty(currentlyPlaying) &&
                (!farmHouse.IsMiniJukeboxPlaying() ||
                Game1.player.isInBed ||
                Game1.timeOfDay >= 2600 ||
                Game1.player.stamina <= -15f
                ))
            {
                // The player has turned the jukebox off
                // or gone to bed
                // or passed out while in the farmhouse
                StopJukeboxMusic();
            }
            if (farmHouse.IsMiniJukeboxPlaying() && farmHouse.miniJukeboxTrack != currentlyPlaying)
            {
                // The jukebox is on but the track has changed
                StartJukeboxMusic();
            }
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            StopJukeboxMusic();
        }

        private void Warped(object sender, WarpedEventArgs e)
        {
            if (e.OldLocation.Name == FARMHOUSE_NAME && e.NewLocation.Name != CELLAR_NAME)
            {
                LeftFarmhouse();
            }
        }

        /// <summary>
        /// Manage music when the player leaves the FarmHouse, but does not go into the Cellar
        /// </summary>
        public void LeftFarmhouse()
        {
            Monitor.Log("Left the house");
            StopJukeboxMusic();
            StartOutsideMusic();
        }

        /// <summary>
        /// Manually start the FarmHouse "jukebox" music
        /// </summary>
        public void StartJukeboxMusic()
        {
            Monitor.Log("Starting music");

            GameLocation farmHouse = Game1.currentLocation;
            Game1.changeMusicTrack(farmHouse.miniJukeboxTrack);
            currentlyPlaying = farmHouse.miniJukeboxTrack;
        }

        /// <summary>
        /// Manually stop the FarmHouse "jukebox" music
        /// </summary>
        public void StopJukeboxMusic()
        {
            Monitor.Log("Stopping music");

            Game1.changeMusicTrack("none", track_interruptable: true);
            currentlyPlaying = "";
        }

        /// <summary>
        /// Start music (again) when the player leaves the FarmHouse
        /// </summary>
        public void StartOutsideMusic()
        {
            Game1.requestedMusicTrack = "fake_ambient"; // Trick Game1.ShouldPlayMorningSong
            Game1.currentLocation.resetForPlayerEntry(); // Includes music selection
        }
    }
}
