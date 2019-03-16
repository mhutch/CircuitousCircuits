using Godot;
using Settworks.Hexagons;
using System;

public class HexMap
{
    CellInfo[][] map;

    public HexMap(int edgeSize)
    {
        int size = edgeSize * 2 - 1;
        map = new CellInfo[size][];

        for (int r = 0; r < size; r++)
        {
            map[r] = (new CellInfo[size - Math.Abs(edgeSize - 1 - r)]);
        }
    }

    public void Initialize(Func<HexCoord,CellInfo> creator)
    {
        int edgeSize = (map.Length + 1) / 2;
        for (int r = 0; r < map.Length; r++)
        {
            var arr = map[r];
            int offset = -Math.Max(0, edgeSize - 1 - r);
            for (int j = 0; j < arr.Length; j++)
            {
                arr[j] = creator (new HexCoord(j - offset, r));
            }
        }
    }

    public void SetCell(CellInfo info)
    {
        var c = info.Coord;
        var row = map[c.r];
        int edgeSize = (map.Length + 1) / 2;
        var index = c.q - Math.Max(0, edgeSize - 1 - c.r);
        row[index] = info;
    }

    public CellInfo? TryGetCell(HexCoord c)
    {
        if (c.r >= 0 && c.r < map.Length)
        {
            var row = map[c.r];
            int edgeSize = (map.Length + 1) / 2;
            var index = c.q - Math.Max(0, edgeSize - 1 - c.r);
            if (index >= 0 && index < row.Length)
            {
                return row[index];
            }
        }
        return null;
    }
}