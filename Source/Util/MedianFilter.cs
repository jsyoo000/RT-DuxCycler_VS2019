using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler
{
    public class MedianFilter
    {
        public double[] GetMedianFiltration(double[] inputArray, int window)
        {
            double[] outputArray = new double[inputArray.Length];

            //We have to add an l times of first value to the start of the array and
            //an r times of last value to the end of the array in order to do median filtration properly
            float add = ((float)window - 1) / 2;
            int l = add % 1 == 0 ? (int)add : (int)add + 1;
            int r = (int)add;

            double[] transformedArray = Enumerable.Repeat(inputArray[0], l)
                .Concat(inputArray).Concat(Enumerable.Repeat(inputArray[inputArray.Length - 1], r)).ToArray();

            //Finding median in windows and writing this values to the output array
            for (int i = 0; i < inputArray.Length; i++)
            {
                double[] tempArray = new double[window];
                Array.Copy(transformedArray, i, tempArray, 0, window);
                outputArray[i] = FindMedian(tempArray);
            }

            return outputArray;
        }

        //Using classical algorithm of finding k-th order statistic
        private double FindMedian(double[] arr)
        {
            int k = arr.Length / 2;

            //if input array has even length we take mean between two medians
            if (arr.Length % 2 == 0)
                return (FindingStatistic(arr, 0, arr.Length - 1, k - 1) + 
                        FindingStatistic(arr, 0, arr.Length - 1, k)) / 2;
            return FindingStatistic(arr, 0, arr.Length - 1, k);
        }

        //This is recursive algorithm of finding k-th order statistic using 3-partition
        //l - index of the left side of the array, r - index of the right side
        private double FindingStatistic(double[] arr, int l, int r, int k)
        {
            if (r <= l)
                return arr[l];

            //this array contains two values - start and end index of middle region of 3-partition
            int[] temp = ThreeRegionPartition(arr, l, r);
            if (k < temp[0])
                return FindingStatistic(arr, l, temp[0] - 1, k);
            else if (k > temp[1])
                return FindingStatistic(arr, temp[1] + 1, r, k);
            else
                return arr[temp[0]];
        }

        //Classical 3-partition
        private int[] ThreeRegionPartition(double[] arr, int l, int r)
        {
            int q = l;
            int p = r;

            //s-smaller than reference, b - bigger, m - equal
            //this is unsorted array: (ssmsmmbbmsms)
            for (int k = l; k < p; k++)
            {
                if (arr[k] < arr[r])
                {
                    Swap(ref arr[k], ref arr[q]);
                    q++;
                }
                else if (arr[k] == arr[r])
                {
                    p--;
                    Swap(ref arr[k], ref arr[p]);
                    k--;
                }
            }
            //(sssssbbmmmmm)
            //q-start of the middle region, z - the end
            int z = q;
            for (; p <= r; p++)
            {
                Swap(ref arr[p], ref arr[z]);
                z++;
            }
            //(sssssmmmmmbb)
            return new int[] { q, --z };
        }

        private void Swap(ref double a, ref double b)
        {
            double temp = a;
            a = b;
            b = temp;
        }
    }
}
