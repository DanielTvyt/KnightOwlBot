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

            Board board = Board.BuildFromFenString("rnbqkb1r/ppppp1pp/7n/4Pp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3");

            board.PrintBoard();

            Move[] moves = Board.GetLegalMoves(board);

            Board.PrintBitboard(board.BitboardAttacked);

            Console.WriteLine("Nodes: " + moves.Length);
            foreach (Move m in moves)
            {
                board.DoMove(m);
                Console.WriteLine(m.Notation + " Is Capture? " + m.IsCapture + " What Piece was Captured? " + m.LastCapture);
                board.PrintBoard();
                board.UndoMove(m);
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