using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightOwlBot.Engine
{
    internal class Evaluation
    {
        public static int Eval(Board board)
        {
            Start.nodes++;

            int score = 0;
            int pstScore = 0;
            int openingRatio = board.pieceCount * 5;
            int endgameRatio = 100 - openingRatio;

            for (int i = 0; i < 64; i++)
            {
                Piece piece = board.board[i];
                if (piece == null) continue;

                score += piece.Material;

                switch (piece.Notation)
                {
                    case Piece.P:
                        pstScore += (PST.pawn[i] * openingRatio) + (PST.egPawn[i] * endgameRatio);
                        break;
                    case Piece.N:
                        pstScore += (PST.knight[i] * openingRatio) + (PST.egKnight[i] * endgameRatio);
                        break;
                    case Piece.B:
                        pstScore += (PST.bishop[i] * openingRatio) + (PST.egBishop[i] * endgameRatio);
                        break;
                    case Piece.R:
                        pstScore += (PST.rook[i] * openingRatio) + (PST.egRook[i] * endgameRatio);
                        break;
                    case Piece.Q:
                        pstScore += (PST.queen[i] * openingRatio) + (PST.egQueen[i] * endgameRatio);
                        break;
                    case Piece.K:
                        pstScore += (PST.king[i] * openingRatio) + (PST.egking[i] * endgameRatio);
                        break;

                    case Piece.p:
                        pstScore -= (PST.pawn[63 - i] * openingRatio) + (PST.egPawn[63 - i] * endgameRatio);
                        break;
                    case Piece.n:
                        pstScore -= (PST.knight[63 - i] * openingRatio) + (PST.egKnight[63 - i] * endgameRatio);
                        break;
                    case Piece.b:
                        pstScore -= (PST.bishop[63 - i] * openingRatio) + (PST.egBishop[63 - i] * endgameRatio);
                        break;
                    case Piece.r:
                        pstScore -= (PST.rook[63 - i] * openingRatio) + (PST.egRook[63 - i] * endgameRatio);
                        break;
                    case Piece.q:
                        pstScore -= (PST.queen[63 - i] * openingRatio) + (PST.egQueen[63 - i] * endgameRatio);
                        break;
                    case Piece.k:
                        pstScore -= (PST.king[63 - i] * openingRatio) + (PST.egking[63 - i] * endgameRatio);
                        break;
                }
            }
            score += pstScore / 100;
            return board.IsWhiteToMove ? score : -score;
        }
    }
}
