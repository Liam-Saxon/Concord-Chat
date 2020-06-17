using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGSclient
{
    class MergeSort
    {
        public void sort(string[] name)
        {
            string[] helper = new string[name.Length];
            mergesort(name, helper, 0, name.Length - 1);
        }

        void mergesort(string[] name, string[] helper, int low, int high)
        {
            if (low < high)
            {
                int middle = (high + low) / 2;
                mergesort(name, helper, low, middle);
                mergesort(name, helper, middle + 1, high);
                merge(name, helper, low, middle, high);
            }
        }

        void merge(string[] name, string[] helper, int low, int middle, int high)
        {
            for (int x = low; x <= high; x++)
            {
                helper[x] = name[x];
            }

            int left = low;
            int curr = low;
            int right = middle + 1;

            while (left <= middle && right <= high)
            {
                if (helper[right].CompareTo(helper[left]) > 0)
                    name[curr++] = helper[left++];
                else
                    name[curr++] = helper[right++];
            }
            while (left <= middle)
                name[curr++] = helper[left++];
        }
    }
}
