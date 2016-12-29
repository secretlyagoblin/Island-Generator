using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager {

    int _blockSize = 256;
    int _totalSize = 4096;

    int _sizeX;
    int _sizeY;

    Coord _currentCoord = new Coord(0, 0);
        
    public Texture2D Texture
    {
        get { return _texture; }
    }

    Texture2D _texture;

    public TextureManager()
    {
        _sizeX = _totalSize / _blockSize;
        _sizeY = _sizeX;

        //Debug.Log(_sizeX + " " + _sizeY);

        _texture = new Texture2D(_totalSize, _totalSize);
    }

    public Rect ApplyTextureAndReturnDomain(Map map)
    {
        var ourMap = new Map(_blockSize, _blockSize);
        ourMap.WarpMapToMatch(map).Normalise();        

        _texture.SetPixels(_currentCoord.TileX * _blockSize, _currentCoord.TileY * _blockSize, _blockSize, _blockSize, ourMap.GetColours());
        _texture.Apply();

        GetNextCoord();

        return new Rect();
    }

    Coord GetNextCoord()
    {
        var x = _currentCoord.TileX + 1;
        var y = _currentCoord.TileY;

        if (x >= _sizeX)
        {
            x = 0;
            y++;
        }

        if(y >= _sizeY)
        {
            x = 0;
            y = 0;
        }

        _currentCoord = new Coord(x, y);

        return _currentCoord;       
    }


}
