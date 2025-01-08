using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UCI_Test
{
    internal class Engine
    {
        public static int nodes = 0;
        public static ulong Perft(Board board, int depth)
        {
            Move[] legalMoves = Board.GetLegalMoves(board);
            ulong leaves = 0;
            depth--;
            if (depth < 1)
            {
                return (ulong)legalMoves.Length;
            }
            foreach (Move move in legalMoves)
            {
                board = Board.DoMove(move, board);
                //Console.WriteLine(board.EnPassentIndex);
                leaves += Perft(board, depth);
                board = Board.UndoMove(move, board);
            }
            return leaves;
        }
        public static string Run(Board board, uint time)
        {
            int depth = 2;
            if (time != 0)
            {
                System.Timers.Timer timer = new System.Timers.Timer(time);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
            
            Move bestmove = Engine.Search(board, depth);

            if (bestmove.PromPiece != '\0')
            {
                //Console.WriteLine("info isProm " + char.ToLower(bestmove.PromPiece));
                return bestmove.Notation + char.ToLower(bestmove.PromPiece);
            }

            return bestmove.Notation;
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }

        private static Move Search(Board board, int depth)
        {
            Move[] moves = Board.GetLegalMoves(board);
            Random rnd = new Random();
            int rand = rnd.Next(moves.Length - 1);
            Move bestmove = moves[rand];
            int score;
            int maxScore = int.MinValue;
            int minScore = int.MaxValue;

            for (int d = 1; d <= depth; d++)
            {
                foreach (Move move in moves)
                {
                    board = Board.DoMove(move, board);
                    if (Board.GetLegalMoves(board).Length == 0) //Mate in one
                    {
                        bestmove = move;
                        Console.WriteLine("info M1");
                        break;
                    }

                    score = Eval(board);

                    board = Board.UndoMove(move, board);

                    if (board.IsWhiteToMove)
                    {
                        if (score > maxScore)
                        {
                            bestmove = move;
                            maxScore = score;
                        }
                    }
                    else
                    {
                        if (score < minScore)
                        {
                            bestmove = move;
                            minScore = score;
                        }
                    }

                }
                if (board.IsWhiteToMove)
                {
                    Console.WriteLine("info depth " + d + " score " + maxScore);
                }
                else
                {
                    Console.WriteLine("info depth " + d + " score " + minScore);
                }
            }
            return bestmove;
        }
        private static int Eval(Board board)
        {
            int Material = 0;
            if (Board.GetLegalMoves(board).Length == 0)
            {
                if (board.IsWhiteToMove)
                {
                    return 1000;
                }
                return -1000;
            }

            foreach (Piece piece in board.board)
            {
                if (piece == null)
                {
                    continue;
                }
                Material += piece.Material;
            }

            return Material;
        }
    }
}
