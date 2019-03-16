using Godot;
using Settworks.Hexagons;

public struct CellInfo
{
    public CellInfo(HexCoord coord, Node2D tile)
    {
        Coord = coord;
        Tile = tile;
    }

    public HexCoord Coord { get; }
    public Node2D Tile { get; }
}

