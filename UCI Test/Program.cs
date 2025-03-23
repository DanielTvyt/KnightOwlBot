using System;

namespace KnightOwlBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Uci.Listen();
            }

            //startpos rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            Board board1 = new Board();
            board1 = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Board.PrintBoard(board1);
            Move[] tests = Board.GetLegalMoves(board1);
            foreach (Move test in tests)
            {
                board1 = Board.DoMove(test, board1);
            
                Console.WriteLine(test.Notation);
                Board.PrintBoard(board1);
            
                board1 = Board.UndoMove(test, board1);
            }

            Console.ReadLine();
            for (int i = 1; i <= 7; i++)
            {
                var timer1 = System.Diagnostics.Stopwatch.StartNew();
                ulong nodes = 0;
                ulong curNodes;
                timer1.Start();
                Move[] moves = Board.GetLegalMoves(board1);
                Console.WriteLine(moves.Length);
                for (int j = 0; j < moves.Length; j++)
                {
                    board1 = Board.DoMove(moves[j], board1);
                    Console.Write(moves[j].Notation + ": ");
                    curNodes = Engine.Perft(board1, i - 1);
                    Console.WriteLine(curNodes);
                    nodes += curNodes;
                    board1 = Board.UndoMove(moves[j], board1);
                }
                
                timer1.Stop();
                Console.WriteLine("ply: " + i + " Time: " + timer1.ElapsedMilliseconds + " Nodes: " + nodes);
            }
            Move move = new Move();
            move.Notation = "e2e4";
            
            var timer = System.Diagnostics.Stopwatch.StartNew();
            timer.Start();
            for (int i = 1; i <= 5000000; i++)
            {
                //Board.GetCaptures(board1);
            }
            timer.Stop();
            Console.WriteLine("Time: " + timer.ElapsedMilliseconds);
            
            //Board.PrintBoard(board1);
            Console.ReadLine();
        }
    }
}