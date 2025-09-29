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
        private static Stopwatch watch = new();

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
            nodes = 0;
            int score;
            List<string> pv = [];
            List<Board> boards = [board];
            for (uint depth = 1; watch.ElapsedMilliseconds < searchTime || depth <= 1; depth++)
            {
                int ply = 0;
                maxDepth = depth;
                int alpha = -10000000;
                int beta  =  10000000;
                try
                {
                    (score, pv) = Search(boards, depth, ply + 1, alpha, beta);
                }
                catch
                {
                    continue;
                }

                long takenTime = watch.ElapsedMilliseconds == 0 ? 1 : watch.ElapsedMilliseconds;
                pv.Reverse();
                pvString = string.Join(" ", pv);
                int selDepth = pv.Count;
                string bestScore;

                if (Math.Abs(score) > 10000) //checkmate
                {
                    string perspective = score > 0 ? "" : "-";
                    bestScore = "mate " + perspective + (100000 - Math.Abs(score)) / 2;
                }
                else
                {
                    bestScore = "cp " + score.ToString();
                }

                Console.WriteLine("info depth " + depth + " seldepth " + selDepth + " score " + bestScore + " nodes " + nodes + " nps " + Convert.ToUInt32(nodes / (decimal)takenTime * 1000) + " time " + takenTime + " pv " + pvString);
            }
            watch = new Stopwatch();
            return pv[0];
        }

        private static (int, List<string>) Search(List<Board> board, uint depth, int ply, int alpha, int beta)
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
                if (board[ply - 1].IsInCheck)
                {
                    return (Convert.ToInt32(-100000 + ply), pv);
                }
                return (0, pv);
            }

            string bestMove = null;
            List<string> bestPv = [];
            moves = SortMoves(moves);
            int score;

            if (board.Count <= ply)
            {
                board.Add(null);
            }

            foreach (Move move in moves)
            {
                board[ply] = Board.DoMove(move, board[ply - 1]);

                if (move.IsCapture && depth == 1) //quiescence search
                {
                    (score, pv) = QuiescenceSearch(board, ply + 1, -beta, -alpha);
                    score *= -1;
                }
                else
                {
                    (score, pv) = Search(board, depth - 1, ply + 1, -beta, -alpha);
                    score *= -1;
                }

                if (score >= beta)
                {
                    return (beta, pv);
                }
                if (score > alpha)
                {
                    bestMove = move.Notation;
                    bestPv = pv;
                    alpha = score;
                }
            }
            bestPv.Add(bestMove);
            return (alpha, bestPv);
        }

        private static (int, List<string>) QuiescenceSearch(List<Board> board, int ply, int alpha, int beta)
        {
            List<string> pv = [];
            int standPat = Eval(board[ply - 1]);

            if (standPat >= beta)
            {
                return (beta, pv);
            }
            if (standPat > alpha)
            {
                alpha = standPat;
            }
            Move[] moves = Board.GetLegalMoves(board[ply - 1]);
            moves = Array.FindAll(moves, m => m.IsCapture);
            moves = SortMoves(moves);
            int score;
            string bestMove = null;
            if (board.Count <= ply)
            {
                board.Add(null);
            }
            foreach (Move move in moves)
            {
                board[ply] = Board.DoMove(move, board[ply - 1]);
                (score, pv) = QuiescenceSearch(board, ply + 1, -beta, -alpha);
                score *= -1;
                if (score >= beta)
                {
                    return (score, pv);
                }
                if (score > alpha)
                {
                    alpha = score;
                    bestMove = move.Notation;
                }
            }
            pv.Add(bestMove);
            return (alpha, pv);
        }

        private static int Eval(Board board)
        {
            nodes++;

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
                    case 1:
                        Material += PST.pawn[i];
                        break;
                    case 2:
                        Material += PST.knight[i];
                        break;
                    case 3:
                        Material += PST.bishop[i];
                        break;
                    case 4:
                        Material += PST.rook[i];
                        break;
                    case 5:
                        Material += PST.queen[i];
                        break;
                    case 6:
                        Material += PST.king[i];
                        break;

                    case 7:
                        Material -= PST.pawn[63 - i];
                        break;
                    case 8:
                        Material -= PST.knight[63 - i];
                        break;
                    case 9:
                        Material -= PST.bishop[63 - i];
                        break;
                    case 10:
                        Material -= PST.rook[63 - i];
                        break;
                    case 11:
                        Material -= PST.queen[63 - i];
                        break;
                    case 12:
                        Material -= PST.king[63 - i];
                        break;
                }
                Material += piece.Material;
            }
            int perspective = board.IsWhiteToMove ? 1 : -1;
            return Material * perspective;
        }

        private static Move[] SortMoves(Move[] moves)
        {
            Array.Sort(moves, delegate (Move x, Move y) { return x.MoveValue.CompareTo(y.MoveValue); });
            return moves;
        }
    }
}
