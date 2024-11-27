using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UCI_Test
{
    internal class Uci
    {
        public static Task Listen()
        { 
            string bestmove;
            string position = "";
            
            Board board = new Board();
            while (true)
            {
                string UciIn = Console.ReadLine();
                //Task<Board> taskGetPos = new Task<Board>(() => { return Uci.GetPos(UciIn); });
                if (UciIn.Contains("position"))
                {
                    position = UciIn;
                }
                else if (UciIn.Contains("go"))
                {
                    board = Uci.GetPos(position);
                    //Board.PrintBoard(board);
                    //Console.WriteLine(board.IsWhiteToMove);
                    bestmove = Engine.Run(board);
                    Console.WriteLine("info depth 5 score 1");
                    Console.WriteLine("bestmove " + bestmove);
                }
                else if (UciIn == "uci")
                {
                    Console.WriteLine("id name UciTest");
                    Console.WriteLine("uciok");
                }
                else if (UciIn.Contains("isready"))
                {
                    Console.WriteLine("readyok");
                }
            }
        }


        public static Board GetPos(string uciIn)
        {
            string fenString;
            Board board = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            if (uciIn.Contains("fen"))   //positon fen <fenstring>
            {
                fenString = uciIn.Remove(0, 13);
                return Board.BuildFromFenString(fenString);
            }
            else
            {
                if (uciIn == "position startpos")
                {
                    return board;
                }
                else                     //position startpos moves <move1 move2 ...>
                {
                    string[] moves = uciIn.Split(' ');

                    for (int i = 3; i < moves.Length; i++) //skip pos, start, moves
                    {
                        Move move = new Move();
                        move.Notation = moves[i];
                        if (move.Notation.Length == 5)
                        {
                            move.PromPiece = move.Notation[4];
                            move.Notation = move.Notation.Remove(4,1);
                            if (board.IsWhiteToMove)
                            {
                                move.PromPiece = char.ToUpper(move.PromPiece);
                            }
                        }
                        board = Board.DoMove(move, board);
                    }
                    return board;
                }
            }
        }
    }
}
