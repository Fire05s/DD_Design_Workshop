using UnityEngine;
using TMPro;
using System.Collections;
using Priestess;
public class DamageTextDriver : MonoBehaviour
{
    [SerializeField] Transform trf;
    [SerializeField] TMP_Text text;
    [SerializeField] float fadeSpeed;
    [SerializeField] float persistenceTime;
    [SerializeField] float flySpeed;
    private float r,g,b,a;
    private Vector2 direction = new Vector2(0, 1f);
    private bool fade;
    
    public void InitializeText(float damage, bool damagedArmor, bool isHealing)
    {
        a=1f;
        if (damagedArmor)
        {
            r=93/255f; g=234/255f; b=254/255f;
        }
        else if (isHealing)
        {
            r=0/255f; g=255/255f; b=0/255f;
        }
        else
        {
            r=1f; g=0f; b=0f;
        }
        text.color = new Color(r,g,b,a);
        SetText(damage);
        StartCoroutine(PersistenceCounter());
    }
    public void InitializeText(float damage, StatusEffectType status)
    {
        a=1f;
        SetColor(status);
        text.color = new Color(r,g,b,a);
        SetText(damage);
        StartCoroutine(PersistenceCounter());
    }
    public void InitializeText(float value)
    {
        r=255/255f; g=215/255f; b=55/255f; a=1f;
        text.color = new Color(r,g,b,a);
        SetText(value);
        StartCoroutine(PersistenceCounter());
    }
    void FixedUpdate()
    {
        trf.Translate(direction * Time.fixedDeltaTime * flySpeed);
        if (fade)
        {
            a -= fadeSpeed / 100;
            text.color = new Color(r,g,b,a);
            if (a <= 0) {Destroy(gameObject);}
        }
    }
    void SetColor(StatusEffectType type)
    {
        switch(type)
        {
            case StatusEffectType.Bleed:
                r=255/255f; g=0/255f; b=0/255f;
                break;
            case StatusEffectType.Poison:
                r=0/255f; g=255/255f; b=0/255f;
                break;
            case StatusEffectType.Burn:
                r=255/255f; g=171/255f; b=0/255f;
                break;
            case StatusEffectType.Withering:
                r=150/255f; g=0/255f; b=255/255f;
                break;
        }
    }
    IEnumerator PersistenceCounter()
    {
        fade = false;
        yield return new WaitForSeconds(persistenceTime);
        fade = true;
    }
    void SetText(float value)
    {
        if (value < 0)
        {
            text.text = $"{Mathf.Floor(value)}";
        }
        else
        {
            text.text = $"+{Mathf.Floor(value)}";
        }
        text.fontSize = 5 + Mathf.Clamp(value, 0f, 1000f) / 1000 * 5;
    }
}
