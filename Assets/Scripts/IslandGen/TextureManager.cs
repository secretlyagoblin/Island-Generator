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

    public Rect ApplyTextureAndReturnDomain(Map map, Coord coord)
    {
        var ourMap = new Map(_blockSize, _blockSize);
        ourMap.WarpMapToMatch(map).Normalise();        

        _texture.SetPixels(coord.TileX * _blockSize, coord.TileY * _blockSize, _blockSize, _blockSize, ourMap.GetColours());
        _texture.Apply();

        return new Rect();
    }

    public Coord RequestCoord()
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

    public Rect RequestRect(Coord coord)
    {
        var pos = new Vector2(
            Mathf.InverseLerp(0, _sizeX, coord.TileX), 
            Mathf.InverseLerp(0, _sizeX, coord.TileX)
            );

        var size = new Vector2(
            Mathf.InverseLerp(1, _sizeX, coord.TileX),
            Mathf.InverseLerp(1, _sizeX, coord.TileX)
            );

        return new Rect(pos, size);
    }

}
