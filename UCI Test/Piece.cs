﻿namespace KnightOwlBot
{
    internal class Piece
    {
        public int[] MoveDelta;
        public bool IsWhite;
        public bool IsSliding;
        public int Material;
        public char Notation;

        public static Piece CreatePiece(char pieceIn)
        { 
            int[] bishop = [-9, -7, 7, 9];
            int[] rook = [-8, -1, 1, 8];
            int[] queen = [-9, -8, -7, -1, 1, 7, 8, 9];
            int[] knight = [-17, -15, -10, -6, 6, 10, 15, 17];
            switch (pieceIn)
            {
                case 'P':
                    Piece pawnW = new Piece
                    {
                        IsWhite = true,
                        Material = 100,
                        IsSliding = false,
                        Notation = pieceIn,
                    };
                    return pawnW;
                case 'N':
                    Piece knightW = new Piece
                    {
                        IsWhite = true,
                        Material = 300,
                        IsSliding = false,
                        MoveDelta = knight,
                        Notation = pieceIn,
                    };
                    return knightW;
                case 'B':
                    Piece bishopW = new Piece
                    {
                        IsWhite = true,
                        Material = 300,
                        IsSliding = true,
                        MoveDelta = bishop,
                        Notation = pieceIn,
                    };
                    return bishopW;
                case 'R':
                    Piece rookW = new Piece
                    {
                        IsWhite = true,
                        Material = 500,
                        IsSliding = true,
                        MoveDelta = rook,
                        Notation = pieceIn,
                    };
                    return rookW;
                case 'Q':
                    Piece queenW = new Piece
                    {
                        IsWhite = true,
                        Material = 900,
                        IsSliding = true,
                        MoveDelta = queen,
                        Notation = pieceIn,
                    };
                    return queenW;
                case 'K':
                    Piece kingW = new Piece
                    {
                        IsWhite = true,
                        Material = 0,
                        IsSliding = false,
                        MoveDelta = queen,
                        Notation = pieceIn,
                    };
                    return kingW;

                case 'p':
                    Piece pawnB = new Piece
                    {
                        IsWhite = false,
                        Material = -100,
                        IsSliding = false,
                        Notation = pieceIn,
                    };
                    return pawnB;
                case 'n':
                    Piece knightB = new Piece
                    {
                        IsWhite = false,
                        Material = -300,
                        IsSliding = false,
                        MoveDelta = knight,
                        Notation = pieceIn,
                    };
                    return knightB;
                case 'b':
                    Piece bishopB = new Piece
                    {
                        IsWhite = false,
                        Material = -300,
                        IsSliding = true,
                        MoveDelta = bishop,
                        Notation = pieceIn,
                    };
                    return bishopB;
                case 'r':
                    Piece rookB = new Piece
                    {
                        IsWhite = false,
                        Material = -500,
                        IsSliding = true,
                        MoveDelta = rook,
                        Notation = pieceIn,
                    };
                    return rookB;
                case 'q':
                    Piece queenB = new Piece
                    {
                        IsWhite = false,
                        Material = -900,
                        IsSliding = true,
                        MoveDelta = queen,
                        Notation = pieceIn,
                    };
                    return queenB;
                case 'k':
                    Piece kingB = new Piece
                    {
                        IsWhite = false,
                        Material = 0,
                        IsSliding = false,
                        MoveDelta = queen,
                        Notation = pieceIn,
                    };
                    return kingB;

                default:
                    return null;
            }
        }
    }
}
