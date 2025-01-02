using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static string Run(Board board)
        {
            Move bestmove;

            bestmove = Engine.Search(board);

            if (bestmove.PromPiece != '\0')
            {
                //Console.WriteLine("info isProm " + char.ToLower(bestmove.PromPiece));
                return bestmove.Notation + char.ToLower(bestmove.PromPiece);
            }

            return bestmove.Notation;
        }
        private static Move Search(Board board)
        {
            Move[] moves = Board.GetLegalMoves(board);
            Random rnd = new Random();
            int rand = rnd.Next(moves.Length - 1);
            Move bestmove = moves[rand];

            foreach (Move move in moves)
            {
                board = Board.DoMove(move, board);
                if (Board.GetLegalMoves(board).Length == 0)
                {
                    bestmove = move;
                    Console.WriteLine("info M1");
                    break;
                }
                board = Board.UndoMove(move, board);
            }
            return bestmove;
        }
        private static int Eval(Board board)
        {
            if (Board.GetLegalMoves(board).Length == 0)
            {
                return 1000;
            }
            return 0;
        }
    }
}
