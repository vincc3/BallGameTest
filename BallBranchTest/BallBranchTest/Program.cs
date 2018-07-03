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
            objPrediction.PredictResult(gateTree);


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

        List<int> GetTreeBitList(int gateFlag, int branchLevel)
        {
            List<int> bits1 = new List<int>();
            List<int> bits0 = new List<int>();
            long i = 0;
            long count = (long)Math.Pow(2, branchLevel - 1);
            for (i = 0; i < count; i++)
            {
                bits1.Add(1);
                bits0.Add(0);
            }
            List<int> bitList = new List<int>();
            if (gateFlag == 0)
            {
                bitList.AddRange(bits0);
                bitList.AddRange(bits1);
            }
            else
            {
                bitList.AddRange(bits1);
                bitList.AddRange(bits0);
            }
            return bitList;
        }

        void GenerateTreeBitList(Gate gateTree, List<int>[] treeLevelBitList)
        {
            if (gateTree.left != null)
                GenerateTreeBitList(gateTree.left, treeLevelBitList);
            if (gateTree.right != null)
                GenerateTreeBitList(gateTree.right, treeLevelBitList);
            treeLevelBitList[gateTree.branchLevel - 1].AddRange(GetTreeBitList(gateTree.gateFlag, gateTree.branchLevel));
        }


        void PrintTreeBitList(List<int> list)
        {
            foreach (int b in list)
                Console.Write(b);
            Console.WriteLine("");
        }

        /// <summary>
        /// convert bit list to integer
        /// </summary>
        long GetLongValueFromBits(List<int> list)
        {
            bool bFirst = true;
            long result = 0;
            foreach (int b in list)
            {
                if (!bFirst)
                {
                    result = result << 1;
                    bFirst = false;
                }
                result = result | (long)b;
            }
            return result;
        }

        /// <summary>
        /// get predicted container index from predict bit lists
        /// </summary>
        int GetPredictResultFromBitsList(List<int>[] list, int depth)
        {
            int nIndex = 0;
            int CountOfList = (int)Math.Pow(2, depth);
            for (int i = 0; i < CountOfList; i++)
            {
                int tempResult = 1;
                for (int k = 0; k < list.Length; k++)
                {
                    int data = list[k].ElementAt<int>(i);
                    tempResult = tempResult & data;
                }
                if (tempResult > 0)
                {
                    nIndex = i;
                    break;
                }
            }
            return nIndex;
        }

        public void PredictResult(Gate gateTree)
        {
            int depth = _depth;
            Console.WriteLine("=== Prediction ===");
            List<int>[] treeLevelBits = new List<int>[depth];
            for (int k = 0; k < depth; k++)
                treeLevelBits[k] = new List<int>();
            // generate predict bit lists to compute predict result
            GenerateTreeBitList(gateTree, treeLevelBits);

            // compute predict result by bit lists
            long bitResult = 0;
            for (int i = 0; i < depth; i++)
            {
                //PrintTreeBitList(treeLevelBits[i]);  // debug
                if (i == 0)
                    bitResult = GetLongValueFromBits(treeLevelBits[i]);
                else
                    bitResult = bitResult & GetLongValueFromBits(treeLevelBits[i]); // bitwise AND
            }
            int nResultIndex = GetPredictResultFromBitsList(treeLevelBits, depth);

            Console.WriteLine("Prediction:\n    Container " + Utility.GetContainerName(nResultIndex) + " does not have a ball\n");

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
