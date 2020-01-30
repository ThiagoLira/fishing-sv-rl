using DsStardewLib.SMAPI;
using DsStardewLib.Utils;
using fishing.HarmonyHacks;
using NumSharp;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace fishing
{

    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {


        private DsModHelper<ModConfig> modHelper = new DsModHelper<ModConfig>();
        private HarmonyWrapper hWrapper = new HarmonyWrapper();


        private Logger log;
        private ModConfig config;


        private int CountFishes = 0;
        private bool IsFishing = false;

        // INITIALIZE STATE

        // this is the bar, the lower end 
        float bobberBarPos = 0;
        // velocity of bar
        float bobberBarSpeed = 0;
        // this is the fish 
        float bobberPosition = 15;
        // reward
        // from 0 to 1
        float distanceFromCatching = 0;



        /*********
        ** Public methods
        *********/


        /// <summary>
        /// Check if the player is waiting for the caught fish popup to go away
        /// </summary>
        /// <param name="currentTool">A FishingRod that is being used to fish</param>
        /// <returns>true if the mod should click the popup away</returns>
        private bool ShouldDoDismissCaughtPopup(FishingRod fr)
        {
            return (!Context.CanPlayerMove) && fr.fishCaught && fr.inUse() && !fr.isCasting && !fr.isFishing &&
                   !fr.isReeling && !fr.isTimingCast && !fr.pullingOutOfWater && !fr.showingTreasure;
        }




        /// <summary>
        /// Test whether the mod can automatically cast the fishing rod for the user.
        /// </summary>
        /// <param name="currentTool">A FishingRod that is being used to fish</param>
        /// <returns>true if the mod should cast for the user</returns>
        private bool ShouldDoAutoCast(FishingRod fr)
        {
            return Context.CanPlayerMove && Game1.activeClickableMenu is null && !fr.castedButBobberStillInAir &&
                   !fr.hit && !fr.inUse() && !fr.isCasting && !fr.isFishing && !fr.isNibbling && !fr.isReeling &&
                   !fr.isTimingCast && !fr.pullingOutOfWater;
        }


        public bool autoCastRod = true;

        private RLAgent Agent;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {



            modHelper.Init(helper, this.Monitor);


            log = modHelper.Log;


            Agent = this.Helper.Data.ReadJsonFile<RLAgent>("RlAgent.json") ?? new RLAgent(log);

            if (Agent == null)
            {
                Agent = new RLAgent(log);
            }

            config = modHelper.Config;

            log.Silly("Created log and config for mod entry.  Loading Harmony.");
            hWrapper.InitHarmony(helper, config, log);

            log.Silly("Loading event handlers");

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
            helper.Events.Display.MenuChanged += OnMenuChanged;

        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>

        private void OnMenuChanged(object sender, MenuChangedEventArgs args)
        {
            Farmer player = Game1.player;
            if (player == null || !player.IsLocalPlayer)
            {
                return;
            }

            if (args.NewMenu is BobberBar bar)
            {

                IsFishing = true;

                // No treasures to mess with training!
                Helper.Reflection.GetField<bool>(bar, "treasure").SetValue(false);
            }





        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs args)
        {
            Farmer player = args.Player;

            foreach (Item item in args.Added)
            {
                player.Items.Remove(item);
            }


        }


        private void OnUpdateTicked(object sender, UpdateTickedEventArgs args)
        {


            Farmer player = Game1.player;


            // freeze time
            Game1.gameTimeInterval = 0;

            // infinite stamina
            player.stamina = player.MaxStamina;

            if (player == null || !player.IsLocalPlayer)
                return;
            if (!(Game1.player.CurrentTool is FishingRod))
                return;



            FishingRod rod = Game1.player.CurrentTool as FishingRod;



            if (autoCastRod & ShouldDoAutoCast(rod))
            {
                rod.beginUsing(Game1.currentLocation,
                                 Game1.player.getStandingX(),
                                 Game1.player.getStandingY(),
                                 Game1.player);
            }

            if (!rod.isNibbling && rod.isFishing && !rod.isReeling && !rod.pullingOutOfWater && !rod.hit)
            {
                rod.timeUntilFishingBite = 0;
                rod.castingTimerSpeed = 0;
                rod.castingPower = 1;
            }

            if (rod.isNibbling && rod.isFishing && !rod.isReeling && !rod.pullingOutOfWater && !rod.hit)
            {
                Farmer.useTool(player);
            }

            // Click away the catched fish.  The conditionals are ordered here in a way
            // the check for the popup happens before the config check so the code can
            // always check the treasure chest.  See the variable doneCaughtFish for more
            // info.
            if (ShouldDoDismissCaughtPopup(rod))
            {
                log.Trace("Tool is sitting at caught fish popup");


                CountFishes++;

                this.Helper.Data.WriteJsonFile("RLmodel.json", Agent);

                log.Trace("Closing popup with Harmony");
                ClickAtAllHack.simulateClick = true;

            }

         

            // 2x per second
            if (args.IsMultipleOf(30))
            {


                    if (Game1.activeClickableMenu is BobberBar bar)
                {


                    int  best_action;

                    double[] old_state = new double[] { (double)bobberBarPos, (double)bobberBarSpeed, (double)bobberPosition, (double)distanceFromCatching };

                    // Update State
                    bobberBarPos = Helper.Reflection.GetField<float>(bar, "bobberBarPos").GetValue();
                    bobberBarSpeed = Helper.Reflection.GetField<float>(bar, "bobberBarSpeed").GetValue();
                    bobberPosition = Helper.Reflection.GetField<float>(bar, "bobberPosition").GetValue();
                    distanceFromCatching = Helper.Reflection.GetField<float>(bar, "distanceFromCatching").GetValue();


                    double [] new_state = new double[] { (double)bobberBarPos, (double)bobberBarSpeed, (double)bobberPosition, (double)distanceFromCatching };


                    best_action = (int) Agent.Update(old_state,new_state);

                    // execute action if needed
                    if(best_action == 1 )
                    {
                        IsButtonDownHack.simulateDown = true;
                    }


                }


            }
       
        }










    }
}