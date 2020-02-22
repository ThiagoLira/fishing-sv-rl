using System;
using System.Collections;
using Accord.Neuro;
using DsStardewLib.Utils;
using Accord.Neuro.Learning;
using Accord.Statistics.Analysis;
using System.Linq;
using Accord.Math;

using System.IO;

namespace fishing


{




    class RLAgent

    {

        public const double bobberBarPosMax = 432d;
        public const double bobberBarPosMin = 0;

        public const double bobberBarSpeedMax = 16.8;
        public const double bobberBarSpeedMin = -16.8;

        public const double bobberPositionMax = 526.001d;
        public const double bobberPositionMin = 0;

        public const double distVictoryMax = 1;
        public const double distVictoryMin = 0;

        // new variable BobberPos - BobberBarPos 
        public const double diffBobberFishMax = 508.0d;
        public const double diffBobberFishMin = -432.0d;


        public Logger log;


        // Q LEARNING PARAMETERS

        private double discount =  0.9d;

        // ANN STUFF

        private ArrayList StateRewardMemory = new ArrayList();


        Random rnd = new Random();

        private double learningRate = 0.1;
        private double sigmoidAlphaValue = 2.0;

        private int iterations = 50;
        private bool useRegularization;


        private double epsilon = .5;


        public ActivationNetwork ann;

        private LevenbergMarquardtLearning teacher;



        // performs min-max normalization to ease learning rate
        public double[] NormalizeState(double[] state)
        {
            double[] temp = new double[3];



            temp[0] = (state[0] - bobberBarPosMin) / (bobberBarPosMax - bobberBarPosMin);
            temp[1] = (state[1] - bobberPositionMin) / (bobberPositionMax - bobberPositionMin);
            temp[2] = (state[2] - bobberBarSpeedMin) / (bobberBarSpeedMax - bobberBarSpeedMin);


            return temp;


        }




        public RLAgent(Logger log)
        {

            this.ann = new ActivationNetwork(new SigmoidFunction(sigmoidAlphaValue),
                                                            // num inputs                            
                                                            3,
                                                            // dimensions layers
                                                            30, 20, 2);


            try
            {
                log.Log("Loading + NN.....");
                ann = (ActivationNetwork)Network.Load("nn_backup");
            }
            catch (FileNotFoundException e)
            {
                log.Log("Creating new ANN, backup not found (or non existent)");
            }

            teacher = new LevenbergMarquardtLearning(ann, useRegularization);

            NguyenWidrow initializer = new NguyenWidrow(ann);

            initializer.Randomize();



            this.log = log;

        }


        public void StoreNetwork()
        {
            log.Log("Saving NN.....");
            ann.Save("nn_backup");
        }

        public void TrainNetwork()
        {

            int NumTraining = 5000;


            
            double[,] inputs = new double[NumTraining,3];

            double[,] outputs = new double[NumTraining, 2];

            rnd = new Random();


            var NumberSelected = 0;


            
            foreach (double[] mem in StateRewardMemory)
            {

                double prob = (double)NumTraining / ((double) NumTraining - (double) NumberSelected);

                if (prob > rnd.NextDouble())
                {
                    inputs[NumberSelected, 0] = mem[0];
                    inputs[NumberSelected, 1] = mem[1];
                    inputs[NumberSelected, 2] = mem[2];


                    // StateRewardMemory : { NormOldState[0], NormOldState[1], NormOldState[2], reward, NormNewState[0], NormNewState[1], NormNewState[2] } 

                    double Reward = mem[3];

                    double[] NextState = new double[] { mem[4], mem[5], mem[6] }; 

                    // outputs are realizations from the frozen network
                    // generate then now
                    outputs[NumberSelected, 0] = Reward + discount*ann.Compute(NextState)[0];
                    outputs[NumberSelected, 1] = Reward + discount*ann.Compute(NextState)[1]; 

                    NumberSelected++;
                }

                // filled everything nicelly
                if (NumberSelected > NumTraining - 1) { break; }

            }






            // TRAIN NETWORK

            log.Log("Started a training phase!");

            double error = Double.PositiveInfinity;
            double total = 0;
            for (int i = 0; i < 10; i++)
            {
                error = teacher.RunEpoch(inputs.ToJagged(), outputs.ToJagged());
                log.Log($"Epoch {i} error: {error}");
                total += error;
            }

            log.Log($"Traning phase mean error: {total/10}");

            // lets not overflow memory
            StateRewardMemory = new ArrayList();

            log.Log("Finished a training phase!");

        }


        public int SampleTransition(double[] OlderState, double[] OldState, double[] NewState)
        {


            double reward = NewState[3] - OldState[3];



            // normalize newstate and oldstate

            double[] NormOldState = NormalizeState(OldState);
            double[] NormNewState = NormalizeState(NewState);


            var action = 0;

            // with some probability just use random action 
            var num = rnd.NextDouble();

            if (num > epsilon)
            {
                // 0 or 1
                action = rnd.Next(2);

            }
            else
            {
                // sample action from network



                double[] output = ann.Compute(NormOldState);

                double MaxOut = output.Max();

                // hacky way of doing an ArgMax
                action = output.ToList().IndexOf(MaxOut);

            }


            // store tuple (s,a,r,s') on buffer to train NN after
            StateRewardMemory.Add(new double[] { NormOldState[0], NormOldState[1], NormOldState[2], reward, NormNewState[0], NormNewState[1], NormNewState[2] });



            return action;

        }






    }
}
