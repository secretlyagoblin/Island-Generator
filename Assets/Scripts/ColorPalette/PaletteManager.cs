using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PaletteManager {

    static bool _loaded = false;

    static List<ColorPalette> _colorPalettes = new List<ColorPalette>();

	// Use this for initialization
	static void Load () {
        _colorPalettes.AddRange(Resources.LoadAll<ColorPalette>(""));
        _loaded = true;
	}

    public static ColorPalette GetPalette()
    {
        if (!_loaded)
            Load();

        if (_colorPalettes.Count <= 0)
            return null;

        return _colorPalettes[0];

    }

}
