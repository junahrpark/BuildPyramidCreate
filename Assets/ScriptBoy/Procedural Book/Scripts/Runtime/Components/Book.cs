using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace ScriptBoy.ProceduralBook
{
    [AddComponentMenu(" Script Boy/Procedural Book/ Book")]
    [ExecuteInEditMode]
    public sealed class Book : MonoBehaviour
    {
        static HashSet<Book> s_Instances = new HashSet<Book>();

        /// <summary>
        /// Returns an array of Book objects.
        /// </summary>
        public static Book[] instances
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return FindObjectsOfType<Book>();
#endif
                return s_Instances.ToArray();
            }
        }

        [Tooltip("The book content object contains pages and covers data.")]
        [SerializeField] BookContent m_Content;

        [Tooltip("The book binding object binds papers.")]
        [SerializeField] BookBinding m_Binding;

        [Tooltip("The start open state of the book (0 = closed on the front cover, 1 = closed on the back cover, values between 0 & 1 open from the beginning to the end of the book).")]
        [SerializeField, Range(0, 1)] float m_StartState;

        [Tooltip("Should the book cast shadows?")]
        [SerializeField] bool m_CastShadows = true;

        [Tooltip("Align the book to the local XZ plane, which is considered the ground.")]
        [SerializeField] bool m_AlignToGround;

        [Tooltip("Toggle the visibility of the binder (e.g., staples or wire)")]
        [SerializeField] bool m_HideBinder;

        [SerializeField] PaperSetup m_CoverPaperSetup = new PaperSetup() { color = Color.red, width = 2.1f, height = 3.1f, thickness = 0.04f, stiffness = 0.5f };
        [SerializeField] PaperSetup m_PagePaperSetup = new PaperSetup() { color = Color.white, width = 2f, height = 3f, thickness = 0.02f, stiffness = 0.2f };
        [NonSerialized] Transform m_Root;
        [NonSerialized] bool m_IsBuilt;
        bool m_HasCover;
        RendererFactory m_RendererFactory;
        MeshFactory m_MeshFactory;

        BookBound m_Bound;
        Paper[] m_Papers;
        Paper m_SelectedPaper;

        BookDirection m_Direction;

        Coroutine m_AutoTurnCoroutine;
        float m_AutoTurningEndTime = -1;

        float m_TotalThickness;
        float m_MinPaperWidth;
        float m_MinPaperHeight;
        float m_MinPaperThickness;
        float m_MaxPaperThickness;

        int[] m_RendererIds = new int[0];

        [NonSerialized] bool m_WasIdle;


        internal float minPaperWidth => m_MinPaperWidth;
        internal float minPaperHeight => m_MinPaperHeight;
        internal float minPaperThickness => m_MinPaperThickness;
        internal float maxPaperThickness => m_MaxPaperThickness;
        internal float totalThickness => m_TotalThickness;

        internal bool hasCover => m_HasCover;

        internal bool castShadows => m_CastShadows;

        internal bool alignToGround => m_AlignToGround;

        internal PaperSetup coverPaperSetup => m_CoverPaperSetup;
        internal PaperSetup pagePaperSetup => m_PagePaperSetup;


        internal BookBound bound => m_Bound;

        internal Paper[] papers => m_Papers;

        public int[] rendererIds => m_RendererIds;

        internal BookDirection direction => m_Direction;


        public BookBinding binding
        {
            get => m_Binding;
            set
            {
                m_Binding = value;
                Clear();
            }
        }

        public BookContent content
        {
            get => m_Content;
            set
            {
                m_Content = value;
                Clear();
            }
        }

        /// <summary>
        /// Indicates whether any page is currently turning (not including auto turning).
        /// </summary>
        public bool isTurning => m_SelectedPaper != null && m_SelectedPaper.isTurning;

        /// <summary>
        /// Indicates whether any page is currently falling.
        /// </summary>
        public bool isFalling
        {
            get
            {
                foreach (var paper in m_Papers) if (paper.isFalling) return true;
                return false;
            }
        }
       
        /// <summary>
        /// Indicates whether all pages are idle (no turning, no auto turning, and no falling).
        /// </summary>
        public bool isIdle => !isTurning && !isFalling && !isAutoTurning;

        /// <summary>
        /// Indicates whether any page is currently auto turning.
        /// </summary>
        public bool isAutoTurning => m_AutoTurningEndTime > Time.time;

        /// <summary>
        /// Indicates if there are pending auto turns that have not started yet.
        /// </summary>
        public bool hasPendingAutoTurns => m_AutoTurnCoroutine != null;

        /// <summary>
        /// Gets the time at which the auto page turning animation will end.
        /// </summary>
        public float autoTurningEndTime => m_AutoTurningEndTime;

        /// <summary>
        /// Starts the page turning animation if the specified ray hits the page. (Returns true if it does not fail.)
        /// </summary>
        /// <param name="ray">The ray used to determine the interaction with the page.</param>
        public bool StartTurning(Ray ray)
        {
            if (!m_IsBuilt) return false;
            if (isTurning) return false;

            List<Paper> frontPapers = GetFrontPapers();

            foreach (var paper in frontPapers)
            {
                if (paper.Raycast(ray, out BookRaycastHit hit))
                {
                    if (hit.pageContent.IsPointOverUI(hit.textureCoordinate)) return false;
                }
            }

            foreach (var paper in frontPapers)
            {
                if (!paper.isFalling && paper.StartTurning(ray))
                {
                    m_SelectedPaper = paper;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the turning animation based on the specified ray.
        /// </summary>
        /// <param name="ray">The ray used to update the animation state.</param>
        public void UpdateTurning(Ray ray)
        {
            if (!m_IsBuilt) return;
            if (m_SelectedPaper == null) return;
            m_SelectedPaper.UpdateTurning(ray);
        }

        /// <summary>
        /// Stops the page turning animation.
        /// </summary>
        public void StopTurning()
        {
            if (!m_IsBuilt) return;
            if (m_SelectedPaper == null) return;

            m_SelectedPaper.StopTurning();
            m_SelectedPaper = null;
        }


        /// <summary>
        /// Retrieves a list of indices of active pages.
        /// </summary>
        /// <param name="indices">The list to populate with active page indices.</param>
        public void GetActivePageIndices(List<int> indices)
        {
            bool reverse = m_Content.direction == BookDirection.RightToLeft;
            indices.Clear();
            int n = m_Papers.Length;
            for (int i = 0; i < n; i++)
            {
                var paper = m_Papers[i];
                if (paper.isFalling || paper.isTurning)
                {
                    if (i > 0) Add(i - 1, true);
                    Add(i, false);
                    Add(i, true);
                    if (i < n - 1) Add(i + 1, false);
                }
                else if (paper.isOnRightStack)
                {
                    if (i > 0) Add(i - 1, true);
                    Add(i, false);
                    break;
                }
            }

            if (indices.Count == 0)
            {
                Add(m_Papers.Length - 1, true);
            }

            if (reverse) indices.Reverse();

            void Add(int paperIndex, bool isBackPage)
            {
                int pageIndex = paperIndex * 2;
                if (isBackPage) pageIndex++;
                if (reverse) pageIndex = n * 2 - pageIndex - 1;
                if (!indices.Contains(pageIndex)) indices.Add(pageIndex);
            }
        }


        /// <summary>
        /// Starts the auto page turning animation. (Returns true if it does not fail.)
        /// </summary>
        public bool StartAutoTurning(AutoTurnDirection direction, AutoTurnSettings settings)
        {
            return StartAutoTurning(direction, settings, 1, 0);
        }

        /// <summary>
        /// Starts the auto page turning animation. (Returns true if it does not fail.)
        /// </summary>
        public bool StartAutoTurning(AutoTurnDirection direction, AutoTurnSettings settings, int turnCount, float delyPerTurn)
        {
            return StartAutoTurning(direction, settings, turnCount, new AutoTurnSetting(delyPerTurn));
        }

        /// <summary>
        /// Starts the auto page turning animation. (Returns true if it does not fail.)
        /// </summary>
        public bool StartAutoTurning(AutoTurnDirection direction, AutoTurnSettings settings, int turnCount, AutoTurnSetting delyPerTurn)
        {
            if (m_AutoTurnCoroutine != null)
            {
                CancelPendingAutoTurns();
            }

            turnCount = Mathf.Min(turnCount, GetMaxAutoTurnCount(direction));
            if (turnCount == 0) return false;
            m_AutoTurnCoroutine = StartCoroutine(CreateAutoTurningCoroutine(direction, turnCount, delyPerTurn, settings));
            return true;
        }

        /// <summary>
        /// Cancels any pending auto turns that have not started yet.
        /// </summary>
        public void CancelPendingAutoTurns()
        {
            if (m_AutoTurnCoroutine != null)
            {
                StopCoroutine(m_AutoTurnCoroutine);
                m_AutoTurnCoroutine = null;
            }
        }


        IEnumerator CreateAutoTurningCoroutine(AutoTurnDirection direction, int turnCount, AutoTurnSetting delyPerTurn, AutoTurnSettings settings)
        {
            for (int i = 0; i < turnCount; i++)
            {
                if (!CanAutoTurn(direction)) break;
                float turnIndexTime = i / (turnCount - 1f);
                float paperIndexTime = GetAutoTurnPaperIndexTime(direction);
                float dely = delyPerTurn.GetValue(paperIndexTime, turnIndexTime);
                if (i > 0) yield return new WaitForSeconds(dely);
                AutoTurnMode mode = settings.GetModeValue();
                float twist = settings.GetTwistValue(paperIndexTime, turnIndexTime);
                float bend = settings.GetBendValue(paperIndexTime, turnIndexTime);
                float duration = settings.GetDurationValue(paperIndexTime, turnIndexTime);
                if (!StartAutoTurning(direction, mode, twist, bend, duration)) break;
            }
            m_AutoTurnCoroutine = null;
        }

        bool StartAutoTurning(AutoTurnDirection direction, AutoTurnMode mode, float twist, float bend, float duration)
        {
            Paper paper = GetAutoTurnPaper(direction);
            if (paper == null) return false;
            paper.StartAutoTurning(mode, twist, bend, duration);
            m_AutoTurningEndTime = Mathf.Max(m_AutoTurningEndTime, Time.time + duration);
            return true;
        }

        Paper GetAutoTurnPaper(AutoTurnDirection direction)
        {
            if (isTurning) return null;

            List<Paper> papers = GetFrontPapers();

            if (papers.Count > 0)
            {
                bool next = direction == AutoTurnDirection.Next;
                Paper paper = papers[next ? papers.Count - 1 : 0];
                if (!paper.isTurning && !paper.isFalling && next == paper.isOnRightStack)
                {
                    return paper;
                }
            }

            return null;
        }

        int GetMaxAutoTurnCount(AutoTurnDirection direction)
        {
            Paper paper = GetAutoTurnPaper(direction);
            if (paper == null) return 0;

            int count = 1;
            if (direction == AutoTurnDirection.Next)
            {
                count += m_Papers.Length - paper.index - 1;
            }
            else
            {
                count += paper.index;
            }
            return count;
        }

        bool CanAutoTurn(AutoTurnDirection direction)
        {
            return GetAutoTurnPaper(direction) != null;
        }

        float GetAutoTurnPaperIndexTime(AutoTurnDirection direction)
        {
            Paper paper = GetAutoTurnPaper(direction);
            if (paper == null) return 0;
            return paper.index / (m_Papers.Length - 1f);
        }



        void Start()
        {
            s_Instances.Add(this);
            HardClear();
            Build();
        }

        void OnDestroy()
        {
            s_Instances.Remove(this);
            HardClear();
        }

        void Reset()
        {
            HardClear();
        }

        void LateUpdate()
        {
            if (!m_IsBuilt) return;
            if (m_Papers == null) return;
            if (m_Bound == null) return;

            if (m_WasIdle && isIdle) return;
            m_WasIdle = isIdle;

            foreach (var paper in m_Papers)
            {
                if (paper.isFalling)
                {
                    paper.UpdateFalling();
                }
            }

            UpdateLivePages();

            m_Bound.OnLateUpdate();
        }

        void UpdateLivePages()
        {
            HashSet<IPageContent> activePages = new HashSet<IPageContent>();

            int n = m_Papers.Length;
            for (int i = 0; i < n; i++)
            {
                var paper = m_Papers[i];
                if (paper.isFalling || paper.isTurning)
                {
                    if (i > 0)
                    {
                        activePages.Add(m_Papers[i - 1].backContent);
                    }
                    activePages.Add(paper.frontContent);
                    activePages.Add(paper.backContent);
                    if (i < n - 1)
                    {
                        activePages.Add(m_Papers[i + 1].frontContent);
                    }
                }
                else if (paper.isOnRightStack)
                {
                    if (i > 0)
                    {
                        activePages.Add(m_Papers[i - 1].backContent);
                    }
                    activePages.Add(paper.frontContent);
                    break;
                }
            }

            if (activePages.Count == 0)
            {
                activePages.Add(m_Papers[m_Papers.Length - 1].backContent);
            }

            foreach (var paper in m_Papers)
            {
                paper.frontContent.SetActive(activePages.Contains(paper.frontContent));
                paper.backContent.SetActive(activePages.Contains(paper.backContent));
            }
        }

        public void Build()
        {
            if (this == null) return;
            Clear();
            if (m_Content == null || m_Content.isEmpty) return;
            if (m_Binding == null) return;
            if (m_MeshFactory == null) m_MeshFactory = new MeshFactory();

            if (m_RendererFactory == null)
            {
                GameObject root = new GameObject("Root");
                root.hideFlags = HideFlags.HideAndDontSave;
                root.transform.SetParent(transform, false);
                m_Root = root.transform;
                m_RendererFactory = new RendererFactory(m_Root);
            }

            m_Direction = m_Content.direction;

            float y = (int)m_Direction > 1 ? 90 : 0;
            m_Root.localEulerAngles = new Vector3(0, y, 0);


            m_CoverPaperSetup.bookDirection = m_Content.direction;
            m_PagePaperSetup.bookDirection = m_Content.direction;

            if (m_PagePaperSetup.height < m_CoverPaperSetup.height)
            {
                m_CoverPaperSetup.margin = 0;
                m_PagePaperSetup.margin = (m_CoverPaperSetup.height - m_PagePaperSetup.height) / 2;
            }
            else
            {
                m_CoverPaperSetup.margin = (m_PagePaperSetup.height - m_CoverPaperSetup.height) / 2;
                m_PagePaperSetup.margin = 0;
            }

            Material coverMaterial = MaterialUtility.FixPaperMaterial(m_CoverPaperSetup.material);
            Material paperMaterial = MaterialUtility.FixPaperMaterial(m_PagePaperSetup.material);
            Material[] coverMaterials = MaterialUtility.CreateArray(coverMaterial, 3);
            Material[] paperMaterials = MaterialUtility.CreateArray(paperMaterial, 3);
            PaperMaterialData coverMaterialData = new PaperMaterialData(coverMaterial, m_CoverPaperSetup.color);
            PaperMaterialData paperMaterialData = new PaperMaterialData(paperMaterial, m_PagePaperSetup.color);

            m_Content.Init(this);

            IPageContent[] covers = m_Content.covers;
            IPageContent[] pages = m_Content.pages;

            float openState = m_StartState;

            if ((int)m_Content.direction % 2 != 0)
            {
                Array.Reverse(covers);
                Array.Reverse(pages);
                openState = 1 - openState;
            }

            m_HasCover = covers.Length > 0;

            int paperCount = covers.Length / 2 + pages.Length / 2;

            int halfCoverPaperCount = covers.Length / 4;

            int pageIndex = 0;
            int coverIndex = 0;
            float totalThickness = 0;
            m_Papers = new Paper[paperCount];
            for (int i = 0; i < paperCount; i++)
            {
                bool isCover = (m_HasCover && (i < halfCoverPaperCount || i >= paperCount - halfCoverPaperCount));

                Paper paper = m_Papers[i] = new Paper(isCover, i, this, m_RendererFactory);
                paper.renderer.castShadows = m_CastShadows;

                if (i < Mathf.FloorToInt(Mathf.Lerp(0, paperCount, openState)))
                {
                    paper.transform.localScale = new Vector3(-1, 1, 1);
                    paper.SetTime(1);
                }

                if (isCover)
                {
                    paper.SetContentData(covers[coverIndex++], covers[coverIndex++], i > halfCoverPaperCount);
                    paper.renderer.SetMaterials(coverMaterials);
                    paper.SetMaterialData(coverMaterialData);
                    paper.SetPaperSetup(m_CoverPaperSetup);
                }
                else
                {
                    paper.SetContentData(pages[pageIndex++], pages[pageIndex++]);
                    paper.renderer.SetMaterials(paperMaterials);
                    paper.SetMaterialData(paperMaterialData);
                    paper.SetPaperSetup(m_PagePaperSetup);
                }

                totalThickness += paper.thickness;
            }

            m_TotalThickness = totalThickness;

            float thickness0 = m_Papers[0].thickness;
            float thickness1 = m_Papers[paperCount / 2].thickness;
            m_MinPaperThickness = Mathf.Min(thickness0, thickness1);
            m_MaxPaperThickness = Mathf.Max(thickness0, thickness1);
            float width0 = m_Papers[0].size.x;
            float width1 = m_Papers[paperCount / 2].size.x;
            m_MinPaperWidth = Mathf.Min(width0, width1);
            float height0 = m_Papers[0].size.y;
            float height1 = m_Papers[paperCount / 2].size.y;
            m_MinPaperHeight = Mathf.Min(height0, height1);


            m_Bound = m_Binding.CreateBound(this, m_Root, m_RendererFactory, m_MeshFactory);
            m_Bound.binderRenderer.SetVisibility(!m_HideBinder);

            PaperMeshDataPool paperLowpolyMeshDataPool = CreatePaperMeshDataPool(m_PagePaperSetup, true);
            PaperMeshDataPool paperHighpolyMeshDataPool = CreatePaperMeshDataPool(m_PagePaperSetup, false);

            PaperMeshDataPool coverLowpolyMeshDataPool = CreatePaperMeshDataPool(m_CoverPaperSetup, true);
            PaperMeshDataPool coverHighpolyMeshDataPool = CreatePaperMeshDataPool(m_CoverPaperSetup, false);

            for (int i = 0; i < paperCount; i++)
            {
                Paper paper = m_Papers[i];

                if (paper.isCover)
                {
                    paper.SetMeshData(coverLowpolyMeshDataPool.Get(), coverHighpolyMeshDataPool);
                }
                else
                {
                    paper.SetMeshData(paperLowpolyMeshDataPool.Get(), paperHighpolyMeshDataPool);
                }
            }

            m_IsBuilt = true;

            m_RendererIds = m_RendererFactory.ids;

            LateUpdate();

            m_CoverPaperSetup.bookDirection = BookDirection.LeftToRight;
            m_PagePaperSetup.bookDirection = BookDirection.LeftToRight;
        }

        internal void Clear()
        {
            m_IsBuilt = false;
            m_WasIdle = false;
            m_Bound = null;

            if (m_Root == null)
            {
                var children = GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (child != null && child.name == "Root") ObjectUtility.Destroy(child.gameObject);
                }
            }

            if (m_MeshFactory != null) m_MeshFactory.Recycle();
            if (m_RendererFactory != null) m_RendererFactory.Recycle();
        }

        void HardClear()
        {
            m_IsBuilt = false;
            m_WasIdle = false;
            m_Bound = null;

            if (m_Root == null)
            {
                var children = GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (child != null && child.name == "Root") ObjectUtility.Destroy(child.gameObject);
                }
            }
            else
            {
                ObjectUtility.Destroy(m_Root.gameObject);
            }

            if (m_MeshFactory != null)
            {
                m_MeshFactory.Destroy();
                m_MeshFactory = null;
            }

            if (m_RendererFactory != null)
            {
                m_RendererFactory.Destroy();
                m_RendererFactory = null;
            }
        }

        PaperMeshDataPool CreatePaperMeshDataPool(PaperSetup setup, bool lowpoly)
        {
            int quality = lowpoly ? 0 : setup.quality;
            PaperPattern pattern = m_Bound.CreatePaperPattern(quality, setup.size, setup.thickness, setup.uvMargin);
            PaperMeshDataPool pool = new PaperMeshDataPool(m_MeshFactory, pattern, lowpoly && m_Bound.useSharedMeshDataForLowpoly);
            return pool;
        }

        List<Paper> GetFrontPapers()
        {
            List<Paper> frontPapers = new List<Paper>();

            if (m_SelectedPaper != null)
            {
                int i = m_SelectedPaper.index;
                if (i > 0) frontPapers.Add(m_Papers[i - 1]);
                frontPapers.Add(m_SelectedPaper);
                if (i < m_Papers.Length - 1) frontPapers.Add(m_Papers[i + 1]);
            }
            else
            {
                for (int i = 0; i < m_Papers.Length; i++)
                {
                    if (m_Papers[i].isOnRightStack)
                    {
                        if (i > 0) frontPapers.Add(m_Papers[i - 1]);
                        frontPapers.Add(m_Papers[i]);
                        break;
                    }
                }

                if (frontPapers.Count == 0)
                {
                    frontPapers.Add(m_Papers[m_Papers.Length - 1]);
                }
            }

            return frontPapers;
        }

        internal float GetAverageTime()
        {
            int paperCount = m_Papers.Length;
            float time = 0;
            foreach (var pp in m_Papers)
            {
                pp.UpdateTime();
                time += pp.zTime;
            }
            time /= paperCount;

            return time;
        }

        internal Vector2 GetTextureCoordinate(Ray ray, LivePageContent livePageContent)
        {
            IPageContent iPageContent = livePageContent;
            foreach (var paper in m_Papers)
            {
                if (paper.backContent == iPageContent || paper.frontContent == iPageContent)
                {
                    return paper.GetTextureCoordinate(ray, livePageContent);
                }
            }

            return Vector2.zero;
        }

        internal bool Raycast(Ray ray, out BookRaycastHit hitInfo)
        {
            List<Paper> frontPapers = GetFrontPapers();
            foreach (var paper in frontPapers)
            {
                if (paper.Raycast(ray, out hitInfo)) return true;
            }
            hitInfo = new BookRaycastHit();
            return false;
        }

#if UNITY_EDITOR
        Ping m_Ping;

        void PingPage(int pageIndex)
        {
            if (Application.isPlaying) return;
            if (m_Papers == null) return;

            int n = m_Papers.Length;
            if (m_Content.direction == BookDirection.RightToLeft)
                pageIndex = n * 2 - pageIndex - 1;

            int paperIndex = Mathf.CeilToInt(pageIndex / 2f);
            for (int i = 0; i < n; i++)
            {
                Paper paper = m_Papers[i];
                if (i < paperIndex)
                {
                    paper.transform.localScale = new Vector3(-1, 1, 1);
                    paper.SetTime(1);
                }
                else
                {
                    paper.transform.localScale = new Vector3(1, 1, 1);
                    paper.SetTime(0);
                }

                paper.UpdateMaterials();
            }

            foreach (var paper in m_Papers)
            {
                m_Bound.UpdatePaperPosition(paper);
            }

            LateUpdate();

            paperIndex = Mathf.FloorToInt(pageIndex / 2f);
            var renderer = m_Papers[paperIndex].renderer;
            m_Ping = new Ping(m_Papers[paperIndex]);
        }

        void OnDrawGizmos()
        {
            if (!m_IsBuilt) return;

            m_RendererFactory.DrawGizmos(new Color(0, 0, 0, 0));
            m_Ping.Draw();

            if (m_HideBinder && m_Bound != null)
            {
                if (UnityEditor.Selection.Contains(gameObject) || 
                    m_Binding != null && UnityEditor.Selection.Contains(m_Binding.gameObject))
                {
                    m_Bound.binderRenderer.DrawGizmoMesh(new Color(0, 0, 0, 0.1f));
                }
            }
        }

        struct Ping
        {
            Paper m_Paper;
            float m_Alpha;

            public Ping(Paper paper)
            {
                m_Paper = paper;
                m_Alpha = 1;
            }

            public void Draw()
            {
                if (m_Alpha <= 0) return;
                m_Paper.DrawWireframe(new Color(1, 0.1f, 0.1f, m_Alpha));
                m_Alpha -= 0.02f;
                UnityEditor.SceneView.RepaintAll();
            }
        }
#endif
    }


    /// <summary>
    /// Defines the direction for auto page turning.
    /// </summary>
    public enum AutoTurnDirection
    {
        /// <summary>
        /// Indicates the next page direction.
        /// </summary>
        Next,

        /// <summary>
        /// Indicates the previous page direction.
        /// </summary>
        Back
    }

    /// <summary>
    /// Defines the mode for auto page turning.
    /// </summary>
    public enum AutoTurnMode
    {
        /// <summary>
        /// This mode simulates swiping the paper surface to turn it.
        /// </summary>
        Surface,

        /// <summary>
        /// This mode simulates holding the paper edge and turning it.
        /// </summary>
        Edge
    }

    /// <summary>
    /// Represents settings for auto page turning. 
    /// </summary>
    [Serializable]
    public class AutoTurnSettings
    {
        internal const float kMinTwist = -1;
        internal const float kMaxTwist = 1;
        internal const float kMinBend = 0;
        internal const float kMaxBend = 1;
        internal const float kMinDuration = 0;
        internal const float kMaxDuration = 5;

        [Tooltip("Choose the mode of auto page turning:\n\n-Surface: This mode simulates swiping the paper surface to turn it.\n\nEdge: This mode simulates holding the paper edge and turning it.")]
        [SerializeField] AutoTurnMode m_Mode;

        [SerializeField, AutoTurnSettingRange(kMinTwist, kMaxTwist)] AutoTurnSetting m_Twist;
        [SerializeField, AutoTurnSettingRange(kMinBend, kMaxBend)] AutoTurnSetting m_Bend = new AutoTurnSetting(1);
        [SerializeField, AutoTurnSettingRange(kMinDuration, kMaxDuration)] AutoTurnSetting m_Duration = new AutoTurnSetting(0.5f);


        public AutoTurnMode mode
        {
            get => m_Mode;
            set => m_Mode = value;
        }

        public AutoTurnSetting twist
        {
            get => m_Twist;
            set => m_Twist = value.Clamp(kMinTwist, kMaxTwist);
        }

        public AutoTurnSetting bend
        {
            get => m_Bend;
            set => m_Bend = value.Clamp(kMinBend, kMaxBend);
        }

        public AutoTurnSetting duration
        {
            get => m_Duration;
            set => m_Duration = value.Clamp(kMinDuration, kMaxDuration);
        }

        internal AutoTurnMode GetModeValue()
        {
            return m_Mode;
        }

        internal float GetBendValue(float paperIndexTime, float turnIndexTime)
        {
            return Mathf.Clamp(m_Bend.GetValue(paperIndexTime, turnIndexTime), kMinBend, kMaxBend);
        }

        internal float GetDurationValue(float paperIndexTime, float turnIndexTime)
        {
            return Mathf.Clamp(m_Duration.GetValue(paperIndexTime, turnIndexTime), kMinDuration, kMaxDuration);
        }

        internal float GetTwistValue(float paperIndexTime, float turnIndexTime)
        {
            return Mathf.Clamp(m_Twist.GetValue(paperIndexTime, turnIndexTime), kMinTwist, kMaxTwist);
        }
    }


    /// <summary>
    /// Represents an individual setting for auto page turning.
    /// </summary>
    [Serializable]
    public struct AutoTurnSetting
    {
        [SerializeField] AutoTurnSettingMode m_Mode;
        [SerializeField] float m_Constant;
        [SerializeField] float m_ConstantMin;
        [SerializeField] float m_ConstantMax;
        [SerializeField] AnimationCurve m_Curve;
        [SerializeField] AnimationCurve m_CurveMin;
        [SerializeField] AnimationCurve m_CurveMax;
        [SerializeField] AutoTurnSettingCurveTimeMode m_CurveTimeMode;

        public AutoTurnSettingMode mode
        {
            get => m_Mode;
            set => m_Mode = value;
        }

        public float constant
        {
            get => m_Constant;
            set => m_Constant = value;
        }

        public float constantMin
        {
            get => m_ConstantMin;
            set => m_ConstantMin = value;
        }

        public AnimationCurve curve
        {
            get => m_Curve;
            set => m_Curve = value;
        }

        public AnimationCurve curveMin
        {
            get => m_CurveMin;
            set => m_CurveMin = value;
        }

        public AnimationCurve curveMax
        {
            get => m_CurveMax;
            set => m_CurveMax = value;
        }

        public AutoTurnSettingCurveTimeMode curveTimeMode
        {
            get => m_CurveTimeMode;
            set => m_CurveTimeMode = value;
        }

        public float constantMax
        {
            get => m_ConstantMax;
            set => m_ConstantMax = value;
        }

        /// <summary>
        /// A constant value.
        /// </summary>
        public AutoTurnSetting(float constant)
        {
            m_Constant = constant;
            m_ConstantMin = constant;
            m_ConstantMax = constant;
            m_Curve = null;
            m_CurveMin = null;
            m_CurveMax = null;
            m_Mode = AutoTurnSettingMode.Constant;
            m_CurveTimeMode = AutoTurnSettingCurveTimeMode.PaperIndexTime;
        }

        /// <summary>
        /// A random value generated between two constant values
        /// </summary>
        public AutoTurnSetting(float constantMin, float constantMax)
        {
            m_Constant = 0;
            m_ConstantMin = constantMin;
            m_ConstantMax = constantMax;
            m_Curve = null;
            m_CurveMin = null;
            m_CurveMax = null;
            m_Mode = AutoTurnSettingMode.RandomBetweenTwoConstants;
            m_CurveTimeMode = AutoTurnSettingCurveTimeMode.PaperIndexTime;
        }

        /// <summary>
        /// A value based on a curve.
        /// </summary>
        public AutoTurnSetting(AnimationCurve curve, AutoTurnSettingCurveTimeMode curveTimeMode)
        {
            m_Constant = 0;
            m_ConstantMin = 0;
            m_ConstantMax = 0;
            m_Curve = curve;
            m_CurveMin = null;
            m_CurveMax = null;
            m_Mode = AutoTurnSettingMode.Curve;
            m_CurveTimeMode = curveTimeMode;
        }

        /// <summary>
        /// A random value generated between two curves.
        /// </summary>
        public AutoTurnSetting(AnimationCurve curveMin, AnimationCurve curveMax, AutoTurnSettingCurveTimeMode curveTimeMode)
        {
            m_Constant = 0;
            m_ConstantMin = 0;
            m_ConstantMax = 0;
            m_Curve = null;
            m_CurveMin = curveMin;
            m_CurveMax = curveMax;
            m_Mode = AutoTurnSettingMode.RandomBetweenTwoCurves;
            m_CurveTimeMode = curveTimeMode;
        }

        internal float GetValue(float paperIndexTime, float turnIndexTime)
        {
            if (m_Mode == AutoTurnSettingMode.Constant) return m_Constant;
            if (m_Mode == AutoTurnSettingMode.RandomBetweenTwoConstants) return Random.Range(m_ConstantMin, m_ConstantMax);

            float time = m_CurveTimeMode == AutoTurnSettingCurveTimeMode.PaperIndexTime ? paperIndexTime : turnIndexTime;

            if (m_Mode == AutoTurnSettingMode.Curve) return m_Curve.Evaluate(time);
            if (m_Mode == AutoTurnSettingMode.RandomBetweenTwoCurves) return Random.Range(m_CurveMin.Evaluate(time), m_CurveMax.Evaluate(time));

            throw new NotImplementedException(); 
        }

        internal AutoTurnSetting Clamp(float min, float max)
        {
            m_Constant = Mathf.Clamp(m_Constant, min, max);
            m_ConstantMin = Mathf.Clamp(m_ConstantMin, min, max);
            m_ConstantMax = Mathf.Clamp(m_ConstantMax, min, max);
            m_Curve = ClampCurve(m_Curve, min, max);
            m_CurveMin = ClampCurve(m_CurveMin, min, max);
            m_CurveMax = ClampCurve(m_CurveMax, min, max);
            return this;
        }

        AnimationCurve ClampCurve(AnimationCurve curve, float min, float max)
        {
            if (m_Curve == null) return null;
            Keyframe[] keys = curve.keys;
            int n = keys.Length;
            float minTime = float.PositiveInfinity;
            float maxTime = float.NegativeInfinity;
            for (int i = 0; i < n; i++)
            {
                Keyframe key = keys[i];
                float time = key.time;
                minTime = Mathf.Min(minTime, time);
                maxTime = Mathf.Max(maxTime, time);
            }

            for (int i = 0; i < n; i++)
            {
                Keyframe key = keys[i];
                float time = key.time;
                float value = key.value;

                time = Mathf.InverseLerp(minTime, maxTime, time);
                value = Mathf.Clamp(value, min, max);

                key.time = time;
                key.value = value;
                keys[i] = key;
            }

            return new AnimationCurve(keys);
        }
    }

    /// <summary>
    /// Defines the mode of AutoTurnSetting.
    /// </summary>
    public enum AutoTurnSettingMode
    {
        /// <summary>
        /// Specifies a constant value for the auto turn setting.
        /// </summary>
        Constant,

        /// <summary>
        /// Specifies a random value generated between two constant values for the auto turn setting.
        /// </summary>
        RandomBetweenTwoConstants,

        /// <summary>
        /// Specifies a value based on a curve for the auto turn setting.
        /// </summary>
        Curve,

        /// <summary>
        /// Specifies a random value generated between two curves for the auto turn setting.
        /// </summary>
        RandomBetweenTwoCurves
    }


    /// <summary>
    /// Defines the curve time mode of AutoTurnSetting when AutoTurnSettingMode is Curve or RandomBetweenTwoCurves.
    /// </summary>
    public enum AutoTurnSettingCurveTimeMode
    {
        /// <summary>
        /// Evaluates the curve based on the current paper index divided by the total paper count.
        /// This gives a time value proportional to the progression through the papers.
        /// </summary>
        PaperIndexTime,

        /// <summary>
        /// Evaluates the curve based on the current turn index divided by the total turn count.
        /// This provides a time value proportional to the progression through the turns.
        /// </summary>
        TurnIndexTime
    }

    /// <summary>
    /// Attribute to limit AutoTurnSetting in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class AutoTurnSettingRangeAttribute : Attribute
    {
        public float min;
        public float max;

        public AutoTurnSettingRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

    struct BookRaycastHit
    {
        public Vector3 point;
        public Vector2 textureCoordinate;
        public IPageContent pageContent;
        public int paperIndex;
        public int pageIndex;
    }

    [Serializable]
    class PaperSetup
    {
        const float kMinSize = 1;
        const float kMinThickness = 0.01f;
        const int kMinQuality = 1;
        const int kMaxQuality = 5;


        [SerializeField]
        Material m_Material;

        [SerializeField]
        Color m_Color;

        [SerializeField, Min(kMinSize)]
        float m_Width;

        [SerializeField, Min(kMinSize)]
        float m_Height;

        [SerializeField, Range(kMinThickness, 0.1f)]
        float m_Thickness;

        [UnityEngine.Serialization.FormerlySerializedAs("m_Hardness")]
        [SerializeField, Range(0, 1)]
        float m_Stiffness;

        [Tooltip("The quality level of the paper mesh.")]
        [SerializeField, Range(kMinQuality, kMaxQuality)]
        int m_Quality;

        [Tooltip("The blank space around the content.")]
        [SerializeField]
        PaperUVMargin m_UVMargin;


        public PaperSetup()
        {
            color = Color.white;
            width = height = kMinSize * 2;
            thickness = kMinThickness * 2;
            stiffness = 0.1f;
            quality = 3;
        }

        public Material material
        {
            get => m_Material;
            set => m_Material = value;
        }

        public Color color
        {
            get => m_Color;
            set => m_Color = value;
        }

        public float width
        {
            get => (int)bookDirection > 1 ? m_Height : m_Width;
            set => m_Width = Mathf.Max(value, kMinSize);
        }

        public float height
        {
            get => (int)bookDirection > 1 ? m_Width : m_Height;
            set => m_Height = Mathf.Max(value, kMinSize);
        }

        public float thickness
        {
            get => m_Thickness;
            set => m_Thickness = Mathf.Max(value, kMinThickness);
        }

        public float stiffness
        {
            get => m_Stiffness;
            set => m_Stiffness = Mathf.Clamp01(value);
        }

        public int quality
        {
            get => m_Quality;
            set => m_Quality = Mathf.Clamp(value, kMinQuality, kMaxQuality);
        }

        public PaperUVMargin uvMargin
        {
            get => m_UVMargin.Transform(bookDirection);
            set => m_UVMargin = value;
        }

        internal Vector2 size => new Vector2(width, height);
        internal float margin { get; set; }

        internal BookDirection bookDirection;
    }

    [Serializable]
    struct PaperUVMargin
    {
        const float kMin = 0;
        const float kMax = 0.25f;

        [SerializeField, Range(kMin, kMax)] float m_Left;
        [SerializeField, Range(kMin, kMax)] float m_Right;
        [SerializeField, Range(kMin, kMax)] float m_Down;
        [SerializeField, Range(kMin, kMax)] float m_Up;

        public float left
        {
            get => m_Left;
            set => m_Left = Clamp(value);
        }

        public float right
        {
            get => m_Right;
            set => m_Right = Clamp(value);
        }

        public float down
        {
            get => m_Down;
            set => m_Down = Clamp(value);
        }

        public float up
        {
            get => m_Up;
            set => m_Up = Clamp(value);
        }

        float Clamp(float value)
        {
            return Mathf.Clamp(value, kMin, kMax);
        }


        public PaperUVMargin Transform(BookDirection direction)
        {
            PaperUVMargin margin;
            switch (direction)
            {
                case BookDirection.LeftToRight:
                    margin.m_Left = m_Left;
                    margin.m_Right = m_Right;
                    margin.m_Down = m_Down;
                    margin.m_Up = m_Up;
                    break;
                case BookDirection.RightToLeft:
                    margin.m_Left = m_Right;
                    margin.m_Right = m_Left;
                    margin.m_Down = m_Down;
                    margin.m_Up = m_Up;
                    break;
                case BookDirection.UpToDown:
                    margin.m_Left = m_Up;
                    margin.m_Right = m_Down;
                    margin.m_Down = m_Left;
                    margin.m_Up = m_Right;
                    break;
                case BookDirection.DownToUp:
                default:
                    margin.m_Left = m_Down;
                    margin.m_Right = m_Up;
                    margin.m_Down = m_Left;
                    margin.m_Up = m_Right;
                    break;
            }
            return margin;
        }


        public Vector2 FixUV(Vector2 uv)
        {
            uv.x = Mathf.InverseLerp(m_Left, 1 - m_Right, uv.x);
            uv.y = Mathf.InverseLerp(m_Down, 1 - m_Up, uv.y);
            return uv;
        }
    }

    class MeshFactory
    {
        Stack<Mesh> m_UsedMeshs = new Stack<Mesh>();
        Stack<Mesh> m_FreeMeshs = new Stack<Mesh>();
        HashSet<Mesh> m_Meshs = new HashSet<Mesh>();

        public Mesh Get()
        {
            Mesh mesh;
            if (m_FreeMeshs.Count > 0)
            {
                mesh = m_FreeMeshs.Pop();
            }
            else
            {
                mesh = new Mesh();
                mesh.hideFlags = HideFlags.DontSave;
                m_Meshs.Add(mesh);
            }
            m_UsedMeshs.Push(mesh);
            return mesh;
        }

        public void Recycle()
        {
            foreach (var mesh in m_UsedMeshs)
            {
                mesh.Clear();
                m_FreeMeshs.Push(mesh);
            }
            m_UsedMeshs.Clear();
        }

        public void Destroy()
        {
            foreach (var mesh in m_Meshs)
            {
                if (mesh != null) ObjectUtility.Destroy(mesh);
            }
        }
    }

    class RendererFactory
    {
        Transform m_Root;

        public RendererFactory(Transform root)
        {
            m_Root = root;
        }


        Stack<Renderer> m_UsedRenderers = new Stack<Renderer>();
        Stack<Renderer> m_FreeRenderers = new Stack<Renderer>();
        HashSet<Renderer> m_Renderers = new HashSet<Renderer>();
        List<int> m_Ids = new List<int>();


        public int[] ids => m_Ids.ToArray();

        public Renderer Get(string name)
        {
            Renderer renderer;
            if (m_FreeRenderers.Count > 0)
            {
                renderer = m_FreeRenderers.Pop();
                renderer.Reset(name);
            }
            else
            {
                renderer = new Renderer(m_Root, name);
                m_Renderers.Add(renderer);
            }
            m_UsedRenderers.Push(renderer);
            m_Ids.Add(renderer.id);
            return renderer;
        }

        public void Recycle()
        {
            foreach (var renderer in m_UsedRenderers)
            {
                renderer.Clear();
                m_FreeRenderers.Push(renderer);
            }
            m_UsedRenderers.Clear();
            m_Ids.Clear();
        }

        public void Destroy()
        {
            foreach (var renderer in m_Renderers)
            {
                renderer.Destroy();
            }
        }

        public void DrawGizmos(Color color)
        {
            foreach (var renderer in m_UsedRenderers)
            {
                renderer.DrawGizmoMesh(color);
            }
        }
    }

    class Renderer
    {
        GameObject m_GameObject;
        Transform m_Transform;
        MeshRenderer m_MeshRenderer;
        MeshFilter m_MeshFilter;
        bool m_Visibility = true;

        public int id => m_MeshRenderer.GetInstanceID();

        public Transform transform => m_Transform;

        public bool visibility => m_Visibility;

        public bool castShadows
        {
            set => m_MeshRenderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        public Mesh mesh
        {
            set => m_MeshFilter.sharedMesh = value;
        }

        public Renderer(Transform root, string name)
        {
            m_GameObject = new GameObject(name);
            m_GameObject.hideFlags = HideFlags.HideAndDontSave;
            m_Transform = m_GameObject.transform;
            m_Transform.SetParent(root, false);
            m_MeshRenderer = m_GameObject.AddComponent<MeshRenderer>();
            m_MeshFilter = m_GameObject.AddComponent<MeshFilter>();
        }

        public void SetMaterials(params Material[] materials)
        {
            m_MeshRenderer.sharedMaterials = materials;
        }

        public void DrawGizmoMesh(Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawMesh(m_MeshFilter.sharedMesh, m_Transform.position, m_Transform.rotation, m_Transform.lossyScale);
            Gizmos.color = oldColor;
        }

        public void DrawWireframeGizmoMesh(Color color, int submeshIndex)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawWireMesh(m_MeshFilter.sharedMesh, submeshIndex, m_Transform.position, m_Transform.rotation, m_Transform.lossyScale);
            Gizmos.color = oldColor;
        }

        public void SetPropertyBlock(MaterialPropertyBlock properties, int materialIndex)
        {
            m_MeshRenderer.SetPropertyBlock(properties, materialIndex);
        }

        public void Reset(string name)
        {
            m_GameObject.name = name;
            SetVisibility(true);
            mesh = null;
            SetMaterials();
        }

        public void Clear()
        {
            m_GameObject.name = "";
            SetVisibility(false);
            m_Transform.localPosition = Vector3.zero;
            m_Transform.localEulerAngles = Vector3.zero;
            m_Transform.localScale = Vector3.one;
        }

        public void Destroy()
        {
            ObjectUtility.Destroy(m_GameObject);
        }

        public void SetVisibility(bool visibility)
        {
            m_Visibility = visibility;
            m_GameObject.SetActive(visibility);
        }
    }

    class PaperMeshDataPool
    {
        Stack<PaperMeshData> m_Stack;
        MeshFactory m_MeshFactory;
        PaperPattern m_Pattern;
        PaperMeshData m_SharedData;
        bool m_UseSharedData;

        public PaperMeshDataPool(MeshFactory meshFactory, PaperPattern pattern, bool useSharedData = false)
        {
            m_MeshFactory = meshFactory;
            m_Pattern = pattern;
            if (m_UseSharedData = useSharedData)
            {
                m_SharedData = new PaperMeshData(m_MeshFactory.Get(), m_Pattern);
                m_SharedData.UpdateMesh();
            }
            else
            {
                m_Stack = new Stack<PaperMeshData>();
            }
        }

        public PaperMeshData Get()
        {
            if (m_UseSharedData) return m_SharedData;

            if (m_Stack.Count > 0)
            {
                return m_Stack.Pop();
            }

            return new PaperMeshData(m_MeshFactory.Get(), m_Pattern);
        }

        public void Free(PaperMeshData mesh)
        {
            if (m_UseSharedData) throw new NotImplementedException();

            m_Stack.Push(mesh);
        }
    }

    class PaperMeshData
    {
        Mesh m_Mesh;
        PaperPattern m_Pattern;
        Vector3[] m_BaseVertices;
        Vector3[] m_Vertices;
        Vector3[] m_Normals;

        public Mesh mesh => m_Mesh;
        public PaperPattern pattern => m_Pattern;
        public Vector3[] baseVertices => m_BaseVertices;

        public PaperMeshData(Mesh mesh, PaperPattern pattern)
        {
            m_Pattern = pattern;
            m_BaseVertices = new Vector3[pattern.baseVertexCount];
            m_Vertices = new Vector3[pattern.vertexCount];
            m_Normals = new Vector3[pattern.vertexCount];

            m_Mesh = mesh;
            m_Mesh.subMeshCount = 3;
            m_Mesh.vertices = m_Vertices;
            m_Mesh.normals = m_Normals;
            m_Mesh.uv = pattern.texcoords;
            m_Mesh.SetTriangles(pattern.frontTriangles, 0);
            m_Mesh.SetTriangles(pattern.backTriangles, 1);
            m_Mesh.SetTriangles(pattern.borderTriangles, 2);

            UpdateBaseVertices();
        }

        public void UpdateBaseVertices()
        {
            Vector3[] baseVertices = m_BaseVertices;
            float[] baseXArray = m_Pattern.baseXArray;
            float[] baseZArray = m_Pattern.baseZArray;
            float baseXOffset = m_Pattern.baseXOffset;
            int nX = baseXArray.Length;
            int nZ = baseZArray.Length;
            int i = 0;
            for (int z = 0; z < nZ; z++)
            {
                for (int x = 0; x < nX; x++)
                {
                    baseVertices[i++] = new Vector3(baseXArray[x] + baseXOffset, 0, baseZArray[z]);
                }
            }
        }

        public void UpdateMesh()
        {
            Vector3[] baseVertices = m_BaseVertices;
            Vector3[] vertices = m_Vertices;
            Vector3[] normals = m_Normals;
            int[] weights = m_Pattern.weights;
            int nX = m_Pattern.baseXArray.Length;
            int nZ = m_Pattern.baseZArray.Length;
            int baseVertexCount = m_Pattern.baseVertexCount;

            Array.Clear(normals, 0, baseVertexCount);

            PaperMeshUtility.UpdateXSeam(m_Pattern.leftSeam, baseVertices, nX, nZ, false);
            PaperMeshUtility.UpdateXSeam(m_Pattern.rightSeam, baseVertices, nX, nZ, false);
            PaperMeshUtility.UpdateZSeam(m_Pattern.upSeam, baseVertices, nX, nZ, false);
            PaperMeshUtility.UpdateZSeam(m_Pattern.downSeam, baseVertices, nX, nZ, false);

            int zJump1 = m_Pattern.downSeam.signedIndex;
            int zJump2 = m_Pattern.upSeam.signedIndex;
            int xJump1 = m_Pattern.leftSeam.signedIndex;
            int xJump2 = m_Pattern.rightSeam.signedIndex;

            for (int z = 0; z < nZ - 1; z++)
            {
                if (z == zJump1 || z == zJump2) continue;

                int zNext = z + 1;

                if (zNext == zJump1 || zNext == zJump2) zNext++;

                for (int x = 0; x < nX - 1; x++)
                {
                    if (x == xJump1 || x == xJump2) continue;

                    int xNext = x + 1;

                    if (xNext == xJump1 || xNext == xJump2) xNext++;

                    int a = z * nX + x;
                    int b = z * nX + xNext;
                    int c = zNext * nX + x;
                    int d = zNext * nX + xNext;

                    Vector3 pa = baseVertices[a];
                    Vector3 pb = baseVertices[b];
                    Vector3 pc = baseVertices[c];
                    Vector3 pd = baseVertices[d];

                    Vector3 na = TriangleUtility.GetNormal(pa, pc, pb);
                    Vector3 nd = TriangleUtility.GetNormal(pd, pb, pc);

                    Vector3 sum = na + nd;
                    normals[a] += sum;
                    normals[b] += sum;
                    normals[c] += sum;
                    normals[d] += sum;
                }
            }


            for (int i = 0; i < baseVertexCount; i++)
            {
                normals[i] = (normals[i] / weights[i]).normalized;
            }

            PaperMeshUtility.UpdateXSeam(m_Pattern.leftSeam, normals, nX, nZ, true);
            PaperMeshUtility.UpdateXSeam(m_Pattern.rightSeam, normals, nX, nZ, true);
            PaperMeshUtility.UpdateZSeam(m_Pattern.upSeam, normals, nX, nZ, true);
            PaperMeshUtility.UpdateZSeam(m_Pattern.downSeam, normals, nX, nZ, true);

            float halfThickness = m_Pattern.thickness / 2;
            for (int i = 0; i < baseVertexCount; i++)
            {
                Vector3 normal = normals[i];
                normals[i + baseVertexCount] = -normal;
                Vector3 vertex = baseVertices[i];
                vertices[i] = vertex + normal * halfThickness;
                vertices[i + baseVertexCount] = vertex - normal * halfThickness;
            }

            PaperMeshUtility.UpdateBorders(pattern.borders, vertices, normals, nX, nZ);

            m_Mesh.vertices = vertices;
            m_Mesh.normals = normals;
            m_Mesh.RecalculateBounds();
        }

        public void DrawWireframe(Matrix4x4 matrix, Color color)
        {
            int nX = m_Pattern.baseXArray.Length;
            int nZ = m_Pattern.baseZArray.Length;
            PaperMeshUtility.DrawWireframe(m_Vertices, nX, nZ, matrix, color);
        }
    }

    class PaperPattern
    {
        public float[] baseXArray;
        public float[] baseZArray;
        public float baseXOffset;
        public int baseVertexCount;

        public PaperSeam leftSeam;
        public PaperSeam xMidSeam;
        public PaperSeam rightSeam;

        public PaperSeam upSeam;
        public PaperSeam zMidSeam;
        public PaperSeam downSeam;

        public PaperBorder[] borders;

        public Vector2[] texcoords;
        public int[] weights;
        public int[] frontTriangles;
        public int[] backTriangles;
        public int[] borderTriangles;
        public int vertexCount;

        public Vector2 size;
        public float thickness;
    }

    struct PaperSeam
    {
        public bool active;
        public int index;
        public float time;

        public int signedIndex => active ? index : -1;

        public PaperSeam(int index, float time)
        {
            active = true;
            this.index = index;
            this.time = time;
        }
    }

    struct PaperBorder
    {
        public int startX;
        public int startZ;
        public int endX;
        public int endZ;
        public bool flip;

        public PaperBorder(int startX, int startZ, int endX, int endZ, bool flip)
        {
            this.startX = startX;
            this.startZ = startZ;
            this.endX = endX;
            this.endZ = endZ;
            this.flip = flip;
        }
    }

    static class PaperMeshUtility
    {
        public static void AddSeams(PaperPattern pattern, List<float> xList, List<float> zList, Vector2 size, PaperUVMargin uvMargin)
        {
            if (uvMargin.left != 0 && uvMargin.right != 0)
            {
                int a = GetSeamIndex(xList, uvMargin.left * size.x);
                int b = GetSeamIndex(xList, (1 - uvMargin.right) * size.x);
                if (a == b)
                {
                    pattern.xMidSeam = AddSeam(xList, size.x / 2);
                }
            }

            if (uvMargin.up != 0 && uvMargin.down != 0)
            {
                int a = GetSeamIndex(zList, uvMargin.down * size.y);
                int b = GetSeamIndex(zList, (1 - uvMargin.up) * size.y);
                if (a == b)
                {
                    pattern.zMidSeam = AddSeam(zList, size.y / 2);
                }
            }

            if (uvMargin.left != 0)
            {
                pattern.leftSeam = AddSeam(xList, uvMargin.left * size.x);
            }

            if (uvMargin.right != 0)
            {
                pattern.rightSeam = AddSeam(xList, (1 - uvMargin.right) * size.x);
            }

            if (uvMargin.down != 0)
            {
                pattern.downSeam = AddSeam(zList, uvMargin.down * size.y);
            }

            if (uvMargin.up != 0)
            {
                pattern.upSeam = AddSeam(zList, (1 - uvMargin.up) * size.y);
            }


        }

        static PaperSeam AddSeam(List<float> list, float value)
        {
            int i = GetSeamIndex(list, value);
            float a = list[i - 1];
            float b = list[i];
            list.Insert(i, value);
            return new PaperSeam(i, Mathf.InverseLerp(a, b, value));
        }

        static int GetSeamIndex(List<float> list, float value)
        {
            int n = list.Count;
            for (int i = 1; i < n; i++)
            {
                float a = list[i - 1];
                float b = list[i];
                if (value >= a && value <= b)
                {
                    return i;
                }
            }
            return 0;
        }

        public static void UpdateXSeam(PaperSeam seam, Vector3[] vertices, int nX, int nZ, bool useSlerp)
        {
            if (!seam.active) return;

            for (int z = 0; z < nZ; z++)
            {
                int iPrev = seam.index - 1;
                int iNext = seam.index + 1;
                Vector3 a = vertices[z * nX + iPrev];
                Vector3 b = vertices[z * nX + iNext];
                Vector3 p = useSlerp ? Vector3.Slerp(a, b, seam.time) : Vector3.Lerp(a, b, seam.time);
                vertices[z * nX + seam.index] = p;
            }
        }

        public static void UpdateZSeam(PaperSeam seam, Vector3[] vertices, int nX, int nZ, bool useSlerp)
        {
            if (!seam.active) return;

            for (int x = 0; x < nX; x++)
            {
                int iPrev = seam.index - 1;
                int iNext = seam.index + 1;
                Vector3 a = vertices[iPrev * nX + x];
                Vector3 b = vertices[iNext * nX + x];
                Vector3 p = useSlerp ? Vector3.Slerp(a, b, seam.time) : Vector3.Lerp(a, b, seam.time);
                vertices[seam.index * nX + x] = p;
            }
        }


        public static void AddBorders(List<PaperBorder> borders, List<int> triangles, List<Vector2> texcoords, int nX, int nZ)
        {
            foreach (var border in borders)
            {
                int nX2 = (border.endX - border.startX + 1) * 2;
                int nZ2 = (border.endZ - border.startZ + 1) * 2;

                int iV = texcoords.Count;
                for (int i = 0, n = border.endX - border.startX; i < n; i++)
                {
                    int a = iV + i * 2 + 0;
                    int b = iV + i * 2 + 1;
                    int c = iV + i * 2 + 2;
                    int d = iV + i * 2 + 3;

                    if (border.flip)
                        Add2BackFaces(triangles, a, b, c, d, nX2);
                    else
                        Add2FrontFaces(triangles, a, b, c, d, nX2);
                }

                for (int i = 0, n = border.endZ - border.startZ; i < n; i++)
                {
                    int a = iV + i * 2 + 0 + nX2 * 2;
                    int b = iV + i * 2 + 1 + nX2 * 2;
                    int c = iV + i * 2 + 2 + nX2 * 2;
                    int d = iV + i * 2 + 3 + nX2 * 2;

                    if (border.flip)
                        Add2BackFaces(triangles, a, b, c, d, nZ2);
                    else
                        Add2FrontFaces(triangles, a, b, c, d, nZ2);
                }

                int nXZ = nX * nZ;
                for (int i = border.startX; i <= border.endX; i++)
                {
                    int j = border.startZ * nX + i;
                    texcoords.Add(texcoords[j]);
                    texcoords.Add(texcoords[j + nXZ]);
                }

                for (int i = border.startX; i <= border.endX; i++)
                {
                    int j = i + border.endZ * nX;
                    texcoords.Add(texcoords[j + nXZ]);
                    texcoords.Add(texcoords[j]);
                }

                for (int i = border.startZ; i <= border.endZ; i++)
                {
                    int j = border.startX + i * nX;
                    texcoords.Add(texcoords[j + nXZ]);
                    texcoords.Add(texcoords[j]);
                }

                for (int i = border.startZ; i <= border.endZ; i++)
                {
                    int j = i * nX + border.endX;
                    texcoords.Add(texcoords[j]);
                    texcoords.Add(texcoords[j + nXZ]);
                }
            }
        }

        public static void UpdateBorders(PaperBorder[] borders, Vector3[] vertices, Vector3[] normals, int nX, int nZ)
        {
            int baseVertexCount = nX * nZ;
            int vertexIndex = baseVertexCount * 2;
            foreach (var border in borders)
            {
                for (int i = border.startX; i <= border.endX; i++)
                {
                    int j = i + border.startZ * nX;
                    Vector3 v = vertices[j];
                    Vector3 v2 = vertices[j + nX];
                    Vector3 n = (v - v2).normalized;
                    if (border.flip) n *= -1;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = v;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = vertices[j + baseVertexCount];
                }

                for (int i = border.startX; i <= border.endX; i++)
                {
                    int j = i + border.endZ * nX;
                    Vector3 v = vertices[j + baseVertexCount];
                    Vector3 v2 = vertices[j + baseVertexCount - nX];
                    Vector3 n = (v - v2).normalized;
                    if (border.flip) n *= -1;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = v;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = vertices[j];
                }

                for (int i = border.startZ; i <= border.endZ; i++)
                {
                    int j = i * nX + border.startX;
                    Vector3 v = vertices[j + baseVertexCount];
                    Vector3 v2 = vertices[j + baseVertexCount + 1];
                    Vector3 n = (v - v2).normalized;
                    if (border.flip) n *= -1;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = v;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = vertices[j];
                }

                for (int i = border.startZ; i <= border.endZ; i++)
                {
                    int j = i * nX + border.endX;
                    Vector3 v = vertices[j];
                    Vector3 v2 = vertices[j - 1];
                    Vector3 n = (v - v2).normalized;
                    if (border.flip) n *= -1;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = v;
                    normals[vertexIndex] = n;
                    vertices[vertexIndex++] = vertices[j + baseVertexCount];
                }
            }
        }

        public static void DebugDrawBorders(PaperBorder[] borders, Vector3[] vertices, int nX, int nZ, Matrix4x4 matrix , Color color)
        {
            int baseVertexCount = nX * nZ;
            int vertexIndex = baseVertexCount * 2;
            foreach (var border in borders)
            {
                for (int i = border.startX; i < border.endX; i++)
                {
                    int j = i + border.startZ * nX;
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[j]);
                    Vector3 b = matrix.MultiplyPoint3x4(vertices[j + 1]);
                    Debug.DrawLine(a, b, color);
                }

                for (int i = border.startX; i < border.endX; i++)
                {
                    int j = i + border.endZ * nX;
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[j]);
                    Vector3 b = matrix.MultiplyPoint3x4(vertices[j + 1]);
                    Debug.DrawLine(a, b, color);
                }
                
                for (int i = border.startZ; i < border.endZ; i++)
                {
                    int j = i * nX + border.startX;
                    int j2 = (i + 1) * nX + border.startX;
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[j]);
                    Vector3 b = matrix.MultiplyPoint3x4(vertices[j2]);
                    Debug.DrawLine(a, b, color);
                }

                for (int i = border.startZ; i < border.endZ; i++)
                {
                    int j = i * nX + border.endX;
                    int j2 = (i + 1) * nX + border.endX;
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[j]);
                    Vector3 b = matrix.MultiplyPoint3x4(vertices[j2]);
                    Debug.DrawLine(a, b, color);
                }
            }
        }


        static void Add2FrontFaces(List<int> triangles, int a, int b, int c, int d, int offset)
        {
            AddFrontFace(triangles, a, b, c, d);
            a += offset;
            b += offset;
            c += offset;
            d += offset;
            AddFrontFace(triangles, a, b, c, d);
        }

        static void Add2BackFaces(List<int> triangles, int a, int b, int c, int d, int offset)
        {
            AddBackFace(triangles, a, b, c, d);
            a += offset;
            b += offset;
            c += offset;
            d += offset;
            AddBackFace(triangles, a, b, c, d);
        }

        public static void AddFrontAndBackFaces(List<int> frontTriangles, List<int> backTriangles, int a, int b, int c, int d, int offset)
        {
            AddFrontFace(frontTriangles, a, b, c, d);
            a += offset;
            b += offset;
            c += offset;
            d += offset;
            AddBackFace(backTriangles, a, b, c, d);
        }

        public static void AddFrontAndBackTexcoords(List<Vector2> texcoords, List<float> xList, List<float> zList, Vector2 size, PaperUVMargin uvMargin, BookDirection direction)
        {
            float uStart = uvMargin.left * size.x;
            float uEnd = (1 - uvMargin.right) * size.x;
            float vStart = uvMargin.down * size.y;
            float vEnd = (1 - uvMargin.up) * size.y;

            int nX = xList.Count;
            int nZ = zList.Count;
            if ((int)direction > 1)
            {
                for (int z = 0; z < nZ; z++)
                {
                    for (int x = 0; x < nX; x++)
                    {
                        float u = Mathf.InverseLerp(uEnd, uStart, xList[x]);
                        float v = Mathf.InverseLerp(vStart, vEnd, zList[z]);
                        texcoords.Add(new Vector2(v, u));
                    }
                }
            }
            else
            {
                for (int z = 0; z < nZ; z++)
                {
                    for (int x = 0; x < nX; x++)
                    {
                        float u = Mathf.InverseLerp(uStart, uEnd, xList[x]);
                        float v = Mathf.InverseLerp(vStart, vEnd, zList[z]);
                        texcoords.Add(new Vector2(u, v));
                    }
                }
            }

            texcoords.AddRange(texcoords);
        }

        static void AddFrontFace(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(d);
        }

        static void AddBackFace(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(d);
            triangles.Add(c);
        }

        
        public static void DrawWireframe(Vector3[] vertices, int nX, int nZ, Matrix4x4 matrix, Color color)
        {
#if UNITY_EDITOR
            Color fillColor = color;
            fillColor.a /= 3;
            using (new UnityEditor.Handles.DrawingScope(color))
            {
                for (int z = 0; z < nZ - 1; z++)
                {
                    for (int x = 0; x < nX - 1; x++)
                    {
                        Vector3 a = vertices[z * nX + x];
                        Vector3 b = vertices[z * nX + x + 1];
                        Vector3 c = vertices[(z + 1) * nX + x];
                        Vector3 d = vertices[(z + 1) * nX + x + 1];

                        //c d
                        //a b

                        a = matrix.MultiplyPoint3x4(a);
                        b = matrix.MultiplyPoint3x4(b);
                        c = matrix.MultiplyPoint3x4(c);
                        d = matrix.MultiplyPoint3x4(d);


                        UnityEditor.Handles.color = fillColor;

                        UnityEditor.Handles.DrawAAConvexPolygon(a, b, d, c);

                        UnityEditor.Handles.color = color;

                        UnityEditor.Handles.DrawLine(a, b);
                        UnityEditor.Handles.DrawLine(b, d);
                        UnityEditor.Handles.DrawLine(d, c);
                        UnityEditor.Handles.DrawLine(c, a);
                    }
                }
            }
#endif
        }
    }

    class PaperMaterialData
    {
        MaterialPropertyBlock m_PropertyBlock;
        Color m_Color;
        int m_MainTextureID;
        int m_MainTextureSTID;
        int m_MainColorID;

        public MaterialPropertyBlock propertyBlock => m_PropertyBlock;

        public PaperMaterialData(Material material, Color color)
        {
            m_PropertyBlock = new MaterialPropertyBlock();

            m_MainTextureID = MaterialUtility.GetMainTextureID(material);
            m_MainTextureSTID = MaterialUtility.GetMainTextureSTID(material);
            m_MainColorID = MaterialUtility.GetMainColorID(material);

            m_Color = color;
        }

        public void UpdatePropertyBlock(Texture texture, Vector4 textureST)
        {
            m_PropertyBlock.Clear();
            m_PropertyBlock.SetColor(m_MainColorID, m_Color);
            m_PropertyBlock.SetTexture(m_MainTextureID, TextureUtility.FixNull(texture));
            m_PropertyBlock.SetVector(m_MainTextureSTID, textureST);
        }
    }

    class Paper
    {
        int m_Index;
        Transform m_Transform;
        IPageContent m_FrontContent;
        IPageContent m_BackContent;
        bool m_UseBackContentForSides;

        PaperMaterialData m_MaterialData;

        Renderer m_Renderer;

        Vector2 m_Size;
        float m_Thickness;
        float m_Stiffness;
        float m_Margin;

        bool m_IsCover;

        PaperUVMargin m_UVMargin;

        PaperMeshData m_MeshData;
        PaperMeshData m_LowpolyMeshData;
        PaperMeshDataPool m_HighpolyMeshDataPool;
        bool m_IsLowpoly;

        Book m_Book;


        Cylinder m_Cylinder;
        bool m_IsRolling;

        bool m_IsAutoTurning;


        Plane m_WorldPlane;

        Vector3 m_StartHandle;
        Vector3 m_CurrentHandle;
        Vector3 m_EndHandle;
        Vector3 m_PrevHandle;
        Vector3 m_HandleOffset;
        Vector3 m_HandleVelocity;
        List<Vector3> m_HandleVelocities = new List<Vector3>(5);

        float m_MinTurningRadius;
        float m_TurningRadius;
        float m_FallDuration;
        float m_FallTime = 0.2f;

        float m_XTime;
        float m_ZTime;

        bool m_IsTurning;
        bool m_IsFalling;
        bool m_IsFallingLeft;

        public float sizeXOffset { get; set; }

        public bool isCover => m_IsCover;

        public int index => m_Index;

        public Transform transform => m_Transform;

        public Renderer renderer => m_Renderer;

        public PaperMeshData meshData => m_MeshData;

        public Vector2 size
        {
            get => m_Size;
            set => m_Size = value;
        }

        public float thickness
        {
            get => m_Thickness;
        }

        public float margin
        {
            get => m_Margin;
        }

        public float zTime
        {
            get
            {
                if (m_IsFalling || m_IsTurning)
                {
                    if (m_Transform.localScale.x == -1)
                    {
                        return 1 - m_ZTime;
                    }
                    else
                    {
                        return m_ZTime;
                    }
                }

                return m_Transform.localScale.x == -1 ? 1 : 0;
            }
        }

        public Vector3 direction
        {
            get
            {
                float z = zTime * 180;
                Vector3 dir = Quaternion.Euler(0, 0, z) * Vector3.left;
                return dir;
            }
        }

        public bool isTurning => m_IsTurning;

        public bool isFalling => m_IsFalling;

        public bool isFlipped => m_Transform.localScale.x == -1;

        public bool isOnRightStack
        {
            get
            {
                if (m_IsFalling)
                {
                    if (m_Transform.localScale.x == -1)
                    {
                        return m_IsFallingLeft;
                    }
                    else
                    {
                        return !m_IsFallingLeft;
                    }
                }

                return m_Transform.localScale.x == -1 ? false : true;
            }
        }


        public IPageContent frontContent => m_FrontContent;
        public IPageContent backContent => m_BackContent;

        public Paper(bool isCover, int index, Book book, RendererFactory rendererFactory)
        {
            m_IsCover = isCover;
            m_Book = book;
            m_Index = index;
            m_Renderer = rendererFactory.Get("Paper");
            m_Transform = m_Renderer.transform;
        }

        public void SetTime(float time)
        {
            m_XTime = time;
            m_ZTime = time;
        }

        public void SetMeshData(PaperMeshData lowpolyMeshData, PaperMeshDataPool highpolyMeshDataPool)
        {
            m_IsLowpoly = true;
            m_MeshData = lowpolyMeshData;
            m_LowpolyMeshData = lowpolyMeshData;
            m_HighpolyMeshDataPool = highpolyMeshDataPool;
            m_Renderer.mesh = m_MeshData.mesh;
        }

        public void SetMaterialData(PaperMaterialData data)
        {
            m_MaterialData = data;
            UpdateMaterials();
        }

        public void SetPaperSetup(PaperSetup settings)
        {
            m_Size = settings.size;
            m_Thickness = settings.thickness;
            m_Stiffness = settings.stiffness;
            m_Margin = settings.margin;
            m_UVMargin = settings.uvMargin;
        }

        internal void SetContentData(IPageContent frontContent, IPageContent backContent, bool useBackContentForSides = false)
        {
            m_FrontContent = frontContent;
            m_BackContent = backContent;
            m_UseBackContentForSides = useBackContentForSides;
        }

        public void SetMinTurningRadius(float min)
        {
            m_MinTurningRadius = min;
        }

        public void UpdateTurningRadius(float bend = 1)
        {
            float h = Mathf.Max(m_Stiffness, (1 - Mathf.Clamp01(bend)));
            if (h <= 0.5f)
            {
                m_TurningRadius = Mathf.InverseLerp(0, 0.5f, h) * m_Size.x / Mathf.PI;
            }
            else
            {
                m_TurningRadius = m_Size.x / (Mathf.Max(180 * (1 - Mathf.InverseLerp(0.5f, 1, h)), 5) * Mathf.Deg2Rad);
            }

            m_TurningRadius = Mathf.Max(m_TurningRadius, m_MinTurningRadius);
        }

        public bool StartTurning(Ray ray)
        {
            Ray oldRay = ray;
            ray = MatrixUtility.Transform(ray, m_Transform.worldToLocalMatrix);
            m_WorldPlane = new Plane(m_Transform.up, m_Transform.position);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float dis))
            {
                Vector3 hit = ray.GetPoint(dis);
                if ((hit.x > 0 && hit.x < m_Size.x) && (hit.z > 0 && hit.z < m_Size.y))
                {
                    m_IsRolling = true;
                    m_IsTurning = true;
                    m_IsFalling = false;
                    m_HandleOffset = Vector3.zero;

                    m_StartHandle = hit;
                    m_StartHandle.x = m_Size.x;
                    m_CurrentHandle = m_StartHandle;

                    if (hit.x < m_Size.x * 0.9f)
                    {
                        m_HandleOffset = new Vector3(hit.x - m_Size.x, 0, 0);
                        Vector3 scale = m_Transform.localScale;
                        scale.x *= -1;
                        m_Transform.localScale = scale;
                    }

                    m_HandleVelocity = Vector3.zero;
                    m_PrevHandle = m_CurrentHandle;
                    m_HandleVelocities.Clear();

                    SwitchMeshData(false);

                    UpdateTurning(oldRay);
                    ClampHandle();
                    UpdateCylinder();
                    return true;
                }
            }
            return false;
        }

        public void StopTurning()
        {
            ClampHandle();

            m_IsTurning = false;
            m_IsFalling = true;

            Vector3 velocity = Vector3.zero;
            foreach (var v in m_HandleVelocities)
            {
                velocity += v;
            }
            velocity /= m_HandleVelocities.Count;


            if (velocity.magnitude > 0.1f)
            //if (Mathf.Abs(handleVelocity.x) > 1f)
            {
                m_IsFallingLeft = velocity.x < 0;
            }
            else
            {
                m_IsFallingLeft = (m_XTime > 0.5f && m_ZTime > 0.1f);
            }


            if (m_IsFallingLeft)
            {
                m_FallTime = 1 - m_XTime;
            }
            else
            {
                m_FallTime = m_XTime;
            }

            m_FallTime = Mathf.Lerp(0.1f, 0.2f, m_FallTime);


            m_EndHandle = m_StartHandle;
            if (m_IsFallingLeft)
            {
                m_EndHandle.x = -m_Size.x;
            }
        }


        public void UpdateTurning(Ray ray)
        {
            if (m_WorldPlane.Raycast(ray, out float dis))
            {
                Vector3 hit = ray.GetPoint(dis);

                m_Book.bound.ResetPaperPosition(this);

                m_CurrentHandle = m_Transform.InverseTransformPoint(hit);
                m_CurrentHandle.y = 0;

                m_CurrentHandle += m_HandleOffset;

                m_HandleVelocity = (m_CurrentHandle - m_PrevHandle) / Time.deltaTime;

                if (m_HandleVelocities.Count == m_HandleVelocities.Capacity)
                {
                    m_HandleVelocities.RemoveAt(0);
                }
                m_HandleVelocities.Add(m_HandleVelocity);



                m_PrevHandle = m_CurrentHandle;

                Debug.DrawLine(m_CurrentHandle, m_StartHandle, Color.green);
            }

            //m_CurrentHandle.z = m_StartHandle.z;


            UpdateBaseVertices();
        }

        public void UpdateFalling()
        {
            bool end = false;

            if (m_IsAutoTurning)
            {
                float t = Mathf.Clamp01(m_FallTime / m_FallDuration);
                t = Mathf.SmoothStep(0, 1, t);
                t = Mathf.SmoothStep(0, 1, t);
                m_CurrentHandle = Vector3.Lerp(m_StartHandle, m_EndHandle, m_IsFallingLeft ? t : 1 - t);
                m_FallTime += Time.deltaTime;
                end = (Mathf.Abs(t - 1) < Mathf.Epsilon);
            }
            else
            {
                Vector3 smoothTime = new Vector3(m_FallTime, 0, m_FallTime * 0.75f);
                m_CurrentHandle = VectorUtility.SmoothDamp(m_CurrentHandle, m_EndHandle, ref m_HandleVelocity, smoothTime);
                end = Mathf.Abs(m_EndHandle.x - m_CurrentHandle.x) < 0.0001f;
               // end = Vector3.Distance(m_CurrentHandle, m_EndHandle) < 0.0001f;
            }

            if (end)
            {
                if (m_IsFallingLeft)
                {
                    Vector3 scale = m_Transform.localScale;
                    scale.x *= -1;
                    m_Transform.localScale = scale;
                    m_IsRolling = false;
                    m_IsFallingLeft = false;
                    m_IsFalling = false;
                    SwitchMeshData(true);
                    m_ZTime = m_Transform.localScale.x == -1 ? 1 : 0;
                    m_Book.bound.UpdatePaperPosition(this);
                    UpdateMaterials();
                }
                else
                {
                    m_ZTime = m_Transform.localScale.x == -1 ? 1 : 0;
                    m_IsRolling = false;
                    m_IsFallingLeft = false;
                    m_IsFalling = false;
                    SwitchMeshData(true);
                    UpdateMaterials();
                }

                if (m_IsAutoTurning)
                {
                    UpdateTurningRadius();
                }

                m_IsAutoTurning = false;
                return;
            }

            UpdateBaseVertices();
        }

        public Vector2 GetTextureCoordinate(Ray ray, LivePageContent livePageContent)
        {
            if (Raycast(ray, out Vector3 hit, true))
            {
                return Hit2UV(hit);
            }

            return Vector2.zero;
        }

        public bool Raycast(Ray ray, out BookRaycastHit hitInfo)
        {
            hitInfo = new BookRaycastHit();

            if (!isFalling && !isTurning && Raycast(ray, out Vector3 hit))
            {
                hitInfo.pageContent = isOnRightStack ? m_FrontContent : m_BackContent;
                hitInfo.point = transform.TransformPoint(hit);
                hitInfo.textureCoordinate = Hit2UV(hit);
                hitInfo.paperIndex = index;
                return true;
            }

            return false;
        }

        Vector2 Hit2UV(Vector3 hit)
        {
            Vector2 uv = new Vector2(Mathf.InverseLerp(-sizeXOffset, size.x, hit.x), hit.z / size.y);

            uv = m_UVMargin.FixUV(uv);

            var dir = m_Book.direction;
            if (dir == BookDirection.UpToDown || dir == BookDirection.DownToUp)
            {
                uv = new Vector2(uv.y, uv.x);

                if (isOnRightStack) uv.y = 1 - uv.y;
            }
            else
            {
                if (!isOnRightStack) uv.x = 1 - uv.x;
            }

            return uv;
        }

        public bool Raycast(Ray ray, out Vector3 hit, bool noBoundsCheck = false)
        {
            ray = MatrixUtility.Transform(ray, transform.worldToLocalMatrix);

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float dis))
            {
                hit = ray.GetPoint(dis);
                return (hit.x > 0 && hit.x < m_Size.x) && (hit.z > 0 && hit.z < m_Size.y) || noBoundsCheck;
            }
            else hit = Vector3.zero;

            return false;
        }

        void SwitchMeshData(bool lowpoly)
        {
            if (m_IsLowpoly == lowpoly) return;
            m_IsLowpoly = lowpoly;

            if (lowpoly)
            {
                m_HighpolyMeshDataPool.Free(m_MeshData);
                m_MeshData = m_LowpolyMeshData;
            }
            else
            {
                m_MeshData = m_HighpolyMeshDataPool.Get();
            }

            m_Renderer.mesh = m_MeshData.mesh;
        }

        public void UpdateMaterials()
        {
            int a = 0;
            int b = 1;

            if (m_Transform.localScale.x == -1)
            {
                a = 1;
                b = 0;
            }

            Vector4 frontST = m_FrontContent.textureST;
            Vector4 backST = m_BackContent.textureST;

            if ((int)m_Book.direction > 1)
            {
                backST = TextureUtility.YFlipST(backST);
            }
            else
            {
                backST = TextureUtility.XFlipST(backST);
            }
          

            Texture frontTexture = m_FrontContent.texture;
            Texture backTexture = m_BackContent.texture;

            m_MaterialData.UpdatePropertyBlock(frontTexture, frontST);
            m_Renderer.SetPropertyBlock(m_MaterialData.propertyBlock, a);

            if (!m_UseBackContentForSides) m_Renderer.SetPropertyBlock(m_MaterialData.propertyBlock, 2);

            m_MaterialData.UpdatePropertyBlock(backTexture, backST);
            m_Renderer.SetPropertyBlock(m_MaterialData.propertyBlock, b);

            if (m_UseBackContentForSides) m_Renderer.SetPropertyBlock(m_MaterialData.propertyBlock, 2);
        }

        public void UpdateBaseVertices()
        {
            ClampHandle();
            UpdateCylinder();
            UpdateTime();

            if (!m_IsRolling) return;

            m_MeshData.UpdateBaseVertices();
            Vector3[] baseVertices = m_MeshData.baseVertices;
            Cylinder cylinder = m_Cylinder;
            int n = baseVertices.Length;
            for (int i = 0; i < n; i++)
            {
                baseVertices[i] = cylinder.RollPoint(baseVertices[i]);
            }
        }

        public void UpdateMesh()
        {
            UpdateMaterials();
            m_MeshData.UpdateMesh();
        }

        public Vector3 GetDirection(float z)
        {
            Vector3 a = new Vector3(0, 0, z);
            Vector3 b = new Vector3(0.1f, 0, z);
            a = RollPoint(a);
            b = RollPoint(b);
            a = m_Transform.TransformPoint(a);
            b = m_Transform.TransformPoint(b);
            a = m_Transform.parent.InverseTransformPoint(a);
            b = m_Transform.parent.InverseTransformPoint(b);
            return (a - b).normalized;
        }

        public void UpdateTime()
        {
            if (isTurning || isFalling)
            {
                float t0 = FindTime(new Vector3(m_Size.x, 0, 0));
                float t1 = FindTime(new Vector3(m_Size.x, 0, m_Size.y));

                m_XTime = Mathf.Lerp(Mathf.Min(t0, t1), Mathf.Max(t0, t1), 0.9f);

                float[] xs = m_MeshData.pattern.baseXArray;
                float[] zs = m_MeshData.pattern.baseZArray;

                Vector3 a = RollPoint(new Vector3(xs[1], 0, 0));
                Vector3 b = RollPoint(new Vector3(xs[2], 0, 0));

                Vector3 c = RollPoint(new Vector3(xs[1], 0, zs[zs.Length - 1]));
                Vector3 d = RollPoint(new Vector3(xs[2], 0, zs[zs.Length - 1]));

                Vector3 ab = (b - a).normalized;
                Vector3 cd = (d - c).normalized;
                float z0 = Mathf.Rad2Deg * Mathf.Atan2(ab.y, ab.x);
                float z1 = Mathf.Rad2Deg * Mathf.Atan2(cd.y, cd.x);
                float z = (z0 + z1) / 2;

                m_ZTime = z / 180;
            }
            else
            {
                m_XTime = 0;
                m_ZTime = 0;
            }
        }

        float FindTime(Vector3 vertex)
        {
            vertex = RollPoint(vertex);
            return Mathf.InverseLerp(m_Size.x, -m_Size.x, vertex.x);
        }

        void ClampHandle()
        {
            m_StartHandle.y = 0;
            m_CurrentHandle.y = 0;

            Vector3 p = m_CurrentHandle;

            //c    c   d
            //     |  start
            //end  | 
            //a    a   b
            Vector3 a = new Vector3(-m_Size.x, 0, 0);
            Vector3 c = new Vector3(-m_Size.x, 0, m_Size.y);
            Vector3 b = new Vector3(m_Size.x, 0, 0);
            Vector3 d = new Vector3(m_Size.x, 0, m_Size.y);
            Vector3 start = m_StartHandle;
            Vector3 end = m_StartHandle;
            end.x *= -1;

            a = new Vector3(0, 0, 0);
            c = new Vector3(0, 0, m_Size.y);

            float ra = Vector3.Distance(a, m_StartHandle);
            float rc = Vector3.Distance(c, m_StartHandle);

            float raz = Mathf.Max(ra - m_TurningRadius, 0.01f);
            float rcz = Mathf.Max(rc - m_TurningRadius, 0.01f);
            float z0 = m_StartHandle.z;

            Vector2 aEllipseCenter = new Vector2(0, z0 + (a.z - z0) * (raz / ra));
            Vector2 cEllipseCenter = new Vector2(0, z0 + (c.z - z0) * (rcz / rc));
            Vector2 aEllipseSize = new Vector2(ra, raz);
            Vector2 cEllipseSize = new Vector2(rc, rcz);

            p.x = Mathf.Clamp(p.x, -m_Size.x, m_Size.x);

            p = VectorUtility.XZ2XY(p);
            p = EllipseUtility.Calmp(p, aEllipseCenter, aEllipseSize);
            p = EllipseUtility.Calmp(p, cEllipseCenter, cEllipseSize);
            p = VectorUtility.XY2XZ(p);

            m_CurrentHandle = p;
        }

        void UpdateCylinder()
        {
            Vector3 startHandle = this.m_StartHandle;
            Vector3 currentHandle = this.m_CurrentHandle;

            Vector3 handleDirection = (startHandle - currentHandle).normalized;
            if (handleDirection.magnitude == 0) handleDirection = Vector3.right;

            Vector3 a = startHandle - handleDirection * (m_Size.x * 2 + m_TurningRadius * Mathf.PI);
            Vector3 b = startHandle;

            Cylinder cylinder = new Cylinder();
            cylinder.radius = m_TurningRadius;
            cylinder.direction = new Vector3(-handleDirection.z, 0, handleDirection.x);

            for (int i = 0; i < 100; i++)
            {
                cylinder.position = (a + b) / 2;
                m_Cylinder = cylinder;
                m_Book.bound.UpdatePaperPosition(this);
                Vector3 v = cylinder.RollPoint(startHandle);
                if (Mathf.Abs(currentHandle.x - v.x) < 0.0001f) break;

                if (v.x > currentHandle.x)
                {
                    b = cylinder.position;
                }
                else
                {
                    a = cylinder.position;
                }
            }
        }

        Vector3 RollPoint(Vector3 point)
        {
            if (m_IsRolling) return m_Cylinder.RollPoint(point);

            return point;
        }

        public void DrawWireframe(Color color)
        {
            m_MeshData.DrawWireframe(transform.localToWorldMatrix, color);
        }

        public void StartAutoTurning(AutoTurnMode mode, float twist, float bend, float duration)
        {
            UpdateTurningRadius(bend);

            m_PrevHandle = m_CurrentHandle;
            m_IsRolling = true;
            m_HandleOffset = Vector3.zero;

            if (mode == AutoTurnMode.Surface)
            {
                Vector3 scale = m_Transform.localScale;
                scale.x *= -1;
                m_Transform.localScale = scale;
            }

            SwitchMeshData(false);

            m_IsFallingLeft = mode == AutoTurnMode.Edge;
            m_IsTurning = false;
            m_IsFalling = true;
            m_FallTime = 0;
            m_FallDuration = duration;

            float x = m_Size.x;
            float z = m_Size.y;

            twist = Mathf.Clamp(twist, -0.99f, 0.99f);
            float turnStartZ = Mathf.LerpUnclamped(0.5f, 1, twist);
            float turnEndZ = Mathf.LerpUnclamped(0.5f, 0, twist);

            m_StartHandle = new Vector3(x, 0, z * turnStartZ);
            m_EndHandle = new Vector3(-x, 0, z * turnEndZ);

            m_IsAutoTurning = true;
        }
    }

    struct Cylinder
    {
        float m_PositionX;
        float m_PositionZ;
        float m_DirectionX;
        float m_DirectionZ;
        float m_EulerY;
        float m_Radius;

        public Vector3 position
        {
            get
            {
                Vector3 p;
                p.x = m_PositionX;
                p.y = 0;
                p.z = m_PositionZ;
                return p;
            }
            set
            {
                m_PositionX = value.x;
                m_PositionZ = value.z;
            }
        }

        public Vector3 direction
        {
            set
            {
                m_DirectionX = value.x;
                m_DirectionZ = value.z;
                m_EulerY = Mathf.Atan2(m_DirectionX, m_DirectionZ) * Mathf.Rad2Deg;
            }
        }

        public float radius
        {
            set
            {
                m_Radius = value;
            }
        }

        public Vector3 RollPoint(Vector3 point)
        {
            return Roll(point) - GetOffset(point);
        }

        Vector3 Roll(Vector3 point)
        {
            if (GetSide(point) >= 0) return point;

            Vector3 closestPoint = GetClosestPoint(point);
            float dis = Vector3.Distance(point, closestPoint);

            if (dis > Mathf.PI * m_Radius)
            {
                dis = dis - Mathf.PI * m_Radius;
                point = Quaternion.Euler(0, m_EulerY, 0) * new Vector3(-dis, 0, 0) + closestPoint;
                point.y += m_Radius * 2;
            }
            else
            {
                float z = Mathf.Rad2Deg * (dis / m_Radius) - 90;
                Quaternion q = Quaternion.Euler(0, m_EulerY, z);
                point = q * new Vector3(m_Radius, 0, 0) + closestPoint;
                point.y += m_Radius;
            }

            return point;
        }

        Vector3 GetOffset(Vector3 point)
        {
            point.x = 0;
            Vector3 offset = Roll(point);
            offset.z -= point.z;
            return offset;
        }

        Vector3 GetClosestPoint(Vector3 point)
        {
            float dx = point.x - m_PositionX;
            float dz = point.z - m_PositionZ;
            float dot = dx * m_DirectionX + dz * m_DirectionZ;
            point.x = m_PositionX + m_DirectionX * dot;
            point.z = m_PositionZ + m_DirectionZ * dot;
            return point;
        }

        float GetSide(Vector3 point)
        {
            float dx = point.x - m_PositionX;
            float dz = point.z - m_PositionZ;
            return dz * m_DirectionX - dx * m_DirectionZ;
        }
    }
}