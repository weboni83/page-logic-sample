using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppPageListCreater
{
    class Program
    {
        static void Main(string[] args)
        {
            const int FIRSTPAGENO = 1;

            IPageConverter pageConverter = new PageConverter();

            var isSelected = false;

            // item1 : 페이지수,  item2 : 공정,  item3 : 출력여부
            List<Tuple<int, string, bool>> TOCs = new List<Tuple<int, string, bool>>();
            TOCs.Add(new Tuple<int, string, bool>(4, "사용 원액 확인 및 사용량 계산", false));
            TOCs.Add(new Tuple<int, string, bool>(8, "원자재 및 분량 확인", false));
            TOCs.Add(new Tuple<int, string, bool>(6, "제조준비", true));
            TOCs.Add(new Tuple<int, string, bool>(7, "동결건조", false));

            // item1 : 페이지번호,  item2 : 페이지숫자,  item3 : 코멘트
            List<Tuple<int, int, string>> ATTs = new List<Tuple<int, int, string>>();
            ATTs.Add(new Tuple<int, int, string>(3, 2, "원자재 확인"));
            ATTs.Add(new Tuple<int, int, string>(7, 2, "이물검사"));


            // item1 : 페이지번호,  item2 : 페이지숫자,  item3 : 코멘트
            List<Tuple<int, int, string>> ETCs = new List<Tuple<int, int, string>>();
            ETCs.Add(new Tuple<int, int, string>(2, 1, "2 페이지의 특이사항"));
            ETCs.Add(new Tuple<int, int, string>(4, 2, "4 페이지의 특이사항"));
            ETCs.Add(new Tuple<int, int, string>(17, 2, "17 페이지의 특이사항"));

            List<EBRPage> ebrPages = new List<EBRPage>();

            // 1. 공정 데이터 생성
            TOCs.ToList().ForEach(f =>
            {
                foreach(int page in Enumerable.Range(1, f.Item1))
                {
                    var pageNo = FIRSTPAGENO;
                    int max = 0;
                    if(ebrPages.Count > 0)
                        max = ebrPages.Max(m => m.PageNumber);
                    if(max > 0)
                        pageNo += max;

                    ebrPages.Add(new EBRPage() { PageNumber = pageNo, Comment = f.Item2 });

                    pageConverter.AddPage(1, f.Item3, $"공정양식, {f.Item2}");
                }
            });
            
            // 2. 별도 양식 데이터 추가
            ATTs.ToList().ForEach(f =>
            {
                // 삽입할 페이지(self)를 찾아서
                var findedIndex = ebrPages.Where(c => c.PageNumber >= f.Item1).FirstOrDefault();
                if (findedIndex != null)
                {
                    // 별도 양식은 3페이지를 찾아서 찾은 페이지를 포함해서 추가할 페이지수 만큼 번호를 늘리고
                    ebrPages.Where(m => m.PageNumber >= findedIndex.PageNumber).ToList().ForEach(
                    p => {
                        p.PageNumber += f.Item2;
                    });
                }
                // EBR 페이지를 추가한다. (삽입 위치는 Index 이므로 -1 위치에 입력한다. (페이지번호 + (추가할 페이지 수 - 1) - 1))
                foreach(int page in Enumerable.Range(1, f.Item2))
                    ebrPages.Insert(f.Item1 + (page - 1) - 1, new EBRPage() { PageNumber = f.Item1 + page - 1, Comment = $"별도양식, {f.Item3}"});
                // NOTE: 공정 선택 여부를 확인하기 위해 추가한 코드(유효하지 않은 코드임)
                isSelected = pageConverter.Pages.Find(p => p.Sequence >= f.Item1).IsPrint;
                // 별도 양식(중간에 삽입)
                pageConverter.InsertPage(f.Item1, f.Item2, isSelected, $"별도양식, {f.Item3}");
            });

            // 3. 끼워넣기 페이지 생성 (끼워넣기는 역순으로 끼워넣어 페이지 누적페이지 계산을 생략한다.
            ETCs.OrderByDescending(o => o.Item1).ToList().ForEach(f =>
            {
                // 삽입할 페이지(self)를 찾아서
                var findedIndex = ebrPages.Where(c => c.PageNumber >= f.Item1).FirstOrDefault();
                if(findedIndex != null)
                {
                    // 코멘트는 추가할 페이지를 찾아서 다음번호에 할당하므로 찾은 페이지를 제외하고 페이지수를 늘린다.
                    ebrPages.Where(m => m.PageNumber > findedIndex.PageNumber).ToList().ForEach(
                    p => {
                        p.PageNumber += f.Item2;
                    });
                }
                // EBR 페이지를 추가한다.
                foreach(int page in Enumerable.Range(1, f.Item2))
                    ebrPages.Insert(f.Item1 + page - 1, new EBRPage() { PageNumber = f.Item1 + page, Comment = $"특이사항, {f.Item3}" });
                // NOTE: 공정 선택 여부를 확인하기 위해 추가한 코드(유효하지 않은 코드임)
                isSelected = pageConverter.Pages.Find(p => p.Sequence >= f.Item1).IsPrint;
                // 특이사항(중간에 삽입) - 앞 페이지에 따라 뒷페이지 번호가 영향 받기 때문에, 첫페이지 입력후, 뒷 페이지 번호는 앞에 끼워넣은 페이지 번호를 감안해야 한다.
                pageConverter.InsertPage(f.Item1, f.Item2, isSelected, $"특이사항, {f.Item3}", true);
            });
           
            

            Console.WriteLine("BEGIN--------------");
            Console.WriteLine(@"{0} |{1} |{2}", "CNT/PNO".PadRight(7), "Type/Comment".PadRight(20), "IsInterleave".PadRight(10));
            int total = 0;
            foreach(var model in pageConverter.Pages.OrderBy(o => o.Sequence))
            {
                total += model.PageCount;

                if(model.Comment.Contains("특이사항"))
                    Console.ForegroundColor = ConsoleColor.Red;

                if(model.Comment.Contains("별도양식"))
                    Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine($"{model.PageCount:D3}/{total:D3}" +
                    $", {model.Comment}" +
                    $", 끼워넣기:{model.IsInterleave}");

                Console.ResetColor();
            }
            Console.WriteLine("END--------------");


            Console.WriteLine("실제 EBR 페이지 리스트--------------");
            Console.WriteLine("BEGIN--------------");
            Console.WriteLine(@"{0} |{1} |{2} |{3}", "IDX".PadRight(7), "Logical".PadRight(7), "Pysical".PadRight(7), "Comment".PadRight(7));
            int logicalNO = 0;
            foreach(var model in ebrPages)
            {
                if(!pageConverter.InterleavePageNumbers.Contains(ebrPages.IndexOf(model)))
                    logicalNO++;

                if(model.Comment.Contains("특이사항"))
                    Console.ForegroundColor = ConsoleColor.Red;

                if(model.Comment.Contains("별도양식"))
                    Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine($"{ebrPages.FindIndex(p=> p==model):D3}/{ebrPages.Count - 1:D3}" +
                    $", {logicalNO:D3}/{ebrPages.Max(m => m.PageNumber) - pageConverter.InterleavePageCount:D3}" +
                    $", {model.PageNumber:D3}/{ebrPages.Max(m => m.PageNumber):D3}" +
                    $", {model.Comment}");

                Console.ResetColor();
            }
            Console.WriteLine("END--------------");


            Console.WriteLine($"Interleave Pages => {string.Join(",", pageConverter.InterleavePageNumbers)}");

            //Console.WriteLine("BEGIN Print Numbering Simulation--------------");

            //int totalPageNO = ebrPages.Max(m => m.PageNumber) - pageConverter.InterleavePageCount;

            //int pageNO = 1;
            //foreach (var model in ebrPages)
            //{
            //    Console.WriteLine($"Physical Index => {model.PageNumber}/{ebrPages.Count:D3}");

            //    if(pageConverter.InterleavePageNumbers.Contains(model.PageNumber))
            //        continue;

            //    string pageString = string.Format("{0}/{1}", pageNO++, totalPageNO);

            //    if(pageString != string.Empty)
            //        Console.WriteLine(pageString);


            //}
            //Console.WriteLine("END--------------");

            //Console.WriteLine("BEGIN Print Numbering Simulation--------------");
            //sbjeon20220818_MES2022(4.3) 특이 사항 발생 시 해당 공정에 내용 작성, EBR 발행 시 내용 기재
            Console.WriteLine("입력한 페이지(logical) -> 출력가능한 페이지(Physical) 로 변환하여 출력 시뮬레이션");
            Console.WriteLine("BEGIN--------------");
            // 출력 시물레이션 (입력한 페이지번호가 출력되는지 검증)
            string pageInfo = @"1-2,5,7,9,16-18";
            Console.WriteLine($"Input Logical Pages : {pageInfo}");
            var pages = PrintHelper.TextToPages(pageInfo);
            string physicalPage = "";
            physicalPage = PrintHelper.PagesToText(pageConverter.GetPhysicalPageNumbers(pages));
            Console.WriteLine($"Convert Logical to Physical PageNumber : {physicalPage}");

            if(pageConverter.HasPrint)
            {
                physicalPage = PrintHelper.UnionPagesText(physicalPage, pageConverter.ToString());
                Console.WriteLine($"Convert With Merge Print Pages : {physicalPage}");
            }
            int[] printPage = PrintHelper.TextToPages(physicalPage);
            List<string> printedPages = new List<string>();
            for(int i = 0; i < ebrPages.Max(m => m.PageNumber); ++i)
            {
                if(printPage.Contains(i + FIRSTPAGENO))
                {
                    var comment = string.Empty;
                    if (ebrPages.Exists(f => f.PageNumber == i + FIRSTPAGENO))
                        comment = ebrPages.Find(f => f.PageNumber == i + FIRSTPAGENO).Comment;
                    printedPages.Add($"Print Page => {comment}, {i + FIRSTPAGENO}/{ebrPages.Count}");
                    continue;
                }
            }

            printedPages.ForEach(p => Console.WriteLine(p));

            Console.WriteLine("END--------------");

            Console.ReadKey();
        }

        public class EBRPage
        {
            public EBRPage()
            {
                Remarks = new List<Remark>();
            }
            public int PageNumber { get; set; }
            /// <summary>
            /// 특이사항이 있을 때 유효한 필드
            /// </summary>
            public int SUB_ID { get; set; }
            public string Comment { get; set; }

            public virtual List<Remark> Remarks { get; set; }
        }

        public class Remark
        {
            public int REMARK_ID { get; set; }
            public string REMRAK_TEXT { get; set; }
        }

    }
}
