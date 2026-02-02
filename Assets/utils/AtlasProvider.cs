using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasProvider : MonoBehaviour
{
    public SpriteAtlas[] atlases;

    private readonly Dictionary<string, Sprite> cache = new();

    // trova uno sprite.
    public Sprite this[string name, bool cacheInteract = true]
    {
        get{
            if (cacheInteract)
                this.GetFromCache(name);

            foreach (SpriteAtlas atlas in atlases)
            {
                Sprite sprite = atlas.GetSprite(name);
                if (sprite != null)
                {
                    if (cacheInteract)
                    {
                        cache.TryAdd(name, sprite);
                    } 
                    return sprite; 
                }
            }

            throw new System.Exception($"Lo sprite '{name}' non esiste.");
        }

        private set => throw new System.Exception("Non settare valori qui, settali nell'editor");
    }

    // prende un suggerimento sul nome dell'atlas; se non lo trova, usa un get normale come fallback.
    public Sprite this[string hint, string name, bool cacheInteract = true]
    {
        get{
            if (cacheInteract)
                this.GetFromCache(name);

            var hintedSprites = 
                atlases.AsEnumerable()
                .Where(x => x.name == hint)
                .Select(a => a.GetSprite(name))
                .Where(s => s != null)
                .ToArray();

            if (hintedSprites.Length != 0)
            {
                Sprite hintedSprite = hintedSprites[0];

                if (cacheInteract)
                {
                    cache.TryAdd(name, hintedSprite);
                } 

                return hintedSprite;
            } else
            {
                Debug.LogWarning("Risoluzione suggerita in AtlasProvider ha fallito, falling back su risoluzone normale.");
                return this[name, cacheInteract];
            }
        }

        private set => throw new System.Exception("Non settare valori qui, settali nell'editor");

    }

    public void ClearCache()
    {
        cache.Clear();
    }
    
    #nullable enable
    Sprite? GetFromCache(string name)
    {
        if (cache.ContainsKey(name))
        {
            return cache[name];
        } else
        {
            return null;
        }
    }

    private void LoadAllCache()
    {
        throw new Exception("TODO: aggiungere una maniera di caricare tutti gli sprite nella cache; rendere poi il metodo public");
    }
}