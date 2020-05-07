using Math.Mpfr.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    //Author: Steven Bos - steven.bos@usn.no
    //License: MIT

    //program to compute error between two bases that approximate each other with extreme precision
    //2DO:
    //     Add parallel computing or go to C with the fastest lib GNU MP bignum library
    //     Unit test to known sequence, compare to performance of other frameworks. see https://oeis.org/A005664/b005664.txt

    //v0.1 initial version
    //v0.2 removed magic number of resolution is now a variable
    //     created an API for generic base N, base M solving
    //     fixed missing zero's after comma,but before first digit which made it seem the error was not converging (this was due to 10^-n symbol missing)
    //     show the elapsed time in the prompt
    //     moved variable creation out of while loop to avoid garbage collection
    //     export to .csv file
    //v0.3 refactor, replaced bigfloat lib with gmp (bigfloat lib gave weird rounding errors after 3^10000)
    //     the gmp lib should be retrieved via nuget, but since it missed libmpfr-6.dll, a clone from github was retrieved and manually put in the root folder. this is a known issue.
    public static void Main(String[] args)
    {
        //settings
        int baseN = 3;
        int baseM = 2;
        string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\results.csv";
        int StartIndex = 1; //start condition
        int AmountOfPairs = 10; //stop condition
        uint Precision = 100; //default = 100, error resolution (digits behind comma)

        //start
        Solve(baseN,baseM,StartIndex,AmountOfPairs, Precision, filePath);
    }

    private static void Solve(int n, int m, int startIndex, int amountOfPairs, uint precision, string filePath)
    {
        // Set default precision to 32 bits.
        mpfr_lib.mpfr_set_default_prec(precision);

        //declare variables
        Stopwatch sw = new Stopwatch();
        List<BaseNNearestBaseMPairResult> Results = new List<BaseNNearestBaseMPairResult>();
        bool maxAmountOfPairs = false;

        mpfr_t index = startIndex.ToString();
        mpfr_t smallestErrorFound = "0.5"; //max distance between two integers 

        mpfr_t baseN = n.ToString();
        mpfr_t logBaseN = new mpfr_t();
        mpfr_lib.mpfr_init(logBaseN);
        mpfr_lib.mpfr_log(logBaseN, baseN, mpfr_rnd_t.MPFR_RNDN);

        mpfr_t baseM = m.ToString();
        mpfr_t logBaseM = new mpfr_t();
        mpfr_lib.mpfr_init(logBaseM);
        mpfr_lib.mpfr_log(logBaseM, baseM, mpfr_rnd_t.MPFR_RNDN);

        mpfr_t baseNapproxWithBaseMOrig = new mpfr_t();
        mpfr_lib.mpfr_init(baseNapproxWithBaseMOrig);
        mpfr_lib.mpfr_div(baseNapproxWithBaseMOrig, logBaseN, logBaseM, mpfr_rnd_t.MPFR_RNDN);

        mpfr_t diff = new mpfr_t();
        mpfr_lib.mpfr_init(diff);

        mpfr_t error = new mpfr_t();
        mpfr_lib.mpfr_init(error);

        mpfr_t baseNapproxWithBaseM = new mpfr_t();
        mpfr_lib.mpfr_init(baseNapproxWithBaseM);

        mpfr_t NearestIntegerBaseNApproxWithBaseM = new mpfr_t();
        mpfr_lib.mpfr_init(NearestIntegerBaseNApproxWithBaseM);
        
        //write header of file
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filePath, true))
        {
            DateTime now = DateTime.Now;
            file.WriteLine(now.ToString("F"));
            file.WriteLine(String.Format("base{0};base{1};distance;elapsed",n,m));
        }

        //start stopwatch
        sw.Start();
        
        while (maxAmountOfPairs == false)
        {
            //abs(5*log2(3) - round(5*log2(3)))
            mpfr_lib.mpfr_mul(baseNapproxWithBaseM, baseNapproxWithBaseMOrig, index, mpfr_rnd_t.MPFR_RNDN);
            mpfr_lib.mpfr_round(NearestIntegerBaseNApproxWithBaseM, baseNapproxWithBaseM);
            mpfr_lib.mpfr_sub(diff, baseNapproxWithBaseM, NearestIntegerBaseNApproxWithBaseM, mpfr_rnd_t.MPFR_RNDN);
            mpfr_lib.mpfr_abs(error, diff, mpfr_rnd_t.MPFR_RNDN);

            int test = mpfr_lib.mpfr_cmp(error, smallestErrorFound);

            if (test < 0)
            {
                mpfr_lib.mpfr_set(smallestErrorFound, error, mpfr_rnd_t.MPFR_RNDN);

                var errorString = error.ToString();
                var indexString = index.ToString();
                var NearestIntegerBaseNApproxWithBaseMString = NearestIntegerBaseNApproxWithBaseM.ToString();

                var result = new BaseNNearestBaseMPairResult(indexString,
                    NearestIntegerBaseNApproxWithBaseMString, errorString, sw.Elapsed);
                    Results.Add(result);

                    Console.WriteLine(String.Format("{0}, Base{1}^{2}, NearestBase{3}^{4}, Error: {5}, Time: {6}",
                        (Results.Count),
                        n,
                        result.baseNInt,
                        m,
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

            if (Results.Count == amountOfPairs)
            {
                maxAmountOfPairs = true;
                sw.Stop();
                Console.WriteLine("Finished.");
            }
            else
            {
                mpfr_lib.mpfr_add_si(index, index,1, mpfr_rnd_t.MPFR_RNDN);
            }
        }

        //Release allocated memory for floating-point numbers.
        mpfr_lib.mpfr_clears(index, smallestErrorFound, baseN, baseM, logBaseN, logBaseM, baseNapproxWithBaseMOrig, diff, error, baseNapproxWithBaseM, NearestIntegerBaseNApproxWithBaseM, null);

        Console.Read();
    }
}

public struct BaseNNearestBaseMPairResult
{
    public string baseNInt;
    public string nearestBaseMInt;
    public string errorString;
    public TimeSpan elapsed;

    public BaseNNearestBaseMPairResult(string baseNInt, string nearestBaseMInt, string errorString, TimeSpan elapsed)
    {
        this.baseNInt = baseNInt;
        this.nearestBaseMInt = nearestBaseMInt;
        this.errorString = errorString;
        this.elapsed = elapsed;
    }
}