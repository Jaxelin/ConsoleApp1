namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int[] nums = new int[] { 5, 7, 7, 8, 8, 10 };
            int target = 8;

            var r = SearchRange(nums, target);

        }


        #region 二分查找法 【时间复杂度 O(log n)】

        // 主函数：返回 target 在升序数组 nums 中的第一个和最后一个位置
        public static int[] SearchRange(int[] nums, int target)
        {
            if (nums == null || nums.Length == 0)
                return new int[] { -1, -1 };

            int first = FindFirst(nums, target);
            if (first == -1)
                return new int[] { -1, -1 }; // 未找到 target，无需查找最后一个

            int last = FindLast(nums, target);
            return new int[] { first, last };
        }

        // 辅助函数：查找 target 的第一个出现位置
        private static int FindFirst(int[] nums, int target)
        {
            int left = 0;
            int right = nums.Length - 1;
            int result = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2; // 防止整数溢出

                if (nums[mid] == target)
                {
                    result = mid;       // 记录可能的第一个位置
                    right = mid - 1;    // 继续向左搜索，看是否有更早的出现
                }
                else if (nums[mid] < target)
                {
                    left = mid + 1;     // target 在右半部分
                }
                else
                {
                    right = mid - 1;    // target 在左半部分
                }
            }

            return result;
        }

        // 辅助函数：查找 target 的最后一个出现位置
        private static int FindLast(int[] nums, int target)
        {
            int left = 0;
            int right = nums.Length - 1;
            int result = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (nums[mid] == target)
                {
                    result = mid;       // 记录可能的最后一个位置
                    left = mid + 1;     // 继续向右搜索，看是否有更晚的出现
                }
                else if (nums[mid] < target)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return result;
        }


        #endregion






        public static double ComputeExpression(int m)
        {
            int fullGroups = m / 4;
            int remainder = m % 4;
            long total = (long)fullGroups * (-4);
            int start = 4 * fullGroups + 1;

            switch (remainder)
            {
                case 1:

                    total += start;
                    break;
                case 2:
                    total += start + (start + 1);
                    break;
                case 3:
                    total += start + (start + 1) - (start + 2);
                    break;
            }

            return (double)total / (-m);
        }



        #region 搭积木算法

        private static void SF_DaJiMu()
        {
            string st = Console.ReadLine() ?? "";
            if (!int.TryParse(st, out int t))
            {
                Console.WriteLine("输入数据组数量值无效！");
                return;
            }
            for (int d = 0; d < t; d++)
            {
                string sn = Console.ReadLine() ?? "";
                if (!int.TryParse(sn, out int n))
                {
                    Console.WriteLine("输入n值无效！");
                    return;
                }

                List<Jimu> jimus = new List<Jimu>();
                for (int i = 0; i < n; i++)
                {
                    var numbers = (Console.ReadLine() ?? "")
                                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(int.Parse)
                                    .ToArray();
                    jimus.Add(new Jimu { Start = numbers[0], End = numbers[1] });
                }

                bool can = false;
                Jimu rootJm = jimus.FirstOrDefault()!;
                jimus.RemoveAt(0);
                Jimu resultJimu = new Jimu() { Start = rootJm.Start, End = rootJm.End };
                can = LoopNextJimus(jimus, rootJm, resultJimu);

                Console.WriteLine(can ? "YES" : "NO");
            }
        }

        internal class Jimu
        {
            public int Start { get; set; }
            public int End { get; set; }

        }

        public static List<Jimu> FindNextJimusCanPin(IList<Jimu> jimus, Jimu fatherJm)
        {
            List<Jimu> nextCanPinJimus = new List<Jimu>();
            for (int j = 0; j < jimus.Count; j++)
            {
                if ((fatherJm.Start == jimus[j].Start || fatherJm.Start == jimus[j].End) || (fatherJm.End == jimus[j].Start) || (fatherJm.End == jimus[j].End))
                {
                    nextCanPinJimus.Add(jimus[j]);
                }
            }

            return nextCanPinJimus!;
        }

        public static bool LoopNextJimus(IList<Jimu> fatherJimus, Jimu fatherJm, Jimu resultJimu)
        {
            if (fatherJimus.Count == 0)
            {
                return true;
            }
            bool can = false;
            var nextCanPinJimus = FindNextJimusCanPin(fatherJimus, resultJimu);
            if (nextCanPinJimus != null)
            {
                for (int i = 0; i < nextCanPinJimus.Count; i++)
                {
                    var tempList = new List<Jimu>(fatherJimus.ToArray());
                    tempList.Remove(nextCanPinJimus[i]);

                    Jimu tempJimu = new Jimu() { Start = resultJimu.Start, End = resultJimu.End };
                    if (tempJimu.Start == nextCanPinJimus[i].Start)
                    {
                        tempJimu.Start = nextCanPinJimus[i].End;
                    }
                    else if (tempJimu.Start == nextCanPinJimus[i].End)
                    {
                        tempJimu.Start = nextCanPinJimus[i].Start;
                    }
                    else if (tempJimu.End == nextCanPinJimus[i].Start)
                    {
                        tempJimu.End = nextCanPinJimus[i].End;
                    }
                    else if (tempJimu.End == nextCanPinJimus[i].End)
                    {
                        tempJimu.End = nextCanPinJimus[i].Start;
                    }

                    can = LoopNextJimus(tempList, nextCanPinJimus[i], tempJimu);
                }
            }
            return can;
        }

        #endregion

        private static void SF_PanDuanSuoYouRenShiFouChangWanGe()
        {
            string sk = Console.ReadLine() ?? "";
            if (!int.TryParse(sk, out int k))
            {
                Console.WriteLine("输入n值无效！");
                return;
            }

            int[] numbers = (Console.ReadLine() ?? "")
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToArray();

            int n = numbers[0];
            List<int[]> ints = new List<int[]>();
            for (int i = 0; i < n; i++)
            {
                ints.Add((Console.ReadLine() ?? "")
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToArray());
            }
            var dict = new Dictionary<string, int>();
            foreach (var arr in ints)
            {
                for (int i = 1; i < arr.Length; i++)
                {
                    if (dict.ContainsKey(arr[i].ToString()))
                    {
                        dict[arr[i].ToString()] += 1;
                    }
                    else
                    {
                        dict[arr[i].ToString()] = 1;
                    }
                }
            }

            int songs = dict.Keys.Count;
            if (songs > numbers[2])
            {
                Console.WriteLine("NO");
                return;
            }
            int x = numbers[1];
            int y = numbers[2];
            var keys = dict.Keys;
            while (y > 0)
            {
                var key = keys.ElementAt(0);
                dict[key] -= x;
                if (dict[key] <= 0)
                {
                    dict.Remove(key);
                }
                y--;
            }

            if (dict.Count > 0)
                Console.WriteLine("NO");
            else
                Console.WriteLine("YES");
        }

        private static void SF_DaShuJieQu_Mod()
        {
            const long MOD = 1_000_000_007L;

            string s = Console.ReadLine()!;
            int n = s.Length;
            var freq = new Dictionary<long, int>();

            for (int i = 0; i < n; i++)
            {
                long num = 0;
                for (int j = i; j < n; j++)
                {
                    num = (num * 10 + (s[j] - '0')) % MOD;
                    freq[num] = freq.GetValueOrDefault(num, 0) + 1;
                }
            }

            int t = int.Parse(Console.ReadLine()!);
            while (t-- > 0)
            {
                long ai = long.Parse(Console.ReadLine()!);
                Console.WriteLine(freq.GetValueOrDefault(ai, 0));
            }

        }

        private static void SF_OuShuPanDuan()
        {
            int n = 0;
            string sk = Console.ReadLine() ?? "";
            if (!int.TryParse(sk, out n))
            {
                Console.WriteLine("输入整数无效！");
                return;
            }

            if (n % 2 == 0)
            {
                Console.WriteLine("1");
            }
            else
            {
                Console.WriteLine("0");
            }
        }

        private static void SF_ZhaoPing_Max_DaXueSheng()
        {
            int k = 0;
            string sk = Console.ReadLine() ?? "";
            if (!int.TryParse(sk, out k))
            {
                Console.WriteLine("输入K值无效！");
                return;
            }

            int[] numbers = (Console.ReadLine() ?? "")
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToArray();

            Array.Sort(numbers);
            Array.Reverse(numbers);

            Console.WriteLine("读取到的整数数组：[" + string.Join(", ", numbers) + "]");

            int startNumber = numbers[0];
            int maxCount = startNumber;
            for (int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i] == startNumber)
                {
                    startNumber = numbers[i] - 1;
                    maxCount += startNumber;

                }
                else if (numbers[i] > 0)
                {
                    maxCount += numbers[i];
                }

            }

            Console.WriteLine(maxCount);


        }
    }
}
