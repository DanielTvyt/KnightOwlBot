using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KnightOwlBot
{
    internal class Engine
    {
        private static uint nodes = 0;
        private static uint maxDepth;
        private static long maxTime;
        private static Stopwatch watch = new Stopwatch();
        private static readonly int[] pawnPST = {  0,   0,   0,   0,   0,   0,   0,   0,
                                                  30,  30,  30,  40,  40,  30,  30,  30,
                                                  20,  20,  20,  30,  30,  30,  20,  20,
                                                  10,  10,  15,  25,  25,  15,  10,  10,
                                                   5,   5,   5,  20,  20,   5,   5,   5,
                                                   5,   0,   0,   5,   5,   0,   0,   5,
                                                   5,   5,   5, -10, -10,   5,   5,   5,
                                                   0,   0,   0,   0,   0,   0,   0,   0};
        private static readonly int[] knightPST = { -5,  -5, -5, -5, -5, -5,  -5, -5,
                                           -5,   0,  0, 10, 10,  0,   0, -5,
                                           -5,   5, 10, 10, 10, 10,   5, -5,
                                           -5,   5, 10, 15, 15, 10,   5, -5,
                                           -5,   5, 10, 15, 15, 10,   5, -5,
                                           -5,   5, 10, 10, 10, 10,   5, -5,
                                           -5,   0,  0,  5,  5,  0,   0, -5,
                                           -5, -10, -5, -5, -5, -5, -10, -5};
        private static readonly int[] bishopPST = { 0,   0,   0,   0,   0,   0,   0,   0,
                                           0,   0,   0,   0,   0,   0,   0,   0,
                                           0,   0,   0,   0,   0,   0,   0,   0,
                                           0,  10,   0,   0,   0,   0,  10,   0,
                                           5,   0,  10,   0,   0,  10,   0,   5,
                                           0,  10,   0,  10,  10,   0,  10,   0,
                                           0,  10,   0,  10,  10,   0,  10,   0,
                                           0,   0, -10,   0,   0, -10,   0,   0};
        private static readonly int[] rookPST = { 10,  10,  10,  10,  10,  10,  10,  10,
                                         10,  10,  10,  10,  10,  10,  10,  10,
                                          0,   0,   0,   0,   0,   0,   0,   0,
                                          0,   0,   0,   0,   0,   0,   0,   0,
                                          0,   0,   0,   0,   0,   0,   0,   0,
                                          0,   0,   0,   0,   0,   0,   0,   0,
                                          0,   0,   0,  10,  10,   0,   0,   0,
                                          0,   0,   0,  10,  10,   5,   0,   0};
        private static readonly int[] queenPST = { -20, -10, -10, -5, -5, -10, -10, -20,
                                          -10,   0,   0,  0,  0,   0,   0, -10,
                                          -10,   0,   5,  5,  5,   5,   0, -10,
                                           -5,   0,   5,  5,  5,   5,   0,  -5,
                                           -5,   0,   5,  5,  5,   5,   0,  -5,
                                          -10,   5,   5,  5,  5,   5,   0, -10,
                                          -10,   0,   5,  0,  0,   0,   0, -10,
                                          -20, -10, -10,  0,  0, -10, -10, -20};
        private static readonly int[] kingPST = { 0, 0,  0,  0,   0,  0,  0, 0,
                                         0, 0,  0,  0,   0,  0,  0, 0,
                                         0, 0,  0,  0,   0,  0,  0, 0,
                                         0, 0,  0,  0,   0,  0,  0, 0,
                                         0, 0,  0,  0,   0,  0,  0, 0,
                                         0, 0,  0,  0,   0,  0,  0, 0,
                                         0, 0,  0, -5,  -5, -5,  0, 0,
                                         0, 0, 10, -5,  -5, -5, 10, 0};

        public static ulong Perft(Board[] boards, int depth, int ply)
        {
            Move[] legalMoves = Board.GetLegalMoves(boards[ply - 1]);
            ulong leaves = 0;
            depth--;
            if (depth < 1)
            {
                return (ulong)legalMoves.Length;
            }
            foreach (Move move in legalMoves)
            {
                boards[ply] = Board.DoMove(move, boards[ply - 1]);
                leaves += Perft(boards, depth, ply + 1);
            }
            return leaves;
        }
        public static string Run(Board board, uint time, uint inc)
        {
            string pvString;
            if (time == 0)
            {
                time = 500 * 100; //if no time is given search for 500ms
            }
            uint searchTime = (time + inc) / 100;
            maxTime = (time + inc) / 10;
            watch.Start();
            List<string> pv = [];
            nodes = 0;
            for (uint depth = 1; watch.ElapsedMilliseconds < searchTime || depth <= 1; depth++)
            {
                Board[] boards = new Board[depth + 1];
                boards[0] = board;
                uint ply = 0;
                maxDepth = depth;
                int score;
                int alpha = int.MinValue;
                int beta = int.MaxValue;
                try
                {
                    (score, pv) = Search(boards, depth, ply + 1, alpha, beta);
                }
                catch
                {
                    continue;
                }

                if (!board.IsWhiteToMove)
                {
                    score *= -1;
                }

                long takenTime = watch.ElapsedMilliseconds == 0 ? 1 : watch.ElapsedMilliseconds;

                pv.Reverse();
                pvString = string.Join(" ", pv);

                Console.WriteLine("info depth " + depth + " time " + takenTime + " nodes " + nodes + " pv " + pvString + " score cp " + score + " nps " + Convert.ToUInt32(nodes / (decimal) takenTime * 1000));
            }
            watch = new Stopwatch();
            return pv[0];
        }

        private static (int, List<string>) Search(Board[] board, uint depth, uint ply, int alpha, int beta)
        {
            List<string> pv = [];

            if (depth == 0)
            {
                return (Eval(board[ply - 1]), pv);
            }

            if (watch.ElapsedMilliseconds > maxTime && maxDepth > 1)
            {
                throw new Exception("0");
            }

            Move[] moves = Board.GetLegalMoves(board[ply - 1]);

            if (moves.Length == 0)
            {
                return (board[ply - 1].IsWhiteToMove ? Convert.ToInt32(-10000 * depth) : Convert.ToInt32(10000 * depth), pv);
            }
            string bestMove = null;
            List<string> bestPv = [];
            moves = SortMoves(moves);
            int score;
            int bestScore = board[ply - 1].IsWhiteToMove ? int.MinValue : int.MaxValue;

            foreach (Move move in moves)
            {
                board[ply] = Board.DoMove(move, board[ply - 1]);

                (score, pv) = Search(board, depth - 1, ply + 1, alpha, beta);

                if (board[ply - 1].IsWhiteToMove)
                {
                    if (beta <= score)
                    {
                        pv.Add(move.Notation);
                        return (beta, pv);
                    }
                    if (score > bestScore)
                    {
                        bestMove = move.Notation;
                        bestPv = pv;
                        bestScore = score;
                        alpha = Math.Max(bestScore, alpha);
                    }
                }
                else
                {
                    if (alpha >= score)
                    {
                        pv.Add(move.Notation);
                        return (alpha, pv);
                    }
                    if (score < bestScore)
                    {
                        bestMove = move.Notation;
                        bestPv = pv;
                        bestScore = score;
                        beta = Math.Min(bestScore, beta);
                    }
                }
            }
            bestPv.Add(bestMove);
            return (bestScore, bestPv);
        }


        private static int Eval(Board board)
        {
            nodes++;
            if (Board.IsDraw(board))
            {
                return 0;
            }

            int Material = 0;

            for (int i = 0; i < board.board.Length; i++)
            {
                Piece piece = board.board[i];
                if (piece == null)
                {
                    continue;
                }

                switch (piece.Notation)
                {
                    case 'P':
                        Material += pawnPST[i];
                        break;
                    case 'N':
                        Material += knightPST[i];
                        break;
                    case 'B':
                        Material += bishopPST[i];
                        break;
                    case 'R':
                        Material += rookPST[i];
                        break;
                    case 'Q':
                        Material += queenPST[i];
                        break;
                    case 'K':
                        Material += kingPST[i];
                        break;

                    case 'p':
                        Material -= pawnPST[63 - i];
                        break;
                    case 'n':
                        Material -= knightPST[63 - i];
                        break;
                    case 'b':
                        Material -= bishopPST[63 - i];
                        break;
                    case 'r':
                        Material -= rookPST[63 - i];
                        break;
                    case 'q':
                        Material -= queenPST[63 - i];
                        break;
                    case 'k':
                        Material -= kingPST[63 - i];
                        break;
                }
                Material += piece.Material;
            }
            return Material;
        }

        private static Move[] SortMoves(Move[] moves)
        {
            Array.Sort(moves, delegate (Move x, Move y) { return x.MoveValue.CompareTo(y.MoveValue); });
            return moves;
        }
    }
}
