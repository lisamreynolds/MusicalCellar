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
            Helper.Events.GameLoop.DayStarted += DayStarted;
            Helper.Events.GameLoop.DayEnding += DayEnding;
            Helper.Events.GameLoop.UpdateTicked += UpdateTicked;
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.currentLocation?.Name != FARMHOUSE_NAME) return;

            GameLocation farmHouse = Game1.currentLocation;
            if ((!farmHouse.IsMiniJukeboxPlaying() && !string.IsNullOrEmpty(currentlyPlaying)) ||
                farmHouse.miniJukeboxTrack != currentlyPlaying ||
                Game1.player.isInBed ||
                Game1.timeOfDay >= 2600 ||
                Game1.player.stamina <= -15f
                )
            {
                // The player has turned the jukebox off or changed the track
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

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Game1.locations.Any(l => l.Name == CELLAR_NAME)) return;

            EnteredFarmHouse();
        }
        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            StopJukeboxMusic();
        }

        private void Warped(object sender, WarpedEventArgs e)
        {
            if (!Game1.locations.Any(l => l.Name == CELLAR_NAME)) return;

            if (e.NewLocation.Name == FARMHOUSE_NAME)
            {
                EnteredFarmHouse();
            }
            else if (e.OldLocation.Name == FARMHOUSE_NAME && e.NewLocation.Name != CELLAR_NAME)
            {
                LeftFarmhouse();
            }
        }

        /// <summary>
        /// Manage music when the player has entered the farmhouse, either by Warp or by DayStarted.
        /// </summary>
        private void EnteredFarmHouse()
        {
            GameLocation farmhouse = Game1.currentLocation;
            GameLocation cellar = Game1.locations.First(l => l.Name == CELLAR_NAME);
            if (farmhouse.IsMiniJukeboxPlaying() && !cellar.IsMiniJukeboxPlaying())
            {
                Monitor.Log("FarmHouse Jukebox is playing " + farmhouse.miniJukeboxTrack);
                StartJukeboxMusic();
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

            GameLocation farmHouse = Game1.locations.First(l => l.Name == FARMHOUSE_NAME);
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
