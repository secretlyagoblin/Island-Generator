using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager {

    int _blockSize = 256;
    int _totalSize = 4096/2;

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

    public void ApplyTexture(Layer map, Coord coord)
    {
        var ourMap = new Layer(_blockSize, _blockSize);
        ourMap.WarpMapToMatch(map);        

        _texture.SetPixels(coord.TileX * _blockSize, coord.TileY * _blockSize, _blockSize, _blockSize, ourMap.GetColours());
        _texture.Apply();
    }

    public Coord RequestCoord()
    {
        var oldCoord = _currentCoord;

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

        return oldCoord;       
    }

    public Rect RequestRect(Coord coord)
    {
        var pos = new Vector2(
            Mathf.InverseLerp(0, _sizeX, coord.TileX), 
            Mathf.InverseLerp(0, _sizeY, coord.TileY)
            );

        var size = new Vector2(
            Mathf.InverseLerp(0, _sizeX, 1),
            Mathf.InverseLerp(0, _sizeY, 1)
            );

        return new Rect(pos, size);
    }

}
