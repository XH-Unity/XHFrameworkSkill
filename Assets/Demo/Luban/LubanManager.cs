using Luban;
using UnityEngine;

public class LubanManager
{
    private static LubanManager _instance;
    public static LubanManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LubanManager();
                _instance.Load();
            }
            return _instance;
        }
    }

    public cfg.Tables Tables { get; private set; }

    private void Load()
    {
        Tables = new cfg.Tables(name =>
        {
            var textAsset = Resources.Load<TextAsset>($"Luban/{name}");
            return new ByteBuf(textAsset.bytes);
        });
    }
}
