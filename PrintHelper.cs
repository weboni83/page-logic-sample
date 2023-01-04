using System;
using System.Linq;
using System.Text;

namespace ConsoleAppPageListCreater
{
    public static class PrintHelper
    {
        /// <summary>
        /// 문자열 페이지를 배열로 반환
        /// </summary>
        /// <param name="pages">1-7,15 와 같은 형태의 문자열</param>
        /// <returns>1,2,3,4,5,6,7,15</returns>
        public static int[] TextToPages(string pages)
        {
            if(string.IsNullOrEmpty(pages))
                return new int[0];

            int[] returnPage = new int[10];
            string addPage = string.Empty;

            string[] pageGb = pages.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for(int a = 0; a < pageGb.Length; a++)
            {
                if(pageGb[a].IndexOf('-') > -1)
                {
                    string[] dash = pageGb[a].Split('-');
                    for(int i = Convert.ToInt32(dash[0]); i <= Convert.ToInt32(dash[1]); i++)
                    {
                        if(addPage == string.Empty) addPage = Convert.ToString(i);
                        else addPage += "," + Convert.ToString(i);
                    }
                }
                else
                {
                    if(addPage == string.Empty) addPage = pageGb[a].ToString();
                    else addPage += "," + pageGb[a].ToString();
                }
            }

            string[] splitPage = addPage.Split(',');

            returnPage = new int[splitPage.Length];
            for(int j = 0; j < splitPage.Length; j++)
            {
                returnPage[j] = Convert.ToInt32(splitPage[j].ToString());
            }


            return returnPage;
        }
        /// <summary>
        /// 배열을 페이지 형식으로 반환
        /// </summary>
        /// <param name="numbers">1,2,3,4,5,6,25</param>
        /// <returns>1-6, 25</returns>
        public static string PagesToText(int[] numbers)
        {
            StringBuilder sb = new StringBuilder();
            Array.Sort(numbers);

            var count = 0;
            for(int idx = 0; idx < numbers.Length; idx++)
            {
                //마지막 인덱스는 현재 상태 그대로 출력한다.
                if(idx < numbers.Length - 1)
                {
                    if(numbers[idx] - numbers[idx + 1] == -1)
                    {
                        count += 1;
                        continue;
                    }
                }

                GetPage(numbers, sb, count, idx);
                count = 0;
            }

            var split = sb.ToString().TrimEnd().Split(' ');
            var text = string.Join(",", split);

            return text;
        }

        static void GetPage(int[] numbers, StringBuilder sb, int count, int idx)
        {
            var beginPage = numbers[idx] - count;
            var endPage = numbers[idx];

            if(beginPage == endPage)
                sb.AppendFormat("{0} ", endPage);
            else
                sb.AppendFormat("{0}-{1} ", beginPage, endPage);
        }

        public static int[] UnionPages(int[] pages1, int[] pages2)
        {
            var all = pages1.Union(pages2).ToArray();
            return all;
        }

        public static string UnionPagesText(string pages1, string pages2)
        {
            var all = UnionPages(TextToPages(pages1), TextToPages(pages2));
            return PagesToText(all);
        }
    }
    }
