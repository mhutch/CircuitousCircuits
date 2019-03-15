// Generated code -- CC0 -- No Rights Reserved -- http://www.redblobgames.com/grids/hexagons/

using System;
using System.Linq;
using System.Collections.Generic;

struct Point
{
    public Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
    public readonly double x;
    public readonly double y;
}

struct Hex
{
    public Hex(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public readonly int q;
    public readonly int r;
    public readonly int s;

    public Hex Add(Hex b)
    {
        return new Hex(q + b.q, r + b.r, s + b.s);
    }


    public Hex Subtract(Hex b)
    {
        return new Hex(q - b.q, r - b.r, s - b.s);
    }


    public Hex Scale(int k)
    {
        return new Hex(q * k, r * k, s * k);
    }


    public Hex RotateLeft()
    {
        return new Hex(-s, -q, -r);
    }


    public Hex RotateRight()
    {
        return new Hex(-r, -s, -q);
    }

    static public List<Hex> directions = new List<Hex> { new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1) };

    static public Hex Direction(int direction)
    {
        return Hex.directions[direction];
    }


    public Hex Neighbor(int direction)
    {
        return Add(Hex.Direction(direction));
    }

    static public List<Hex> diagonals = new List<Hex> { new Hex(2, -1, -1), new Hex(1, -2, 1), new Hex(-1, -1, 2), new Hex(-2, 1, 1), new Hex(-1, 2, -1), new Hex(1, 1, -2) };

    public Hex DiagonalNeighbor(int direction)
    {
        return Add(Hex.diagonals[direction]);
    }


    public int Length()
    {
        return (int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2);
    }


    public int Distance(Hex b)
    {
        return Subtract(b).Length();
    }

}

struct FractionalHex
{
    public FractionalHex(double q, double r, double s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public readonly double q;
    public readonly double r;
    public readonly double s;

    public Hex HexRound()
    {
        int qi = (int)(Math.Round(q));
        int ri = (int)(Math.Round(r));
        int si = (int)(Math.Round(s));
        double q_diff = Math.Abs(qi - q);
        double r_diff = Math.Abs(ri - r);
        double s_diff = Math.Abs(si - s);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            qi = -ri - si;
        }
        else
            if (r_diff > s_diff)
        {
            ri = -qi - si;
        }
        else
        {
            si = -qi - ri;
        }
        return new Hex(qi, ri, si);
    }


    public FractionalHex HexLerp(FractionalHex b, double t)
    {
        return new FractionalHex(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
    }


    static public List<Hex> HexLinedraw(Hex a, Hex b)
    {
        int N = a.Distance(b);
        FractionalHex a_nudge = new FractionalHex(a.q + 0.000001, a.r + 0.000001, a.s - 0.000002);
        FractionalHex b_nudge = new FractionalHex(b.q + 0.000001, b.r + 0.000001, b.s - 0.000002);
        List<Hex> results = new List<Hex> { };
        double step = 1.0 / Math.Max(N, 1);
        for (int i = 0; i <= N; i++)
        {
            results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
        }
        return results;
    }

}

struct OffsetCoord
{
    public OffsetCoord(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
    public readonly int col;
    public readonly int row;
    static public int EVEN = 1;
    static public int ODD = -1;

    static public OffsetCoord QoffsetFromCube(int offset, Hex h)
    {
        int col = h.q;
        int row = h.r + (int)((h.q + offset * (h.q & 1)) / 2);
        return new OffsetCoord(col, row);
    }


    static public Hex QoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col;
        int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
        int s = -q - r;
        return new Hex(q, r, s);
    }


    static public OffsetCoord RoffsetFromCube(int offset, Hex h)
    {
        int col = h.q + (int)((h.r + offset * (h.r & 1)) / 2);
        int row = h.r;
        return new OffsetCoord(col, row);
    }


    static public Hex RoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col - (int)((h.row + offset * (h.row & 1)) / 2);
        int r = h.row;
        int s = -q - r;
        return new Hex(q, r, s);
    }

}

struct DoubledCoord
{
    public DoubledCoord(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
    public readonly int col;
    public readonly int row;

    static public DoubledCoord QdoubledFromCube(Hex h)
    {
        int col = h.q;
        int row = 2 * h.r + h.q;
        return new DoubledCoord(col, row);
    }


    public Hex QdoubledToCube()
    {
        int q = col;
        int r = (int)((row - col) / 2);
        int s = -q - r;
        return new Hex(q, r, s);
    }


    static public DoubledCoord RdoubledFromCube(Hex h)
    {
        int col = 2 * h.q + h.r;
        int row = h.r;
        return new DoubledCoord(col, row);
    }


    public Hex RdoubledToCube()
    {
        int q = (int)((col - row) / 2);
        int r = row;
        int s = -q - r;
        return new Hex(q, r, s);
    }

}

struct Orientation
{
    public Orientation(double f0, double f1, double f2, double f3, double b0, double b1, double b2, double b3, double start_angle)
    {
        this.f0 = f0;
        this.f1 = f1;
        this.f2 = f2;
        this.f3 = f3;
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
        this.b3 = b3;
        this.start_angle = start_angle;
    }
    public readonly double f0;
    public readonly double f1;
    public readonly double f2;
    public readonly double f3;
    public readonly double b0;
    public readonly double b1;
    public readonly double b2;
    public readonly double b3;
    public readonly double start_angle;
}

struct Layout
{
    public Layout(Orientation orientation, Point size, Point origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }
    public readonly Orientation orientation;
    public readonly Point size;
    public readonly Point origin;
    static public Orientation pointy = new Orientation(Math.Sqrt(3.0), Math.Sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.Sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);
    static public Orientation flat = new Orientation(3.0 / 2.0, 0.0, Math.Sqrt(3.0) / 2.0, Math.Sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.Sqrt(3.0) / 3.0, 0.0);

    public Point HexToPixel(Hex h)
    {
        Orientation M = orientation;
        double x = (M.f0 * h.q + M.f1 * h.r) * size.x;
        double y = (M.f2 * h.q + M.f3 * h.r) * size.y;
        return new Point(x + origin.x, y + origin.y);
    }


    public FractionalHex PixelToHex(Point p)
    {
        Orientation M = orientation;
        Point pt = new Point((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        double q = M.b0 * pt.x + M.b1 * pt.y;
        double r = M.b2 * pt.x + M.b3 * pt.y;
        return new FractionalHex(q, r, -q - r);
    }


    public Point HexCornerOffset(int corner)
    {
        Orientation M = orientation;
        double angle = 2.0 * Math.PI * (M.start_angle - corner) / 6.0;
        return new Point(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
    }


    public List<Point> PolygonCorners(Hex h)
    {
        List<Point> corners = new List<Point> { };
        Point center = HexToPixel(h);
        for (int i = 0; i < 6; i++)
        {
            Point offset = HexCornerOffset(i);
            corners.Add(new Point(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }

}



// Tests


struct Tests
{

    static public void EqualHex(String name, Hex a, Hex b)
    {
        if (!(a.q == b.q && a.s == b.s && a.r == b.r))
        {
            Tests.Complain(name);
        }
    }


    static public void EqualOffsetcoord(String name, OffsetCoord a, OffsetCoord b)
    {
        if (!(a.col == b.col && a.row == b.row))
        {
            Tests.Complain(name);
        }
    }


    static public void EqualDoubledcoord(String name, DoubledCoord a, DoubledCoord b)
    {
        if (!(a.col == b.col && a.row == b.row))
        {
            Tests.Complain(name);
        }
    }


    static public void EqualInt(String name, int a, int b)
    {
        if (!(a == b))
        {
            Tests.Complain(name);
        }
    }


    static public void EqualHexArray(String name, List<Hex> a, List<Hex> b)
    {
        Tests.EqualInt(name, a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            Tests.EqualHex(name, a[i], b[i]);
        }
    }


    static public void TestHexArithmetic()
    {
        Tests.EqualHex("hex_add", new Hex(4, -10, 6), new Hex(1, -3, 2).Add(new Hex(3, -7, 4)));
        Tests.EqualHex("hex_subtract", new Hex(-2, 4, -2), new Hex(1, -3, 2).Subtract(new Hex(3, -7, 4)));
    }


    static public void TestHexDirection()
    {
        Tests.EqualHex("hex_direction", new Hex(0, -1, 1), Hex.Direction(2));
    }


    static public void TestHexNeighbor()
    {
        Tests.EqualHex("hex_neighbor", new Hex(1, -3, 2), new Hex(1, -2, 1).Neighbor(2));
    }


    static public void TestHexDiagonal()
    {
        Tests.EqualHex("hex_diagonal", new Hex(-1, -1, 2), new Hex(1, -2, 1).DiagonalNeighbor(3));
    }


    static public void TestHexDistance()
    {
        Tests.EqualInt("hex_distance", 7, new Hex(3, -7, 4).Distance(new Hex(0, 0, 0)));
    }


    static public void TestHexRotateRight()
    {
        Tests.EqualHex("hex_rotate_right", new Hex(1, -3, 2).RotateRight(), new Hex(3, -2, -1));
    }


    static public void TestHexRotateLeft()
    {
        Tests.EqualHex("hex_rotate_left", new Hex(1, -3, 2).RotateLeft(), new Hex(-2, -1, 3));
    }


    static public void TestHexRound()
    {
        FractionalHex a = new FractionalHex(0.0, 0.0, 0.0);
        FractionalHex b = new FractionalHex(1.0, -1.0, 0.0);
        FractionalHex c = new FractionalHex(0.0, -1.0, 1.0);
        Tests.EqualHex("hex_round 1", new Hex(5, -10, 5), new FractionalHex(0.0, 0.0, 0.0).HexLerp(new FractionalHex(10.0, -20.0, 10.0), 0.5).HexRound());
        Tests.EqualHex("hex_round 2", a.HexRound(), a.HexLerp(b, 0.499).HexRound());
        Tests.EqualHex("hex_round 3", b.HexRound(), a.HexLerp(b, 0.501).HexRound());
        Tests.EqualHex("hex_round 4", a.HexRound(), new FractionalHex(a.q * 0.4 + b.q * 0.3 + c.q * 0.3, a.r * 0.4 + b.r * 0.3 + c.r * 0.3, a.s * 0.4 + b.s * 0.3 + c.s * 0.3).HexRound());
        Tests.EqualHex("hex_round 5", c.HexRound(), new FractionalHex(a.q * 0.3 + b.q * 0.3 + c.q * 0.4, a.r * 0.3 + b.r * 0.3 + c.r * 0.4, a.s * 0.3 + b.s * 0.3 + c.s * 0.4).HexRound());
    }


    static public void TestHexLinedraw()
    {
        Tests.EqualHexArray("hex_linedraw", new List<Hex> { new Hex(0, 0, 0), new Hex(0, -1, 1), new Hex(0, -2, 2), new Hex(1, -3, 2), new Hex(1, -4, 3), new Hex(1, -5, 4) }, FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(1, -5, 4)));
    }


    static public void TestLayout()
    {
        Hex h = new Hex(3, 4, -7);
        Layout flat = new Layout(Layout.flat, new Point(10.0, 15.0), new Point(35.0, 71.0));
        Tests.EqualHex("layout", h, flat.PixelToHex(flat.HexToPixel(h)).HexRound());
        Layout pointy = new Layout(Layout.pointy, new Point(10.0, 15.0), new Point(35.0, 71.0));
        Tests.EqualHex("layout", h, pointy.PixelToHex(pointy.HexToPixel(h)).HexRound());
    }


    static public void TestOffsetRoundtrip()
    {
        Hex a = new Hex(3, 4, -7);
        OffsetCoord b = new OffsetCoord(1, -3);
        Tests.EqualHex("conversion_roundtrip even-q", a, OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, a)));
        Tests.EqualOffsetcoord("conversion_roundtrip even-q", b, OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, b)));
        Tests.EqualHex("conversion_roundtrip odd-q", a, OffsetCoord.QoffsetToCube(OffsetCoord.ODD, OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, a)));
        Tests.EqualOffsetcoord("conversion_roundtrip odd-q", b, OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, OffsetCoord.QoffsetToCube(OffsetCoord.ODD, b)));
        Tests.EqualHex("conversion_roundtrip even-r", a, OffsetCoord.RoffsetToCube(OffsetCoord.EVEN, OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, a)));
        Tests.EqualOffsetcoord("conversion_roundtrip even-r", b, OffsetCoord.RoffsetFromCube(OffsetCoord.EVEN, OffsetCoord.RoffsetToCube(OffsetCoord.EVEN, b)));
        Tests.EqualHex("conversion_roundtrip odd-r", a, OffsetCoord.RoffsetToCube(OffsetCoord.ODD, OffsetCoord.RoffsetFromCube(OffsetCoord.ODD, a)));
        Tests.EqualOffsetcoord("conversion_roundtrip odd-r", b, OffsetCoord.RoffsetFromCube(OffsetCoord.ODD, OffsetCoord.RoffsetToCube(OffsetCoord.ODD, b)));
    }


    static public void TestOffsetFromCube()
    {
        Tests.EqualOffsetcoord("offset_from_cube even-q", new OffsetCoord(1, 3), OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, new Hex(1, 2, -3)));
        Tests.EqualOffsetcoord("offset_from_cube odd-q", new OffsetCoord(1, 2), OffsetCoord.QoffsetFromCube(OffsetCoord.ODD, new Hex(1, 2, -3)));
    }


    static public void TestOffsetToCube()
    {
        Tests.EqualHex("offset_to_cube even-", new Hex(1, 2, -3), OffsetCoord.QoffsetToCube(OffsetCoord.EVEN, new OffsetCoord(1, 3)));
        Tests.EqualHex("offset_to_cube odd-q", new Hex(1, 2, -3), OffsetCoord.QoffsetToCube(OffsetCoord.ODD, new OffsetCoord(1, 2)));
    }


    static public void TestDoubledRoundtrip()
    {
        Hex a = new Hex(3, 4, -7);
        DoubledCoord b = new DoubledCoord(1, -3);
        Tests.EqualHex("conversion_roundtrip doubled-q", a, DoubledCoord.QdoubledFromCube(a).QdoubledToCube());
        Tests.EqualDoubledcoord("conversion_roundtrip doubled-q", b, DoubledCoord.QdoubledFromCube(b.QdoubledToCube()));
        Tests.EqualHex("conversion_roundtrip doubled-r", a, DoubledCoord.RdoubledFromCube(a).RdoubledToCube());
        Tests.EqualDoubledcoord("conversion_roundtrip doubled-r", b, DoubledCoord.RdoubledFromCube(b.RdoubledToCube()));
    }


    static public void TestDoubledFromCube()
    {
        Tests.EqualDoubledcoord("doubled_from_cube doubled-q", new DoubledCoord(1, 5), DoubledCoord.QdoubledFromCube(new Hex(1, 2, -3)));
        Tests.EqualDoubledcoord("doubled_from_cube doubled-r", new DoubledCoord(4, 2), DoubledCoord.RdoubledFromCube(new Hex(1, 2, -3)));
    }


    static public void TestDoubledToCube()
    {
        Tests.EqualHex("doubled_to_cube doubled-q", new Hex(1, 2, -3), new DoubledCoord(1, 5).QdoubledToCube());
        Tests.EqualHex("doubled_to_cube doubled-r", new Hex(1, 2, -3), new DoubledCoord(4, 2).RdoubledToCube());
    }


    static public void TestAll()
    {
        Tests.TestHexArithmetic();
        Tests.TestHexDirection();
        Tests.TestHexNeighbor();
        Tests.TestHexDiagonal();
        Tests.TestHexDistance();
        Tests.TestHexRotateRight();
        Tests.TestHexRotateLeft();
        Tests.TestHexRound();
        Tests.TestHexLinedraw();
        Tests.TestLayout();
        Tests.TestOffsetRoundtrip();
        Tests.TestOffsetFromCube();
        Tests.TestOffsetToCube();
        Tests.TestDoubledRoundtrip();
        Tests.TestDoubledFromCube();
        Tests.TestDoubledToCube();
    }


    static public void Main()
    {
        Tests.TestAll();
    }


    static public void Complain(String name)
    {
        Console.WriteLine("FAIL " + name);
    }

}