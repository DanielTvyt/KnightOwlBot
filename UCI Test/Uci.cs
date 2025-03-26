using System;

namespace KnightOwlBot
{
    internal class Uci
    {
        public static void Listen()
        { 
            string bestmove;
            string position = "";
            Board board;

            while (true)
            {
                uint time = 0;
                uint inc = 0;
                string sTime = "";
                string sInc = "";
                string UciIn = Console.ReadLine();

                if (UciIn.Contains("position"))
                {
                    position = UciIn;
                }
                else if (UciIn.Contains("go"))
                {
                    board = Uci.GetPos(position);
                    if (board.IsWhiteToMove)
                    {
                        if (UciIn.Contains("wtime"))
                        {
                            int x = UciIn.IndexOf("wtime");
                            for (int i = x + 6; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sTime += UciIn[i];
                            }
                            time = Convert.ToUInt32(sTime);
                        }
                        if (UciIn.Contains("winc"))
                        {
                            int x = UciIn.IndexOf("winc");
                            for (int i = x + 5; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sInc += UciIn[i];
                            }
                            inc = Convert.ToUInt32(sTime);
                        }
                    }
                    else
                    {
                        if (UciIn.Contains("btime"))
                        {
                            int x = UciIn.IndexOf("btime");
                            for (int i = x + 6; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sTime += UciIn[i];
                            }
                            time = Convert.ToUInt32(sTime);
                        }
                        if (UciIn.Contains("binc"))
                        {
                            int x = UciIn.IndexOf("binc");
                            for (int i = x + 5; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sInc += UciIn[i];
                            }
                            inc = Convert.ToUInt32(sTime);
                        }
                    }
                    if (UciIn.Contains("infinite"))
                    {
                        time = uint.MaxValue;
                    }
                    bestmove = Engine.Run(board, time, inc);
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
                else if (UciIn == "print")
                {
                    board = Uci.GetPos(position);
                    Board.PrintBoard(board);
                }
                else if (UciIn == "quit")
                {
                    Environment.Exit(0);
                }
            }
        }


        public static Board GetPos(string uciIn)
        {
            string fenString;
            if (uciIn.Contains("fen"))   //positon fen <fenstring>
            {
                fenString = uciIn.Remove(0, 13);
                return Board.BuildFromFenString(fenString);
            }
            else
            {
                Board board = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                if (uciIn == "position startpos")
                {
                    return board;
                }
                else                     //position startpos moves <move1 move2 ...>
                {
                    string[] moves = uciIn.Split(' ');

                    for (int i = 3; i < moves.Length; i++) //skip pos, start, moves
                    {
                        Move move = new()
                        {
                            Notation = moves[i]
                        };
                        if (move.Notation.Length == 5)
                        {
                            move.PromPiece = move.Notation[4];
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
