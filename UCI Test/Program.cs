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

            Board[] boards = new Board[10];

            boards[0] = Board.BuildFromFenString("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");

            boards[0] = Board.DoMove(new Move { Index1 = 52, Index2 = 36, Notation = "e2e4" }, boards[0]);

            boards[0] = Board.DoMove(new Move { Index1 = 10, Index2 = 26, Notation = "c7c5" }, boards[0]);

            Board.PrintBoard(boards[0]);

            Move[] tests = Board.GetLegalMoves(boards[0]);

            Board.PrintBitboard(boards[0].BiboardAttacked);

            Console.WriteLine("Nodes: " + tests.Length);
            foreach (Move test in tests)
            {
                boards[1] = Board.DoMove(test, boards[0]);
            
                Console.WriteLine(test.Notation);
                Board.PrintBoard(boards[1]);
            }
            
            Console.ReadLine();
            
            
            var timer = System.Diagnostics.Stopwatch.StartNew();
            timer.Start();
            for (int i = 1; i <= 5000000; i++)
            {
                //Board.DoMove(move, boards[0]);
            }
            timer.Stop();
            Console.WriteLine("Time: " + timer.ElapsedMilliseconds);
            
            //Board.PrintBoard(board1);
            Console.ReadLine();
        }
    }
}