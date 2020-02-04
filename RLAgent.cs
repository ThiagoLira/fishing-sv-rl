using System;
using System.IO;
using DsStardewLib.Utils;

using System.Collections;
using Newtonsoft.Json;

using NumSharp;


namespace fishing


{
    class RLAgent

    {

        // ALWAYS ON THIS ORDER
        public const double bobberBarPosMax = 432;
        public const double bobberBarPosMin = 0;

        public const double bobberBarSpeedMax = 16.8;
        public const double bobberBarSpeedMin = -16.8;

        public const double bobberPositionMax = 508.0000;
        public const double bobberPositionMin = 0;

        public const double distVictoryMax = 1;
        public const double distVictoryMin = 0;

        // new variable BobberPos - BobberBarPos 
        public const double diffBobberFishMax = 508.0d;
        public const double diffBobberFishMin = -432.0d;


        // for discretization purposes
        // https://pythonprogramming.net/q-learning-reinforcement-learning-python-tutorial/


        // first 3 entries are number of discrete values to be taken by each variable on a state
        public NDArray nBuckets = np.array(new double[] { 20,20 });

        // the AI can CLICK or NOT CLICK
        public int nActions = 2;

        public NDArray DiscreteStep = new double[2];

        public NDArray QTable;


        // Q-learning settings
        private float LearningRate = 0.2F;
        private float Discount = 0.90F;
        private int NumEpisodes = 25000;

        private Logger Log;

        private int NumItersElapsed = 0;

        private double[] RewardBuffer = new double[10000];

        public double GetMeanReward()
        {

            double sum = 0;
            Array.ForEach(RewardBuffer, delegate (double i) { sum += i; });

            return sum / (double)(Math.Min(NumItersElapsed, 10000));

        }


        public void ReadQTableFromJson()
        {
            using (StreamReader r = new StreamReader("QTable.json"))
            {
                string json = r.ReadToEnd();

                double[,,] temp = JsonConvert.DeserializeObject<double[,,] >(json);



                try
                {
                    this.QTable = NDArray.FromMultiDimArray<double>(temp);

                    this.QTable.reshape(new int[] { 1+ Convert.ToInt32((double) nBuckets[0]),
                                                    1+ Convert.ToInt32((double) nBuckets[1]),
                                                    nActions });

                    Log.Log("Successfuly loaded QTable from json");

                }
                catch (Exception e)
                {
                    Log.Log("WARNING Mismatch on QTable size stored on Json");
                }

            }


        }
        public void DumpQTableJson()
        {
            string json = JsonConvert.SerializeObject(this.QTable.ToMuliDimArray<double>());

            System.IO.File.WriteAllText(@"QTable.json", json);


        }



        public int[] DiscretizeState(double[] state)
        {

            int[] temp = new int[2];


            temp[0] = (int)Math.Floor((double)((state[0] - diffBobberFishMin) / DiscreteStep[0]));
            temp[1] = (int)Math.Floor((double)((state[1] - bobberBarSpeedMin) / DiscreteStep[1]));


            return temp;


        }


        public RLAgent(Logger log)
        {




            // calculate the increment on each discretized feature 
            // this is then used to create "buckets" on the Q-table for each possible state
            DiscreteStep[0] = (diffBobberFishMax - diffBobberFishMin) / nBuckets[0];
            DiscreteStep[1] = (bobberBarSpeedMax - bobberBarSpeedMin) / nBuckets[1];



            // initialize Q-table
            // one more position since the last bucket is indexed by the arraysize instead of arraysize -1 
            QTable = np.random.uniform(0, .05, new int[] { 1+ Convert.ToInt32((double) nBuckets[0]),
                                                          1+ Convert.ToInt32((double) nBuckets[1]),
                                                          nActions });



            Log = log;


        }


        public int Update(double[] OlderState, double[] OldState, double[] NewState)
        {


            NumItersElapsed++;

            int BestAction;

            // DistanceFromCatch
            // the closer the agent is better the reward
            // this way we don't have many local minima

            int[] DOldState = DiscretizeState(OldState);
            int[] DNewState = DiscretizeState(NewState);

            // simple difference of winning bar height
            double reward = OldState[2] - OlderState[2];

  
            
            // update reward buffer
            RewardBuffer[NumItersElapsed % 10000] = reward;
           



            try
            {
                // array with q values for 2 possible actions
                BestAction = np.argmax(QTable[DOldState[0], DOldState[1]]);

               

                QTable[DOldState[0], DOldState[1]][BestAction] = QTable[DOldState[0], DOldState[1]][BestAction] + 
                                    LearningRate * ( reward + Discount * np.max(QTable[DNewState[0], DNewState[1]]) - QTable[DOldState[0], DOldState[1]][BestAction]);


                if (NewState[2] == 1d)
                {
                    // terminal state!
                    QTable[DOldState[0], DOldState[1]][BestAction] = 0;
                    Log.Log("Reached terminal state!");
                }

            }
            catch (Exception e)
            {
                Log.Log($"Qtable shape: {QTable.shape[0]}");
                Log.Log($"Qtable shape1: {QTable.shape[1]}");
                Log.Log($"Qtable shape2: {QTable.shape[2]}");
                Log.Log(e.Message);
                Log.Log($"New State: \n " +
                  $"{DOldState[0]} \n" +
                  $"{DOldState[1]} \n" +
                  $"{DOldState[2]} \n");

                return 0;
            }



            Log.Log("-----------------------------------------------");

            Log.Log($"Current State: \n " +
             $"{DOldState[0]} \n" +
             $"{DOldState[1]} \n");
            Log.Log($" Last reward : {reward}");
            Log.Log($" Mean Reward : {GetMeanReward()} ");

            return BestAction;
        }


    }
}
