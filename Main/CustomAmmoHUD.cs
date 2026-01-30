using UnityEngine.UI;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Shotgun.Utility;
using Il2CppTLD.Gear;

namespace Shotgun
{
    [RegisterTypeInIl2Cpp(false)]
    public class CustomAmmoHUD : MonoBehaviour
    {
        public class HUDPreset
        {
            public Sprite LoadedSprite;
            public Sprite UnloadedSprite;
            public float IconHeight;
            public float IconWidth;
            public float IconSpacing;
            public int IconsPerRow;
        }


        private static CustomAmmoHUD _Current = null;

        public static CustomAmmoHUD Current => GetOrCreteCustomAmmoHUD();

        private static CustomAmmoHUD GetOrCreteCustomAmmoHUD()
        {
            if (_Current != null) return _Current;
            GameObject obj = new GameObject("CustomAmmoHUD");
            _Current = obj.AddComponent<CustomAmmoHUD>();
            _Current.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            GameObject.DontDestroyOnLoad(obj);
            return _Current;
        }

        private Dictionary<string, HUDPreset> mHUDPresets = new();
        private GameObject mCanvasObject;
        private Canvas mCanvas;
        private CanvasScaler mCanvasScaler;
        private GameObject mIconPanel;
        private GridLayoutGroup mGridLayoutGroup;
        private RectTransform mIconPanelRect;
        private List<Image> mIcons = new();
        private string mCurrentHUDPreset;
        private int mLastAmmoCount;
        private float mSinceLastCheck = 0f;

        private GearItem mItemInHands;
        private GunItem mGunInHands;
        private GearItemData mLoadedAmmo;

        public CustomAmmoHUD() { }
        public CustomAmmoHUD(IntPtr pointer) : base(pointer) { }

        public string CurrentHUDPreset => mCurrentHUDPreset;
        public GearItem ItemInHands => mItemInHands;
        public GunItem GunInHands => mGunInHands;
        public GearItemData LoadedAmmo => mLoadedAmmo;


        private void Awake()
        {
            mCanvasObject = new GameObject("Canvas");
            mCanvasObject.transform.SetParent(null, false);

            RectTransform canvasRect = mCanvasObject.AddComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            mCanvas = mCanvasObject.AddComponent<Canvas>();
            mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mCanvas.sortingOrder = 1000;

            mCanvasScaler = mCanvasObject.AddComponent<CanvasScaler>();
            mCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            mCanvasScaler.referenceResolution = new Vector2(1920, 1080);
            mCanvasScaler.matchWidthOrHeight = 0.5f; 

            mCanvasObject.AddComponent<GraphicRaycaster>();

            mIconPanel = new GameObject("IconPanel");
            mIconPanel.transform.SetParent(mCanvasObject.transform, false);

            mIconPanelRect = mIconPanel.AddComponent<RectTransform>();
            mIconPanelRect.anchorMin = new Vector2(1f, 0f);
            mIconPanelRect.anchorMax = new Vector2(1f, 0f);
            mIconPanelRect.pivot = new Vector2(1f, 0f);

            mGridLayoutGroup = mIconPanel.AddComponent<GridLayoutGroup>();
            mGridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerRight;
            mGridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            mGridLayoutGroup.childAlignment = TextAnchor.LowerRight;
            mGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            mLastAmmoCount = 0;

            UpdateIconPanelPosition();
        }

        public void UpdateIconPanelPosition()
        {
            if (Settings.Instance == null) return;

            float x = -Screen.width * ((float)Settings.Instance.CustomAmmoHUDXOffset / 1920f);
            float y = Screen.height * ((float)Settings.Instance.CustomAmmoHUDYOffset / 1080f);

            mIconPanelRect.anchoredPosition = new Vector2(x, y);

            foreach (var preset in mHUDPresets.Values)
            {
                preset.IconHeight = Settings.Instance.CustomAmmoHUDIconHeight;
                preset.IconWidth = preset.IconHeight / 3;
                preset.IconSpacing = preset.IconWidth * Settings.Instance.CustomAmmoHUDIconSpacingRatio;
            }

            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateIconPanelPosition();
        }

        public void Update()
        {
            mSinceLastCheck += Time.deltaTime;
            if (mSinceLastCheck <= 0.25f) return;
            mSinceLastCheck = 0f;
            Refresh();
        }

        public void Refresh()
        {
            if (GameManager.m_IsPaused)
            {
                Reset();
                return;
            }

            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            if (playerManager == null)
            {
                Reset();
                return;
            }

            mItemInHands = playerManager.m_ItemInHands;
            if (mItemInHands == null)
            {
                Reset();
                return;
            }

            mGunInHands = mItemInHands.gameObject.GetComponent<GunItem>();
            if (mGunInHands == null)
            {
                Reset();
                return;
            }

            if (!mGunInHands.TryGetSelectedAmmo(out mLoadedAmmo))
            {
                Reset();
                return;
            }

            string hudPresetName = $"{mItemInHands.name}.{mLoadedAmmo.name}";

            if (mCurrentHUDPreset == hudPresetName && mLastAmmoCount == mGunInHands.m_Clip.Count) return;
            mCurrentHUDPreset = hudPresetName;
            mLastAmmoCount = mGunInHands.m_Clip.Count;

            if (mHUDPresets.TryGetValue(hudPresetName, out HUDPreset preset))
            {
                ActivateHud(mGunInHands, preset);
            }
            else
            {
                DeactivateHUD();
            }
        }

        private void Reset()
        {
            mCurrentHUDPreset = string.Empty;
            mLastAmmoCount = 0;
        }
   

        public bool TryRegisterHUDPreset(string gunGearName, string ammoGearName, HUDPreset preset)
        {
            string pairingName = gunGearName + "." + ammoGearName;
            if (mHUDPresets.Keys.Contains(pairingName))
            {
                MelonLogger.Msg($"Duplicate weapon.ammo pairing {pairingName} while trying to register for custom ammo HUD, Skipping!");
                return true;
            }

            mHUDPresets.Add(pairingName, preset);
            return true;
        }


        private void ActivateHud(GunItem gun, HUDPreset preset)
        {
            int existingIconCount = mIcons.Count;
            for (int i = 0, iMax = gun.m_ClipSize; i < iMax; i++)
            {
                if (i >= existingIconCount)
                {
                    GameObject newIconObject = new GameObject($"CustomAmmoIcon{i}");
                    newIconObject.transform.SetParent(mIconPanel.transform);
                    RectTransform iconRect = newIconObject.AddComponent<RectTransform>();
                    iconRect.localScale = Vector3.one;
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.sizeDelta = Vector2.zero;
                    Image newIcon = newIconObject.AddComponent<Image>();
                    newIcon.preserveAspect = false;
                    newIcon.type = Image.Type.Simple;
                    newIcon.sprite = i < mLastAmmoCount ? preset.LoadedSprite : preset.UnloadedSprite;
                    mIcons.Add(newIcon);
                }
                else
                {
                    Image existingIcon = mIcons[i];
                    GameObject existingIconObject = existingIcon.gameObject;
                    RectTransform existingIconRect = existingIconObject.GetComponent<RectTransform>();
                    existingIconRect.localScale = Vector3.one;
                    existingIconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    existingIconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    existingIconRect.sizeDelta = Vector2.zero;
                    existingIcon.preserveAspect = false;
                    existingIcon.type = Image.Type.Simple;
                    existingIcon.sprite = i < mLastAmmoCount ? preset.LoadedSprite : preset.UnloadedSprite;
                }
            }

            mGridLayoutGroup.cellSize = new Vector2(preset.IconWidth, preset.IconHeight);
            mGridLayoutGroup.spacing = new Vector2(preset.IconSpacing, preset.IconSpacing);
            mGridLayoutGroup.constraintCount = preset.IconsPerRow;

            for (int i = mIcons.Count - 1, iMin = gun.m_ClipSize; i >= iMin; i--)
            {
                GameObject.Destroy(mIcons[i].gameObject);
                mIcons.RemoveAt(i);
            }
        }


        private void DeactivateHUD()
        {
            for (int i = mIcons.Count - 1, iMin = 0; i >= iMin; i--)
            {
                GameObject.Destroy(mIcons[i].gameObject);
                mIcons.RemoveAt(i);
            }
        }
    }
}