using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using StardewModdingAPI;
using DsStardewLib.Utils;



using NumSharp;


namespace fishing


{
    class RLAgent

    {
        //bobberbarpos MAX: 432 , MIN: 0

        //bobberBarSpeed MAX: 16.8 , MIN: -17.66666

        //bobberPosition MAX: 508 , MIN: 339.0656

        // ALWAYS ON THIS ORDER
        public const float bobberBarPosMax = 432;
        public const float bobberBarPosMin = 0;

        public const float bobberBarSpeedMax = 16.8F;
        public const float bobberBarSpeedMin = -17.66F;

        public const float bobberPositionMax = 508;
        public const float bobberPositionMin = 339.0656F;

        // for discretization purposes
        // https://pythonprogramming.net/q-learning-reinforcement-learning-python-tutorial/


        // first 3 entries are number of discrete values to be taken by each variable on a state
        // last entry is the number of actions! to click or not to click
        public NDArray nDims = new float [] { 20, 20, 20 , 2 };

        public NDArray DiscreteSteps = new float[3];

        public int[][] QTable;

        private Logger Log;

        public int [] DiscretizeState(float[] state)
        {

            int[] temp = { 0, 0, 0 };


            temp[0] = state[0] - bobberBarPosMin / DiscreteSteps[0];
            temp[1] = state[1] - bobberBarSpeedMin / DiscreteSteps[1];
            temp[2] = state[2] - bobberPositionMin / DiscreteSteps[2];


            return temp;



        }


        public RLAgent(Logger log)
        {





            DiscreteSteps[0] = (bobberBarPosMax - bobberBarPosMin) / nDims[0];
            DiscreteSteps[1] = (bobberBarSpeedMax + bobberBarSpeedMin) / nDims[1];
            DiscreteSteps[2] = (bobberPositionMax - bobberPositionMin) / nDims[2];



            //NDArray q_table = np.random.uniform( -2,  0, nDims);



            Log = log;

        }
        

        public int Update(float[] state) 
        {

            int[] d_state = DiscretizeState(state);


            Log.Log($"New State: " +
                $"{d_state[0]}" +
                $"{d_state[1]}" +
                $"{d_state[2]}");

            return 0;
        }


    }
}
