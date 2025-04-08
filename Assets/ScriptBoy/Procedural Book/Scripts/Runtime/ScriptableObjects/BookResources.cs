using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Rendering;
using UnityEditor;
#endif
namespace ScriptBoy.ProceduralBook
{
    //[CreateAssetMenu(menuName = " Script Boy/Procedural Book/ Book Resources", fileName = "Book Resources")]
    public sealed class BookResources : ScriptableObject
    {
        [HideInInspector]
        [SerializeField] Material m_DefaultPaperMaterial;

        [HideInInspector]
        [SerializeField] Material m_DefaultMetalMaterial;

        public static Material defaultPaperMaterial => instance.m_DefaultPaperMaterial;
        public static Material defaultMetalMaterial => instance.m_DefaultMetalMaterial;

        static BookResources s_Instance;
        static BookResources instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<BookResources>("Book Resources");
                }

                return s_Instance;
            }
        }

#if UNITY_EDITOR

        //[ContextMenu("Create Materials")]
        void Create()
        {
            if (m_DefaultPaperMaterial) AssetDatabase.RemoveObjectFromAsset(m_DefaultPaperMaterial);
            if (m_DefaultMetalMaterial) AssetDatabase.RemoveObjectFromAsset(m_DefaultMetalMaterial);

            m_DefaultPaperMaterial = new Material(MaterialUtility.defaultMaterial);
            m_DefaultPaperMaterial.name = "Paper";

            m_DefaultMetalMaterial = new Material(MaterialUtility.defaultMaterial);
            m_DefaultMetalMaterial.name = "Metal";

            AssetDatabase.AddObjectToAsset(m_DefaultPaperMaterial, this);
            AssetDatabase.AddObjectToAsset(m_DefaultMetalMaterial, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }


        //[ContextMenu("Hide Materials")]
        void HideMaterials()
        {
            m_DefaultPaperMaterial.hideFlags = HideFlags.HideInHierarchy;
            m_DefaultMetalMaterial.hideFlags = HideFlags.HideInHierarchy;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        //[ContextMenu("Show Materials")]
        void ShowMaterials()
        {
            m_DefaultPaperMaterial.hideFlags = HideFlags.None;
            m_DefaultMetalMaterial.hideFlags = HideFlags.None;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }


        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.delayCall += UpgradeDefaultMaterials;
            RenderPipelineManager.activeRenderPipelineTypeChanged += UpgradeDefaultMaterials;
        }

        static void UpgradeDefaultMaterials()
        {
            instance.UpgradeMaterials();
        }

        void UpgradeMaterials()
        {
            UpgradeMaterial(m_DefaultPaperMaterial);
            UpgradeMaterial(m_DefaultMetalMaterial);
        }

        void UpgradeMaterial(Material material)
        {
            Shader shader = MaterialUtility.defaultMaterial.shader;

            if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
            }
        }
#endif
    }
}