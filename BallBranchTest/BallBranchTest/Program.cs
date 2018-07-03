using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallBranchTest
{
    class Program
    {
        const int DEFAULT_DEPTH = 4;
        static void Main(string[] args)
        {
            int depth = DEFAULT_DEPTH;
            Console.WriteLine("****** Ball Branch Test Program *****\n");
            Console.WriteLine("Input the depth of the tree [from 1 to 5] and press [Enter]");
            string input = Console.ReadLine();
            //Console.ReadLine();
            try
            {
                depth = Convert.ToInt32(input);
            }
            catch (Exception ex)
            {
                depth = DEFAULT_DEPTH;
            }


            if (depth < 1 || depth > 5)
                depth = DEFAULT_DEPTH;

            long totalContainerCount = (long)Math.Pow(2, depth);
            long totalBallCount = totalContainerCount - 1;


            GenerateGateTree objGenerate = new GenerateGateTree(depth, totalContainerCount, totalBallCount);
            Gate gateTree = objGenerate.InitialiseBranchGateTree();

            Prediction objPrediction = new Prediction(depth, totalContainerCount, totalBallCount);
            objPrediction.PredictResultOfContainerNoBall(gateTree);


            Simulation objSimulate = new Simulation(depth, totalContainerCount, totalBallCount);
            objSimulate.SimulateBalls(gateTree);

            Console.WriteLine("Press [Enter] to close the program ...");
            Console.ReadLine();

        }
    }

    class Gate
    {
        public int gateFlag;  // 0: left open, 1: right open
        public int branchLevel;
        public Gate left;
        public Gate right;
    }

    class GenerateGateTree
    {
        Random _randomNumber = null;
        readonly int _depth;
        readonly long _containerCount;
        readonly long _ballCount;

        public GenerateGateTree(int depth, long containerCount, long ballCount)
        {
            _depth = depth;
            _containerCount = containerCount;
            _ballCount = ballCount;
            _randomNumber = new Random();
        }

        public Gate InitialiseBranchGateTree()
        {
            Gate gateTree = NewGateTree(_depth);
            ShowGateTree(gateTree);
            return gateTree;
        }

        /// <summary>
        /// Create Gate Tree
        /// </summary>
        Gate NewGateTree(int depth)
        {
            Gate gate = new Gate();
            gate.gateFlag = _randomNumber.Next() % 2;  // open left or right
            gate.branchLevel = depth;
            if (depth <= 1)
            {
                gate.left = null;
                gate.right = null;
            }
            else
            {
                gate.left = NewGateTree(depth - 1);
                gate.right = NewGateTree(depth - 1);
            }
            return gate;
        }


        /// <summary>
        /// Display Gate Tree
        /// </summary>
        void ShowGateTree(Gate gateTree)
        {
            int depth = _depth;
            StringBuilder[] treeLevelGateString = new StringBuilder[depth];
            for (int k = 0; k < depth; k++)
                treeLevelGateString[k] = new StringBuilder();
            GenerateTreeGateString(gateTree, treeLevelGateString);

            Console.WriteLine("\n\n=== Initial Gate Tree ===");
            Console.WriteLine("=== [L]: gate open left, [R]: gate open right");
            Console.WriteLine("=== Tree Depth is " + depth + " ===\n");
            for (int i = depth - 1; i >= 0; i--)
            {
                Console.WriteLine(treeLevelGateString[i].ToString());
            }

            for (int j = 0; j < (int)Math.Pow(2, depth); j++)
            {
                Console.Write(Utility.GetContainerName(j));
            }
            Console.WriteLine("\n");
        }

        string TreeGateDisplayString(Gate gate)
        {
            string format = String.Format("{1}0,-{0}{2}", (long)Math.Pow(2, gate.branchLevel) * 4, "{", "}");
            return String.Format(format, (gate.gateFlag == 0) ? "[L]" : "[R]");
        }

        void GenerateTreeGateString(Gate gateTree, StringBuilder[] treeLevelGateString)
        {
            if (gateTree.left != null)
                GenerateTreeGateString(gateTree.left, treeLevelGateString);
            if (gateTree.right != null)
                GenerateTreeGateString(gateTree.right, treeLevelGateString);
            treeLevelGateString[gateTree.branchLevel - 1].Append(TreeGateDisplayString(gateTree));
        }

    }


    /// <summary>
    /// Prediction:
    ///    Use Tree bit list to check bit value in the list of container
    /// </summary>

    class Prediction
    {
        readonly int _depth;
        readonly long _containerCount;
        readonly long _ballCount;

        public Prediction(int depth, long containerCount, long ballCount)
        {
            _depth = depth;
            _containerCount = containerCount;
            _ballCount = ballCount;
        }

        public long PredictContainerNoBall(Gate gateTree)
        {
            long nContainerIndex = (gateTree.gateFlag == 0) ? (long)Math.Pow(2, gateTree.branchLevel - 1) : 0;
            if (gateTree.branchLevel > 1)
            {
                if (gateTree.gateFlag == 0)
                    nContainerIndex += PredictContainerNoBall(gateTree.right);
                else
                    nContainerIndex += PredictContainerNoBall(gateTree.left);
            }
            return nContainerIndex;
        }

        public void PredictResultOfContainerNoBall(Gate gateTree)
        {
            Console.WriteLine("=== Prediction ===");
            long nResultContainerIndex = PredictContainerNoBall(gateTree);

            Console.WriteLine("Prediction:\n    Container " + Utility.GetContainerName(nResultContainerIndex) + " does not have a ball\n");

        }

    }

    class Simulation
    {
        readonly int _depth;
        readonly long _containerCount;
        readonly long _ballCount;

        public Simulation(int depth, long containerCount, long ballCount)
        {
            _depth = depth;
            _containerCount = containerCount;
            _ballCount = ballCount;
        }

        public void SimulateBalls(Gate gateTree)
        {
            int depth = _depth;
            Console.WriteLine("=== Simulation ===");
            int[] containerArray = new int[_containerCount];
            for (long k = 0; k < _containerCount; k++)
                containerArray[k] = 0;

            for (long i = 0; i < _ballCount; i++)
                RunBallRoute(gateTree, i, containerArray);

            for (long j = 0; j < _containerCount; j++)
                if (containerArray[j] == 0)
                {
                    Console.WriteLine("Simulation Result:\n   Container " + Utility.GetContainerName(j) + " does not have a ball\n\n");
                    break;
                }
        }

        void GenerateTrack(Gate gate, StringBuilder s, ref long containerId)
        {
            if (gate.gateFlag == 0)
            {
                gate.gateFlag = 1;

                s.Append("-L");
                if (gate.left != null)
                    GenerateTrack(gate.left, s, ref containerId);
            }
            else
            {
                gate.gateFlag = 0;

                containerId += (long)Math.Pow(2, gate.branchLevel - 1);

                s.Append("-R");
                if (gate.right != null)
                    GenerateTrack(gate.right, s, ref containerId);
            }

        }

        void RunBallRoute(Gate gateTree, long ballNumber, int[] containerArray)
        {
            long containerId = 0;
            StringBuilder s = new StringBuilder();
            GenerateTrack(gateTree, s, ref containerId);
            Console.WriteLine("Ball #" + ballNumber + ": " + s.ToString() + "  => Go to container " + Utility.GetContainerName(containerId));
            containerArray[containerId] = 1;
        }

    }

    static class Utility
    {
        public static string GetContainerName(long nContainerId)
        {
            long nTurn = nContainerId / 26;
            long nLeterIndex = nContainerId % 26;
            string letter = (Convert.ToChar((int)'A' + nLeterIndex)).ToString();
            if (nTurn > 0)
                letter += nTurn.ToString();
            return String.Format("{0,-4}", "(" + letter + ")");
        }
    }
}
