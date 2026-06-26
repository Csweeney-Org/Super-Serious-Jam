using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. We create a custom class to hold our animation data
[System.Serializable]
public class VFXAnimation
{
    public string AnimationName;
    
    [Tooltip("If you put your sprite sheet in a 'Resources' folder, just type the file name here (no extension) and it will auto-load!")]
    public string SpriteSheetName; 
    
    [Tooltip("You can leave this empty if you are using the SpriteSheetName above.")]
    public Sprite[] Frames;
    
    public float FrameRate = 12f; 
    public float Scale = 1.5f;
}

public class AnimationVFXManager : MonoBehaviour
{
    public static AnimationVFXManager Instance;

    [Header("Animation Library")]
    public VFXAnimation[] Animations;

    // A dictionary lets us instantly look up animations by their string name
    private Dictionary<string, VFXAnimation> animationDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        animationDict = new Dictionary<string, VFXAnimation>();
        
        foreach (var anim in Animations)
        {
            if (anim.Scale <= 0f) anim.Scale = 1.5f;
            if (anim.FrameRate <= 0f) anim.FrameRate = 12f;

            // --- THE FIX: Aggressive Auto-Loading ---
            // If we provided a SpriteSheetName, ALWAYS force-load it. 
            // This prevents Unity's array-duplication bug from trapping old sprites!
            if (!string.IsNullOrEmpty(anim.SpriteSheetName))
            {
                anim.Frames = Resources.LoadAll<Sprite>(anim.SpriteSheetName);
                
                if (anim.Frames == null || anim.Frames.Length == 0)
                {
                    Debug.LogError($"Could not find sliced sprites for '{anim.SpriteSheetName}' in a Resources folder!");
                }
            }
            // ----------------------------------------

            if (!animationDict.ContainsKey(anim.AnimationName))
            {
                animationDict.Add(anim.AnimationName, anim);
            }
        }
    }

    public void PlayAnimation(string animName, Vector3 spawnPosition)
    {
        Debug.Log($"VFX Manager was asked to play: '{animName}'");
        if (!animationDict.TryGetValue(animName, out VFXAnimation animData) || animData.Frames.Length == 0)
        {
            Debug.LogWarning($"VFX Manager: Could not find an animation named '{animName}' or it has no frames.");
            return;
        }
        Debug.Log($"SUCCESS! Found blueprint for '{animName}'. It contains {animData.Frames.Length} frames. The very first frame is named: '{animData.Frames[0].name}'");

        // 1. Create the blank GameObject
        GameObject vfxObject = new GameObject($"VFX_{animName}");
        vfxObject.transform.position = spawnPosition + (Vector3.up * 1.5f);
        vfxObject.transform.localScale = Vector3.one * animData.Scale;

        // 2. The Chaotic Billboard Effect
        ApplyChaoticBillboarding(vfxObject.transform);

        // 3. Add SpriteRenderer and start playback
        SpriteRenderer sr = vfxObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10; 

        StartCoroutine(AnimateSpriteSheet(vfxObject, sr, animData));
    }

    private void ApplyChaoticBillboarding(Transform vfxTransform)
    {
        if (Camera.main != null)
        {
            // Base rotation: Look directly at the camera
            Quaternion baseRotation = Camera.main.transform.rotation;

            // Apply deviation
            // Z-Axis (Roll): Tilts the sprite left or right like a comic book "BAM!"
            float randomTilt = Random.Range(-25f, 25f); 
            // Y-Axis (Yaw): Twists it slightly left or right
            float randomTwist = Random.Range(-15f, 15f);

            // We explicitly leave X at 0 so it never tilts backward into the sky/floor!
            Quaternion randomDeviation = Quaternion.Euler(0f, randomTwist, randomTilt);

            vfxTransform.rotation = baseRotation * randomDeviation;
        }
        else
        {
            vfxTransform.rotation = Quaternion.identity;
        }
    }

    private IEnumerator AnimateSpriteSheet(GameObject targetObj, SpriteRenderer sr, VFXAnimation anim)
    {
        // Calculate how long each frame should stay on screen
        float timePerFrame = 1f / anim.FrameRate;

        // Loop through the array of sprites
        for (int i = 0; i < anim.Frames.Length; i++)
        {
            if (targetObj == null) yield break; // Failsafe if destroyed early

            sr.sprite = anim.Frames[i];
            
            // Wait for the exact duration of this frame
            yield return new WaitForSeconds(timePerFrame);
        }

        // The loop finished, the animation is over. Destroy the object.
        if (targetObj != null) Destroy(targetObj);
    }
}
