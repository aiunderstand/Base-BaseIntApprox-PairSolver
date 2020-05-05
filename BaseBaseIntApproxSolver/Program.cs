using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;

class Program
{
    //program to compute error between two bases, using extremely large floats. 
    //2do: make it more generic, eg. API interface Find(radix N, radix M, maxPairs);
    //fine tune resolution, garbage collector, parallel computing. This is just a quick exploratory script.
    public static void Main(String[] args)
    {
        int AmountOfPairs = 10; //stop condition
        int index = 1; //start condition

        Stopwatch sw = new Stopwatch();
        List<Base3NearestBase2PairResult> ErrorAtIndexList = new List<Base3NearestBase2PairResult>();
        bool maxResolutionReached = false;
        BigFloat smallestErrorFound = 0.5; //max distance between two integers 
        BigFloat base3approxWithBase2Orig = Math.Log(3, 2);

        BigFloat base3approxWithBase2;
        BigFloat NearestIntegerBase3ApproxWithBase2;

        while (maxResolutionReached == false)
        {
            sw.Restart();

            base3approxWithBase2 = new BigFloat(base3approxWithBase2Orig).Multiply(index);
            NearestIntegerBase3ApproxWithBase2 = new BigFloat(base3approxWithBase2).Round();

            BigFloat diff = base3approxWithBase2 - NearestIntegerBase3ApproxWithBase2;
            BigFloat error = BigFloat.Abs(diff);
            string errorString = error.ToString((5));

            int test = error.CompareTo(smallestErrorFound);
            if (test < 0)
            {
                string nearestBase2 = NearestIntegerBase3ApproxWithBase2.ToString(10);
                int nearestBase2Int = int.Parse(nearestBase2);
                var result = new Base3NearestBase2PairResult(index, nearestBase2Int, errorString, sw.Elapsed);
                ErrorAtIndexList.Add(result);

                Console.WriteLine(String.Format("Base3: {0}, NearestBase2: {1}, Error: {2}",
                    index,
                    nearestBase2Int,
                    error.ToString(8)));

                smallestErrorFound = new BigFloat(error);
            }

            if (ErrorAtIndexList.Count == AmountOfPairs)
            {
                maxResolutionReached = true;
                Console.WriteLine("Finished correctly.");
            }
            else
            {
                index++;
            }
        }

        Console.Read();
    }
}

public struct Base3NearestBase2PairResult
{
    private int index;
    private int nearestBase2Int;
    private string errorString;
    private TimeSpan elapsed;

    public Base3NearestBase2PairResult(int index, int nearestBase2Int, string errorString, TimeSpan elapsed)
    {
        this.index = index;
        this.nearestBase2Int = nearestBase2Int;
        this.errorString = errorString;
        this.elapsed = elapsed;
    }
}