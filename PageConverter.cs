using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleAppPageListCreater
{

    public interface IPageConverter
    {
        /// <summary>
        /// 페이지수를 입력한다.
        /// </summary>
        /// <param name="count">페이지수</param>
        /// <param name="isPrint">출력여부</param>
        void AddPage(int count, bool isPrint);
        /// <summary>
        /// 페이지수를 입력한다.
        /// </summary>
        /// <param name="count">페이지수</param>
        /// <param name="isPrint">출력여부</param>
        /// <param name="comment">코멘트</param>
        void AddPage(int count, bool isPrint, string comment);
        /// <summary>
        /// 페이지수를 입력한다.
        /// </summary>
        /// <param name="count">페이지수</param>
        /// <param name="isPrint">출력여부</param>
        /// <param name="comment">코멘트</param>
        /// <param name="isInterleave">끼워넣은 백지 페이지 여부(채번안함)</param>
        void AddPage(int count, bool isPrint, string comment, bool isInterleave);
        /// <summary>
        /// 지정한 위치에 페이지를 추가한다.
        /// </summary>
        /// <param name="pageNumber">지정한 페이지 위에 뒤에 끼워넣는다. 1 입력시 0,{여기에 추가된다.},1</param>
        /// <param name="count">페이지수</param>
        /// <param name="isPrint">출력여부</param>
        /// <param name="comment">코멘트</param>
        void InsertPage(int pageNumber, int count, bool isPrint, string comment);
        /// <summary>
        /// 지정한 위치에 페이지를 추가한다.
        /// </summary>
        /// <param name="pageNumber">지정한 페이지 위에 뒤에 끼워넣는다. 1 입력시 0,{여기에 추가된다.},1</param>
        /// <param name="count">페이지수</param>
        /// <param name="isPrint">출력여부</param>
        /// <param name="comment">코멘트</param>
        /// <param name="isInterleave">끼워넣은 백지 페이지 여부(채번안함)</param>
        void InsertPage(int pageNumber, int count, bool isPrint, string comment, bool isInterleave);
        /// <summary>
        /// 출력여부가 True인 항목들
        /// </summary>
        /// <returns></returns>
        string ToString();
        /// <summary>
        /// 출력여부가 True인 항목들의 갯수
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 출력할 페이지가 존재하면 True
        /// </summary>
        bool HasPrint { get; }

        List<PageModel> Pages { get; }

        /// <summary>
        /// 끼워넣기 페이지 목록
        /// </summary>
        int[] InterleavePageNumbers { get; }
        /// <summary>
        /// 끼워넣기 페이지 수
        /// </summary>
        int InterleavePageCount { get; }

        /// <summary>
        /// 논리적인 페이지번호를 물리적인(끼워넣는 페이지 번호 포함된 페이지로 변환)
        /// </summary>
        /// <param name="pages"></param>
        /// <returns></returns>
        int[] GetPhysicalPageNumbers(int[] pages);

    }

    public class PageModel
    {
        /// <summary>
        /// 끼워넣은 페이지 여부 
        /// 끼워넣은 페이지는 항상 출력하지만, 페이지번호와 같은 부가정보는 채번하지 않는다.
        /// 
        /// child page Model을 추가한다. Text만 넣어야 할까? 페이지로 변환되어야 할까?
        /// Q1. 페이지의 숫자는 어떻게 판단할까? 
        /// A. PDF를 만들고 DrawString 이후에 pages의 숫자를 판단해야 한다.
        /// Q2. 끼워넣는 페이지는 언제 채번해야 하는가?
        /// A. 코멘트가 존재하면 PDF로 즉시만들고 PDF 페이지 번호를 1/2 와 같이 출력해야 한다.
        /// Q3. 끼워넣는 페이지는 언제 추가하는가?
        /// A. 페이지가 틀어질 수 있으므로 별도양식, 기기PDF 추가 이후에 페이지를 끼워넣는다.
        /// Q4. PageModel에서 끼워넣는 페이지가 관리되어야 하나?
        /// A. 특이사항이 있다면 기록하지 말고 별도의 List로 관리한다. 하지만 추가하는 페이지는 페이지 채번할 때 페이지 Count에 누적되도록 Pages에 추가한다.
        /// Comment comment = new Comment(){InsertPageNumber = 1, Text = "text"};
        /// 특이사항과 같이 출력을 해야하지만 채번을 생략해야 하는 페이지가 있다면 구분을 위해 필드가 추가되어야 한다.
        /// </summary>
        public bool IsInterleave { get; set; }
        public string Comment { get; set; }
        public int Sequence { get; set; }
        public int PageCount { get; set; }
        public bool IsPrint { get; set; }
    }

    public class PageConverter : IPageConverter
    {
        const int FIRSTPAGENO = 1;
        private List<PageModel> _pageModels;
        private int _index;

        public PageConverter()
        {
            _pageModels = new List<PageModel>();
            _index = 0;
        }

        public int[] InterleavePageNumbers
        {
            get
            {
                List<int> numbers = new List<int>();
                foreach(PageModel page in _pageModels.Where(p => p.IsInterleave))
                {
                    var beginPage = _pageModels.Where(p => p.Sequence < page.Sequence).Sum(f => f.PageCount);
                    var endPage = beginPage + page.PageCount;

                    //페이지 번호(+1)를 더해줘야 페이지번호가 되고 더하지 않으면 Index를 반환한다.
                    var pages = Enumerable.Range(beginPage, page.PageCount);
                    numbers.AddRange(pages.ToArray());
                }

                return numbers.ToArray();
            }
        }

        public int InterleavePageCount { get => _pageModels.Where(p => p.IsInterleave).Sum(f => f.PageCount); }

        public int Count { get => _pageModels.Where(p => p.IsPrint == true).Sum(f => f.PageCount); }
        public bool HasPrint { get => this.Count > 0 ? true : false; }
        public List<PageModel> Pages { get => _pageModels; }

        public void AddPage(int count, bool isPrint)
        {
            foreach(int page in Enumerable.Range(1, count))
                _pageModels.Add(new PageModel { Sequence = _index++, PageCount = 1, IsPrint = isPrint });
        }

        public void AddPage(int count, bool isPrint, string comment)
        {
            foreach(int page in Enumerable.Range(1, count))
                _pageModels.Add(new PageModel { Sequence = _index++, PageCount = 1, IsPrint = isPrint, Comment = comment });
        }

        public void AddPage(int count, bool isPrint, string comment, bool isInterleave)
        {
            if (!isInterleave)
            {
                AddPage(count, isPrint, comment);
                return;
            }

            _pageModels.Add(new PageModel { Sequence = _index++, PageCount = 1, IsPrint = isPrint, Comment = comment, IsInterleave = isInterleave });
        }

        int FindIndexByPageNumber(int pageNumber, bool ignoreInterleave = false)
        {
            int index = -1;

            foreach(PageModel page in _pageModels.OrderBy(o => o.Sequence))
            {
                // 끼워넣기 페이지 무시 옵션을 사용하면,
                if(ignoreInterleave)
                    // 끼워넣은 페이지 무시
                    if(page.IsInterleave)
                        continue;

                if(page.PageCount == 0)
                    continue;

                var beginPage = _pageModels.Where(p => p.Sequence < page.Sequence).Sum(f => f.PageCount);

                if (ignoreInterleave)
                    beginPage = _pageModels.Where(p => p.Sequence < page.Sequence && p.IsInterleave == false).Sum(f => f.PageCount);

                var endPage = beginPage + page.PageCount;

                // 1 -> 0<1<=1,1-4,45-46
                if(beginPage < pageNumber && endPage >= pageNumber)
                    return page.Sequence;
            }


            // NOTE: 페이지 번호가 포함된 인덱스를 찾지 못하면 마지막 인덱스를 반환한다. ?
            // not matched => last page index 반환하자 45 페이지 마지막 인덱스가 45-46일 경우?
            index = _pageModels.OrderBy(o => o.Sequence).LastOrDefault().Sequence;

            return index;
        }

        public void InsertPage(int pageNumber, int count, bool isPrint, string comment)
        {
            // NOTE: 추가 하는 페이지가 마지막이면 마지막 페이지에 추가한다.
            var index = FindIndexByPageNumber(pageNumber);

            // NOTE: targetPageNumber 보다 큰 Sequence 를 가진 페이지를 모두 조정한다.
            _pageModels.Where(m => m.Sequence >= index).ToList().ForEach(
                p => {
                    p.Sequence += 1;
                });
            // 추가하려는 순번 이후에 순번은 모두 증가시킨다.
            _pageModels.Insert(index, new PageModel() { Sequence = index, PageCount = count, Comment = comment });
        }

        public void InsertPage(int pageNumber, int count, bool isPrint, string comment, bool isInterleave)
        {
            if(!isInterleave)
            {
                InsertPage(pageNumber, count, isPrint, comment);
                return;
            }

            // NOTE: 추가 하는 페이지가 마지막이면 마지막 페이지에 추가한다.
            var index = FindIndexByPageNumber(pageNumber, true);

            // NOTE: targetPageNumber 보다 큰 Sequence 를 가진 페이지를 모두 조정한다.
            // 끼워넣는 페이지는 찾은 페이지의 뒷페이지에 입력되어야 하므로 자기보다 큰 페이지의 순서만 조정한다.
            _pageModels.Where(m => m.Sequence > index).ToList().ForEach(
                p => {
                    p.Sequence += 1;
                });

            // NOTE: 끼워넣는 페이지는 찾은 index 뒤로 입력한다.
            _pageModels.Insert(index + 1, new PageModel { Sequence = index + 1, PageCount = count, IsPrint = isPrint, Comment = comment, IsInterleave = isInterleave });
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(PageModel page in _pageModels.OrderBy(o => o.Sequence))
            {
                if(page.PageCount == 0)
                    continue;
                if(!page.IsPrint)
                    continue;

                var beginPage = _pageModels.Where(p => p.Sequence < page.Sequence).Sum(f => f.PageCount);
                var endPage = beginPage + page.PageCount;

                if(page.PageCount == 1)
                    sb.AppendFormat("{0} ", endPage);
                else
                    sb.AppendFormat("{0}-{1} ", beginPage + FIRSTPAGENO, endPage);
            }

            var split = sb.ToString().TrimEnd().Split(' ');
            var text = string.Join(",", split);

            return text;
        }

        public int[] GetPhysicalPageNumbers(int[] pages)
        {
            List<int> returnPages = new List<int>();

            foreach (var no in pages)
            {
                var index = FindIndexByPageNumber(no, true);
                //var physicalPageNo = _pageModels.Where(p => p.Sequence <= index).Sum(f => f.PageCount);

                var beginPage = _pageModels.Where(p => p.Sequence < index).Sum(f => f.PageCount);
                var pageCount = _pageModels.Find(p => p.Sequence == index).PageCount;
                var endPage = beginPage + pageCount;

                var page = beginPage + FIRSTPAGENO;


                //페이지 번호(+1)를 더해줘야 페이지번호가 되고 더하지 않으면 Index를 반환한다.
                //foreach(int page in Enumerable.Range(beginPage + FIRSTPAGENO, pageCount))
                if(!returnPages.Contains(page))
                    returnPages.Add(page);
                else
                    returnPages.Add(endPage);
            }

            return returnPages.ToArray();
        }
    }


}
