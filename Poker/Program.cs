using System.Formats.Tar;
using static Poker.Program; 
namespace Poker;

/*
 * musí se dodělat logika check
 * musí se dodělat fold
 * není zde CheckPlayerBets() - místo toho je jen LINQ funkce, která checkuje, zda jsou vsechny sazky rovny currentRoundBet
 * Musí se dodělat COR
 * */

class Program
{
    static void Main(string[] args)
    {
        //inicialization
        Player player1 = new Player("Denis", 100);
        Player player2 = new Player("Daniel", 200);
        Player player3 = new Player("Ondřej", 700);
        Player player4 = new Player("Petr", 230);
        List<Player> players = new List<Player> { player1, player2, player3 };
        GameManager game = new GameManager(players);

        //game loop
        Deck.DealTheCardsToPlayers(players);
        player1.isOnTurn = true;
        Deck.DealFlop();
        Deck.DealTurn();
        Deck.DealRiver();
        game.currentPlayer = player1;
        player1.currentBet = 1;

        while (game.gameRound != 3) //vykonávat tohle celé, než bude konec hry
        {
            game.GameInputController();
            game.RefreshStats();
            player1.currentBet = 1;
        }

    }

    public class Player
    {
        public string playerName;
        public int playerCash;
        public bool isWinner;
        public List<Card> hand;
        public bool isOnTurn;
        public int currentBet;
        public bool didFold;
        public bool didAllIn;
        public List<int> bets;
        public string choice;
        public bool didCheck;

        public Player(string name, int cash)
        {
            playerName = name;
            playerCash = cash;
            hand = new List<Card>();
            bets = new List<int>();
            currentBet = 0;
            didFold = false;
            didAllIn = false;
            didCheck = false;
            choice = "";
            isWinner = false;
        }

        public void ShowHand()
        {
            foreach (Card card in hand)
            {
                Console.WriteLine($"{card}");
            }
        }
    }

    public enum Suit { Hearts, Diamonds, Clubs, Spades }

    public enum Rank
    {
        Ace = 1,
        Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
        Jack, // 11
        Queen, // 12
        King // 13
    }

    public static class Deck
    {
        public static Random random = new Random();
        public static int deckSize = 52;
        public static List<Card> deck = new List<Card>();
        public static List<Card> flop = new List<Card>();
        public static Card turn;
        public static Card river;

        static Deck()
        {
            deckSize = 52;
            deck = new List<Card>(deckSize);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    Card card = new Card(rank, suit);
                    deck.Add(card);
                }
            }
        }

        public static void Shuffle()
        {
            random = new Random();

            while (deckSize > 1)
            {
                deckSize--;
                int k = random.Next(deckSize);
                Card value = deck[k];
                deck[k] = deck[deckSize];
                deck[deckSize] = value;
            }
        }

        public static bool DealTheCardsToPlayers(List<Player> players)
        {
            Shuffle();

            foreach (Player player in players)
            {
                Card card1 = deck[0];
                deck.RemoveAt(0);
                Card card2 = deck[0];
                deck.RemoveAt(0);
                player.hand.Add(card1);
                player.hand.Add(card2);
            }

            return true;
        }

        public static void DealFlop()
        {
            int randomIndex = random.Next(deck.Count);

            for (int i = 0; i < 3; i++)
            {
                Card randomCard = deck[randomIndex];
                flop.Add(randomCard);
                deck.RemoveAt(randomIndex);
            }
        }

        public static void DealTurn()
        {
            int randomIndex = random.Next(deck.Count);
            Card randomCard = deck[randomIndex];
            turn = randomCard;
            deck.RemoveAt(randomIndex);
        }

        public static void DealRiver()
        {
            int randomIndex = random.Next(deck.Count);
            Card randomCard = deck[randomIndex];
            river = randomCard;
            deck.RemoveAt(randomIndex);
        }
    }


    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Rank rank, Suit suit)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString() => $"{(int)Rank} of {Suit}";

        public Rank GetRank() => Rank;

        public Suit GetSuit() => Suit;
    }


    class GameManager
    {
        public Player currentPlayer;
        public List<Player> players;
        public int playersChoice;
        public int currentRoundBet;
        public int pot;
        public int numberOfPLayers;
        public int gameRound;
        public bool isGameOver;
        public bool allPlayersBetsSame;
        public Player playerToSkip;


        public GameManager(List<Player> listOfPlayers)
        {
            players = listOfPlayers;
            numberOfPLayers = players.Count;
            pot = 0;
            gameRound = 0;
            isGameOver = false; //if players.Count == 1 a nebo jiny pravidla
            allPlayersBetsSame = false;
            currentRoundBet = 0;
        }

        public void PrintFlop()
        {
            Console.WriteLine("");
            Console.WriteLine($"  Pot value: {pot}");
            Console.WriteLine("");
            string flopString = "Flop cards: ";

            //Concatenate the string representations of the flop cards
            foreach (Card card in Deck.flop)
            {
                flopString += card.ToString() + "    ";
            }

            //Print the concatenated string
            Console.WriteLine(flopString);
        }

        public void PrintTurn()
        {
            Console.WriteLine("");
            Console.WriteLine($"  Pot value: {pot}");
            Console.WriteLine("");
            string flopString = "Flop cards: ";

            foreach (Card card in Deck.flop)
            {
                flopString += card + "    ";
            }

            Console.WriteLine(flopString);
            Console.WriteLine($"Turn cards: {Deck.turn}");
        }

        public void PrintRiver()
        {
            Console.WriteLine("");
            Console.WriteLine($"  Pot value: {pot}");
            Console.WriteLine("");
            string flopString = "Flop cards: ";

            foreach (Card card in Deck.flop)
            {
                flopString += card.ToString() + "    ";
            }

            Console.WriteLine(flopString);
            Console.WriteLine($"Turn card: {Deck.turn}");
            Console.WriteLine($"River card: {Deck.river}");
        }


        public void SwitchPlayer()
        {
            for (int i = 0; i < numberOfPLayers; i++)
            {
                if (players[i].isOnTurn)
                {
                    players[i].isOnTurn = false;
                    int nextIndex = (i + 1) % numberOfPLayers;
                    players[nextIndex].isOnTurn = true;
                    currentPlayer = players[nextIndex];
                    break;
                }
            }
        }

        public void ProcessPlayersInput()
        {
            bool validInput = false;

            while (!validInput)
            {
                Console.WriteLine("0| check");
                Console.WriteLine("1| call"); //( cant call if there is no bet :) )
                Console.WriteLine("2| raise");
                Console.WriteLine("3| fold");
                Console.WriteLine("4| all in");
                string input = Console.ReadLine();

                playersChoice = Convert.ToInt16(input);

                switch (playersChoice)
                {
                    case 0:
                        validInput = Check(currentPlayer);
                        currentPlayer.choice = "check";
                        break;
                    case 1:
                        validInput = Call(currentPlayer);
                        currentPlayer.choice = "call";
                        break;
                    case 2:
                        validInput = Raise(currentPlayer);
                        currentPlayer.choice = "raise";
                        break;
                    case 3:
                        validInput = true;
                        Fold(currentPlayer);
                        currentPlayer.choice = "fold";
                        break;
                    case 4:
                        validInput = AllIn(currentPlayer);
                        currentPlayer.choice = "allin";
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please choose again.");
                        break;
                }

                if (validInput && (playersChoice == 1 || playersChoice == 2)) //pokud je vstup validni a zaroven dal hrac call nebo raise (tedy je tam nejaka castka), pak se vypise, ktery hrac to byl a kolik vsadil
                {
                    Console.WriteLine($"Player {currentPlayer.playerName} bet {currentPlayer.currentBet}");
                }
            }
        }

        public bool Raise(Player player)
        {
            Console.WriteLine("How much would you like to bet?");
            string bet = Console.ReadLine();
            player.currentBet = Convert.ToInt32(bet);

            if (player.currentBet < currentRoundBet)
            {
                Console.WriteLine("Bet again!");

                return false;
            }
            else if (player.playerCash >= player.currentBet)
            {
                currentRoundBet = player.currentBet;
                return true;
            }
            else
            {
                Console.WriteLine("Not enough cash");
                return false;
            }
        }


        public bool Call(Player player)
        {
            if (player.playerCash >= currentRoundBet)
            {
                //when a call occurs, player's bet is equal to the round bet
                player.currentBet = currentRoundBet;
                return true;
            }
            else
            {
                Console.WriteLine("Not enough cash");
                return false;
            }
        }

        public void Fold(Player player)
        {
            player.didFold = true;
            playerToSkip = player;

        }

        public bool Check(Player player)
        {
            player.didCheck = true;
            return true;
        }



        public bool AllIn(Player player)
        {
            bool didSomeoneAllIn = players.Any(p => p.didAllIn);
            List<Player> allInners = new List<Player>();
            bool isAllInnersEmpty = !allInners.Any();

            if (isAllInnersEmpty || player.playerCash < currentRoundBet)
            {
                player.currentBet = player.playerCash;
                currentRoundBet = player.currentBet;
                player.didAllIn = true;
                return true;
            }
            else
            {
                //vezmeme částku allInnera, který vsadil nejvíce a přiřadíme ji hráči jako jeho sázku
                int maxBetFromAllInners = allInners.Max(aI => aI.currentBet);
                player.currentBet = maxBetFromAllInners;
                currentRoundBet = player.currentBet;
                player.didAllIn = true;
                return true;
            }
        }

        public int UpdateGamePot()
        {
            int finalBet = 0;
            foreach (Player player in players)
            {
                player.playerCash -= player.currentBet;
                player.bets.Add(player.currentBet);
                finalBet += player.currentBet;
            }
            return pot += finalBet;
        }

        public bool DidAllPlayersAllIn() => players.All(player => player.didAllIn);

        public int NPlayersFold() => players.Count(player => player.didFold);

        public bool didAllPlayersCheck() => players.All(player => player.didCheck);

        //dodelat!
        public bool FindWinner()
        {
            List<Player> foldedPlayers = new List<Player>();
            foreach (Player player in players)
            {
                if (player.didFold && NPlayersFold() > 1)
                {
                    foldedPlayers.Add(player);
                }
            }

            List<Player> differenceList = players.Except(foldedPlayers).ToList();

            foreach (Player p in differenceList)
            {
                if (differenceList.Count == 1)
                {
                    p.isWinner = true;
                    Console.WriteLine($"{p.playerName} won {p.currentBet + pot}!");
                    return true;
                }
            }
            return false;
        }

        public void RoundCounter()
        {
            if (players.All(player => player.currentBet == currentRoundBet) || players.All(player => player.didCheck))
            {
                gameRound += 1;
            }
            else if (DidAllPlayersAllIn()) //pokud všichni dají all in, tak můžeme rovnou ukázat všechny karty
            {
                gameRound += 3;
            }
        }

        public void RefreshStats()
        {
            foreach (Player player in players)
            {
                player.didCheck = false;
                player.didFold = false;
                player.didAllIn = false;
                player.currentBet = 0;
                playerToSkip = null;
            }
            currentRoundBet = 0;

        }

        public void GameInputController()
        {
            do
            {
                Console.WriteLine(currentPlayer.playerName);
                ProcessPlayersInput();

                SwitchPlayer();
            }

            while (!didAllPlayersCheck() && !players.All(player => player.currentBet == currentRoundBet));

            if (didAllPlayersCheck())
            {
                pot -= 1;
            }

            RoundCounter();

            UpdateGamePot();

            switch (gameRound)
            {
                case 1:
                    PrintFlop();
                    break;
                case 2:
                    PrintTurn();
                    break;
                case 3:
                    PrintRiver();
                    break;
            }
        }
    }
}