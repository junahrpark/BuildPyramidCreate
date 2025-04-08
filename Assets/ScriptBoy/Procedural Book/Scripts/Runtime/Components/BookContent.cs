using UnityEngine;
using System.Collections.Generic;

namespace ScriptBoy.ProceduralBook
{
    [AddComponentMenu(" Script Boy/Procedural Book/Book Content")]
    public sealed class BookContent : MonoBehaviour
    {
        //[SerializeField] BookOrientation m_Orientation = BookOrientation.Landscape;
        [Tooltip("The reading direction of the book.")]
        [SerializeField] BookDirection m_Direction;



        [SerializeField] Object[] m_Covers = new Object[4];
        [SerializeField] Object[] m_Pages = new Object[8];

        Book m_Book;

        public Book book => m_Book;

        bool isShareable
        {
            get
            {
                foreach (var cover in m_Covers) if (cover && !(cover is Sprite)) return false;
                foreach (var page in m_Pages) if (page && !(page is Sprite)) return false;
                return true;
            }
        }

        internal bool isEmpty => m_Covers.Length == 0 && m_Pages.Length == 0;

        //internal BookOrientation orientation => m_Orientation;

        internal BookDirection direction => m_Direction;

        internal IPageContent[] covers => GetContents(m_Covers, false);
        internal IPageContent[] pages => GetContents(m_Pages, false);

        IPageContent[] GetContents(Object[] contents, bool isCover)
        {
            int n = contents.Length;
            int n2 = Mathf.CeilToInt(n / 4f) * 4;
            if (isCover) n2 = Mathf.Min(n2, 4);
            IPageContent[] interfaces = new IPageContent[n2];
            for (int i = 0; i < n2; i++)
            {
                interfaces[i] = GetContent(i < n ? contents[i] : null);
            }
            return interfaces;
        }

        IPageContent GetContent(Object content)
        {
            if (content != null)
            {
                if (content is Sprite) return new SpritePageContent2(content as Sprite);
                if (content is IPageContent) return content as IPageContent;
            }

            return new SpritePageContent2(null);
        }

        internal void Init(Book book)
        {
            if (!isShareable && m_Book != null && m_Book != book)
            {
                Debug.LogError("The book content is already in use. It can only be assigned to one book.", this);
                m_Book.Clear();
            }

            m_Book = book;

            CheckDuplicatedElements();

            foreach (var cover in covers)
            {
                cover.Init(this);
            }

            foreach (var page in pages)
            {
                page.Init(this);
            }
        }

        void OnValidate()
        {
            Rename(m_Covers, "Cover");
            Rename(m_Pages, "Page");
        }

        void Rename(Object[] objects, string name)
        {
            HashSet<Object> set = new HashSet<Object>();
            foreach (var obj in objects)
            {
                if (obj is PageContent) set.Add(obj);
            }

            List<int> nums = new List<int>();
            int n = objects.Length;
            foreach (var obj in set)
            {
                for (int i = 0; i < n; i++)
                {
                    if (obj == objects[i]) nums.Add(i);
                }

                string numStr = " ";
                for (int i = 0; i < nums.Count; i++)
                {
                    if (i != 0) numStr += "&";
                    numStr += nums[i];
                }

                obj.name = name + numStr;
                nums.Clear();
            }
        }

        void CheckDuplicatedElements()
        {
            foreach (var cover in m_Covers) CheckDuplicatedElement(cover);
            foreach (var page in m_Pages) CheckDuplicatedElement(page);
        }

        void CheckDuplicatedElement(Object element)
        {
            if (element == null) return;
            if (element is Sprite) return;
            if (!(element is PageContent)) return;
            if ((element as PageContent).isShareable) return;

            int count = Count(m_Covers, element) + Count(m_Pages, element);

            if (count > 1)
            {
                throw new DuplicatedElementException(element.name);
            }
        }

        int Count(Object[] objects, Object obj)
        {
            int count = 0;
            foreach (var obj2 in objects)
            {
                if (obj == obj2) count++;
            }
            return count;
        }

        int GetPageContentIndex(PageContent pageContent)
        {
            int n = m_Covers.Length;
            for (int i = 0; i < n; i++)
            {
                if (m_Covers[i] == pageContent) return GetPageIndex(i, true);
            }

            n = m_Pages.Length;
            for (int i = 0; i < n; i++)
            {
                if (m_Pages[i] == pageContent) return GetPageIndex(i, false);
            }

            return -1;
        }

        int GetPageIndex(int i, bool isCoverArray)
        {
            int a = m_Covers.Length;
            int b = m_Pages.Length;

            a = Mathf.CeilToInt(a / 4f) * 4;
            b = Mathf.CeilToInt(b / 4f) * 4;

            if (isCoverArray)
            {
                if (b == 0) return i;
                if (i < a / 2) return i;
                return b + i;
            }

            if (a == 0) return i;
            return a / 2 + i;
        }


        class DuplicatedElementException : System.Exception
        {
            public DuplicatedElementException(string name) : base($"The page content ({name}) is assigned to the book content multiple times.")
            {
            }
        }
    }

    internal enum BookDirection { LeftToRight = 0, RightToLeft = 1, UpToDown = 2, DownToUp = 3 }
}