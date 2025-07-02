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

            boards[0] = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Board postion = boards[0];

            Board.PrintBoard(boards[0]);
            Move[] tests = Board.GetLegalMoves(boards[0]);
            foreach (Move test in tests)
            {
                boards[1] = Board.DoMove(test, boards[0]);
            
                Console.WriteLine(test.Notation);
                Board.PrintBoard(boards[1]);
            }
            
            Console.ReadLine();
            
            Move move = new Move();
            move.Notation = "e2e4";
            
            var timer = System.Diagnostics.Stopwatch.StartNew();
            timer.Start();
            for (int i = 1; i <= 5000000; i++)
            {
                Board.DoMove(move, boards[0]);
            }
            timer.Stop();
            Console.WriteLine("Time: " + timer.ElapsedMilliseconds);
            
            //Board.PrintBoard(board1);
            Console.ReadLine();
        }
    }
}