using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TicTacToeWeb.Data.Models
{
    public class GameDataModel
    {
        [Key]
        public int GameID { get; set; }
        public String? GameName { get; set; }
        public String? PlayerIDs { get; set; }
        public GameResult? Result { get; set; }
        public String? GameTags { get; set; }
        public DateTime? GameStarted { get; set; }
        public String? GameFieldJSON { get; set; }
        public String? CurrentPlayerMove { get; set; }

        public void AddPlayer(String playerID)
        {
            if (PlayerIDs.Length == 0)
            {
                PlayerIDs = playerID + ",";
                CurrentPlayerMove = playerID;
            }
            else
            {
                PlayerIDs += playerID;
                if (CurrentPlayerMove == "") CurrentPlayerMove = playerID;
            }
        }
        public GameResult? RemovePlayer(String playerID)
        {
            if (PlayerIDs.Contains(playerID))
            {
                var players = PlayerIDs.Split(",").ToList();
                players.Remove(playerID);
                if (players[0] != null && players[0] != "")
                {
                    PlayerIDs = players[0] + ",";
                }
                else
                {
                    return GameResult.END;
                }
            }
            return null;
        }
        public bool Turn(String id,String chosenFieldCell)
        {
            if(id == CurrentPlayerMove)
            {
                var chosen = int.Parse(chosenFieldCell);
                var players = PlayerIDs.Split(",").ToList();
                var currentField = new JavaScriptSerializer().Deserialize<String[]>(GameFieldJSON);
                if(currentField[chosen] != "")
                {
                    return false;
                }
                currentField[chosen] = id;
                Result = DetermineWinner(currentField);
                GameFieldJSON = new JavaScriptSerializer().Serialize(currentField);
                if(Result == GameResult.NONE)
                    CurrentPlayerMove = (players[0] == id) ? players[1] : players[0];
                return true;
            }
            return false;
        }
        private GameResult? DetermineWinner(String[] gameField)
        {
            #region Horzontal Winning Condtion
            if (gameField[0] == gameField[1] && gameField[1] == gameField[2] && gameField[0] != "")
            {
                return GameResult.END;
            }
            else if (gameField[3] == gameField[4] && gameField[4] == gameField[5] && gameField[3] != "")
            {
                return GameResult.END;
            }
            else if (gameField[6] == gameField[7] && gameField[7] == gameField[8] && gameField[6] != "")
            {
                return GameResult.END;
            }
            #endregion
            #region vertical Winning Condtion
            else if (gameField[0] == gameField[3] && gameField[3] == gameField[6] && gameField[0] != "")
            {
                return GameResult.END;
            }
            else if (gameField[1] == gameField[4] && gameField[4] == gameField[7] && gameField[1] != "")
            {
                return GameResult.END;
            }
            else if (gameField[2] == gameField[5] && gameField[5] == gameField[8] && gameField[2] != "")
            {
                return GameResult.END;
            }
            #endregion
            #region Diagonal Winning Condition
            else if (gameField[0] == gameField[4] && gameField[4] == gameField[8] && gameField[0] != "")
            {
                return GameResult.END;
            }
            else if (gameField[2] == gameField[4] && gameField[4] == gameField[6] && gameField[2] != "")
            {
                return GameResult.END;
            }
            #endregion
            #region Checking For Draw
            else if (gameField[0] != "" && gameField[1] != "" && gameField[2] != "" && gameField[3] != "" && gameField[4] != "" && gameField[5] != "" && gameField[6] != "" && gameField[7] != "" && gameField[8] != "")
            {
                return GameResult.DRAW;
            }
            #endregion
            else
            {
                return GameResult.NONE;
            }
        }
    }
}
