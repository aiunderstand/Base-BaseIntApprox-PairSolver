using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

class Program
{
    //Author: Steven Bos - steven.bos@usn.no
    //License: MIT

    //program to compute error between two bases, using extremely large floats (2^32)
    //2DO:
    //     Add parallel computing or go to C with the fastest lib GNU MP bignum library
    //     Unit test to known sequence, previous performance.
    //     Refactor, replaced bigfloat lib with intx(it gives weird rounding errors after 3^10000)

    //v0.1 initial version
    //v0.2 removed magic number of resolution is now a variable
    //     created an API for generic base N, base M solving
    //     fixed missing zero's after comma,but before first digit which made it seem the error was not converging
    //     show the elapsed time in the prompt
    //     moved variable creation out of while loop to avoid garbage collection
    //     export to .csv file

    public static void Main(String[] args)
    {
        //settings
        int baseN = 3;
        int baseM = 2;
        string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\results.csv";
        int StartIndex = 1; //start condition
        int AmountOfPairs = 15; //stop condition
        int Precision = 10; //default = 100, error resolution (digits behind comma)

        //start
        Solve(baseN,baseM,StartIndex,AmountOfPairs, Precision, filePath);
    }

    private static void Solve(int baseN, int baseM, int startIndex, int amountOfPairs, int precision, string filePath)
    {
        //declare variables
        BigFloat index = new BigFloat(startIndex);
        Stopwatch sw = new Stopwatch();
        List<BaseNNearestBaseMPairResult> Results = new List<BaseNNearestBaseMPairResult>();
        bool maxAmountOfPairs = false;
        BigFloat smallestErrorFound = BigFloat.OneHalf; //max distance between two integers 
        BigFloat baseNapproxWithBaseMOrig = new BigFloat(Math.Log(baseN, baseM));
        BigFloat baseNapproxWithBaseM;
        BigFloat NearestIntegerBaseNApproxWithBaseM;
        BigFloat diff;
        BigFloat error;
      
        //write header of file
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filePath, true))
        {
            DateTime now = DateTime.Now;
            file.WriteLine(now.ToString("F"));
            file.WriteLine(String.Format("base{0};base{1};distance;elapsed",baseN,baseM));
        }

        //start stopwatch
        sw.Start();
        
        while (maxAmountOfPairs == false)
        {
            //abs(5*log2(3) - round(5*log2(3)))
            baseNapproxWithBaseM = new BigFloat(baseNapproxWithBaseMOrig * index) ;
            NearestIntegerBaseNApproxWithBaseM = new BigFloat(baseNapproxWithBaseM).Round();

            diff = baseNapproxWithBaseM - NearestIntegerBaseNApproxWithBaseM;
            error = BigFloat.Abs(diff); 
            int test = error.CompareTo(smallestErrorFound);

            if (test < 0)
            {
                smallestErrorFound = new BigFloat(error);
                var errorString = error.ToString(precision);

                if (index>1 && errorString.Equals("0"))
                {
                    throw new Exception("Loss of precision, increase precision");
                }
                else
                {
                    var result = new BaseNNearestBaseMPairResult(index.numerator,
                        NearestIntegerBaseNApproxWithBaseM.numerator, errorString, sw.Elapsed);
                    Results.Add(result);

                    Console.WriteLine(String.Format("{0}, Base{1}^{2}, NearestBase{3}^{4}, Error: {5}, Time: {6}",
                        (Results.Count),
                        baseN,
                        result.baseNInt,
                        baseM,
                        result.nearestBaseMInt,
                        result.errorString,
                        result.elapsed));


                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(filePath, true))
                    {
                        string resultCsvFormat = String.Format("{0};{1};{2};{3}",
                            result.baseNInt,
                            result.nearestBaseMInt,
                            result.errorString,
                            result.elapsed);

                        file.WriteLine(resultCsvFormat);
                    }
                }
            }

            if (Results.Count == amountOfPairs)
            {
                maxAmountOfPairs = true;
                sw.Stop();
                Console.WriteLine("Finished.");
            }
            else
            {
                index++;
            }
        }

        Console.Read();
    }
}

public struct BaseNNearestBaseMPairResult
{
    public BigInteger baseNInt;
    public BigInteger nearestBaseMInt;
    public string errorString;
    public TimeSpan elapsed;

    public BaseNNearestBaseMPairResult(BigInteger baseNInt, BigInteger nearestBaseMInt, string errorString, TimeSpan elapsed)
    {
        this.baseNInt = baseNInt;
        this.nearestBaseMInt = nearestBaseMInt;
        this.errorString = errorString;
        this.elapsed = elapsed;
    }
}