using System;
using Godot;

public static class Resources
{
    public static class Textures
    {
        public static Texture Tile = GD.Load<Texture>("res://Tiles/HexTile.png");
        public static Texture Board = GD.Load<Texture>("res://Tiles/TileBoard.png");
        public static Texture Cursor = GD.Load<Texture>("res://Tiles/TileCursor.png");
        public static Texture CurveLarge = GD.Load<Texture>("res://Tiles/curveLarge.png");
        public static Texture CurveSmall = GD.Load<Texture>("res://Tiles/curveSmall.png");
        public static Texture Line = GD.Load<Texture>("res://Tiles/line.png");
        public static Texture Stop = GD.Load<Texture>("res://Tiles/stop.png");
    }

    public static class Music
    {
        public static AudioStreamOGGVorbis Melody = GD.Load<AudioStreamOGGVorbis>("res://Music/1_melody_loop.ogg");
        public static AudioStreamOGGVorbis Bass = GD.Load<AudioStreamOGGVorbis>("res://Music/2_bass_loop.ogg");
        public static AudioStreamOGGVorbis Swells = GD.Load<AudioStreamOGGVorbis>("res://Music/3_swells_loop.ogg");
        public static AudioStreamOGGVorbis Decoration = GD.Load<AudioStreamOGGVorbis>("res://Music/4_decoration_loop.ogg");
        public static AudioStreamOGGVorbis Eighths = GD.Load<AudioStreamOGGVorbis>("res://Music/5_eighths_loop.ogg");
    }

    public static class Sfx
    {
        public static AudioStreamOGGVorbis CircuitComplete = GD.Load<AudioStreamOGGVorbis>("res://SFX/circuitComplete.ogg");
        public static AudioStreamOGGVorbis CircuitFail = GD.Load<AudioStreamOGGVorbis>("res://SFX/circuitFail.ogg");
        public static AudioStreamOGGVorbis PickUpTile = GD.Load<AudioStreamOGGVorbis>("res://SFX/pickUpTile.ogg");
        public static AudioStreamOGGVorbis PlaceTile = GD.Load<AudioStreamOGGVorbis>("res://SFX/placeTile.ogg");
        public static AudioStreamOGGVorbis RotateTile = GD.Load<AudioStreamOGGVorbis>("res://SFX/rotateTile.ogg");
        public static AudioStreamOGGVorbis WhooshBack = GD.Load<AudioStreamOGGVorbis>("res://SFX/whooshBack.ogg");
    }
}
