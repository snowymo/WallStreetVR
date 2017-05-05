using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class CitywalkCameraController : MonoBehaviour {

    private bool isCameraEnabled;

    public Camera ScreenshotCamera;
    public GameObject PreviewScreen;
    public Material BlitMaterial;
    public GameObject QuadPrefab;
    public Material QuadMaterial;

    public Vector2 PictureSize = new Vector2(1600, 1200);
    public int AntiAliasing = 8;

    private RenderTexture sourceRenderTexture;
    private RenderTexture blitRenderTexture;

    public static CitywalkCameraController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        sourceRenderTexture = new RenderTexture((int) PictureSize.x, (int) PictureSize.y, 0, RenderTextureFormat.ARGB32){
            antiAliasing = AntiAliasing,
        };

        blitRenderTexture = new RenderTexture((int) PictureSize.x, (int) PictureSize.y, 0, RenderTextureFormat.ARGB32){
        };
    }
	
	void Update ()
	{
	    bool cameraEnabled = GameModeManager.Instance.CitywalkCameraEnabled;
        ScreenshotCamera.gameObject.SetActive(cameraEnabled);
        PreviewScreen.gameObject.SetActive(cameraEnabled);

        if (cameraEnabled && InputManager.Instance.GetButtonDown(InputManager.Button.Capture)) {
            PreviewScreen.GetComponent<SoundTriggerByScript>().Play();
	        createPreview();
	        StartCoroutine(takePicture());
	    }
	}

    private void createPreview() {
        RenderTexture previewRenderTexture = ScreenshotCamera.targetTexture;
        int previewWidth = previewRenderTexture.width;
        int previewHeight = previewRenderTexture.height;

        RenderTexture originalActiveRenderTexture = RenderTexture.active;
        RenderTexture.active = previewRenderTexture;
        var previewTexture = new Texture2D(previewWidth, previewHeight, TextureFormat.ARGB32, false, true);
        previewTexture.ReadPixels(new Rect(0, 0, previewWidth, previewHeight), 0, 0);
        previewTexture.Apply();
        RenderTexture.active = originalActiveRenderTexture;

        var newQuad = Instantiate(QuadPrefab, transform.position, transform.rotation) as GameObject;
        newQuad.name = "Keyframe_" + MapModelSwitcher.Instance.currentMapModel.Name;
        newQuad.transform.localScale = transform.lossyScale;

        var material = new Material(QuadMaterial);
        newQuad.GetComponentInChildren<Renderer>().material = material;
        material.mainTexture = previewTexture;
    }

    private IEnumerator takePicture() {

        int width = blitRenderTexture.width;
        int height = blitRenderTexture.height;

        RenderTexture originalRenderTarget = ScreenshotCamera.targetTexture;
        RenderTexture originalActiveRenderTexture = RenderTexture.active;

        ScreenshotCamera.targetTexture = sourceRenderTexture;

        yield return null;

        Graphics.Blit(sourceRenderTexture, blitRenderTexture, BlitMaterial);

        yield return null;

        RenderTexture.active = blitRenderTexture;
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        RenderTexture.active = originalActiveRenderTexture;
        ScreenshotCamera.targetTexture = originalRenderTarget;

        yield return null;

        byte[] jpg = texture.EncodeToJPG(85);

        string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/CityVR";
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }

        string fileName = string.Format("{0}-{1:yyyy-MM-dd_hh-mm-ss-tt}", MapModelSwitcher.Instance.currentMapModel.Name, DateTime.Now);
        string path = string.Format("{0}/{1}.jpg", dir, fileName);
        File.WriteAllBytes(path, jpg);

        Debug.Log("Saved picture to " + path);

        UnityAnalytics.TakePicture(ScreenshotCamera.transform.position);

        DestroyImmediate(texture);
    }
}
