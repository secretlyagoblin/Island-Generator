using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager {

    int _blockSize = 256;
    int _totalSize = 8192;

    int _sizeX;
    int _sizeY;

    Coord _currentCoord = new Coord(0, 0);



    public TextureManager()
    {
        _sizeX = _totalSize / _blockSize;
        _sizeY = _sizeX;
    }

    public Rect ApplyTextureAndReturnDomain(Map map)
    {
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
